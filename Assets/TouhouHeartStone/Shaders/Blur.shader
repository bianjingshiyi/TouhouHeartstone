// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "THH/Blur"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
		_Offset("Offset", Float) = 0.01
	}

	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
		
		Stencil
		{
			Ref [_Stencil]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
			CompFront [_StencilComp]
			PassFront [_StencilOp]
			FailFront Keep
			ZFailFront Keep
			CompBack Always
			PassBack Keep
			FailBack Keep
			ZFailBack Keep
		}


		Cull Off
		Lighting Off
		ZWrite Off
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		
		Pass
		{
			Name "Default"
		CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile __ UNITY_UI_CLIP_RECT
			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			
			
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				
			};
			
			uniform fixed4 _Color;
			uniform fixed4 _TextureSampleAdd;
			uniform float4 _ClipRect;
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float _Offset;
			
			v2f vert( appdata_t IN  )
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID( IN );
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
				OUT.worldPosition = IN.vertex;
				
				
				OUT.worldPosition.xyz +=  float3( 0, 0, 0 ) ;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord = IN.texcoord;
				
				OUT.color = IN.color * _Color;
				return OUT;
			}

			fixed4 frag(v2f IN  ) : SV_Target
			{
				float2 uv0_MainTex = IN.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float x27 = uv0_MainTex.x;
				float offset26 = _Offset;
				float y28 = uv0_MainTex.y;
				float2 appendResult17 = (float2(( x27 - offset26 ) , ( y28 + offset26 )));
				float3x3 matrix95 = float3x3(0.05,0.1,0.05,0.1,0.4,0.1,0.05,0.1,0.05);
				float2 appendResult81 = (float2(x27 , ( y28 + offset26 )));
				float2 appendResult126 = (float2(( x27 + offset26 ) , ( y28 - offset26 )));
				float2 appendResult51 = (float2(( x27 - offset26 ) , y28));
				float2 appendResult69 = (float2(x27 , y28));
				float2 appendResult114 = (float2(( x27 + offset26 ) , y28));
				float2 appendResult61 = (float2(( x27 - offset26 ) , ( y28 - offset26 )));
				float2 appendResult90 = (float2(x27 , ( y28 - offset26 )));
				float2 appendResult134 = (float2(( x27 + offset26 ) , ( y28 - offset26 )));
				
				half4 color = ( ( ( tex2D( _MainTex, appendResult17 ) * matrix95[0].x ) + ( tex2D( _MainTex, appendResult81 ) * matrix95[1].x ) + ( tex2D( _MainTex, appendResult126 ) * matrix95[2].x ) + ( tex2D( _MainTex, appendResult51 ) * matrix95[0].y ) + ( tex2D( _MainTex, appendResult69 ) * matrix95[1].y ) + ( tex2D( _MainTex, appendResult114 ) * matrix95[2].y ) + ( tex2D( _MainTex, appendResult61 ) * matrix95[0].z ) + ( tex2D( _MainTex, appendResult90 ) * matrix95[1].z ) + ( tex2D( _MainTex, appendResult134 ) * matrix95[2].z ) ) * _Color * IN.color );
				
				#ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
				
				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif

				return color;
			}
		ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=17000
27;454;1395;756;2143.911;-1244.757;2.157559;True;False
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;3;-4460.354,1813.429;Float;False;0;0;_MainTex;Shader;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;11;-4211.843,2011.329;Float;False;Property;_Offset;Offset;0;0;Create;True;0;0;False;0;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;4;-4278.35,1876.404;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;13;-3760.987,1471.278;Float;False;951.1001;304.7;LeftTop;11;30;31;29;19;18;17;16;15;42;97;96;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;109;-1655.44,1883.159;Float;False;951.1001;304.7;Left;10;140;139;118;116;115;114;113;112;111;143;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;110;-1660.108,2264.364;Float;False;951.1001;304.7;LeftBottom;11;142;141;136;135;134;133;132;131;129;128;144;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;27;-4047.186,1823.18;Float;False;x;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;28;-4049.186,1911.18;Float;False;y;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;26;-4058.955,2007.181;Float;False;offset;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;83;-2718.52,2246.72;Float;False;951.1001;304.7;LeftTop;10;92;91;90;89;88;87;85;84;106;107;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;108;-1646.932,1488.113;Float;False;951.1001;304.7;LeftTop;11;138;137;127;126;125;124;123;122;121;120;119;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;64;-2705.344,1470.471;Float;False;951.1001;304.7;Top;10;82;81;80;79;78;77;75;74;102;103;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;53;-3774.163,2247.528;Float;False;951.1001;304.7;LeftBottom;11;62;61;60;59;58;56;55;54;63;100;101;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;43;-3769.495,1866.324;Float;False;951.1001;304.7;Left;10;52;51;50;49;48;46;45;44;98;99;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;116;-1605.692,2105.846;Float;False;26;offset;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;112;-1618.671,1937.404;Float;False;27;x;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Matrix3X3Node;93;-4320.402,2100.648;Float;False;Constant;_Matrix;Matrix;2;0;Create;True;0;0;False;0;0.05,0.1,0.05,0.1,0.4,0.1,0.05,0.1,0.05;0;1;FLOAT3x3;0
Node;AmplifyShaderEditor.GetLocalVarNode;30;-3719.24,1610.663;Float;False;28;y;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;59;-3737.416,2468.913;Float;False;26;offset;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;48;-3732.726,1920.569;Float;False;27;x;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;58;-3737.394,2301.772;Float;False;27;x;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;123;-1610.163,1542.358;Float;False;27;x;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;31;-3724.24,1692.663;Float;False;26;offset;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;49;-3719.747,2089.01;Float;False;26;offset;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;29;-3724.218,1525.522;Float;False;27;x;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;60;-3732.416,2386.913;Float;False;28;y;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;124;-1610.185,1709.499;Float;False;26;offset;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;89;-2676.773,2386.105;Float;False;28;y;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;131;-1623.339,2318.608;Float;False;27;x;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;132;-1623.361,2485.749;Float;False;26;offset;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;65;-2713.852,1865.517;Float;False;951.1001;304.7;Center;8;73;70;69;68;67;66;104;105;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;88;-2681.773,2468.105;Float;False;26;offset;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;133;-1618.361,2403.749;Float;False;28;y;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;79;-2668.597,1691.856;Float;False;26;offset;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;125;-1605.185,1627.499;Float;False;28;y;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;80;-2663.597,1609.856;Float;False;28;y;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;41;-4269.516,1738.24;Float;False;tex;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;95;-4050.616,2103.024;Float;False;matrix;-1;True;1;0;FLOAT3x3;0,0,0,0,1,0,0,0,1;False;1;FLOAT3x3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;144;-1436.773,2330.295;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;143;-1428.939,1965.397;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;63;-3537.111,2424.305;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;121;-1418.737,1673.033;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;122;-1421.364,1571.564;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;113;-1613.693,2022.546;Float;False;28;y;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;19;-3504.419,1642.728;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;136;-1423.056,2441.141;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;18;-3520.792,1537.198;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;56;-3533.968,2313.448;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;50;-3727.748,2005.71;Float;False;28;y;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;67;-2677.083,1919.762;Float;False;27;x;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;46;-3544.901,1928.345;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;78;-2668.575,1524.715;Float;False;27;x;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;77;-2448.776,1641.921;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;87;-2681.751,2300.964;Float;False;27;x;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;68;-2672.105,2004.903;Float;False;28;y;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;92;-2481.468,2423.497;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;105;-2094.028,2084.748;Float;False;95;matrix;1;0;OBJECT;0;False;1;FLOAT3x3;0
Node;AmplifyShaderEditor.GetLocalVarNode;107;-2107.832,2473.035;Float;False;95;matrix;1;0;OBJECT;0;False;1;FLOAT3x3;0
Node;AmplifyShaderEditor.DynamicAppendNode;90;-2337.174,2398.918;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;91;-2350.293,2308.159;Float;False;41;tex;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;102;-2091.593,1699.475;Float;False;95;matrix;1;0;OBJECT;0;False;1;FLOAT3x3;0
Node;AmplifyShaderEditor.DynamicAppendNode;134;-1278.762,2416.562;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;115;-1302.813,1938.098;Float;False;41;tex;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;70;-2361.225,1920.456;Float;False;41;tex;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;139;-1038.039,2113.612;Float;False;95;matrix;1;0;OBJECT;0;False;1;FLOAT3x3;0
Node;AmplifyShaderEditor.GetLocalVarNode;127;-1278.705,1549.552;Float;False;41;tex;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.DynamicAppendNode;81;-2323.998,1622.669;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;114;-1274.094,2035.358;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;135;-1291.881,2325.803;Float;False;41;tex;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;98;-3152.094,2096.776;Float;False;95;matrix;1;0;OBJECT;0;False;1;FLOAT3x3;0
Node;AmplifyShaderEditor.GetLocalVarNode;137;-1040.909,1715.151;Float;False;95;matrix;1;0;OBJECT;0;False;1;FLOAT3x3;0
Node;AmplifyShaderEditor.GetLocalVarNode;141;-1056.344,2491.948;Float;False;95;matrix;1;0;OBJECT;0;False;1;FLOAT3x3;0
Node;AmplifyShaderEditor.GetLocalVarNode;97;-3154.964,1698.316;Float;False;95;matrix;1;0;OBJECT;0;False;1;FLOAT3x3;0
Node;AmplifyShaderEditor.GetLocalVarNode;62;-3405.936,2308.967;Float;False;41;tex;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.DynamicAppendNode;61;-3392.817,2399.726;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;69;-2332.506,2017.715;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;82;-2337.117,1531.91;Float;False;41;tex;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;42;-3392.76,1532.717;Float;False;41;tex;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.DynamicAppendNode;17;-3379.641,1623.476;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;51;-3388.149,2018.522;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;52;-3416.868,1921.263;Float;False;41;tex;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;100;-3170.399,2475.112;Float;False;95;matrix;1;0;OBJECT;0;False;1;FLOAT3x3;0
Node;AmplifyShaderEditor.DynamicAppendNode;126;-1265.586,1640.312;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.VectorFromMatrixNode;140;-851.0131,2039.607;Float;False;Row;2;1;0;FLOAT3x3;1,0,0,0,0,1,1,0,1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;84;-2192.734,2288.15;Float;True;Property;_TextureSample9;Texture Sample 9;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;128;-1134.322,2305.794;Float;True;Property;_TextureSample12;Texture Sample 12;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;15;-3235.201,1512.708;Float;True;Property;_TextureSample1;Texture Sample 1;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;119;-1121.146,1529.544;Float;True;Property;_TextureSample11;Texture Sample 11;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;74;-2179.558,1511.901;Float;True;Property;_TextureSample8;Texture Sample 8;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VectorFromMatrixNode;103;-1904.567,1625.47;Float;False;Row;1;1;0;FLOAT3x3;1,0,0,1,0,0,1,0,1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VectorFromMatrixNode;138;-853.883,1641.147;Float;False;Row;2;1;0;FLOAT3x3;1,0,0,1,0,0,1,0,1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;111;-1129.655,1924.589;Float;True;Property;_TextureSample10;Texture Sample 10;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VectorFromMatrixNode;104;-1910.002,2008.743;Float;False;Row;1;1;0;FLOAT3x3;1,0,0,0,0,1,1,0,1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VectorFromMatrixNode;101;-2983.373,2401.107;Float;False;Row;0;1;0;FLOAT3x3;1,0,0,1,0,0,1,0,1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VectorFromMatrixNode;99;-2965.068,2022.771;Float;False;Row;0;1;0;FLOAT3x3;1,0,0,1,0,0,1,0,1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VectorFromMatrixNode;96;-2967.938,1624.311;Float;False;Row;0;1;0;FLOAT3x3;1,0,0,0,1,0,0,0,1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VectorFromMatrixNode;106;-1920.806,2399.03;Float;False;Row;1;1;0;FLOAT3x3;1,0,0,0,0,1,1,0,1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;44;-3243.71,1907.754;Float;True;Property;_TextureSample3;Texture Sample 3;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;54;-3248.377,2288.958;Float;True;Property;_TextureSample6;Texture Sample 6;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;66;-2188.067,1906.947;Float;True;Property;_TextureSample7;Texture Sample 7;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VectorFromMatrixNode;142;-869.3181,2417.943;Float;False;Row;2;1;0;FLOAT3x3;1,0,0,0,0,1,1,0,1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;120;-826.2299,1533.399;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.05;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;129;-839.4059,2309.649;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.05;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;75;-1884.642,1515.756;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-2940.285,1516.563;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.05;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;118;-834.739,1928.444;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;73;-1893.151,1910.802;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.4;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-2953.461,2292.813;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.05;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;-2948.794,1911.609;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;85;-1897.818,2292.005;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.05;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;14;-346.9648,1798.334;Float;False;9;9;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;7;-227.7211,2137.56;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;9;-379.036,2142.228;Float;False;0;0;_Color;Shader;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-72.37833,1913.387;Float;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;102.4532,1910.08;Float;False;True;2;Float;ASEMaterialInspector;0;4;THH/Blur;5056123faa0c79b47ab6ad7e8bf059a4;True;Default;0;0;Default;2;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;True;2;False;-1;True;True;True;True;True;0;True;-9;True;True;0;True;-5;255;True;-8;255;True;-7;0;True;-4;0;True;-6;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;False;-1;True;0;False;-1;False;True;5;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;CanUseSpriteAtlas=True;False;0;False;False;False;False;False;False;False;False;False;False;True;2;0;;0;0;Standard;0;0;1;True;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;0
WireConnection;4;2;3;0
WireConnection;27;0;4;1
WireConnection;28;0;4;2
WireConnection;26;0;11;0
WireConnection;41;0;3;0
WireConnection;95;0;93;0
WireConnection;144;0;131;0
WireConnection;144;1;132;0
WireConnection;143;0;112;0
WireConnection;143;1;116;0
WireConnection;63;0;60;0
WireConnection;63;1;59;0
WireConnection;121;0;125;0
WireConnection;121;1;124;0
WireConnection;122;0;123;0
WireConnection;122;1;124;0
WireConnection;19;0;30;0
WireConnection;19;1;31;0
WireConnection;136;0;133;0
WireConnection;136;1;132;0
WireConnection;18;0;29;0
WireConnection;18;1;31;0
WireConnection;56;0;58;0
WireConnection;56;1;59;0
WireConnection;46;0;48;0
WireConnection;46;1;49;0
WireConnection;77;0;80;0
WireConnection;77;1;79;0
WireConnection;92;0;89;0
WireConnection;92;1;88;0
WireConnection;90;0;87;0
WireConnection;90;1;92;0
WireConnection;134;0;144;0
WireConnection;134;1;136;0
WireConnection;81;0;78;0
WireConnection;81;1;77;0
WireConnection;114;0;143;0
WireConnection;114;1;113;0
WireConnection;61;0;56;0
WireConnection;61;1;63;0
WireConnection;69;0;67;0
WireConnection;69;1;68;0
WireConnection;17;0;18;0
WireConnection;17;1;19;0
WireConnection;51;0;46;0
WireConnection;51;1;50;0
WireConnection;126;0;122;0
WireConnection;126;1;121;0
WireConnection;140;0;139;0
WireConnection;84;0;91;0
WireConnection;84;1;90;0
WireConnection;128;0;135;0
WireConnection;128;1;134;0
WireConnection;15;0;42;0
WireConnection;15;1;17;0
WireConnection;119;0;127;0
WireConnection;119;1;126;0
WireConnection;74;0;82;0
WireConnection;74;1;81;0
WireConnection;103;0;102;0
WireConnection;138;0;137;0
WireConnection;111;0;115;0
WireConnection;111;1;114;0
WireConnection;104;0;105;0
WireConnection;101;0;100;0
WireConnection;99;0;98;0
WireConnection;96;0;97;0
WireConnection;106;0;107;0
WireConnection;44;0;52;0
WireConnection;44;1;51;0
WireConnection;54;0;62;0
WireConnection;54;1;61;0
WireConnection;66;0;70;0
WireConnection;66;1;69;0
WireConnection;142;0;141;0
WireConnection;120;0;119;0
WireConnection;120;1;138;1
WireConnection;129;0;128;0
WireConnection;129;1;142;3
WireConnection;75;0;74;0
WireConnection;75;1;103;1
WireConnection;16;0;15;0
WireConnection;16;1;96;1
WireConnection;118;0;111;0
WireConnection;118;1;140;2
WireConnection;73;0;66;0
WireConnection;73;1;104;2
WireConnection;55;0;54;0
WireConnection;55;1;101;3
WireConnection;45;0;44;0
WireConnection;45;1;99;2
WireConnection;85;0;84;0
WireConnection;85;1;106;3
WireConnection;14;0;16;0
WireConnection;14;1;75;0
WireConnection;14;2;120;0
WireConnection;14;3;45;0
WireConnection;14;4;73;0
WireConnection;14;5;118;0
WireConnection;14;6;55;0
WireConnection;14;7;85;0
WireConnection;14;8;129;0
WireConnection;8;0;14;0
WireConnection;8;1;9;0
WireConnection;8;2;7;0
WireConnection;0;0;8;0
ASEEND*/
//CHKSM=21BC895909C3F2178D9E573653A3D00E669FDF27