// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/LE_SnapToObjectUIShader"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		//ZWrite Off
		//ZTest Always
		Fog { Color (0, 0, 0) }
		ColorMask RGB
		Offset 10, 10
		
		Pass
		{
			CGPROGRAM
	        #include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
			
			struct v2f
			{
				float4 pos : SV_POSITION;
			};
			
			fixed4 _Color;
			
			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return _Color;
			}
			ENDCG
		}
	}
}
