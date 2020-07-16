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
		_BlurRadius("BlurRadius", Float) = 3
		_LerpOffset("LerpOffset", Vector) = (0,0,0,0)
		_LerpDirection("LerpDirection", Vector) = (0,0,0,0)
		_LerpPow("LerpPow", Float) = 0
		_BlackWhite("BlackWhite", Range( 0 , 1)) = 0
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
			uniform float _BlurRadius;
			uniform float4 _MainTex_TexelSize;
			uniform float2 _LerpOffset;
			uniform float2 _LerpDirection;
			uniform float _LerpPow;
			uniform float _BlackWhite;
			float4 GaussBlur4( float2 UV , float BlurRadius , float4 TexelSize )
			{
				float sigma = BlurRadius / 3.0;
				float4 col = float4(0, 0, 0, 0);
				for (int x = - BlurRadius; x <= BlurRadius; ++x)
				{
				    for (int y = - BlurRadius; y <= BlurRadius; ++y)
				    {
				        float4 color = tex2D(_MainTex, UV + float2(x * TexelSize.x, y * TexelSize.y));
				        float sigma2 = pow(sigma, 2.0f);
				        float left = 1 / (2 * sigma2 * 3.1415926f);
				        float right = exp(-(x*x+y*y)/(2*sigma2));
				        float weight = left * right;
				        col += color * weight;
				    }
				}
				return col;
			}
			
			
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
				float2 UV4 = uv0_MainTex;
				float BlurRadius4 = _BlurRadius;
				float4 TexelSize4 = _MainTex_TexelSize;
				float4 localGaussBlur4 = GaussBlur4( UV4 , BlurRadius4 , TexelSize4 );
				float2 normalizeResult61_g5 = normalize( _LerpDirection );
				float dotResult15_g5 = dot( ( uv0_MainTex + _LerpOffset ) , normalizeResult61_g5 );
				float4 lerpResult20_g5 = lerp( float4( 0,0,0,0 ) , float4( 1,1,1,1 ) , pow( dotResult15_g5 , _LerpPow ));
				float4 clampResult63_g5 = clamp( ( lerpResult20_g5 * float4( 1,1,1,1 ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				float4 lerpResult13 = lerp( localGaussBlur4 , tex2D( _MainTex, uv0_MainTex ) , clampResult63_g5);
				float4 temp_output_2_0_g9 = lerpResult13;
				float temp_output_6_0_g9 = max( max( (temp_output_2_0_g9).r , (temp_output_2_0_g9).g ) , (temp_output_2_0_g9).b );
				float4 appendResult7_g9 = (float4(temp_output_6_0_g9 , temp_output_6_0_g9 , temp_output_6_0_g9 , (temp_output_2_0_g9).a));
				float4 lerpResult30 = lerp( lerpResult13 , appendResult7_g9 , _BlackWhite);
				
				half4 color = ( _Color * lerpResult30 * IN.color );
				
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
143;384;1285;732;795.5815;712.3377;1.604851;True;False
Node;AmplifyShaderEditor.CommentaryNode;11;-1140.537,-163.9033;Float;False;759.7766;596.4423;Blur;5;4;6;5;2;1;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;21;-251.2752,-482.203;Float;False;805.7698;592.8358;BulrLerp;5;20;19;18;13;23;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;1;-1125.426,101.981;Float;True;0;0;_MainTex;Shader;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;6;-861.5546,-28.59505;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;20;-204.4956,-28.46183;Float;False;Property;_LerpPow;LerpPow;3;0;Create;True;0;0;False;0;0;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;14;-777.7402,-486.2733;Float;False;397;294;Base;1;12;;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector2Node;18;-209.2578,-353.2095;Float;False;Property;_LerpOffset;LerpOffset;1;0;Create;True;0;0;False;0;0,0;0,0.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;2;-823.2628,119.199;Float;False;Property;_BlurRadius;BlurRadius;0;0;Create;True;0;0;False;0;3;20;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexelSizeNode;5;-842.7303,237.7296;Float;False;-1;1;0;SAMPLER2D;_Sampler05;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;19;-214.407,-196.9344;Float;False;Property;_LerpDirection;LerpDirection;2;0;Create;True;0;0;False;0;0,0;0,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.CustomExpressionNode;4;-609.4324,100.8302;Float;False;float sigma = BlurRadius / 3.0@$float4 col = float4(0, 0, 0, 0)@$for (int x = - BlurRadius@ x <= BlurRadius@ ++x)${$    for (int y = - BlurRadius@ y <= BlurRadius@ ++y)$    {$        float4 color = tex2D(_MainTex, UV + float2(x * TexelSize.x, y * TexelSize.y))@$        float sigma2 = pow(sigma, 2.0f)@$        float left = 1 / (2 * sigma2 * 3.1415926f)@$        float right = exp(-(x*x+y*y)/(2*sigma2))@$        float weight = left * right@$        col += color * weight@$    }$}$return col@;4;False;3;True;UV;FLOAT2;0,0;In;;Float;False;True;BlurRadius;FLOAT;3;In;;Float;False;True;TexelSize;FLOAT4;0,0,0,0;In;;Float;False;GaussBlur;True;False;1;145;3;0;FLOAT2;0,0;False;1;FLOAT;3;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;23;5.339761,-334.1485;Float;True;ColorGradient;-1;;5;7164b3169adbd02439a443ddbbc79a71;0;7;3;COLOR;1,1,1,1;False;6;FLOAT2;0,0;False;22;COLOR;0,0,0,0;False;23;COLOR;1,1,1,1;False;58;FLOAT2;0,0.35;False;13;FLOAT2;0,1;False;56;FLOAT;7.03;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;12;-699.1429,-418.1136;Float;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;13;302.5374,-332.5739;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0.7803922;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;31;675.2725,-303.5525;Float;False;ColorBlackWhite;-1;;9;e7ac3cb4fee77a54fad4dbae8b9735cb;0;1;2;COLOR;1,1,1,1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;27;648.3007,-208.4976;Float;False;Property;_BlackWhite;BlackWhite;4;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;30;923.275,-344.9123;Float;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;7;1439.119,-526.4244;Float;False;0;0;_Color;Shader;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;8;1450.052,-157.0411;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;1650.638,-349.1315;Float;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;1870.337,-409.8091;Float;False;True;2;Float;ASEMaterialInspector;0;4;THH/Blur;5056123faa0c79b47ab6ad7e8bf059a4;True;Default;0;0;Default;2;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;True;2;False;-1;True;True;True;True;True;0;True;-9;True;True;0;True;-5;255;True;-8;255;True;-7;0;True;-4;0;True;-6;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;False;-1;True;0;False;-1;False;True;5;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;CanUseSpriteAtlas=True;False;0;False;False;False;False;False;False;False;False;False;False;True;2;0;;0;0;Standard;0;0;1;True;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;0
WireConnection;6;2;1;0
WireConnection;5;0;1;0
WireConnection;4;0;6;0
WireConnection;4;1;2;0
WireConnection;4;2;5;0
WireConnection;23;6;6;0
WireConnection;23;58;18;0
WireConnection;23;13;19;0
WireConnection;23;56;20;0
WireConnection;12;0;1;0
WireConnection;12;1;6;0
WireConnection;13;0;4;0
WireConnection;13;1;12;0
WireConnection;13;2;23;0
WireConnection;31;2;13;0
WireConnection;30;0;13;0
WireConnection;30;1;31;0
WireConnection;30;2;27;0
WireConnection;10;0;7;0
WireConnection;10;1;30;0
WireConnection;10;2;8;0
WireConnection;0;0;10;0
ASEEND*/
//CHKSM=02928FC34769233A1030C707314770281949D1C1