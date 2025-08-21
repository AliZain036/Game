using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Specialized rendering optimization for Unity mobile games
/// Target: Reduce 17.53% slow session rate and maintain 30 FPS
/// </summary>
public class MobileRenderingOptimizer : MonoBehaviour
{
    [Header("Rendering Settings")]
    [SerializeField] private int targetFrameRate = 30;
    [SerializeField] private bool enableAdaptiveQuality = true;
    [SerializeField] private bool enableDynamicLOD = true;
    [SerializeField] private bool enableOcclusionCulling = true;
    
    [Header("Quality Levels")]
    [SerializeField] private QualityLevel[] qualityLevels = new QualityLevel[3];
    [SerializeField] private int currentQualityLevel = 1;
    
    [Header("Performance Monitoring")]
    [SerializeField] private bool enablePerformanceMonitoring = true;
    [SerializeField] private float performanceCheckInterval = 2f;
    [SerializeField] private int frameTimeHistorySize = 60;
    
    // Performance tracking
    private float[] frameTimeHistory;
    private int frameTimeIndex = 0;
    private float averageFrameTime = 0f;
    private float targetFrameTime;
    
    // Adaptive quality system
    private float qualityChangeCooldown = 0f;
    private const float QUALITY_CHANGE_DELAY = 5f;
    
    // LOD management
    private Dictionary<LODGroup, float> lodDistances = new Dictionary<LODGroup, float>();
    
    void Awake()
    {
        InitializeRenderingOptimizer();
    }
    
    void Start()
    {
        SetupQualityLevels();
        SetupLODSystem();
        SetupOcclusionCulling();
        
        if (enablePerformanceMonitoring)
        {
            StartCoroutine(PerformanceMonitoring());
        }
    }
    
    void Update()
    {
        UpdateFrameTimeTracking();
        
        if (enableAdaptiveQuality)
        {
            UpdateAdaptiveQuality();
        }
        
        if (enableDynamicLOD)
        {
            UpdateDynamicLOD();
        }
    }
    
    #region INITIALIZATION
    
    private void InitializeRenderingOptimizer()
    {
        // Set target frame rate
        Application.targetFrameRate = targetFrameRate;
        targetFrameTime = 1f / targetFrameRate;
        
        // Initialize frame time history
        frameTimeHistory = new float[frameTimeHistorySize];
        
        // Disable vsync for better control
        QualitySettings.vSyncCount = 0;
        
        // Set mobile-optimized rendering settings
        SetMobileRenderingSettings();
    }
    
    private void SetMobileRenderingSettings()
    {
        // Disable expensive features on mobile
        QualitySettings.antiAliasing = 0;
        QualitySettings.softParticles = false;
        QualitySettings.realtimeReflectionProbes = false;
        QualitySettings.billboardsFaceCameraPosition = false;
        
        // Optimize shadows
        QualitySettings.shadowResolution = ShadowResolution.Low;
        QualitySettings.shadowDistance = 15f;
        QualitySettings.shadowCascades = 0;
        
        // Optimize textures
        QualitySettings.masterTextureLimit = 1;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        
        // Optimize lighting
        QualitySettings.pixelLightCount = 1;
        QualitySettings.maxQueuedFrames = 2;
    }
    
    #endregion
    
    #region QUALITY LEVEL SYSTEM
    
    [System.Serializable]
    public class QualityLevel
    {
        public string name;
        public int textureQuality;
        public int shadowResolution;
        public float shadowDistance;
        public int pixelLightCount;
        public bool softParticles;
        public bool realtimeReflectionProbes;
        public int antiAliasing;
    }
    
    private void SetupQualityLevels()
    {
        // Define quality levels for mobile
        qualityLevels[0] = new QualityLevel
        {
            name = "Low",
            textureQuality = 2,
            shadowResolution = 0,
            shadowDistance = 5f,
            pixelLightCount = 1,
            softParticles = false,
            realtimeReflectionProbes = false,
            antiAliasing = 0
        };
        
        qualityLevels[1] = new QualityLevel
        {
            name = "Medium",
            textureQuality = 1,
            shadowResolution = 1,
            shadowDistance = 10f,
            pixelLightCount = 2,
            softParticles = false,
            realtimeReflectionProbes = false,
            antiAliasing = 0
        };
        
        qualityLevels[2] = new QualityLevel
        {
            name = "High",
            textureQuality = 0,
            shadowResolution = 2,
            shadowDistance = 15f,
            pixelLightCount = 3,
            softParticles = true,
            realtimeReflectionProbes = false,
            antiAliasing = 0
        };
        
        // Apply initial quality level
        ApplyQualityLevel(currentQualityLevel);
    }
    
    private void ApplyQualityLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= qualityLevels.Length)
            return;
            
        QualityLevel level = qualityLevels[levelIndex];
        
        QualitySettings.masterTextureLimit = level.textureQuality;
        QualitySettings.shadowResolution = (ShadowResolution)level.shadowResolution;
        QualitySettings.shadowDistance = level.shadowDistance;
        QualitySettings.pixelLightCount = level.pixelLightCount;
        QualitySettings.softParticles = level.softParticles;
        QualitySettings.realtimeReflectionProbes = level.realtimeReflectionProbes;
        QualitySettings.antiAliasing = level.antiAliasing;
        
        Debug.Log($"Applied quality level: {level.name}");
    }
    
    #endregion
    
    #region ADAPTIVE QUALITY SYSTEM
    
    private void UpdateAdaptiveQuality()
    {
        if (Time.time < qualityChangeCooldown)
            return;
            
        // Check if we need to reduce quality
        if (averageFrameTime > targetFrameTime * 1.2f) // 20% over target
        {
            ReduceQuality();
            qualityChangeCooldown = Time.time + QUALITY_CHANGE_DELAY;
        }
        // Check if we can increase quality
        else if (averageFrameTime < targetFrameTime * 0.8f) // 20% under target
        {
            IncreaseQuality();
            qualityChangeCooldown = Time.time + QUALITY_CHANGE_DELAY;
        }
    }
    
    private void ReduceQuality()
    {
        if (currentQualityLevel > 0)
        {
            currentQualityLevel--;
            ApplyQualityLevel(currentQualityLevel);
            Debug.Log($"Reduced quality to level {currentQualityLevel}");
        }
    }
    
    private void IncreaseQuality()
    {
        if (currentQualityLevel < qualityLevels.Length - 1)
        {
            currentQualityLevel++;
            ApplyQualityLevel(currentQualityLevel);
            Debug.Log($"Increased quality to level {currentQualityLevel}");
        }
    }
    
    #endregion
    
    #region LOD SYSTEM
    
    private void SetupLODSystem()
    {
        if (!enableDynamicLOD)
            return;
            
        // Find all LOD groups and store their original distances
        LODGroup[] lodGroups = FindObjectsOfType<LODGroup>();
        foreach (LODGroup lodGroup in lodGroups)
        {
            LOD[] lods = lodGroup.GetLODs();
            if (lods.Length > 0)
            {
                lodDistances[lodGroup] = lods[0].screenRelativeTransitionHeight;
            }
        }
    }
    
    private void UpdateDynamicLOD()
    {
        if (!enableDynamicLOD)
            return;
            
        // Adjust LOD distances based on performance
        float performanceMultiplier = Mathf.Clamp(averageFrameTime / targetFrameTime, 0.5f, 2f);
        
        foreach (var kvp in lodDistances)
        {
            LODGroup lodGroup = kvp.Key;
            float originalDistance = kvp.Value;
            
            LOD[] lods = lodGroup.GetLODs();
            if (lods.Length > 0)
            {
                // Adjust LOD distance based on performance
                lods[0].screenRelativeTransitionHeight = originalDistance * performanceMultiplier;
                lodGroup.SetLODs(lods);
            }
        }
    }
    
    #endregion
    
    #region OCCLUSION CULLING
    
    private void SetupOcclusionCulling()
    {
        if (!enableOcclusionCulling)
            return;
            
        // Enable occlusion culling on main camera
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.useOcclusionCulling = true;
        }
        
        // Mark static objects for occlusion culling
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer.gameObject.isStatic)
            {
                renderer.gameObject.isStatic = true;
            }
        }
    }
    
    #endregion
    
    #region PERFORMANCE MONITORING
    
    private void UpdateFrameTimeTracking()
    {
        // Track frame time
        frameTimeHistory[frameTimeIndex] = Time.deltaTime;
        frameTimeIndex = (frameTimeIndex + 1) % frameTimeHistory.Length;
        
        // Calculate average frame time
        float sum = 0f;
        for (int i = 0; i < frameTimeHistory.Length; i++)
        {
            sum += frameTimeHistory[i];
        }
        averageFrameTime = sum / frameTimeHistory.Length;
    }
    
    private IEnumerator PerformanceMonitoring()
    {
        while (true)
        {
            // Log performance metrics
            float currentFPS = 1f / averageFrameTime;
            Debug.Log($"Performance: {currentFPS:F1} FPS, Quality: {qualityLevels[currentQualityLevel].name}");
            
            // Check for severe performance issues
            if (averageFrameTime > targetFrameTime * 1.5f)
            {
                Debug.LogWarning("Severe performance issue detected!");
                EmergencyOptimizations();
            }
            
            yield return new WaitForSeconds(performanceCheckInterval);
        }
    }
    
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
    
    private void DisableDistantObjects()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;
        
        float maxDistance = 20f;
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            float distance = Vector3.Distance(mainCamera.transform.position, renderer.transform.position);
            if (distance > maxDistance)
            {
                renderer.enabled = false;
            }
        }
    }
    
    private void ReduceParticleEffects()
    {
        ParticleSystem[] particleSystems = FindObjectsOfType<ParticleSystem>();
        foreach (ParticleSystem ps in particleSystems)
        {
            var emission = ps.emission;
            emission.rateOverTime = emission.rateOverTime.constant * 0.5f;
        }
    }
    
    #endregion
    
    #region CAMERA OPTIMIZATION
    
    public void OptimizeCamera(Camera camera)
    {
        // Optimize camera settings for mobile
        camera.allowHDR = false;
        camera.allowMSAA = false;
        camera.renderingPath = RenderingPath.Forward;
        camera.clearFlags = CameraClearFlags.Skybox;
        
        // Use lower resolution on mobile if needed
        if (Application.platform == RuntimePlatform.Android || 
            Application.platform == RuntimePlatform.IPhonePlayer)
        {
            // Consider using lower resolution rendering
            // camera.pixelRect = new Rect(0, 0, Screen.width / 2, Screen.height / 2);
        }
    }
    
    #endregion
    
    #region MATERIAL OPTIMIZATION
    
    public void OptimizeMaterials()
    {
        // Replace expensive shaders with mobile-optimized versions
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            foreach (Material material in materials)
            {
                OptimizeMaterial(material);
            }
        }
    }
    
    private void OptimizeMaterial(Material material)
    {
        // Replace expensive shaders with mobile versions
        if (material.shader.name.Contains("Standard"))
        {
            material.shader = Shader.Find("Mobile/Diffuse");
        }
        else if (material.shader.name.Contains("Particles"))
        {
            material.shader = Shader.Find("Mobile/Particles/Alpha Blended");
        }
        
        // Disable expensive material features
        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", 0f);
        }
        if (material.HasProperty("_Glossiness"))
        {
            material.SetFloat("_Glossiness", 0.5f);
        }
    }
    
    #endregion
    
    #region LIGHTING OPTIMIZATION
    
    public void OptimizeLighting()
    {
        // Optimize all lights in the scene
        Light[] lights = FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            OptimizeLight(light);
        }
    }
    
    private void OptimizeLight(Light light)
    {
        // Use baked lighting instead of real-time when possible
        if (light.type == LightType.Directional)
        {
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.8f;
        }
        else
        {
            light.shadows = LightShadows.None;
        }
        
        // Reduce light intensity for better performance
        light.intensity = Mathf.Clamp(light.intensity, 0f, 2f);
        
        // Use lower resolution shadows
        light.shadowResolution = LightShadowResolution.Low;
    }
    
    #endregion
    
    #region PUBLIC API
    
    public float GetCurrentFPS()
    {
        return 1f / averageFrameTime;
    }
    
    public float GetAverageFrameTime()
    {
        return averageFrameTime;
    }
    
    public string GetCurrentQualityLevel()
    {
        return qualityLevels[currentQualityLevel].name;
    }
    
    public void SetQualityLevel(int level)
    {
        if (level >= 0 && level < qualityLevels.Length)
        {
            currentQualityLevel = level;
            ApplyQualityLevel(currentQualityLevel);
        }
    }
    
    public void ForceQualityReduction()
    {
        ReduceQuality();
    }
    
    #endregion
}

/// <summary>
/// Utility class for mobile-specific rendering optimizations
/// </summary>
public static class MobileRenderingUtils
{
    public static void OptimizeForMobile()
    {
        // Set mobile-specific quality settings
        QualitySettings.masterTextureLimit = 1;
        QualitySettings.antiAliasing = 0;
        QualitySettings.softParticles = false;
        QualitySettings.realtimeReflectionProbes = false;
        QualitySettings.shadowDistance = 10f;
        QualitySettings.shadowResolution = ShadowResolution.Low;
        QualitySettings.pixelLightCount = 2;
        
        // Optimize for mobile GPUs
        QualitySettings.shadowCascades = 0;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        QualitySettings.maxQueuedFrames = 2;
    }
    
    public static void OptimizeTextureForMobile(Texture2D texture)
    {
        // Apply mobile-optimized texture settings
        if (Application.platform == RuntimePlatform.Android)
        {
            // Use ETC2 compression for Android
            texture.Compress(true);
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            // Use ASTC compression for iOS
            texture.Compress(true);
        }
    }
    
    public static void OptimizeMeshForMobile(Mesh mesh)
    {
        // Optimize mesh for mobile rendering
        mesh.Optimize();
        
        // Reduce vertex count if too high
        if (mesh.vertexCount > 1000)
        {
            // Consider using mesh simplification
            Debug.LogWarning($"Mesh {mesh.name} has high vertex count: {mesh.vertexCount}");
        }
    }
}