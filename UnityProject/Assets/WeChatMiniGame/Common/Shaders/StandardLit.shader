

Shader "WXBBShader/StandardLit" {

	Properties{
		_MainTex("Main Map", 2D) = "white" {}
		_Color("Base Color", Color) = (1,1,1,1)
		_NormalMap("Normal", 2D) = "bump" {}
		_MetallicGlossMap("MatallicGloss", 2D) = "black" {}
		_Smoothness("Smoothness", Range(0.0, 1)) = 1.0
		_OcclusionMap("Occlusion", 2D) = "black" {}
		_EmissionMap("Emissive Texture", 2D) = "black" {}
		_EmissionColor("Emissive Color", Color) = (1.0, 1.0, 1.0, 1.0)
		[HideInInspector] _SpecularSource("__source", Float) = 0.0

		
		
		[ToggleOff] _AlphaTest("AlphaTest", Float) = 0.0
		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.01
		[ToggleOff] _AlphaBlend("AlphaBlend", Float) = 0.0

		[HideInInspector] _Lighting("__Lighting", Float) = 0.0
		[HideInInspector] _Fog("__Fog", Float) = 0.0
		[HideInInspector] _Cull("__cull", Float) = 2.0
		[HideInInspector] _Mode("__mode", Float) = 0.0
		[HideInInspector] _SrcBlend("__src", Float) = 1.0
		[HideInInspector] _DstBlend("__dst", Float) = 0.0
		[HideInInspector] _ZWrite("__zw", Float) = 1.0
		[HideInInspector] _ZTest("__zt", Float) = 4.0
		[HideInInspector] _RenderQueue("__rq", Float) = 2000.0
	}

	SubShader{
		Tags {"IgnoreProjector" = "True" "RenderType" = "Opaque"}

		Pass {
			Tags { "LightMode" = "ForwardBase" }

			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]
			ZTest[_ZTest]
			Cull[_Cull]

			CGPROGRAM
			#pragma shader_feature NormalTexture
			#pragma shader_feature EmissiveTexture
			#pragma shader_feature OcclusionTexture
			#pragma shader_feature MetallicTexture
			#pragma shader_feature EnableAlphaCutoff
			#pragma shader_feature EnableLighting
			#pragma shader_feature EnableFog

			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog
			#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON

			#pragma vertex vert
			#pragma fragment frag

			#include "Lighting.cginc"
			#include "AutoLight.cginc"
			#include "BRDF.cginc"

			float4 _Color;
			float4 _EmissionColor;
			float4 _ColorCustom;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _NormalMap;
			sampler2D _EmissionMap;
			sampler2D _OcclusionMap;
			half _Cutoff;
			half _Smoothness;

			sampler2D _MetallicGlossMap;


			struct a2v {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1: TEXCOORD1;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2: TEXCOORD1; // lightmap uv
				float3 normalWS: TEXCOORD2;
				float3 viewDirWS: TEXCOORD3;
				float3 lightDirWS: TEXCOORD4;
				#if defined(NormalTexture)
					float3 tangentWS: TEXCOORD5;
					float3 bitangentWS: TEXCOORD6;
				#endif
				float4 positionWS: TEXCOORD7;
				SHADOW_COORDS(8)
				UNITY_FOG_COORDS(9)
			};

			

			#if UNITY_VERSION < 560
				#define unity_ShadowColor fixed4(0.42,0.48,0.63,1.0)
			#endif

			inline half3 ReadNormal(half4 color)
            {
	            half2 normalxy = (color.rg - 0.5f)*2.0f;
	            half normalz = sqrt(max(1e-3, 1.0f - dot(normalxy, normalxy)));
	            return half3(normalxy, normalz);
            }
			
			// from UnityGlobalIllumination.cginc
			inline half3 MixLightmapWithRealtimeAttenuation(half3 lightmap, half attenuation, half3 normalWorld)
			{
				// Let's try to make realtime shadows work on a surface, which already contains
				// baked lighting and shadowing from the main sun light.
				half3 shadowColor = unity_ShadowColor.rgb;
				half shadowStrength = _LightShadowData.x;

				// Summary:
				// 1) Calculate possible value in the shadow by subtracting estimated light contribution from the places occluded by realtime shadow:
				//      a) preserves other baked lights and light bounces
				//      b) eliminates shadows on the geometry facing away from the light
				// 2) Clamp against user defined ShadowColor.
				// 3) Pick original lightmap value, if it is the darkest one.


				// 1) Gives good estimate of illumination as if light would've been shadowed during the bake.
				//    Preserves bounce and other baked lights
				//    No shadows on the geometry facing away from the light
				half ndotl = saturate(dot(normalWorld, _WorldSpaceLightPos0.xyz));
				half3 estimatedLightContributionMaskedByInverseOfShadow = ndotl * (1 - attenuation) * _LightColor0.rgb;
				half3 subtractedLightmap = lightmap - estimatedLightContributionMaskedByInverseOfShadow;

				// 2) Allows user to define overall ambient of the scene and control situation when realtime shadow becomes too dark.
				half3 realtimeShadow = max(subtractedLightmap, shadowColor);
				realtimeShadow = lerp(realtimeShadow, lightmap, shadowStrength);

				// 3) Pick darkest color
				return min(lightmap, realtimeShadow);
			}

			v2f vert(a2v v) {

				v2f o;

				o.pos = UnityObjectToClipPos(v.vertex);

				o.positionWS = mul(unity_ObjectToWorld, v.vertex);

				o.viewDirWS = UnityWorldSpaceViewDir(o.positionWS);

				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				
				o.normalWS = UnityObjectToWorldNormal(v.normal);

				#ifdef LIGHTMAP_ON
					o.uv2 = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				o.lightDirWS = normalize(UnityWorldSpaceLightDir(o.positionWS));

				#if defined(NormalTexture)
					o.tangentWS = UnityObjectToWorldDir(v.tangent.xyz);
					o.bitangentWS = cross(o.normalWS, o.tangentWS) * v.tangent.w;
				#endif

				// Pass shadow coordinates to pixel shader
				TRANSFER_SHADOW(o);
				UNITY_TRANSFER_FOG(o, o.pos);

				return o;
			}
			
			void InitializeSurfaceData(fixed2 uv, out SurfaceData surfaceData){
				
				// base color and alpha
				fixed4 albedo = tex2D(_MainTex, uv);
				albedo = albedo * _Color;
				surfaceData.baseColor = albedo.xyz;	
				
				// alpha		
				surfaceData.alpha = albedo.a; // * _AlbedoIntensity;
				#if EnableAlphaCutoff
					clip(surfaceData.alpha - _Cutoff);
				#endif
				
				// metallic工作流
				#if defined(MetallicTexture)
					fixed4 metallicGloss = tex2D(_MetallicGlossMap, uv);
					surfaceData.metallic = metallicGloss.r;
					surfaceData.smoothness = metallicGloss.a;
				#else
					surfaceData.metallic = 0.0;
					surfaceData.smoothness = 0.5;
				#endif

				surfaceData.smoothness *= _Smoothness;
				
				surfaceData.specular = fixed3(0, 0, 0);
				
				// normal
				#if defined(NormalTexture)
					fixed3 normal = ReadNormal(tex2D(_NormalMap, uv));
					surfaceData.normalTS = normal;
				#else
					surfaceData.normalTS = half3(0, 0, 1);
				#endif

				// occulusion
				#if defined(OcclusionTexture)
					surfaceData.occlusion = tex2D(_OcclusionMap, uv);
				#else
					surfaceData.occlusion = 1.0;
				#endif
				
				// emission
				#if defined(EmissiveTexture)
					fixed4 emissionTexColor = tex2D(_EmissionMap, uv);
					surfaceData.emission = _EmissionColor.rgb * emissionTexColor.rgb;
				#else 
					surfaceData.emission = fixed3(0, 0, 0);
				#endif
			}

			void InitializePixelInput(v2f input, fixed3 normalTS, out PixelInput pixelInput){
				pixelInput = (PixelInput)0;

				pixelInput.positionWS = input.positionWS;
				pixelInput.viewDirWS = SafeNormalize(input.viewDirWS);

				#if defined(NormalTexture)
					half3x3 TBN = half3x3(input.tangentWS, input.bitangentWS, input.normalWS);
					pixelInput.normalWS = TransformTangentToWorld(normalTS, TBN);
				#else
					pixelInput.normalWS = input.normalWS;
				#endif

				// gi
				#ifdef LIGHTMAP_ON
					pixelInput.bakeGI = SampleLightmap(input.uv2);
				#else
					// pixelInput.bakeGI = ShadeSH9(float4(pixelInput.normalWS,1));
					pixelInput.bakeGI = fixed3(UNITY_LIGHTMODEL_AMBIENT.rgb);
				#endif
				
			}
			
			fixed4 frag(v2f i) : SV_Target {
				
				SurfaceData surfaceData;
				InitializeSurfaceData(i.uv, surfaceData);

				PixelInput pixelInput;
				InitializePixelInput(i, surfaceData.normalTS, pixelInput);

				BRDFData brdfData;
				InitializeBRDFData(surfaceData, brdfData);

				float attenuation = SHADOW_ATTENUATION(i);

				#ifdef LIGHTMAP_ON
					pixelInput.bakeGI = MixLightmapWithRealtimeAttenuation(pixelInput.bakeGI, attenuation, pixelInput.normalWS);
				#endif

				fixed3 lighting = GlobalIllumination(brdfData, pixelInput.bakeGI, surfaceData.occlusion, pixelInput.normalWS, pixelInput.viewDirWS);
				
				lighting += PhysicallyBasedLighting(brdfData, _LightColor0.xyz, i.lightDirWS, attenuation, pixelInput.normalWS,  pixelInput.viewDirWS);

				fixed4 color = fixed4(lighting, surfaceData.alpha);
				
				#if EnableFog
					UNITY_APPLY_FOG(i.fogCoord, color);
				#endif
				
				return color;

			}

			ENDCG
		}

	}
	CustomEditor "WeChat.StandardLitGUI"
	FallBack "Standard"
}