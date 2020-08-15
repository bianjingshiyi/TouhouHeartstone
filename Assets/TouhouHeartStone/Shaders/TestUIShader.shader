// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "THH/TestUIShader"
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
		_Border("Border", Vector) = (0.1,0.1,0,0)
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
			uniform float2 _Border;
			
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
				float2 _Center = float2(0.5,0.5);
				float2 uv050_g4 = IN.texcoord.xy * float2( 1,1 ) + -_Center;
				float2 Pos7_g4 = abs( uv050_g4 );
				float2 BorderWidth4_g4 = _Border;
				float2 BorderVertex9_g4 = ( _Center - BorderWidth4_g4 );
				float clampResult116_g4 = clamp( ( distance( Pos7_g4 , BorderVertex9_g4 ) / (BorderWidth4_g4).x ) , 0.0 , 1.0 );
				float2 break17_g4 = Pos7_g4;
				float2 break21_g4 = BorderVertex9_g4;
				float XMask35_g4 = step( break17_g4.y , break21_g4.y );
				float2 break22_g4 = Pos7_g4;
				float2 break16_g4 = BorderVertex9_g4;
				float YMask31_g4 = step( break22_g4.x , break16_g4.x );
				float clampResult40_g4 = clamp( ( ( break17_g4.x - break21_g4.x ) / (BorderWidth4_g4).x ) , 0.0 , 1.0 );
				float clampResult39_g4 = clamp( ( ( break22_g4.y - break16_g4.y ) / (BorderWidth4_g4).y ) , 0.0 , 1.0 );
				float4 temp_cast_0 = (( ( ( 1.0 - clampResult116_g4 ) * ( 1.0 - XMask35_g4 ) * ( 1.0 - YMask31_g4 ) ) + ( XMask35_g4 * ( 1.0 - clampResult40_g4 ) ) + ( YMask31_g4 * ( 1.0 - clampResult39_g4 ) ) )).xxxx;
				
				half4 color = temp_cast_0;
				
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
821;222;1093;671;-1336.103;-181.1223;1.992131;True;False
Node;AmplifyShaderEditor.Vector2Node;2;2335.181,564.1911;Float;False;Property;_Border;Border;0;0;Create;True;0;0;False;0;0.1,0.1;0.1,0.1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.FunctionNode;107;2523.8,565.0187;Float;True;CreateTexBlurCircularRect;-1;;4;c832c8b2a92b1ac48849c9fe49e113a4;0;1;56;FLOAT2;0.2,0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;2806.462,568.2465;Float;False;True;2;Float;ASEMaterialInspector;0;4;THH/TestUIShader;5056123faa0c79b47ab6ad7e8bf059a4;True;Default;0;0;Default;2;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;True;2;False;-1;True;True;True;True;True;0;True;-9;True;True;0;True;-5;255;True;-8;255;True;-7;0;True;-4;0;True;-6;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;False;-1;True;0;False;-1;False;True;5;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;CanUseSpriteAtlas=True;False;0;False;False;False;False;False;False;False;False;False;False;True;2;0;;0;0;Standard;0;0;1;True;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;0
WireConnection;107;56;2;0
WireConnection;0;0;107;0
ASEEND*/
//CHKSM=B1432E09A967FF46171216E342AB971C82014FAB