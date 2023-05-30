using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Custom/DrunkennessComponent", typeof(UniversalRenderPipeline))]
public class DrunkennessComponent : VolumeComponent, IPostProcessComponent
{
    public ClampedFloatParameter _HorizontalIntensity = new ClampedFloatParameter(value: 0, min: 0, max: 0.5f, overrideState: true);
    public ClampedFloatParameter _VerticalIntensity = new ClampedFloatParameter(value: 0, min: 0, max: 0.5f, overrideState: true);
    public ClampedFloatParameter _TimeScale = new ClampedFloatParameter(value: 1, min: 0, max: 10, overrideState: true);

    public bool IsActive() => _HorizontalIntensity.value > 0 || _VerticalIntensity.value > 0;

    public bool IsTileCompatible() => true;
}