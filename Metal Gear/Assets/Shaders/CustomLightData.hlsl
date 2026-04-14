
void CustomLightData_float(float3 WorldPos, bool AdditionalLights, out half3 Direction, out half3 Color, out half DistanceAtten, out float ShadowAtten)
{

#ifdef SHADERGRAPH_PREVIEW
    Direction = normalize(float3(1, 1, -0.4));
    Color = float4(1, 1, 1, 1);
    DistanceAtten = 1;
    ShadowAtten = 1;
#else
    float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
    Light mainLight = GetMainLight(shadowCoord);
    Direction = mainLight.direction;
    Color = mainLight.color;
    DistanceAtten = mainLight.distanceAttenuation;
    ShadowAtten = 1;//mainLight.shadowAttenuation;

    if (AdditionalLights) {

        uint lightCount = GetAdditionalLightsCount();

        for (uint i = 0; i < lightCount; i++) {
            Light light = GetAdditionalLight(i, WorldPos,1);

            ShadowAtten *= light.shadowAttenuation;
        }


    }


#endif
}