// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "THH/Circle"
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
		_BlurRange("BlurRange", Range( 0 , 1)) = 0
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
			uniform float _BlurRange;
			
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
				float2 uv023 = IN.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_4_0_g20 = step( distance( uv023 , float2( 0.5,0.5 ) ) , 0.5 );
				float4 appendResult5_g20 = (float4(temp_output_4_0_g20 , temp_output_4_0_g20 , temp_output_4_0_g20 , temp_output_4_0_g20));
				float4 break14_g32 = appendResult5_g20;
				float temp_output_27_0_g32 = ( 1.0 - _BlurRange );
				float temp_output_28_0_g32 = ( ( 1.0 - ( distance( uv023 , float2( 0.5,0.5 ) ) / 0.5 ) ) + temp_output_27_0_g32 );
				float clampResult29_g32 = clamp( temp_output_28_0_g32 , 0.0 , 1.0 );
				float clampResult35_g32 = clamp( ( (0.0 + (clampResult29_g32 - temp_output_27_0_g32) * (1.0 - 0.0) / (1.0 - temp_output_27_0_g32)) + step( 1.0 , temp_output_28_0_g32 ) ) , 0.0 , 1.0 );
				float temp_output_16_0_g32 = ( break14_g32.a * clampResult35_g32 );
				float4 appendResult15_g32 = (float4(break14_g32.r , break14_g32.g , break14_g32.b , ( temp_output_16_0_g32 * temp_output_16_0_g32 )));
				
				half4 color = ( appendResult15_g32 * IN.color * _Color );
				
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
8;317;1395;756;2752.843;602.2988;2.12481;True;False
Node;AmplifyShaderEditor.CommentaryNode;19;-1642.128,-275.392;Float;False;976.9738;451.511;Base;5;17;16;9;23;56;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;23;-1563.8,-218.9595;Float;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;63;-1347.595,-367.4453;Float;False;Property;_BlurRange;BlurRange;0;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;56;-1320.159,-215.5915;Float;True;CreateTexCircle;-1;;20;09b09404724526f4196cf0e6daaa6d34;0;1;1;FLOAT2;0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;16;-1337.037,14.6801;Float;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;17;-1069.738,-39.27136;Float;False;0;0;_Color;Shader;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;69;-1098.576,-229.3596;Float;False;ColorBulr;-1;;32;146070b85406dbe41b9a1cc8aef18a4b;0;3;17;FLOAT;0.91;False;8;COLOR;1,1,1,1;False;1;FLOAT2;0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-885.2326,-216.8138;Float;True;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;36;617.213,-206.7634;Float;False;True;2;Float;ASEMaterialInspector;0;4;THH/Circle;5056123faa0c79b47ab6ad7e8bf059a4;True;Default;0;0;Default;2;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;True;2;False;-1;True;True;True;True;True;0;True;-9;True;True;0;True;-5;255;True;-8;255;True;-7;0;True;-4;0;True;-6;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;False;-1;True;0;False;-1;False;True;5;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;CanUseSpriteAtlas=True;False;0;False;False;False;False;False;False;False;False;False;False;True;2;0;;0;0;Standard;0;0;1;True;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;0
WireConnection;56;1;23;0
WireConnection;69;17;63;0
WireConnection;69;8;56;0
WireConnection;69;1;23;0
WireConnection;9;0;69;0
WireConnection;9;1;16;0
WireConnection;9;2;17;0
WireConnection;36;0;9;0
ASEEND*/
//CHKSM=F038D33B8AD483768CDD005CEED0B6399E244549