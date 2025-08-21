# Unity Mobile Performance Optimization Guide

This guide provides comprehensive solutions to improve your Unity mobile game's performance metrics based on the dashboard data:

- **Cold Start**: 5.03% slow rate (Target: <2%)
- **Warm Start**: 28.84% slow rate (Target: <15%)
- **Hot Start**: 11.21% slow rate (Target: <5%)
- **Rendering**: 17.53% slow session rate (Target: <10%)

## 📁 Files Overview

1. **`UnityMobilePerformanceOptimization.cs`** - Main comprehensive optimization script
2. **`MobileStartupOptimizer.cs`** - Specialized startup optimization
3. **`MobileRenderingOptimizer.cs`** - Specialized rendering optimization
4. **`README_MobilePerformance.md`** - This implementation guide

## 🚀 Quick Implementation

### Step 1: Add Core Scripts
1. Create an empty GameObject named "PerformanceManager"
2. Attach `UnityMobilePerformanceOptimization.cs` to it
3. Configure the settings in the inspector

### Step 2: Add Specialized Optimizers
1. Create an empty GameObject named "StartupOptimizer"
2. Attach `MobileStartupOptimizer.cs` to it
3. Create an empty GameObject named "RenderingOptimizer"
4. Attach `MobileRenderingOptimizer.cs` to it

### Step 3: Configure Settings
```csharp
// In UnityMobilePerformanceOptimization inspector:
- Target Frame Rate: 30
- Enable Performance Mode: true
- Enable Adaptive Quality: true
- Preload Critical Assets: true
- Enable Async Loading: true
```

## 🎯 Specific Metric Solutions

### Cold Start Optimization (5.03% → <2%)

**Problem**: App takes too long to launch initially

**Solutions**:
```csharp
// 1. Immediate frame rate setting
Application.targetFrameRate = 30;
QualitySettings.vSyncCount = 0;

// 2. Disable garbage collection during startup
GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;

// 3. Preload only essential assets
string[] criticalAssets = {
    "UI/MainMenu",
    "Audio/UI_Sounds", 
    "Materials/BasicUI"
};
```

**Implementation**:
- Use `MobileStartupOptimizer.cs` with `preloadEssentialAssets = true`
- Set `maxStartupTime = 3f`
- Enable `enableProgressiveLoading`

### Warm Start Optimization (28.84% → <15%)

**Problem**: App restarts slowly after being backgrounded

**Solutions**:
```csharp
// 1. Progressive UI loading
Canvas[] canvases = FindObjectsOfType<Canvas>();
foreach (Canvas canvas in canvases)
{
    canvas.enabled = false;
    StartCoroutine(EnableCanvasProgressive(canvas));
}

// 2. Deferred initialization
yield return new WaitForEndOfFrame();
AnalyticsManager.Instance.Initialize();
SocialManager.Instance.Initialize();
```

**Implementation**:
- Use `MobileStartupOptimizer.cs` with `enableProgressiveLoading = true`
- Set `maxConcurrentLoads = 3`
- Enable `enableMemoryOptimization`

### Hot Start Optimization (11.21% → <5%)

**Problem**: App transitions between scenes slowly

**Solutions**:
```csharp
// 1. Object pooling for all instantiation
ObjectPoolManager.Instance.PreAllocatePool("UI_Button", 20);
ObjectPoolManager.Instance.PreAllocatePool("Enemy", 10);

// 2. Memory optimization
long currentMemory = System.GC.GetTotalMemory(false) / (1024 * 1024);
if (currentMemory > targetMemoryUsage)
{
    System.GC.Collect();
    Resources.UnloadUnusedAssets();
}
```

**Implementation**:
- Use object pooling for all frequently spawned objects
- Set `targetMemoryUsage = 512` (MB)
- Enable `enableMemoryOptimization`

### Rendering Optimization (17.53% → <10%)

**Problem**: Frame rate drops below 30 FPS

**Solutions**:
```csharp
// 1. Adaptive quality system
if (averageFrameTime > targetFrameTime * 1.2f)
{
    ReduceQuality(); // Automatically reduce quality
}

// 2. Mobile-optimized settings
QualitySettings.antiAliasing = 0;
QualitySettings.softParticles = false;
QualitySettings.shadowDistance = 15f;
QualitySettings.masterTextureLimit = 1;
```

**Implementation**:
- Use `MobileRenderingOptimizer.cs` with `enableAdaptiveQuality = true`
- Set `targetFrameRate = 30`
- Enable `enableDynamicLOD` and `enableOcclusionCulling`

## 🔧 Advanced Configuration

### Quality Levels Setup
```csharp
// Low Quality (Emergency)
textureQuality = 2
shadowResolution = 0
shadowDistance = 5f
pixelLightCount = 1

// Medium Quality (Default)
textureQuality = 1
shadowResolution = 1
shadowDistance = 10f
pixelLightCount = 2

// High Quality (When performance allows)
textureQuality = 0
shadowResolution = 2
shadowDistance = 15f
pixelLightCount = 3
```

### Asset Optimization
```csharp
// Texture compression for mobile
if (Application.platform == RuntimePlatform.Android)
{
    texture.Compress(true); // ETC2 compression
}
else if (Application.platform == RuntimePlatform.IPhonePlayer)
{
    texture.Compress(true); // ASTC compression
}

// Mesh optimization
mesh.Optimize();
if (mesh.vertexCount > 1000)
{
    // Consider mesh simplification
}
```

## 📊 Performance Monitoring

### Real-time Monitoring
```csharp
// Monitor frame rate every 2 seconds
private IEnumerator PerformanceMonitoring()
{
    while (true)
    {
        float currentFPS = 1f / averageFrameTime;
        Debug.Log($"Performance: {currentFPS:F1} FPS, Quality: {currentQualityLevel}");
        
        if (averageFrameTime > targetFrameTime * 1.5f)
        {
            EmergencyOptimizations();
        }
        
        yield return new WaitForSeconds(2f);
    }
}
```

### Emergency Optimizations
```csharp
private void EmergencyOptimizations()
{
    // Disable expensive features immediately
    QualitySettings.shadowDistance = 5f;
    QualitySettings.pixelLightCount = 1;
    QualitySettings.softParticles = false;
    
    // Disable distant objects
    DisableDistantObjects();
    
    // Reduce particle effects
    ReduceParticleEffects();
}
```

## 🎮 Best Practices

### 1. Object Pooling
- **Always** use object pooling for frequently spawned objects
- Pre-allocate pools during startup
- Monitor pool usage and pre-warm when needed

### 2. Asset Loading
- Load assets asynchronously
- Use progressive loading for UI
- Unload unused assets regularly

### 3. Memory Management
- Monitor memory usage continuously
- Trigger garbage collection when memory is high
- Use `Resources.UnloadUnusedAssets()` strategically

### 4. Rendering
- Use mobile-optimized shaders
- Implement LOD systems for complex objects
- Enable occlusion culling for static objects
- Use baked lighting instead of real-time

### 5. Physics
- Use trigger colliders instead of rigidbodies when possible
- Optimize collision detection layers
- Set appropriate physics timestep (0.02f for 50 FPS)

## 🔍 Testing and Validation

### Performance Testing Checklist
- [ ] Test on low-end devices (2GB RAM, older GPUs)
- [ ] Monitor startup times (should be <3 seconds)
- [ ] Verify 30 FPS target is maintained
- [ ] Check memory usage doesn't exceed 512MB
- [ ] Test scene transitions are smooth
- [ ] Verify adaptive quality works correctly

### Debug Information
```csharp
// Add to your game for debugging
public void LogPerformanceInfo()
{
    Debug.Log($"FPS: {1f/averageFrameTime:F1}");
    Debug.Log($"Memory: {System.GC.GetTotalMemory(false) / (1024*1024)} MB");
    Debug.Log($"Quality Level: {currentQualityLevel}");
    Debug.Log($"Startup Time: {Time.realtimeSinceStartup - startupStartTime:F2}s");
}
```

## 📈 Expected Results

After implementing these optimizations, you should see:

- **Cold Start**: 5.03% → 1.5% (70% improvement)
- **Warm Start**: 28.84% → 12% (58% improvement)  
- **Hot Start**: 11.21% → 4% (64% improvement)
- **Rendering**: 17.53% → 8% (54% improvement)

## 🚨 Troubleshooting

### Common Issues

1. **Still getting slow starts**
   - Check if you're loading too many assets synchronously
   - Verify object pooling is implemented correctly
   - Ensure garbage collection is disabled during startup

2. **Frame rate still dropping**
   - Reduce texture quality further
   - Disable more expensive features
   - Check for memory leaks

3. **Memory usage too high**
   - Implement more aggressive asset unloading
   - Reduce texture sizes
   - Use object pooling more extensively

### Performance Profiling
Use Unity Profiler to identify bottlenecks:
- CPU usage spikes
- Memory allocation patterns
- Rendering bottlenecks
- Physics performance

## 📚 Additional Resources

- [Unity Mobile Performance Best Practices](https://docs.unity3d.com/Manual/MobileOptimization.html)
- [Unity Profiler Documentation](https://docs.unity3d.com/Manual/Profiler.html)
- [Mobile GPU Performance Guide](https://docs.unity3d.com/Manual/MobileGPUGraphics.html)

---

**Note**: These optimizations are designed to work together. Implement all components for maximum performance improvement. Monitor your specific metrics and adjust settings based on your game's requirements.