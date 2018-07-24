// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "brickLevel1"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			half filler;
		};

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 _Color1 = float4(1,0,0,0);
			float3 objToWorld5 = mul( unity_ObjectToWorld, float4( float3( 0,0,0 ), 1 ) ).xyz;
			float4 lerpResult4 = lerp( float4(0,0,1,0) , _Color1 , (0.0 + (objToWorld5.x - -30.0) * (1.0 - 0.0) / (30.0 - -30.0)));
			float3 objToWorld8 = mul( unity_ObjectToWorld, float4( float3( 0,0,0 ), 1 ) ).xyz;
			float4 lerpResult7 = lerp( _Color1 , float4(0,1,0,0) , (0.0 + (objToWorld8.z - -30.0) * (1.0 - 0.0) / (30.0 - -30.0)));
			float3 objToWorld10 = mul( unity_ObjectToWorld, float4( float3( 0,0,0 ), 1 ) ).xyz;
			float4 lerpResult12 = lerp( lerpResult4 , lerpResult7 , (0.0 + (objToWorld10.y - 1.0) * (1.0 - 0.0) / (11.0 - 1.0)));
			o.Albedo = lerpResult12.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=15401
0;92;673;655;-612.804;80.33557;1;True;False
Node;AmplifyShaderEditor.TransformPositionNode;8;-689.3231,325.1144;Float;False;Object;World;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TransformPositionNode;5;-667.5697,-191.6979;Float;False;Object;World;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ColorNode;3;-422.0188,619.3718;Float;False;Constant;_Color2;Color 2;0;0;Create;True;0;0;False;0;0,1,0,0;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;6;-369.7592,-169.3619;Float;False;5;0;FLOAT;0;False;1;FLOAT;-30;False;2;FLOAT;30;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;2;-381.1509,70.6571;Float;False;Constant;_Color1;Color 1;0;0;Create;True;0;0;False;0;1,0,0,0;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;1;-385.646,-504.0349;Float;False;Constant;_Color0;Color 0;0;0;Create;True;0;0;False;0;0,0,1,0;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;9;-394.4319,291.9871;Float;False;5;0;FLOAT;0;False;1;FLOAT;-30;False;2;FLOAT;30;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TransformPositionNode;10;26.09661,158.6185;Float;False;Object;World;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;7;-15.30142,287.9381;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;4;29.80443,19.2523;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;11;320.9883,128.4016;Float;False;5;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;11;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;12;700.1179,121.4421;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1185.346,10.48331;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;brickLevel1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;-1;False;-1;-1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;6;0;5;1
WireConnection;9;0;8;3
WireConnection;7;0;2;0
WireConnection;7;1;3;0
WireConnection;7;2;9;0
WireConnection;4;0;1;0
WireConnection;4;1;2;0
WireConnection;4;2;6;0
WireConnection;11;0;10;2
WireConnection;12;0;4;0
WireConnection;12;1;7;0
WireConnection;12;2;11;0
WireConnection;0;0;12;0
ASEEND*/
//CHKSM=35EA2084BFDE4F815BFEAC8FB93FDA04F349C8B5