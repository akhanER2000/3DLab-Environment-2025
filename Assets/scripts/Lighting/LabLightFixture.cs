using UnityEngine;

[ExecuteInEditMode]
public class LabLightFixture : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private Light[] lightSources;
    [SerializeField] private float intensity = 2500f;
    [SerializeField] private Color lightColor = new Color(0.96f, 0.97f, 1f);
    [Range(0, 1)]
    [SerializeField] private float dimmerValue = 1f;

    [Header("Physical Properties")]
    [SerializeField] private MeshRenderer housingRenderer;
    [SerializeField] private MeshRenderer diffuserRenderer;
    [SerializeField] private Material emissiveMaterial;

    [Header("Performance")]
    [SerializeField] private bool useBakedLighting = false;
    [SerializeField] private bool castShadows = false;

    private float baseIntensity;

    void Start()
    {
        baseIntensity = intensity;
        SetupLighting();
    }

    void SetupLighting()
    {
        if (lightSources == null) return;

        foreach (var light in lightSources)
        {
            if (light == null) continue;

            light.type = LightType.Rectangle;
            light.intensity = intensity * dimmerValue;
            light.color = lightColor;
            light.range = 10f;
            light.shadows = castShadows ? LightShadows.Soft : LightShadows.None;
            light.bounceIntensity = 0.5f;
            light.renderMode = useBakedLighting ?
                LightRenderMode.ForcePixel : LightRenderMode.Auto;
        }

        if (diffuserRenderer && emissiveMaterial)
        {
            emissiveMaterial.SetColor("_EmissionColor",
                lightColor * Mathf.LinearToGammaSpace(dimmerValue * 2f));
        }
    }

    public void SetDimmer(float value)
    {
        dimmerValue = Mathf.Clamp01(value);
        foreach (var light in lightSources)
        {
            if (light != null)
                light.intensity = baseIntensity * dimmerValue;
        }
    }

    public void ToggleLight()
    {
        dimmerValue = dimmerValue > 0 ? 0 : 1;
        SetDimmer(dimmerValue);
    }

    void OnValidate()
    {
        SetupLighting();
    }
}