// Amplify Impostors
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

Shader "Hidden/Amplify Impostors/Octahedron Impostor"
{
	Properties
	{
		[NoScaleOffset]_Albedo("Albedo & Alpha", 2D) = "white" {}
		[NoScaleOffset]_Normals("Normals & Depth", 2D) = "white" {}
		[NoScaleOffset]_Specular("Specular & Smoothness", 2D) = "white" {}
		[NoScaleOffset]_Emission("Emission & Occlusion", 2D) = "white" {}
		[HideInInspector]_Frames("Frames", Float) = 16
		[HideInInspector]_ImpostorSize("Impostor Size", Float) = 1
		[HideInInspector]_Offset("Offset", Vector) = (0,0,0,0)
		_TextureBias("Texture Bias", Float) = -1
		_Parallax("Parallax", Range( -1 , 1)) = 1
		[HideInInspector]_DepthSize("DepthSize", Float) = 1
		_ClipMask("Clip", Range( 0 , 1)) = 0.5
		_ShadowBias("Shadow Bias", Range( 0 , 2)) = 0.25
		[Toggle(_HEMI_ON)] _Hemi("Hemi", Float) = 0
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
		float2 VectortoOctahedron( float3 N )
		{
			N /= dot( 1.0, abs(N) );
			if( N.z <= 0 )
			{
				N.xy = ( 1 - abs(N.yx) ) * ( N.xy >= 0 ? 1.0 : -1.0 );
			}
			return N.xy;
		}

		float2 VectortoHemiOctahedron( float3 N )
		{
			N.xy /= dot( 1.0, abs(N) );
			return float2( N.x + N.y, N.x - N.y );
		}

		float3 OctahedronToVector( float2 Oct )
		{
			float3 N = float3( Oct, 1.0 - dot( 1.0, abs(Oct) ) );
			if( N.z < 0 )
			{
				N.xy = ( 1 - abs(N.yx) ) * ( N.xy >= 0 ? 1.0 : -1.0 );
			}
			return normalize(N);
		}

		float3 HemiOctahedronToVector( float2 Oct )
		{
			Oct = float2( Oct.x + Oct.y, Oct.x - Oct.y ) *0.5;
			float3 N = float3( Oct, 1 - dot( 1.0, abs(Oct) ) );
			return normalize(N);
		}

		uniform float _Frames;
		uniform float _ImpostorSize;
		uniform float _Parallax;
		uniform sampler2D _Albedo;
		uniform sampler2D _Normals;
		uniform sampler2D _Specular;
		uniform sampler2D _Emission;
		uniform float _TextureBias;
		uniform float _ClipMask;
		uniform float _DepthSize;
		uniform float _ShadowBias;
		uniform float4 _Offset;

		#ifdef EFFECT_HUE_VARIATION
			half4 _HueVariation;
		#endif

		inline void OctaImpostorVertex( inout appdata_full v, inout float4 uvsFrame1, inout float4 uvsFrame2, inout float4 uvsFrame3, inout float4 octaFrame, inout float4 viewPos )
		{
			// Inputs
			float framesXY = _Frames;
			float prevFrame = framesXY - 1;
			float2 fractions = 1.0 / float2( framesXY, prevFrame );
			float fractionsFrame = fractions.x;
			float fractionsPrevFrame = fractions.y;
			float UVscale = _ImpostorSize;
			float parallax = -_Parallax; // check sign later

			// Basic data
			v.vertex.xyz += _Offset.xyz;
			float3 worldOrigin = float3(unity_ObjectToWorld[0].w, unity_ObjectToWorld[1].w, unity_ObjectToWorld[2].w);
			#if defined(UNITY_PASS_SHADOWCASTER)
				float3 worldCameraPos = 0;
				if( unity_LightShadowBias.y == 0 )
					worldCameraPos = _WorldSpaceCameraPos;
				else
					worldCameraPos = UnityWorldSpaceLightDir( mul(unity_ObjectToWorld, v.vertex).xyz ) * -5000.0;
			#else
				float3 worldCameraPos = _WorldSpaceCameraPos;
			#endif

			float3 objectCameraDirection = normalize( mul( (float3x3)unity_WorldToObject, worldCameraPos - worldOrigin ) - _Offset.xyz );
			float3 objectCameraPosition = mul( unity_WorldToObject, float4( worldCameraPos, 1 ) ).xyz - _Offset.xyz; //ray origin

			// Create orthogonal vectors to define the billboard
			float3 upVector = float3( 0,1,0 );
			float3 objectHorizontalVector = normalize( cross( objectCameraDirection, upVector ) );
			float3 objectVerticalVector = cross( objectHorizontalVector, objectCameraDirection );

			// Billboard
			float2 uvExpansion = ( v.texcoord.xy - 0.5f ) * framesXY * fractionsFrame * UVscale;
			float3 billboard = objectHorizontalVector * uvExpansion.x + objectVerticalVector * uvExpansion.y + _Offset.xyz;

			float3 localDir = billboard - objectCameraPosition - _Offset.xyz;

			// Octahedron Frame
			#ifdef _HEMI_ON
				objectCameraDirection.y = max(0.001, objectCameraDirection.y);
				float2 frameOcta = VectortoHemiOctahedron( objectCameraDirection.xzy ) * 0.5 + 0.5;
			#else
				float2 frameOcta = VectortoOctahedron( objectCameraDirection.xzy ) * 0.5 + 0.5;
			#endif

			// Setup for octahedron
			float2 prevOctaFrame = frameOcta * prevFrame;
			float2 baseOctaFrame = floor( prevOctaFrame );
			float2 fractionOctaFrame = ( baseOctaFrame * fractionsFrame );

			// Octa 1
			float2 octaFrame1 = ( baseOctaFrame * fractionsPrevFrame ) * 2.0 - 1.0;
			#ifdef _HEMI_ON
				float3 octa1WorldY = HemiOctahedronToVector( octaFrame1 ).xzy;
			#else
				float3 octa1WorldY = OctahedronToVector( octaFrame1 ).xzy;
			#endif
			float3 octa1WorldX = normalize( cross( upVector , octa1WorldY ) );
			float3 octa1WorldZ = cross( octa1WorldX , octa1WorldY );

			float dotY1 = dot( octa1WorldY , localDir );
			float3 octa1LocalY = normalize( float3( dot( octa1WorldX , localDir ), dotY1, dot( octa1WorldZ , localDir ) ) );

			float lineInter1 = dot( octa1WorldY , -objectCameraPosition ) / dotY1; //minus??
			float3 intersectPos1 = ( lineInter1 * localDir + objectCameraPosition ); // should subtract offset??

			float dotframeX1 = dot( octa1WorldX , -intersectPos1 );
			float dotframeZ1 = dot( octa1WorldZ , -intersectPos1 );

			float2 uvFrame1 = float2( dotframeX1 , dotframeZ1 );

			if( lineInter1 <= 0.0 )
				uvFrame1 = 0;

			float2 uvParallax1 = octa1LocalY.xz * fractionsFrame * parallax;
			uvFrame1 = ( ( uvFrame1 / UVscale ) + 0.5 ) * fractionsFrame + fractionOctaFrame;
			uvsFrame1 = float4( uvParallax1, uvFrame1);

			// Octa 2
			float2 fractPrevOctaFrame = frac( prevOctaFrame );
			float2 cornerDifference = lerp( float2( 0,1 ) , float2( 1,0 ) , saturate( ceil( ( fractPrevOctaFrame.x - fractPrevOctaFrame.y ) ) ));
			float2 octaFrame2 = ( ( baseOctaFrame + cornerDifference ) * fractionsPrevFrame ) * 2.0 - 1.0;
			#ifdef _HEMI_ON
				float3 octa2WorldY = HemiOctahedronToVector( octaFrame2 ).xzy;
			#else
				float3 octa2WorldY = OctahedronToVector( octaFrame2 ).xzy;
			#endif

			float3 octa2WorldX = normalize( cross( upVector , octa2WorldY ) );
			float3 octa2WorldZ = cross( octa2WorldX , octa2WorldY );

			float dotY2 = dot( octa2WorldY , localDir );
			float3 octa2LocalY = normalize( float3( dot( octa2WorldX , localDir ), dotY2, dot( octa2WorldZ , localDir ) ) );

			float lineInter2 = dot( octa2WorldY , -objectCameraPosition ) / dotY2; //minus??
			float3 intersectPos2 = ( lineInter2 * localDir + objectCameraPosition );

			float dotframeX2 = dot( octa2WorldX , -intersectPos2 );
			float dotframeZ2 = dot( octa2WorldZ , -intersectPos2 );

			float2 uvFrame2 = float2( dotframeX2 , dotframeZ2 );

			if( lineInter2 <= 0.0 )
				uvFrame2 = 0;

			float2 uvParallax2 = octa2LocalY.xz * fractionsFrame * parallax;
			uvFrame2 = ( ( uvFrame2 / UVscale ) + 0.5 ) * fractionsFrame + ( ( cornerDifference * fractionsFrame ) + fractionOctaFrame );
			uvsFrame2 = float4( uvParallax2, uvFrame2);


			// Octa 3
			float2 octaFrame3 = ( ( baseOctaFrame + 1 ) * fractionsPrevFrame  ) * 2.0 - 1.0;
			#ifdef _HEMI_ON
				float3 octa3WorldY = HemiOctahedronToVector( octaFrame3 ).xzy;
			#else
				float3 octa3WorldY = OctahedronToVector( octaFrame3 ).xzy;
			#endif

			float3 octa3WorldX = normalize( cross( upVector , octa3WorldY ) ); // check this later
			float3 octa3WorldZ = cross( octa3WorldX , octa3WorldY );

			float dotY3 = dot( octa3WorldY , localDir );
			float3 octa3LocalY = normalize( float3( dot( octa3WorldX , localDir ), dotY3, dot( octa3WorldZ , localDir ) ) );

			float lineInter3 = dot( octa3WorldY , -objectCameraPosition ) / dotY3; //minus??
			float3 intersectPos3 = ( lineInter3 * localDir + objectCameraPosition );

			float dotframeX3 = dot( octa3WorldX , -intersectPos3 );
			float dotframeZ3 = dot( octa3WorldZ , -intersectPos3 );

			float2 uvFrame3 = float2( dotframeX3 , dotframeZ3 );

			if( lineInter3 <= 0.0 )
				uvFrame3 = 0;

			float2 uvParallax3 = octa3LocalY.xz * fractionsFrame * parallax;
			uvFrame3 = ( ( uvFrame3 / UVscale ) + 0.5 ) * fractionsFrame + ( fractionOctaFrame + fractionsFrame );
			uvsFrame3 = float4( uvParallax3, uvFrame3);

			// maybe remove this?
			octaFrame = 0;
			octaFrame.xy = prevOctaFrame;

			// view pos
			viewPos = 0;
			viewPos.xyz = UnityObjectToViewPos( billboard );

			#ifdef EFFECT_HUE_VARIATION
				float hueVariationAmount = frac(unity_ObjectToWorld[0].w + unity_ObjectToWorld[1].w + unity_ObjectToWorld[2].w);
				viewPos.w = saturate(hueVariationAmount * _HueVariation.a);
			#endif

			v.vertex.xyz = billboard;
			v.normal.xyz = objectCameraDirection;
		}

		inline void OctaImpostorFragment( inout SurfaceOutputStandardSpecular o, out float4 clipPos, out float3 worldPos, float4 uvsFrame1, float4 uvsFrame2, float4 uvsFrame3, float4 octaFrame, float4 interpViewPos )
		{
			float depthBias = -1.0;
			float textureBias = _TextureBias;

			// Octa1
			float4 parallaxSample1 = tex2Dbias( _Normals, float4( uvsFrame1.zw, 0, depthBias) );
			float2 parallax1 = ( ( 0.5 - parallaxSample1.a ) * uvsFrame1.xy ) + uvsFrame1.zw;
			float4 albedo1 = tex2Dbias( _Albedo, float4( parallax1, 0, textureBias) );
			float4 normals1 = tex2Dbias( _Normals, float4( parallax1, 0, textureBias) );
			float4 mask1 = tex2Dbias( _Emission, float4( parallax1, 0, textureBias) );
			float4 spec1 = tex2Dbias( _Specular, float4( parallax1, 0, textureBias) );

			// Octa2
			float4 parallaxSample2 = tex2Dbias( _Normals, float4( uvsFrame2.zw, 0, depthBias) );
			float2 parallax2 = ( ( 0.5 - parallaxSample2.a ) * uvsFrame2.xy ) + uvsFrame2.zw;
			float4 albedo2 = tex2Dbias( _Albedo, float4( parallax2, 0, textureBias) );
			float4 normals2 = tex2Dbias( _Normals, float4( parallax2, 0, textureBias) );
			float4 mask2 = tex2Dbias( _Emission, float4( parallax2, 0, textureBias) );
			float4 spec2 = tex2Dbias( _Specular, float4( parallax2, 0, textureBias) );

			// Octa3
			float4 parallaxSample3 = tex2Dbias( _Normals, float4( uvsFrame3.zw, 0, depthBias) );
			float2 parallax3 = ( ( 0.5 - parallaxSample3.a ) * uvsFrame3.xy ) + uvsFrame3.zw;
			float4 albedo3 = tex2Dbias( _Albedo, float4( parallax3, 0, textureBias) );
			float4 normals3 = tex2Dbias( _Normals, float4( parallax3, 0, textureBias) );
			float4 mask3 = tex2Dbias( _Emission, float4( parallax3, 0, textureBias) );
			float4 spec3 = tex2Dbias( _Specular, float4( parallax3, 0, textureBias) );

			// Weights
			float2 fraction = frac( octaFrame.xy );
			float2 asd = 1 - fraction;
			float dota = dot( fraction , float2( 1,-1 ) );
			float3 weights = float3( min(asd.x, asd.y), abs(dota), min(fraction.x, fraction.y) );

			// Blends
			float4 blendedAlbedo = albedo1 * weights.x  + albedo2 * weights.y + albedo3 * weights.z;
			float4 blendedNormal = normals1 * weights.x  + normals2 * weights.y + normals3 * weights.z;
			float4 blendedMask = mask1 * weights.x  + mask2 * weights.y + mask3 * weights.z;
			float4 blendedSpec = spec1 * weights.x  + spec2 * weights.y + spec3 * weights.z;

			float3 localNormal = blendedNormal.rgb * 2.0 - 1.0;
			float3 worldNormal = normalize( mul( unity_ObjectToWorld, float4( localNormal, 0 ) ).xyz );

			float3 viewPos = interpViewPos.xyz;
			viewPos.z += ( ( parallaxSample1.a * weights.x + parallaxSample2.a * weights.y + parallaxSample3.a * weights.z ) * 2.0 - 1.0) * 0.5 * _DepthSize * length( unity_ObjectToWorld[2].xyz );
			#ifdef UNITY_PASS_SHADOWCASTER
				viewPos.z += -_ShadowBias * unity_LightShadowBias.y;
			#endif

			worldPos = mul( UNITY_MATRIX_I_V, float4( viewPos.xyz, 1 ) ).xyz;
			clipPos = mul( UNITY_MATRIX_P, float4( viewPos, 1 ) );
			#ifdef UNITY_PASS_SHADOWCASTER
				clipPos = UnityApplyLinearShadowBias( clipPos );
			#endif
			clipPos.xyz /= clipPos.w;
			if( UNITY_NEAR_CLIP_VALUE < 0 )
				clipPos = clipPos * 0.5 + 0.5;

			#ifdef EFFECT_HUE_VARIATION
				half3 shiftedColor = lerp(blendedAlbedo.rgb, _HueVariation.rgb, interpViewPos.w);
				half maxBase = max(blendedAlbedo.r, max(blendedAlbedo.g, blendedAlbedo.b));
				half newMaxBase = max(shiftedColor.r, max(shiftedColor.g, shiftedColor.b));
				maxBase /= newMaxBase;
				maxBase = maxBase * 0.5f + 0.5f;
				shiftedColor.rgb *= maxBase;
				blendedAlbedo.rgb = saturate(shiftedColor);
			#endif

			o.Albedo = blendedAlbedo.rgb;
			o.Normal = worldNormal;
			o.Emission = blendedMask.rgb;
			o.Specular = blendedSpec.rgb;
			o.Smoothness = blendedSpec.a;
			o.Occlusion = blendedMask.a;
			o.Alpha = ( blendedAlbedo.a - _ClipMask );
			clip( o.Alpha );
		}

		ENDCG
		Tags { "RenderType"="Opaque" "Queue"="Geometry" "DisableBatching"="True" }
		Cull Back

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

			#pragma shader_feature _HEMI_ON
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
				float4 uvsFrame1 : TEXCOORD5;
				float4 uvsFrame2 : TEXCOORD6;
				float4 uvsFrame3 : TEXCOORD7;
				float4 octaFrame : TEXCOORD8;
				float4 viewPos : TEXCOORD9;
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

				OctaImpostorVertex( v, o.uvsFrame1, o.uvsFrame2, o.uvsFrame3, o.octaFrame, o.viewPos );

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
				OctaImpostorFragment( o, clipPos, worldPos, IN.uvsFrame1, IN.uvsFrame2, IN.uvsFrame3, IN.octaFrame, IN.viewPos );

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
				#if UNITY_SPECCUBE_BLENDING || UNITY_SPECCUBE_BOX_PROJECTION
					giInput.boxMin[0] = unity_SpecCube0_BoxMin;
				#endif
				#if UNITY_SPECCUBE_BOX_PROJECTION
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

			#pragma shader_feature _HEMI_ON
			#pragma shader_feature EFFECT_HUE_VARIATION

			struct v2f_surf {
				UNITY_POSITION(pos);
				UNITY_SHADOW_COORDS(1)
				UNITY_FOG_COORDS(2)
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				float4 uvsFrame1 : TEXCOORD5;
				float4 uvsFrame2 : TEXCOORD6;
				float4 uvsFrame3 : TEXCOORD7;
				float4 octaFrame : TEXCOORD8;
				float4 viewPos : TEXCOORD9;
			};

			v2f_surf vert_surf (appdata_full v ) {
				UNITY_SETUP_INSTANCE_ID(v);
				v2f_surf o;
				UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				OctaImpostorVertex( v, o.uvsFrame1, o.uvsFrame2, o.uvsFrame3, o.octaFrame, o.viewPos );

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
				OctaImpostorFragment( o, clipPos, worldPos, IN.uvsFrame1, IN.uvsFrame2, IN.uvsFrame3, IN.octaFrame, IN.viewPos );

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

			#pragma shader_feature _HEMI_ON
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
				float4 uvsFrame1 : TEXCOORD5;
				float4 uvsFrame2 : TEXCOORD6;
				float4 uvsFrame3 : TEXCOORD7;
				float4 octaFrame : TEXCOORD8;
				float4 viewPos : TEXCOORD9;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f_surf vert_surf (appdata_full v ) {
				UNITY_SETUP_INSTANCE_ID(v);
				v2f_surf o;
				UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				OctaImpostorVertex( v, o.uvsFrame1, o.uvsFrame2, o.uvsFrame3, o.octaFrame, o.viewPos );

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
				OctaImpostorFragment( o, clipPos, worldPos, IN.uvsFrame1, IN.uvsFrame2, IN.uvsFrame3, IN.octaFrame, IN.viewPos );

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
			//#pragma multi_compile_instancing

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

			#pragma shader_feature _HEMI_ON
			#pragma shader_feature EFFECT_HUE_VARIATION

			struct v2f_surf {
				UNITY_POSITION(pos);
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				float4 uvsFrame1 : TEXCOORD5;
				float4 uvsFrame2 : TEXCOORD6;
				float4 uvsFrame3 : TEXCOORD7;
				float4 octaFrame : TEXCOORD8;
				float4 viewPos : TEXCOORD9;
			};

			v2f_surf vert_surf (appdata_full v ) {
				UNITY_SETUP_INSTANCE_ID(v);
				v2f_surf o;
				UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				OctaImpostorVertex( v, o.uvsFrame1, o.uvsFrame2, o.uvsFrame3, o.octaFrame, o.viewPos );

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
				OctaImpostorFragment( o, clipPos, worldPos, IN.uvsFrame1, IN.uvsFrame2, IN.uvsFrame3, IN.octaFrame, IN.viewPos );

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
			//#ifndef UNITY_PASS_SHADOWCASTER
			//#define UNITY_PASS_SHADOWCASTER
			//#endif
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#pragma multi_compile_instancing
			#include "HLSLSupport.cginc"
			#if !defined( UNITY_INSTANCED_LOD_FADE )
				#define UNITY_INSTANCED_LOD_FADE
			#endif
			#include "UnityShaderVariables.cginc"
			#include "UnityShaderUtilities.cginc"
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			#include "UnityStandardUtils.cginc"

			#pragma shader_feature _HEMI_ON
			#pragma shader_feature EFFECT_HUE_VARIATION

			struct v2f_surf {
				V2F_SHADOW_CASTER;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				float4 uvsFrame1 : TEXCOORD5;
				float4 uvsFrame2 : TEXCOORD6;
				float4 uvsFrame3 : TEXCOORD7;
				float4 octaFrame : TEXCOORD8;
				float4 viewPos : TEXCOORD9;
			};

			v2f_surf vert_surf (appdata_full v) {
				UNITY_SETUP_INSTANCE_ID(v);
				v2f_surf o;
				UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				OctaImpostorVertex( v, o.uvsFrame1, o.uvsFrame2, o.uvsFrame3, o.octaFrame, o.viewPos );

				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
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
				OctaImpostorFragment( o, clipPos, worldPos, IN.uvsFrame1, IN.uvsFrame2, IN.uvsFrame3, IN.octaFrame, IN.viewPos );

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
