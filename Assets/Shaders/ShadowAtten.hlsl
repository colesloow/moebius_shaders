// Returns the main directional light shadow factor (0 = fully in shadow, 1 = fully lit).

#if defined(SHADERGRAPH_PREVIEW)

void MainLightShadowAttenuation_float(float3 worldPos, out float shadowAtten)
{
    // In preview, just output 1 (fully lit)
    shadowAtten = 1.0;
}

#else

void MainLightShadowAttenuation_float(float3 worldPos, out float shadowAtten)
{
    float4 shadowCoord = TransformWorldToShadowCoord(worldPos);
    Light mainLight = GetMainLight(shadowCoord);
    shadowAtten = mainLight.shadowAttenuation;
}

#endif // SHADERGRAPH_PREVIEW