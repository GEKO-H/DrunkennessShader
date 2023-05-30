using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class DrunkennessPostProcessPass : ScriptableRenderPass
{
    RenderTargetIdentifier source;
    RenderTargetIdentifier destinationA;
    RenderTargetIdentifier destinationB;
    RenderTargetIdentifier latestDest;

    readonly int temporaryRTIdA = Shader.PropertyToID("_TempRT");
    readonly int temporaryRTIdB = Shader.PropertyToID("_TempRTB");

    public Material _Material;

    public DrunkennessPostProcessPass(Shader shader)
    {
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        _Material = CoreUtils.CreateEngineMaterial(shader);
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0;

        var renderer = renderingData.cameraData.renderer;
        source = renderer.cameraColorTarget;

        cmd.GetTemporaryRT(temporaryRTIdA, descriptor, FilterMode.Bilinear);
        destinationA = new RenderTargetIdentifier(temporaryRTIdA);
        cmd.GetTemporaryRT(temporaryRTIdB, descriptor, FilterMode.Bilinear);
        destinationB = new RenderTargetIdentifier(temporaryRTIdB);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.isSceneViewCamera)
            return;

        if (_Material == null)
        {
            Debug.LogError("マテリアルがありません。");
            return;
        }

        CommandBuffer cmd = CommandBufferPool.Get("Custom Post Processing");
        cmd.Clear();

        var stack = VolumeManager.instance.stack;

        #region Local Methods

        void BlitTo(Material mat, int pass = 0)
        {
            var first = latestDest;
            var last = first == destinationA ? destinationB : destinationA;
            Blit(cmd, first, last, mat, pass);

            latestDest = last;
        }

        #endregion

        latestDest = source;

        var customEffect = stack.GetComponent<DrunkennessComponent>();
        if (customEffect.IsActive())
        {
            float cos = Mathf.Cos(Time.time * customEffect._TimeScale.value) * customEffect._HorizontalIntensity.value;
            float sin = Mathf.Sin(Time.time * customEffect._TimeScale.value) * customEffect._VerticalIntensity.value;
            Vector4 offset = new Vector4(cos, sin, -cos -sin);

            _Material.SetVector(Shader.PropertyToID("_Offset"), offset);

            BlitTo(_Material);
        }

        Blit(cmd, latestDest, source);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    //Cleans the temporary RTs when we don't need them anymore
    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(temporaryRTIdA);
        cmd.ReleaseTemporaryRT(temporaryRTIdB);
    }
}