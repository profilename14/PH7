using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_6000_0_OR_NEWER
using System;
using UnityEngine.Rendering.RenderGraphModule;
#endif

public class ScreenRenderPass : ScriptableRenderPass {
    private Material _passMaterial;
    private bool _requiresColor;
    private bool _isBeforeTransparents;
    private PassData _passData;
    private ProfilingSampler _profilingSampler;
    private RTHandle _copiedColor;

    private static readonly Vector4 BlitScaleBias = new(1f, 1f, 0f, 0f);
    private static readonly MaterialPropertyBlock SharedPropertyBlock = new();
    private const string TexName = "_BlitTexture";
    private static readonly int BlitTextureShaderID = Shader.PropertyToID(TexName);
    private static readonly int BlitScaleBiasID = Shader.PropertyToID("_BlitScaleBias");

    public void Setup(Material mat, bool requiresColor, bool isBeforeTransparents, string featureName,
        in RenderingData renderingData) {
        _passMaterial = mat;
        _requiresColor = requiresColor;
        _isBeforeTransparents = isBeforeTransparents;
        _profilingSampler ??= new ProfilingSampler(featureName);

        var colorCopyDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        colorCopyDescriptor.depthBufferBits = (int)DepthBits.None;

#if UNITY_6000_0_OR_NEWER
        requiresIntermediateTexture = _requiresColor && !_isBeforeTransparents;
        RenderingUtils.ReAllocateHandleIfNeeded(ref _copiedColor, colorCopyDescriptor, FilterMode.Point,
            TextureWrapMode.Clamp, name: "_FullscreenPassColorCopy");
#elif UNITY_2022_3_OR_NEWER
        RenderingUtils.ReAllocateIfNeeded(ref _copiedColor, colorCopyDescriptor, name: "_FullscreenPassColorCopy");
#endif

        _passData ??= new PassData();
    }

    public void Dispose() {
        _copiedColor?.Release();
    }

#if UNITY_6000_0_OR_NEWER
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
        var resourceData = frameData.Get<UniversalResourceData>();
        var cameraData = frameData.Get<UniversalCameraData>();

        if (_passMaterial == null) {
            return;
        }

        if (!resourceData.activeColorTexture.IsValid()) {
            return;
        }

        if (_requiresColor && resourceData.isActiveTargetBackBuffer) {
            Debug.LogWarning("[Flat Kit] ScreenRenderPass requires sampling the active color buffer, but the renderer " +
                             "is currently writing directly to the backbuffer. Enable an intermediate color target " +
                             "(Renderer > Rendering > Intermediate Texture Mode = Always) or move the feature to an " +
                             "earlier render event.");
            return;
        }

        TextureHandle copySource = TextureHandle.nullHandle;

        if (_requiresColor) {
            TextureDesc descriptor;
            if (resourceData.cameraColor.IsValid()) {
                descriptor = renderGraph.GetTextureDesc(resourceData.cameraColor);
            } else {
                descriptor = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
            }

            descriptor.name = "_FlatKit_FullScreenCopy";
            descriptor.clearBuffer = false;

            TextureHandle copiedColor = renderGraph.CreateTexture(descriptor);

            using (var builder = renderGraph.AddRasterRenderPass<CopyPassData>("FlatKit Copy Color", out var passData,
                       _profilingSampler)) {
                passData.source = resourceData.activeColorTexture;
                builder.UseTexture(passData.source, AccessFlags.Read);
                builder.SetRenderAttachment(copiedColor, 0, AccessFlags.Write);
                builder.SetRenderFunc((CopyPassData data, RasterGraphContext ctx) => {
                    Blitter.BlitTexture(ctx.cmd, data.source, BlitScaleBias, 0, false);
                });
            }

            copySource = copiedColor;
        }

        using (var builder = renderGraph.AddRasterRenderPass<MainPassData>(_profilingSampler.name, out var passData,
                   _profilingSampler)) {
            passData.material = _passMaterial;
            passData.source = copySource;

            if (passData.source.IsValid()) {
                builder.UseTexture(passData.source, AccessFlags.Read);
            }

            var passInput = input;
            bool needsColor = (passInput & ScriptableRenderPassInput.Color) != ScriptableRenderPassInput.None;
            bool needsDepth = (passInput & ScriptableRenderPassInput.Depth) != ScriptableRenderPassInput.None;
            bool needsNormals = (passInput & ScriptableRenderPassInput.Normal) != ScriptableRenderPassInput.None;
            bool needsMotion = (passInput & ScriptableRenderPassInput.Motion) != ScriptableRenderPassInput.None;

            if (needsColor && cameraData.renderer.SupportsCameraOpaque() && resourceData.cameraOpaqueTexture.IsValid()) {
                builder.UseTexture(resourceData.cameraOpaqueTexture);
            }

            if (needsDepth && resourceData.cameraDepthTexture.IsValid()) {
                builder.UseTexture(resourceData.cameraDepthTexture);
            }

            if (needsNormals && cameraData.renderer.SupportsCameraNormals() &&
                resourceData.cameraNormalsTexture.IsValid()) {
                builder.UseTexture(resourceData.cameraNormalsTexture);
            }

            if (needsMotion && cameraData.renderer.SupportsMotionVectors()) {
                if (resourceData.motionVectorColor.IsValid()) {
                    builder.UseTexture(resourceData.motionVectorColor);
                }

                if (resourceData.motionVectorDepth.IsValid()) {
                    builder.UseTexture(resourceData.motionVectorDepth);
                }
            }

            builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);

            builder.SetRenderFunc((MainPassData data, RasterGraphContext ctx) => {
                SharedPropertyBlock.Clear();

                if (data.source.IsValid()) {
                    SharedPropertyBlock.SetTexture(BlitTextureShaderID, data.source);
                }

                SharedPropertyBlock.SetVector(BlitScaleBiasID, BlitScaleBias);

                ctx.cmd.DrawProcedural(Matrix4x4.identity, data.material, 0, MeshTopology.Triangles, 3, 1,
                    SharedPropertyBlock);
            });
        }
    }

    [Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled).", false)]
#endif
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
        _passData.effectMaterial = _passMaterial;
        _passData.requiresColor = _requiresColor;
        _passData.isBeforeTransparents = _isBeforeTransparents;
        _passData.profilingSampler = _profilingSampler;
        _passData.copiedColor = _copiedColor;

        ExecutePass(_passData, ref renderingData, ref context);
    }

#if UNITY_6000_0_OR_NEWER
    [Obsolete("This rendering path is for compatibility mode only (when Render Graph is disabled).", false)]
#endif
    private static void ExecutePass(PassData passData, ref RenderingData renderingData,
        ref ScriptableRenderContext context) {
        var passMaterial = passData.effectMaterial;
        var requiresColor = passData.requiresColor;
        var copiedColor = passData.copiedColor;
        var profilingSampler = passData.profilingSampler;

        if (passMaterial == null) {
            return;
        }

        if (renderingData.cameraData.isPreviewCamera) {
            return;
        }

        CommandBuffer cmd;
        bool releaseCommandBuffer;
        // Unity 6.3+ no longer exposes RenderingData.commandBuffer outside URP, so rely on pooled buffers there (and on pre-2022.3).

#if UNITY_6000_3_OR_NEWER
        cmd = CommandBufferPool.Get();
        releaseCommandBuffer = true;
#elif UNITY_2022_3_OR_NEWER && !UNITY_6000_3_OR_NEWER
        cmd = renderingData.commandBuffer;
        releaseCommandBuffer = false;
#else
        cmd = CommandBufferPool.Get();
        releaseCommandBuffer = true;
#endif
        var cameraData = renderingData.cameraData;

        using (new ProfilingScope(cmd, profilingSampler)) {
            if (requiresColor) {
#if UNITY_6000_3_OR_NEWER
                RTHandle source = cameraData.renderer.cameraColorTargetHandle;
                Blitter.BlitCameraTexture(cmd, source, copiedColor);
#elif UNITY_2022_3_OR_NEWER && !UNITY_6000_3_OR_NEWER
                RTHandle source = passData.isBeforeTransparents
                    ? cameraData.renderer.GetCameraColorBackBuffer(cmd)
                    : cameraData.renderer.cameraColorTargetHandle;
                Blitter.BlitCameraTexture(cmd, source, copiedColor);
#else
                RenderTargetIdentifier source = cameraData.renderer.cameraColorTarget;
                cmd.Blit(source, copiedColor);
#endif

                passMaterial.SetTexture(BlitTextureShaderID, copiedColor);
            }

#if UNITY_6000_3_OR_NEWER
            CoreUtils.SetRenderTarget(cmd, cameraData.renderer.cameraColorTargetHandle);
#elif UNITY_2022_3_OR_NEWER && !UNITY_6000_3_OR_NEWER
            CoreUtils.SetRenderTarget(cmd, cameraData.renderer.GetCameraColorBackBuffer(cmd));
#else
            CoreUtils.SetRenderTarget(cmd, cameraData.renderer.cameraColorTarget);
#endif
            CoreUtils.DrawFullScreen(cmd, passMaterial);
            context.ExecuteCommandBuffer(cmd);

            if (releaseCommandBuffer) {
                CommandBufferPool.Release(cmd);
            } else {
                cmd.Clear();
            }
        }
    }

    private class PassData {
        internal Material effectMaterial;
        internal bool requiresColor;
        internal bool isBeforeTransparents;
        public ProfilingSampler profilingSampler;
        public RTHandle copiedColor;
    }

#if UNITY_6000_0_OR_NEWER
    private class CopyPassData {
        internal TextureHandle source;
    }

    private class MainPassData {
        internal TextureHandle source;
        internal Material material;
    }
#endif
}