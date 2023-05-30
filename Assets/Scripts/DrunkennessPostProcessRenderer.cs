using UnityEngine;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class DrunkennessPostProcessRenderer : ScriptableRendererFeature
{
    DrunkennessPostProcessPass pass;

    public override void Create()
    {
        var shader = Shader.Find("Custom/Drunkenness");
        pass = new DrunkennessPostProcessPass(shader);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }
}
