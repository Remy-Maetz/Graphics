#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
#endif
using System;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// Deprecated, kept for backward compatibility with existing ForwardRendererData asset files.
    /// Use StandardRendererData instead.
    /// </summary>
    [System.Obsolete("ForwardRendererData has been deprecated. Use StandardRendererData instead (UnityUpgradable) -> StandardRendererData", true)]
    [Serializable, ReloadGroup, ExcludeFromPreset]
    [MovedFrom("UnityEngine.Rendering.LWRP")]
    public class ForwardRendererData : ScriptableRendererData
    {
        private static readonly string k_ErrorMessage = "ForwardRendererData has been deprecated. Use StandardRendererData instead";

        [Serializable, ReloadGroup]
        public sealed class ShaderResources
        {
            [Reload("Shaders/Utils/Blit.shader")]
            public Shader blitPS;

            [Reload("Shaders/Utils/CopyDepth.shader")]
            public Shader copyDepthPS;

            [Reload("Shaders/Utils/ScreenSpaceShadows.shader")]
            public Shader screenSpaceShadowPS;

            [Reload("Shaders/Utils/Sampling.shader")]
            public Shader samplingPS;

            [Reload("Shaders/Utils/TileDepthInfo.shader")]
            public Shader tileDepthInfoPS;

            [Reload("Shaders/Utils/TileDeferred.shader")]
            public Shader tileDeferredPS;

            [Reload("Shaders/Utils/StencilDeferred.shader")]
            public Shader stencilDeferredPS;

            [Reload("Shaders/Utils/FallbackError.shader")]
            public Shader fallbackErrorPS;
        }

        [Reload("Runtime/Data/PostProcessData.asset")]
        public PostProcessData postProcessData = null;

#if ENABLE_VR && ENABLE_XR_MODULE
        [Reload("Runtime/Data/XRSystemData.asset")]
        public XRSystemData xrSystemData = null;
#endif

        public ShaderResources shaders = null;

        protected override ScriptableRenderer Create()
        {
            throw new NotSupportedException(k_ErrorMessage);
        }

        public LayerMask opaqueLayerMask
        {
        	get { throw new NotSupportedException(k_ErrorMessage); }
        	set { throw new NotSupportedException(k_ErrorMessage); }
        }

        public LayerMask transparentLayerMask
        {
        	get { throw new NotSupportedException(k_ErrorMessage); }
        	set { throw new NotSupportedException(k_ErrorMessage); }
        }

        public StencilStateData defaultStencilState
        {
        	get { throw new NotSupportedException(k_ErrorMessage); }
        	set { throw new NotSupportedException(k_ErrorMessage); }
        }

        public bool shadowTransparentReceive
        {
        	get { throw new NotSupportedException(k_ErrorMessage); }
        	set { throw new NotSupportedException(k_ErrorMessage); }
        }

        public RenderingMode renderingMode
        {
        	get { throw new NotSupportedException(k_ErrorMessage); }
        	set { throw new NotSupportedException(k_ErrorMessage); }
        }

		public bool accurateGbufferNormals
		{
        	get { throw new NotSupportedException(k_ErrorMessage); }
        	set { throw new NotSupportedException(k_ErrorMessage); }
		}
    }
}
