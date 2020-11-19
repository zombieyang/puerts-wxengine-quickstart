
#define MIN_POS  0.00001
#define HALF_MIN 0.00001
#define kDieletricSpec half4(0.04, 0.04, 0.04, 1.0 - 0.04)

#ifndef UNITY_SPECUBE_LOD_STEPS
    #define UNITY_SPECUBE_LOD_STEPS 6
#endif

#define PositivePow(a,b) pow(a,b)
#define SAMPLE_TEXURECUBE_LOD(textureName, samplerName, coord3, lod) textureName.SampleLevel(samplerName, coord3, lod)
// utils
fixed3 TransformTangentToWorld(fixed3 vectorTS, fixed3x3 tangentToWorld)
{
    return mul(vectorTS, tangentToWorld);
}

fixed3 SafeNormalize(float3 vec3){
    float vecdot = max(MIN_POS, dot(vec3, vec3));
    return vec3 * rsqrt(vecdot);
}

fixed3 SampleLightmap(float2 lightmapUV)
{
    #if !defined(SHADOWS_SHADOWMASK)
        fixed3 lightMapColor = fixed4(DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, lightmapUV)), 1.0).rgb;
    #else
        fixed3 lightMapColor = fixed3(0,0,0);
    #endif
    return lightMapColor;
}

// metallic 工作流没有高光颜色，0.04是绝缘体的最低反射率 
fixed OneMinusReflectivityMetallic(half metallic)
{
    // 1 - reflectivity = 1 - lerp(dielectricSpec, 1, metallic) = lerp(1 -  dielectricSpec, 0, metallic)
    // kDieletricSpec.a = 1 -  dielectricSpec
    // 1 - reflectivity = lerp(alpha, 0, metallic) = alpha + metallic*(0 - alpha) = alpha - metallic * alpha
    half oneMinusReflectivityMetallic = kDieletricSpec.a;
    return oneMinusReflectivityMetallic - metallic * oneMinusReflectivityMetallic;
}

fixed RoughnessToMipmapLevel(fixed roughness)
{
    uint mipMapCount = UNITY_SPECUBE_LOD_STEPS;
    roughness = roughness * (1.7 - 0.7 * roughness);
    return roughness * mipMapCount;
}

half3 DecodeHDREnviroment(fixed4 encodedIrradiance, fixed4 decodedInstructions)
{   
    fixed alpha = max(decodedInstructions.w*(encodedIrradiance.a - 1.0) + 1.0, 0.0);
    return (decodedInstructions.x * PositivePow(alpha, decodedInstructions.y))*encodedIrradiance.rgb;
}

/*
* Physically Based Lighting
*/
struct SurfaceData{
    fixed3 baseColor;
    fixed3 specular;
    fixed  metallic;
    fixed  smoothness;
    fixed3 normalTS;
    fixed3 emission;
    fixed3 occlusion;
    fixed  alpha;
};

struct PixelInput{
    float3 positionWS;
    fixed3 normalWS;
    fixed3 viewDirWS;
    // float4 shadowCoord;
    // half fogCoord;
    // fixed3 vertexLighting;
    fixed3 bakeGI;    
};

/**
* diffuse = albedo * (1 - 反射率)
* specular = lerp(0.04, albedo, 反射率)
* grazingTerm = smoothness + 反射率
* perceptualRoughness = 1 - smoothness
* roughness = perceptualRoughness* perceptualRoughness
* roughness2 = perceptualRoughness* perceptualRoughness* perceptualRoughness* perceptualRoughness
* normalizationTerm = roughness * 4 + 2
* roughness2MinusOne = roughness2 - 1
*/
struct BRDFData{
    fixed3 diffuse;
    fixed3 specular;
    fixed  perceptualRoughness;
    fixed  roughness;
    fixed  roughness2;
    fixed  grazingTerm;

    fixed  normalizationTerm; // roughness * 4.0 - 2.0
    fixed  roughness2MinusOne;
};

inline void InitializeBRDFData(SurfaceData surfaceData, out BRDFData outData){

    // mettalic工作流
    fixed kd = OneMinusReflectivityMetallic(surfaceData.metallic);
    fixed reflectivity = 1.0 - kd;

    outData.diffuse = surfaceData.baseColor * kd;
    outData.specular = lerp(kDieletricSpec.rgb, surfaceData.baseColor, surfaceData.metallic); // F0

    outData.grazingTerm = saturate(surfaceData.smoothness + reflectivity);
    outData.perceptualRoughness = 1.0 - surfaceData.smoothness;
    outData.roughness = outData.perceptualRoughness * outData.perceptualRoughness;
    outData.roughness2 = outData.roughness * outData.roughness;

    outData.normalizationTerm = outData.roughness * 4.0 + 2.0;
    outData.roughness2MinusOne = outData.roughness2 - 1.0;
}
/*
Fresnel approximated with 1/ LoH

https://community.arm.com/cfs-file/__key/communityserver-blogs-components-weblogfiles/00-00-00-20-66/siggraph2015_2D00_mmg_2D00_renaldas_2D00_slides.pdf
*/
half3 DirectLighting(BRDFData brdfData, half3 normalWS, half3 lightDirWS, half3 viewDirWS){
    float3 halfDir = SafeNormalize(float3(lightDirWS) + float3(viewDirWS));
    float  NoH = saturate(dot(normalWS, halfDir));
    float  LoH = saturate(dot(lightDirWS, halfDir));
    
    // Minimalist CookTorrance BRDF
    // BRDF_Spec = (D * V * F) / 4.0
    // D = roughness2 / (NoH2 *(roughness2 - 1) + 1)2
    // V * F = 1.0 / (LoH2 * (roughness + 0.5))
    // Final BRDF_Spec = roughness2 / (NoH2 * (roughness2 -1) + 1)2 * (LoH2 * (roughness + 0.5) * 4.0)
    // MAD: BRDF.normalizationTerm = outData.roughness * 4.0 + 2.0
    float d = NoH * NoH * brdfData.roughness2MinusOne + 1.000001f;
    half LoH2 = LoH * LoH;
    half specularTerm = brdfData.roughness2 /((d*d)*max(0.1h, LoH2)*brdfData.normalizationTerm);

    specularTerm = specularTerm - HALF_MIN;
    specularTerm = clamp(specularTerm, 0.0, 100.0);

    half3 color = specularTerm * brdfData.specular + brdfData.diffuse;
    return color;

}

half3 PhysicallyBasedLighting(BRDFData brdfData, half3 lightColor, half3 lightDirWS, half lightAttenuation, half3 normalWS, half3 viewDirWS){
    
    half NoL = saturate(dot(normalWS, lightDirWS));
    half3 radiance = lightColor * (lightAttenuation * NoL);
    half3 lighting = DirectLighting(brdfData, normalWS, lightDirWS, viewDirWS);
    return lighting * radiance;
}

half3 GlossyEnvironmentReflection(half3 reflectVec, half roughness, half occlusion, half3 normalWS, half3 viewDirWS)
{
    half mip = RoughnessToMipmapLevel(roughness);
    half4 encodedIrradiance = SAMPLE_TEXURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectVec, mip);
    return encodedIrradiance.rgb * occlusion;
}

half3 EnviromentBRDF(BRDFData brdfData, half3 indirectDiffuse, half3 indirectSpecular, half fresnelTerm)
{
    half3 color = indirectDiffuse * brdfData.diffuse;
    float surfaceReduction = 1.0/(brdfData.roughness2 + 1.0);
    color += surfaceReduction * indirectSpecular * lerp(brdfData.specular, brdfData.grazingTerm, fresnelTerm);
    return color;
}

half3 GlobalIllumination(BRDFData brdfData, half3 bakeGI, half occlusion, half3 normalWS, half3 viewDirWS)
{
    half3 relfectVector = reflect(-viewDirWS, normalWS);
 
    half fresnelTerm = Pow4(1.0 - saturate(dot(normalWS, viewDirWS)));

    half3 indirectDiffuse = bakeGI * occlusion;
    half3 indirectSpecular = GlossyEnvironmentReflection(relfectVector, brdfData.perceptualRoughness, occlusion, normalWS, viewDirWS);
    return EnviromentBRDF(brdfData, indirectDiffuse, indirectSpecular, fresnelTerm);

}
