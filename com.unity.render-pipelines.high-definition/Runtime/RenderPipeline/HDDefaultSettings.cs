using UnityEngine.Rendering;using UnityEditor;using UnityEditor.Rendering;using UnityEditor.Rendering.HighDefinition;using System.Collections.Generic; //needed for list of Custom Post Processes injections
#if UNITY_EDITOR
using UnityEditorInternal;
#endif
namespace UnityEngine.Rendering.HighDefinition{
    /// <summary>    /// High Definition Render Pipeline Default Settings.    /// Default settings are unique per Render Pipeline type. In HD, Default Settings contain:    /// - a default Volume (Global) combined with its Default Profile (defines which components are active by default)    /// - this Volume's profile    /// - the LookDev Volume Profile    /// - Frame Settings    /// - Various resources for runtime, editor-only, and raytracing    /// </summary>    public partial class HDDefaultSettings:RenderPipelineDefaultSettings    {        private static HDDefaultSettings cachedInstance = null;        public static HDDefaultSettings instance
        {
            get
            {
                if(cachedInstance == null)
                    cachedInstance = GraphicsSettings.GetSettingsForRenderPipeline<HDRenderPipeline>() as HDDefaultSettings;
                return cachedInstance;
            }
        }

        static public void UpdateGraphicsSettings(HDDefaultSettings newSettings)
        {
            if(newSettings == null || newSettings == cachedInstance)
                return;
            GraphicsSettings.RegisterRenderPipelineSettings<HDRenderPipeline>(newSettings as RenderPipelineDefaultSettings);
            cachedInstance = newSettings;
        }

        #if UNITY_EDITOR
        //Making sure there is at least one HDDefaultSettings instance in the project
        static public void Ensure()
        {
            if(HDDefaultSettings.instance)
                return;

            HDDefaultSettings assetCreated = null;
            string path = "Assets/HDRPDefaultResources/DefaultSettings.asset";
            assetCreated = AssetDatabase.LoadAssetAtPath<HDDefaultSettings>(path);
            if(assetCreated == null)
            {
                //TODOJENNY do something less expensive?
                var guidHDDefaultAssets = AssetDatabase.FindAssets("t:HDDefaultSettings");
                if(guidHDDefaultAssets.Length > 0)
                {
                    var curGUID = guidHDDefaultAssets[0];
                    path = AssetDatabase.GUIDToAssetPath(curGUID);
                    assetCreated = AssetDatabase.LoadAssetAtPath<HDDefaultSettings>(path);
                }
                else
                {
                    if(!AssetDatabase.IsValidFolder("Assets/HDRPDefaultResources/"))
                        AssetDatabase.CreateFolder("Assets","HDRPDefaultResources");
                    assetCreated = ScriptableObject.CreateInstance<HDDefaultSettings>();
                    AssetDatabase.CreateAsset(assetCreated,path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            Debug.Assert(assetCreated,"Could not create HD Default Settings - HDRP may not work correctly - Open the Graphics Window for additional help.");
            UpdateGraphicsSettings(assetCreated);
        }
        #endif

        #region Volume
        [SerializeField]        private Volume s_DefaultVolume = null;        internal Volume GetOrCreateDefaultVolume()        {            GetOrCreateDefaultVolumeProfile(); //TODOJENNY: investigate why I happen to have a null defaultProfile in some cases (UpdateCurrentStaticLightingSky)            if (s_DefaultVolume == null || s_DefaultVolume.Equals(null))            {                var go = new GameObject("Default Volume") { hideFlags = HideFlags.HideAndDontSave }; //TODO: does this leak?                s_DefaultVolume = go.AddComponent<Volume>();                s_DefaultVolume.isGlobal = true;                s_DefaultVolume.priority = float.MinValue;                s_DefaultVolume.sharedProfile = defaultVolumeProfile;
#if UNITY_EDITOR            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += () =>                             {                                 DestroyDefaultVolume();                             };
#endif            }            if (                // In case the asset was deleted or the reference removed                s_DefaultVolume.sharedProfile == null || s_DefaultVolume.sharedProfile.Equals(null)#if UNITY_EDITOR                // In case the serialization recreated an empty volume sharedProfile                || !UnityEditor.AssetDatabase.Contains(s_DefaultVolume.sharedProfile)#endif            )            {                s_DefaultVolume.sharedProfile = defaultVolumeProfile;            }            if (s_DefaultVolume.sharedProfile != defaultVolumeProfile)            {                s_DefaultVolume.sharedProfile = defaultVolumeProfile;            }            if(s_DefaultVolume == null)            {                Debug.LogError("[HDRP] Cannot Create Default Volume.");            }            return s_DefaultVolume;        }        private void DestroyDefaultVolume()        {            if (s_DefaultVolume != null && !s_DefaultVolume.Equals(null))            {                CoreUtils.Destroy(s_DefaultVolume.gameObject);                s_DefaultVolume = null;            }        }                #endregion        #region VolumeProfile        [SerializeField] private VolumeProfile m_DefaultVolumeProfile;        internal VolumeProfile defaultVolumeProfile        {            get => m_DefaultVolumeProfile;            set => m_DefaultVolumeProfile = value;        }        internal VolumeProfile GetOrCreateDefaultVolumeProfile()        {            if (defaultVolumeProfile == null || defaultVolumeProfile.Equals(null))            {                defaultVolumeProfile = renderPipelineEditorResources.defaultSettingsVolumeProfile; //TODOJENNY should it be moved to this class?            }            return defaultVolumeProfile;        }
        #endregion

        #region Look Dev Profile
#if UNITY_EDITOR        [SerializeField] private VolumeProfile m_DefaultLookDevProfile;

       internal VolumeProfile defaultLookDevProfile {            get => m_DefaultLookDevProfile;            set => m_DefaultLookDevProfile = value;        }        internal VolumeProfile GetOrAssignLookDevVolumeProfile() 
       {
           if(HDDefaultSettings.instance.defaultLookDevProfile == null || HDDefaultSettings.instance.defaultLookDevProfile.Equals(null)) 
           { 
               HDDefaultSettings.instance.defaultLookDevProfile =
                  HDDefaultSettings.instance.renderPipelineEditorResources.lookDev.defaultLookDevVolumeProfile;
           }
           return HDDefaultSettings.instance.defaultLookDevProfile;        }
#endif
       #endregion
              #region Camera's FrameSettings        // To be able to turn on/off FrameSettings properties at runtime for debugging purpose without affecting the original one
        // we create a runtime copy (m_ActiveFrameSettings that is used, and any parametrization is done on serialized frameSettings)
        [SerializeField]        FrameSettings m_RenderingPathDefaultCameraFrameSettings = FrameSettings.NewDefaultCamera();        [SerializeField]        FrameSettings m_RenderingPathDefaultBakedOrCustomReflectionFrameSettings = FrameSettings.NewDefaultCustomOrBakeReflectionProbe();        [SerializeField]        FrameSettings m_RenderingPathDefaultRealtimeReflectionFrameSettings = FrameSettings.NewDefaultRealtimeReflectionProbe();        internal ref FrameSettings GetDefaultFrameSettings(FrameSettingsRenderType type)        {            switch (type)            {                case FrameSettingsRenderType.Camera:                    return ref m_RenderingPathDefaultCameraFrameSettings;                case FrameSettingsRenderType.CustomOrBakedReflection:                    return ref m_RenderingPathDefaultBakedOrCustomReflectionFrameSettings;                case FrameSettingsRenderType.RealtimeReflection:                    return ref m_RenderingPathDefaultRealtimeReflectionFrameSettings;                default:                    throw new System.ArgumentException("Unknown FrameSettingsRenderType");            }        }        #endregion        #region Runtime Resources        [SerializeField]        RenderPipelineResources m_RenderPipelineResources;        internal RenderPipelineResources renderPipelineResources        {            get { EnsureResources(forceReload:false);  return m_RenderPipelineResources; }            set { m_RenderPipelineResources = value; }        }        internal void EnsureResources(bool forceReload)        {            if (AreResourcesCreated())                return;            m_RenderPipelineResources = AssetDatabase.LoadAssetAtPath<RenderPipelineResources>(HDUtils.GetHDRenderPipelinePath() + "Runtime/RenderPipelineResources/HDRenderPipelineResources.asset");            if (forceReload)                ResourceReloader.ReloadAllNullIn(m_RenderPipelineResources, HDUtils.GetHDRenderPipelinePath());        }        internal bool AreResourcesCreated()        {            return (m_RenderPipelineResources != null && !m_RenderPipelineResources.Equals(null));        }

#if UNITY_EDITOR        internal void EnsureShadersCompiled()        {            // We iterate over all compute shader to verify if they are all compiled, if it's not the case            // then we throw an exception to avoid allocating resources and crashing later on by using a null            // compute kernel.            foreach (var computeShader in m_RenderPipelineResources.shaders.GetAllComputeShaders())            {                foreach (var message in UnityEditor.ShaderUtil.GetComputeShaderMessages(computeShader))                {                    if (message.severity == UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error)                    {                        // Will be catched by the try in HDRenderPipelineAsset.CreatePipeline()                        throw new System.Exception(System.String.Format("Compute Shader compilation error on platform {0} in file {1}:{2}: {3}{4}\n" +"HDRP will not run until the error is fixed.\n",message.platform, message.file, message.line, message.message, message.messageDetails));                    }                }            }        }
#endif //UNITY_EDITOR
        #endregion // Runtime Resources

        #region Editor Resources
#if UNITY_EDITOR        [SerializeField]        HDRenderPipelineEditorResources m_RenderPipelineEditorResources;        internal HDRenderPipelineEditorResources renderPipelineEditorResources        {            get            {                //there is no clean way to load editor resources without having it serialized                // - impossible to load them at deserialization                // - constructor only called at asset creation                // - cannot rely on OnEnable                //thus fallback with lazy init for them                EnsureEditorResources(forceReload:false);                return m_RenderPipelineEditorResources;            }            set { m_RenderPipelineEditorResources = value; }        }        internal void EnsureEditorResources(bool forceReload)        {            if (AreEditorResourcesCreated())                return;            var editorResourcesPath = HDUtils.GetHDRenderPipelinePath() + "Editor/RenderPipelineResources/HDRenderPipelineEditorResources.asset";            if(forceReload)            {
                var objs = InternalEditorUtility.LoadSerializedFileAndForget(editorResourcesPath);
                m_RenderPipelineEditorResources = (objs != null && objs.Length > 0) ? objs[0] as HDRenderPipelineEditorResources : null;                if (ResourceReloader.ReloadAllNullIn(m_RenderPipelineEditorResources,HDUtils.GetHDRenderPipelinePath()))
                {
                    InternalEditorUtility.SaveToSerializedFileAndForget(
                        new Object[]{ m_RenderPipelineEditorResources },
                        editorResourcesPath,
                        true);
                }            }            else if (!EditorUtility.IsPersistent(m_RenderPipelineEditorResources))
            {
                m_RenderPipelineEditorResources = AssetDatabase.LoadAssetAtPath<HDRenderPipelineEditorResources>(editorResourcesPath);
            }        }        internal bool AreEditorResourcesCreated()        {            return (m_RenderPipelineEditorResources != null && !m_RenderPipelineEditorResources.Equals(null));        }
#endif

        #endregion //Editor Resources

        #region Ray Tracing Resources
#if UNITY_EDITOR        [SerializeField]        HDRenderPipelineRayTracingResources m_RenderPipelineRayTracingResources;        internal HDRenderPipelineRayTracingResources renderPipelineRayTracingResources        {            get { return m_RenderPipelineRayTracingResources; }            set { m_RenderPipelineRayTracingResources = value; }        }        internal void EnsureRayTracingResources(bool forceReload)        {            if (AreRayTracingResourcesCreated())                return;            m_RenderPipelineRayTracingResources = UnityEditor.AssetDatabase.LoadAssetAtPath<HDRenderPipelineRayTracingResources>(HDUtils.GetHDRenderPipelinePath() + "Runtime/RenderPipelineResources/HDRenderPipelineRayTracingResources.asset");            if (forceReload)                ResourceReloader.ReloadAllNullIn(m_RenderPipelineEditorResources, HDUtils.GetHDRenderPipelinePath());        }        internal bool AreRayTracingResourcesCreated()        {            return (m_RenderPipelineRayTracingResources != null && !m_RenderPipelineRayTracingResources.Equals(null));        }#endif
        #endregion //Ray Tracing Resources
        #region Custom Post Processes Injections
        // List of custom post process Types that will be executed in the project, in the order of the list (top to back)
        [SerializeField]        internal List<string> beforeTransparentCustomPostProcesses = new List<string>();        [SerializeField]        internal List<string> beforePostProcessCustomPostProcesses = new List<string>();        [SerializeField]        internal List<string> afterPostProcessCustomPostProcesses = new List<string>();        [SerializeField]        internal List<string> beforeTAACustomPostProcesses = new List<string>();

        #endregion    }}