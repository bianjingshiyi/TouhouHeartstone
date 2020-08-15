// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "THH/UIAddMoveTex"
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
		_SubTex("SubTex", 2D) = "white" {}
		_SubTexColor("SubTexColor", Color) = (0.3254902,1,0.6907774,1)
		_Speed("Speed", Vector) = (0,0,0,0)
		_Border("Border", Vector) = (0.05,0.05,0,0)
		_AlphaRemap("AlphaRemap", Vector) = (0,0,0,0)
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
			
			#include "UnityShaderVariables.cginc"

			
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
			uniform float4 _SubTexColor;
			uniform sampler2D _SubTex;
			uniform float2 _Speed;
			uniform float4 _SubTex_ST;
			uniform float2 _Border;
			uniform float2 _AlphaRemap;
			
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
				float2 uv0_SubTex = IN.texcoord.xy * _SubTex_ST.xy + _SubTex_ST.zw;
				float2 panner19 = ( _Time.y * _Speed + uv0_SubTex);
				float2 _Center = float2(0.5,0.5);
				float2 uv050_g2 = IN.texcoord.xy * float2( 1,1 ) + -_Center;
				float2 Pos7_g2 = abs( uv050_g2 );
				float2 BorderWidth4_g2 = _Border;
				float2 BorderVertex9_g2 = ( _Center - BorderWidth4_g2 );
				float clampResult116_g2 = clamp( ( distance( Pos7_g2 , BorderVertex9_g2 ) / (BorderWidth4_g2).x ) , 0.0 , 1.0 );
				float2 break17_g2 = Pos7_g2;
				float2 break21_g2 = BorderVertex9_g2;
				float XMask35_g2 = step( break17_g2.y , break21_g2.y );
				float2 break22_g2 = Pos7_g2;
				float2 break16_g2 = BorderVertex9_g2;
				float YMask31_g2 = step( break22_g2.x , break16_g2.x );
				float clampResult40_g2 = clamp( ( ( break17_g2.x - break21_g2.x ) / (BorderWidth4_g2).x ) , 0.0 , 1.0 );
				float clampResult39_g2 = clamp( ( ( break22_g2.y - break16_g2.y ) / (BorderWidth4_g2).y ) , 0.0 , 1.0 );
				float clampResult123_g2 = clamp( ( ( ( 1.0 - clampResult116_g2 ) * ( 1.0 - XMask35_g2 ) * ( 1.0 - YMask31_g2 ) ) + ( XMask35_g2 * ( 1.0 - clampResult40_g2 ) ) + ( YMask31_g2 * ( 1.0 - clampResult39_g2 ) ) ) , 0.0 , 1.0 );
				float4 appendResult28 = (float4(_SubTexColor.r , _SubTexColor.g , _SubTexColor.b , ( tex2D( _SubTex, panner19 ).r * clampResult123_g2 * (_AlphaRemap.x + (_SinTime.w - -1.0) * (_AlphaRemap.y - _AlphaRemap.x) / (1.0 - -1.0)) * _SubTexColor.a )));
				
				half4 color = appendResult28;
				
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
149;654;1093;671;1581.23;-341.8702;1;True;False
Node;AmplifyShaderEditor.TexturePropertyNode;9;-1572.766,124.4728;Float;True;Property;_SubTex;SubTex;0;0;Create;True;0;0;False;0;cd460ee4ac5c1e746b7a734cc7cc64dd;cd460ee4ac5c1e746b7a734cc7cc64dd;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SimpleTimeNode;21;-1297.299,564.0749;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;18;-1302.562,208.3526;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;20;-1309.158,404.3214;Float;False;Property;_Speed;Speed;2;0;Create;True;0;0;False;0;0,0;0,-0.2;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.PannerNode;19;-1102.187,413.4479;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SinTimeNode;29;-886.791,408.2587;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;27;-1115.122,599.6713;Float;False;Property;_Border;Border;3;0;Create;True;0;0;False;0;0.05,0.05;0.05,0.05;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;31;-921.7578,645.5727;Float;False;Property;_AlphaRemap;AlphaRemap;4;0;Create;True;0;0;False;0;0,0;0.6,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SamplerNode;8;-1016.449,186.119;Float;True;Property;_TextureSample1;Texture Sample 1;0;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;11;-784.923,5.435944;Float;False;Property;_SubTexColor;SubTexColor;1;0;Create;True;0;0;False;0;0.3254902,1,0.6907774,1;0.1921568,0.8,0.5914012,0.7882353;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;30;-694.486,380.9291;Float;False;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;0.75;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;26;-913.8162,581.5854;Float;False;CreateTexBlurCircularRect;-1;;2;c832c8b2a92b1ac48849c9fe49e113a4;0;1;56;FLOAT2;0.05,0.05;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-499.6897,230.9005;Float;False;4;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;-800.348,-219.752;Float;True;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;3;-1123.733,-346.2289;Float;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;False;0;None;6fd09abd3f0c1fd4fb338afd5dd8cffc;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;5;-1396.095,-409.9984;Float;True;0;0;_MainTex;Shader;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;2;-1106.488,-155.9565;Float;False;0;0;_Color;Shader;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexCoordVertexDataNode;6;-1359.431,-164.2637;Float;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;15;-290.1293,-123.4684;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;1;-973.0512,-87.63727;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;28;-560.3196,36.27527;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;0,0;Float;False;True;2;Float;ASEMaterialInspector;0;4;THH/UIAddMoveTex;5056123faa0c79b47ab6ad7e8bf059a4;True;Default;0;0;Default;2;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;True;2;False;-1;True;True;True;True;True;0;True;-9;True;True;0;True;-5;255;True;-8;255;True;-7;0;True;-4;0;True;-6;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;False;-1;True;0;False;-1;False;True;5;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;CanUseSpriteAtlas=True;False;0;False;False;False;False;False;False;False;False;False;False;True;2;0;;0;0;Standard;0;0;1;True;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;0
WireConnection;18;2;9;0
WireConnection;19;0;18;0
WireConnection;19;2;20;0
WireConnection;19;1;21;0
WireConnection;8;0;9;0
WireConnection;8;1;19;0
WireConnection;30;0;29;4
WireConnection;30;3;31;1
WireConnection;30;4;31;2
WireConnection;26;56;27;0
WireConnection;25;0;8;1
WireConnection;25;1;26;0
WireConnection;25;2;30;0
WireConnection;25;3;11;4
WireConnection;4;0;3;0
WireConnection;4;1;2;0
WireConnection;4;2;1;0
WireConnection;3;0;5;0
WireConnection;3;1;6;0
WireConnection;15;0;4;0
WireConnection;15;1;28;0
WireConnection;28;0;11;1
WireConnection;28;1;11;2
WireConnection;28;2;11;3
WireConnection;28;3;25;0
WireConnection;0;0;28;0
ASEEND*/
//CHKSM=E1FE0D2D031E5A53D2F52886800FA5651E627A3F