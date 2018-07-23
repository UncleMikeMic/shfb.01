// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/LE_Brush"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Brush", 2D) = "white" {}
	}
	
	Subshader
	{
// UNITY 4 & 5
		Tags {"Queue"="Transparent"}
		Pass
		{
			ZWrite Off
			Fog { Color (0, 0, 0) }
			ColorMask RGB
			Blend SrcAlpha One
			Offset -1, -1

			CGPROGRAM
	        #include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
			
			struct v2f
			{
				float4 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};
			
			float4x4 unity_Projector;
			fixed4 _Color;
			
			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (vertex);
				o.uv = mul (unity_Projector, vertex);
				return o;
			}
			
			sampler2D _MainTex;
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 texS = tex2Dproj (_MainTex, UNITY_PROJ_COORD(i.uv));
				return texS * _Color;
			}
			ENDCG
		}

// UNITY 4
//		Pass
//		{
//			ZWrite off
//			Fog { Color (0, 0, 0) }
//			Color [_Color]
//			ColorMask RGB
//			Blend SrcAlpha One
//			Offset -1, -1
//			SetTexture [_MainTex]
//			{
//				combine texture * primary, texture * primary
//				Matrix [_Projector]
//        	}
//     	}
  	}
}