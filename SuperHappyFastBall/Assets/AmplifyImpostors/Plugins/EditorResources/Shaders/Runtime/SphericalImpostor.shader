// Amplify Impostors
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

Shader "Hidden/Amplify Impostors/Spherical Impostor"
{
	Properties
	{
		[NoScaleOffset]_Albedo("Albedo & Alpha", 2D) = "white" {}
		[NoScaleOffset]_Normals("Normals & Depth", 2D) = "white" {}
		[NoScaleOffset]_Specular("Specular & Smoothness", 2D) = "white" {}
		[NoScaleOffset]_Emission("Emission & Occlusion", 2D) = "white" {}
		_ClipMask("Clip", Range( 0 , 1)) = 0.5
		_TextureBias("Texture Bias", Float) = -1
		_ShadowBias("Shadow Bias", Range( 0 , 2)) = 0.25
		[HideInInspector]_FramesX("Frames X", Float) = 16
		[HideInInspector]_FramesY("Frames Y", Float) = 16
		[HideInInspector]_DepthSize("DepthSize", Float) = 1
		[HideInInspector]_ImpostorSize("Impostor Size", Float) = 1
		[HideInInspector]_Offset("Offset", Vector) = (0,0,0,0)
		[Toggle(EFFECT_HUE_VARIATION)] _Hue("Use SpeedTree Hue", Float) = 0
		_HueVariation("Hue Variation", Color) = (0,0,0,0)
	}

	SubShader
	{
		CGINCLUDE
		#pragma target 3.0
		#define UNITY_SAMPLE_FULL_SH_PER_PIXEL 1
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		uniform float _FramesX;
		uniform float _FramesY;
		uniform float _ImpostorSize;
		uniform float _TextureBias;
		uniform sampler2D _Albedo;
		uniform sampler2D _Normals;
		uniform sampler2D _Specular;
		uniform sampler2D _Emission;
		uniform float _DepthSize;
		uniform float _ClipMask;
		uniform float _ShadowBias;
		uniform float4 _Offset;

		#ifdef EFFECT_HUE_VARIATION
			half4 _HueVariation;
		#endif

		inline void SphereImpostorVertex( inout appdata_full v, inout float2 frameUVs, inout float4 viewPos )
		{
			// INPUTS
			float sizeX = _FramesX;
			float sizeY = _FramesY - 1; // adjusted
			float3 fractions = 1 / float3( sizeX, _FramesY, sizeY );
			float2 sizeFraction = fractions.xy;
			float axisSizeFraction = fractions.z;

			// Basic data
			v.vertex.xyz += _Offset.xyz;
			float3 worldOrigin = float3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
			#if defined(UNITY_PASS_SHADOWCASTER)
				float3 worldCameraPos = 0;
				if( unity_LightShadowBias.y == 0 ) // Collector or Caster?
					worldCameraPos = _WorldSpaceCameraPos;
				else
					worldCameraPos = UnityWorldSpaceLightDir(mul(unity_ObjectToWorld, v.vertex).xyz) * -5000;
			#else
				float3 worldCameraPos = _WorldSpaceCameraPos;
			#endif
			float3 objectCameraDirection = normalize( mul( (float3x3)unity_WorldToObject, worldCameraPos - worldOrigin ) - _Offset.xyz );

			// Create orthogonal vectors to define the billboard
			float3 upVector = float3( 0,1,0 );
			float3 objectHorizontalVector = normalize( cross( objectCameraDirection, upVector ) );
			float3 objectVerticalVector = cross( objectHorizontalVector, objectCameraDirection );

			// Create vertical radial angle
			float verticalAngle = frac( atan2( -objectCameraDirection.z, -objectCameraDirection.x ) / UNITY_TWO_PI ) * sizeX + 0.5;

			// Create horizontal radial angle
			float verticalDot = dot( objectCameraDirection, upVector );
			float upAngle = ( acos( -verticalDot ) / UNITY_PI ) + axisSizeFraction * 0.5f;
			float yRot = sizeFraction.x * UNITY_PI * verticalDot * ( 2 * frac( verticalAngle ) - 1 );

			// Billboard rotation
			float2 uvExpansion = v.texcoord.xy - 0.5;
			float cosY = cos( yRot );
			float sinY = sin( yRot );
			float2 uvRotator = mul( uvExpansion, float2x2( cosY , -sinY , sinY , cosY ) ) * _ImpostorSize;

			// Billboard
			float3 billboard = objectHorizontalVector * uvRotator.x + objectVerticalVector * uvRotator.y + _Offset.xyz;

			// Frame coords
			float2 relativeCoords = float2( floor( verticalAngle ), min( floor( upAngle * sizeY ), sizeY ) );
			float2 frameUV = ( v.texcoord.xy + relativeCoords ) * sizeFraction;

			frameUVs.xy = frameUV;
			viewPos.xyz = UnityObjectToViewPos( billboard );

			#ifdef EFFECT_HUE_VARIATION
				float hueVariationAmount = frac(unity_ObjectToWorld[0].w + unity_ObjectToWorld[1].w + unity_ObjectToWorld[2].w);
				viewPos.w = saturate(hueVariationAmount * _HueVariation.a);
			#endif

			v.vertex.xyz = billboard;
			v.normal.xyz = objectCameraDirection;
		}

		inline void SphereImpostorFragment( inout SurfaceOutputStandardSpecular o, out float4 clipPos, out float3 worldPos, float2 frameUV, float4 viewPos )
		{
			float4 albedoSample = tex2Dbias( _Albedo, float4( frameUV, 0, _TextureBias) );
			float4 normalSample = tex2Dbias( _Normals, float4( frameUV, 0, _TextureBias) );
			float4 specularSample = tex2Dbias( _Specular, float4( frameUV, 0, _TextureBias) );
			float4 emissionSample = tex2Dbias( _Emission, float4( frameUV, 0, _TextureBias) );

			// Simple outputs
			float3 albedo = albedoSample.rgb;
			float3 emission = emissionSample.rgb;
			float3 specular = specularSample.rgb;
			float smoothness = specularSample.a;
			float occlusion = emissionSample.a;
			float alphaMask = albedoSample.a;

			// Normal
			float4 remapNormal = normalSample * 2 - 1; // object normal is remapNormal.rgb
			float3 worldNormal = normalize( mul( (float3x3)unity_ObjectToWorld, remapNormal.xyz ) );

			// Depth
			float depth = remapNormal.a * _DepthSize * 0.5;
			#if defined(UNITY_PASS_SHADOWCASTER)
			if( unity_LightShadowBias.y != 0 )
			{
				depth = depth * 0.95 - 0.05  - _ShadowBias;
			}
			#endif
			viewPos.z += depth;

			// Modified clip position and world position
			worldPos = mul( UNITY_MATRIX_I_V, float4( viewPos.xyz, 1 ) ).xyz;

			clipPos = mul( UNITY_MATRIX_P, float4( viewPos.xyz, 1 ) );
			#ifdef UNITY_PASS_SHADOWCASTER
				clipPos = UnityApplyLinearShadowBias( clipPos );
			#endif
			clipPos.xyz /= clipPos.w;
			if( UNITY_NEAR_CLIP_VALUE < 0 )
				clipPos = clipPos * 0.5 + 0.5;

			#ifdef EFFECT_HUE_VARIATION
				half3 shiftedColor = lerp(albedo.rgb, _HueVariation.rgb, viewPos.w);
				half maxBase = max(albedo.r, max(albedo.g, albedo.b));
				half newMaxBase = max(shiftedColor.r, max(shiftedColor.g, shiftedColor.b));
				maxBase /= newMaxBase;
				maxBase = maxBase * 0.5f + 0.5f;
				shiftedColor.rgb *= maxBase;
				albedo.rgb = saturate(shiftedColor);
			#endif

			o.Albedo = albedo;
			o.Normal = worldNormal;
			o.Emission = emission;
			o.Specular = specular;
			o.Smoothness = smoothness;
			o.Occlusion = occlusion;
			o.Alpha = ( alphaMask - _ClipMask );
			clip( o.Alpha );
		}
		ENDCG

		Tags { "RenderType"="Opaque" "Queue"="Geometry+0" "DisableBatching"="True" }
		Cull Off

		Pass
		{
			ZWrite On
			Name "ForwardBase"
			Tags { "LightMode"="ForwardBase" }

			CGPROGRAM
			// compile directives
			#pragma vertex vert_surf
			#pragma fragment frag_surf
			#pragma multi_compile_fog
			#pragma multi_compile_fwdbase
			#pragma multi_compile_instancing
			#pragma multi_compile __ LOD_FADE_CROSSFADE
			#include "HLSLSupport.cginc"
			#if !defined( UNITY_INSTANCED_LOD_FADE )
				#define UNITY_INSTANCED_LOD_FADE
			#endif
			#if !defined( UNITY_INSTANCED_SH )
				#define UNITY_INSTANCED_SH
			#endif
			#if !defined( UNITY_INSTANCED_LIGHTMAPSTS )
				#define UNITY_INSTANCED_LIGHTMAPSTS
			#endif
			#include "UnityShaderVariables.cginc"
			#include "UnityShaderUtilities.cginc"
			#ifndef UNITY_PASS_FORWARDBASE
			#define UNITY_PASS_FORWARDBASE
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			#include "AutoLight.cginc"
			#include "UnityStandardUtils.cginc"

			#pragma shader_feature EFFECT_HUE_VARIATION

			struct v2f_surf {
				UNITY_POSITION(pos);
				#ifndef LIGHTMAP_ON
					#if SHADER_TARGET >= 30
						float4 lmap : TEXCOORD1;
					#endif
					#if UNITY_SHOULD_SAMPLE_SH
						half3 sh : TEXCOORD2;
					#endif
				#endif
				#ifdef LIGHTMAP_ON
					float4 lmap : TEXCOORD1;
				#endif
				float2 frameUVs : TEXCOORD5;
				float4 viewPos : TEXCOORD6;
				UNITY_SHADOW_COORDS(3)
				UNITY_FOG_COORDS(4)
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f_surf vert_surf (appdata_full v ) {
				UNITY_SETUP_INSTANCE_ID(v);
				v2f_surf o;
				UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				SphereImpostorVertex( v, o.frameUVs, o.viewPos );

				o.pos = UnityObjectToClipPos(v.vertex);

				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
				#ifdef DYNAMICLIGHTMAP_ON
					o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif
				#ifdef LIGHTMAP_ON
					o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				#ifndef LIGHTMAP_ON
					#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
						o.sh = 0;
						#ifdef VERTEXLIGHT_ON
						o.sh += Shade4PointLights (
							unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
							unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
							unity_4LightAtten0, worldPos, worldNormal);
						#endif
						o.sh = ShadeSHPerVertex (worldNormal, o.sh);
					#endif
				#endif

				UNITY_TRANSFER_SHADOW(o, v.texcoord1.xy);
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}

			fixed4 frag_surf (v2f_surf IN, out float outDepth : SV_Depth ) : SV_Target {
				UNITY_SETUP_INSTANCE_ID(IN);
				#ifdef UNITY_COMPILER_HLSL
					SurfaceOutputStandardSpecular o = (SurfaceOutputStandardSpecular)0;
				#else
					SurfaceOutputStandardSpecular o;
				#endif

				float4 clipPos;
				float3 worldPos;
				SphereImpostorFragment( o, clipPos, worldPos, IN.frameUVs, IN.viewPos );

				outDepth = clipPos.z;

				#ifndef USING_DIRECTIONAL_LIGHT
					fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
				#else
					fixed3 lightDir = _WorldSpaceLightPos0.xyz;
				#endif

				fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));

				UNITY_APPLY_DITHER_CROSSFADE(IN.pos.xy);
				IN.pos = clipPos;
				UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
				fixed4 c = 0;

				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
				gi.indirect.diffuse = 0;
				gi.indirect.specular = 0;
				gi.light.color = _LightColor0.rgb;
				gi.light.dir = lightDir;

				UnityGIInput giInput;
				UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
				giInput.light = gi.light;
				giInput.worldPos = worldPos;
				giInput.worldViewDir = worldViewDir;
				giInput.atten = atten;
				#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
					giInput.lightmapUV = IN.lmap;
				#else
					giInput.lightmapUV = 0.0;
				#endif
				#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
					giInput.ambient = IN.sh;
				#else
					giInput.ambient.rgb = 0.0;
				#endif
				giInput.probeHDR[0] = unity_SpecCube0_HDR;
				giInput.probeHDR[1] = unity_SpecCube1_HDR;
				#if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
					giInput.boxMin[0] = unity_SpecCube0_BoxMin;
				#endif
				#ifdef UNITY_SPECCUBE_BOX_PROJECTION
					giInput.boxMax[0] = unity_SpecCube0_BoxMax;
					giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
					giInput.boxMax[1] = unity_SpecCube1_BoxMax;
					giInput.boxMin[1] = unity_SpecCube1_BoxMin;
					giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
				#endif
				LightingStandardSpecular_GI(o, giInput, gi);

				c += LightingStandardSpecular (o, worldViewDir, gi);
				c.rgb += o.Emission;
				//UNITY_TRANSFER_FOG(IN,IN.pos);
				UNITY_APPLY_FOG(IN.fogCoord, c);
				return c;
			}

			ENDCG
		}

		Pass
		{
			Name "ForwardAdd"
			Tags { "LightMode"="ForwardAdd" }
			ZWrite Off
			Blend One One

			CGPROGRAM
			// compile directives
			#pragma vertex vert_surf
			#pragma fragment frag_surf
			#pragma multi_compile_fog
			#pragma multi_compile_instancing
			#pragma multi_compile_fwdadd_fullshadows
			#pragma multi_compile __ LOD_FADE_CROSSFADE
			#pragma skip_variants INSTANCING_ON
			#include "HLSLSupport.cginc"
			#if !defined( UNITY_INSTANCED_LOD_FADE )
				#define UNITY_INSTANCED_LOD_FADE
			#endif
			#if !defined( UNITY_INSTANCED_SH )
				#define UNITY_INSTANCED_SH
			#endif
			#if !defined( UNITY_INSTANCED_LIGHTMAPSTS )
				#define UNITY_INSTANCED_LIGHTMAPSTS
			#endif
			#include "UnityShaderVariables.cginc"
			#include "UnityShaderUtilities.cginc"
			#ifndef UNITY_PASS_FORWARDADD
			#define UNITY_PASS_FORWARDADD
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			#include "AutoLight.cginc"
			#include "UnityStandardUtils.cginc"

			#pragma shader_feature EFFECT_HUE_VARIATION

			struct v2f_surf {
				UNITY_POSITION(pos);
				float2 frameUVs : TEXCOORD5;
				float4 viewPos : TEXCOORD6;
				UNITY_SHADOW_COORDS(1)
				UNITY_FOG_COORDS(2)
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f_surf vert_surf (appdata_full v ) {
				UNITY_SETUP_INSTANCE_ID(v);
				v2f_surf o;
				UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				SphereImpostorVertex( v, o.frameUVs, o.viewPos );

				o.pos = UnityObjectToClipPos(v.vertex);
				UNITY_TRANSFER_SHADOW(o, v.texcoord1.xy);
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}

			fixed4 frag_surf (v2f_surf IN, out float outDepth : SV_Depth ) : SV_Target {
				UNITY_SETUP_INSTANCE_ID(IN);
				#ifdef UNITY_COMPILER_HLSL
					SurfaceOutputStandardSpecular o = (SurfaceOutputStandardSpecular)0;
				#else
					SurfaceOutputStandardSpecular o;
				#endif

				float4 clipPos;
				float3 worldPos;
				SphereImpostorFragment( o, clipPos, worldPos, IN.frameUVs, IN.viewPos );

				outDepth = clipPos.z;

				#ifndef USING_DIRECTIONAL_LIGHT
					fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
				#else
					fixed3 lightDir = _WorldSpaceLightPos0.xyz;
				#endif

				fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));

				UNITY_APPLY_DITHER_CROSSFADE(IN.pos.xy);
				IN.pos = clipPos;
				UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
				fixed4 c = 0;

				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
				gi.indirect.diffuse = 0;
				gi.indirect.specular = 0;
				gi.light.color = _LightColor0.rgb;
				gi.light.dir = lightDir;
				gi.light.color *= atten;
				c += LightingStandardSpecular (o, worldViewDir, gi);
				//UNITY_TRANSFER_FOG(IN,IN.pos);
				UNITY_APPLY_FOG(IN.fogCoord, c);
				return c;
			}
			ENDCG
		}

		Pass
		{
			Name "Deferred"
			Tags { "LightMode"="Deferred" }

			CGPROGRAM
			#pragma vertex vert_surf
			#pragma fragment frag_surf
			#pragma multi_compile_instancing
			#pragma multi_compile __ LOD_FADE_CROSSFADE
			#pragma exclude_renderers nomrt
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#pragma multi_compile_prepassfinal
			#include "HLSLSupport.cginc"
			#if !defined( UNITY_INSTANCED_LOD_FADE )
				#define UNITY_INSTANCED_LOD_FADE
			#endif
			#if !defined( UNITY_INSTANCED_SH )
				#define UNITY_INSTANCED_SH
			#endif
			#if !defined( UNITY_INSTANCED_LIGHTMAPSTS )
				#define UNITY_INSTANCED_LIGHTMAPSTS
			#endif
			#include "UnityShaderVariables.cginc"
			#include "UnityShaderUtilities.cginc"
			#ifndef UNITY_PASS_DEFERRED
			#define UNITY_PASS_DEFERRED
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			#include "UnityStandardUtils.cginc"

			#pragma shader_feature EFFECT_HUE_VARIATION

			#ifdef LIGHTMAP_ON
			float4 unity_LightmapFade;
			#endif
			fixed4 unity_Ambient;

			struct v2f_surf {
				UNITY_POSITION(pos);
				#ifndef DIRLIGHTMAP_OFF
					half3 viewDir : TEXCOORD1;
				#endif
				float4 lmap : TEXCOORD2;
				#ifndef LIGHTMAP_ON
					#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
						half3 sh : TEXCOORD3;
					#endif
				#else
					#ifdef DIRLIGHTMAP_OFF
						float4 lmapFadePos : TEXCOORD4;
					#endif
				#endif
				float2 frameUVs : TEXCOORD5;
				float4 viewPos : TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f_surf vert_surf (appdata_full v ) {
				UNITY_SETUP_INSTANCE_ID(v);
				v2f_surf o;
				UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				SphereImpostorVertex( v, o.frameUVs, o.viewPos );

				o.pos = UnityObjectToClipPos(v.vertex);

				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
				float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
				#ifndef DIRLIGHTMAP_OFF
					o.viewDir = viewDirForLight;
				#endif
				#ifdef DYNAMICLIGHTMAP_ON
					o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#else
					o.lmap.zw = 0;
				#endif
				#ifdef LIGHTMAP_ON
					o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
					#ifdef DIRLIGHTMAP_OFF
						o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
						o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
					#endif
				#else
					o.lmap.xy = 0;
					#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
						o.sh = 0;
						o.sh = ShadeSHPerVertex (worldNormal, o.sh);
					#endif
				#endif
				return o;
			}

			void frag_surf (v2f_surf IN , out half4 outGBuffer0 : SV_Target0, out half4 outGBuffer1 : SV_Target1, out half4 outGBuffer2 : SV_Target2, out half4 outEmission : SV_Target3
			#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
				, out half4 outShadowMask : SV_Target4
			#endif
			, out float outDepth : SV_Depth
			) {
				UNITY_SETUP_INSTANCE_ID(IN);
				#ifdef UNITY_COMPILER_HLSL
					SurfaceOutputStandardSpecular o = (SurfaceOutputStandardSpecular)0;
				#else
					SurfaceOutputStandardSpecular o;
				#endif

				float4 clipPos;
				float3 worldPos;
				SphereImpostorFragment( o, clipPos, worldPos, IN.frameUVs, IN.viewPos );

				outDepth = clipPos.z;

				#ifndef USING_DIRECTIONAL_LIGHT
					fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
				#else
					fixed3 lightDir = _WorldSpaceLightPos0.xyz;
				#endif

				fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));

				UNITY_APPLY_DITHER_CROSSFADE(IN.pos.xy);
				IN.pos = clipPos;
				half atten = 1;

				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
				gi.indirect.diffuse = 0;
				gi.indirect.specular = 0;
				gi.light.color = 0;
				gi.light.dir = half3(0,1,0);

				UnityGIInput giInput;
				UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
				giInput.light = gi.light;
				giInput.worldPos = worldPos;
				giInput.worldViewDir = worldViewDir;
				giInput.atten = atten;
				#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
					giInput.lightmapUV = IN.lmap;
				#else
					giInput.lightmapUV = 0.0;
				#endif
				#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
					giInput.ambient = IN.sh;
				#else
					giInput.ambient.rgb = 0.0;
				#endif
				giInput.probeHDR[0] = unity_SpecCube0_HDR;
				giInput.probeHDR[1] = unity_SpecCube1_HDR;
				#if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
					giInput.boxMin[0] = unity_SpecCube0_BoxMin;
				#endif
				#ifdef UNITY_SPECCUBE_BOX_PROJECTION
					giInput.boxMax[0] = unity_SpecCube0_BoxMax;
					giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
					giInput.boxMax[1] = unity_SpecCube1_BoxMax;
					giInput.boxMin[1] = unity_SpecCube1_BoxMin;
					giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
				#endif
				LightingStandardSpecular_GI(o, giInput, gi);

				outEmission = LightingStandardSpecular_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
				#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
					outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, float3(0, 0, 0));
				#endif
				#ifndef UNITY_HDR_ON
					outEmission.rgb = exp2(-outEmission.rgb);
				#endif
			}
			ENDCG
		}

		Pass
		{
			Name "Meta"
			Tags { "LightMode"="Meta" }
			Cull Off

			CGPROGRAM
			#pragma vertex vert_surf
			#pragma fragment frag_surf
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#pragma skip_variants INSTANCING_ON
			#pragma shader_feature EDITOR_VISUALIZATION
			#pragma multi_compile_instancing

			#include "HLSLSupport.cginc"
			#if !defined( UNITY_INSTANCED_LOD_FADE )
				#define UNITY_INSTANCED_LOD_FADE
			#endif
			#if !defined( UNITY_INSTANCED_SH )
				#define UNITY_INSTANCED_SH
			#endif
			#if !defined( UNITY_INSTANCED_LIGHTMAPSTS )
				#define UNITY_INSTANCED_LIGHTMAPSTS
			#endif
			#include "UnityShaderVariables.cginc"
			#include "UnityShaderUtilities.cginc"
			#ifndef UNITY_PASS_META
			#define UNITY_PASS_META
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			#include "UnityStandardUtils.cginc"
			#include "UnityMetaPass.cginc"

			#pragma shader_feature EFFECT_HUE_VARIATION

			struct v2f_surf {
				UNITY_POSITION(pos);
				float2 frameUVs : TEXCOORD5;
				float4 viewPos : TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f_surf vert_surf (appdata_full v ) {
				UNITY_SETUP_INSTANCE_ID(v);
				v2f_surf o;
				UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				SphereImpostorVertex( v, o.frameUVs, o.viewPos );

				o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
				return o;
			}

			fixed4 frag_surf (v2f_surf IN, out float outDepth : SV_Depth  ) : SV_Target {
				UNITY_SETUP_INSTANCE_ID(IN);
				#ifdef UNITY_COMPILER_HLSL
					SurfaceOutputStandardSpecular o = (SurfaceOutputStandardSpecular)0;
				#else
					SurfaceOutputStandardSpecular o;
				#endif

				float4 clipPos;
				float3 worldPos;
				SphereImpostorFragment( o, clipPos, worldPos, IN.frameUVs, IN.viewPos );

				outDepth = clipPos.z;

				#ifndef USING_DIRECTIONAL_LIGHT
					fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
				#else
					fixed3 lightDir = _WorldSpaceLightPos0.xyz;
				#endif

				fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));

				UNITY_APPLY_DITHER_CROSSFADE(IN.pos.xy);
				IN.pos = clipPos;

				UnityMetaInput metaIN;
				UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
				metaIN.Albedo = o.Albedo;
				metaIN.Emission = o.Emission;
				return UnityMetaFragment(metaIN);
			}
			ENDCG
		}

		Pass
		{
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }
			ZWrite On

			CGPROGRAM
			#pragma vertex vert_surf
			#pragma fragment frag_surf
			#pragma multi_compile_shadowcaster
			#pragma multi_compile __ LOD_FADE_CROSSFADE
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if !defined( UNITY_INSTANCED_LOD_FADE )
				#define UNITY_INSTANCED_LOD_FADE
			#endif
			#include "UnityShaderVariables.cginc"
			#include "UnityShaderUtilities.cginc"
			//#ifndef UNITY_PASS_SHADOWCASTER
			//#define UNITY_PASS_SHADOWCASTER
			//#endif
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			#include "UnityStandardUtils.cginc"

			//float4x4 unity_WorldToLight;

			struct v2f_surf {
				V2F_SHADOW_CASTER;
				float2 frameUVs : TEXCOORD5;
				float4 viewPos : TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f_surf vert_surf (appdata_full v) {
				UNITY_SETUP_INSTANCE_ID(v);
				v2f_surf o;
				UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				SphereImpostorVertex( v, o.frameUVs, o.viewPos );

				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);

				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				//o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag_surf (v2f_surf IN, out float outDepth : SV_Depth ) : SV_Target {
				UNITY_SETUP_INSTANCE_ID(IN);
				#ifdef UNITY_COMPILER_HLSL
					SurfaceOutputStandardSpecular o = (SurfaceOutputStandardSpecular)0;
				#else
					SurfaceOutputStandardSpecular o;
				#endif

				float4 clipPos;
				float3 worldPos;
				SphereImpostorFragment( o, clipPos, worldPos, IN.frameUVs, IN.viewPos );

				outDepth = clipPos.z;

				UNITY_APPLY_DITHER_CROSSFADE(IN.pos.xy);
				IN.pos = clipPos;

				SHADOW_CASTER_FRAGMENT(IN)
			}

			ENDCG
		}
	}
	Fallback "Diffuse"
}
