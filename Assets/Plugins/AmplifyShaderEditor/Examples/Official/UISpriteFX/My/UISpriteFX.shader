// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "My/UISpriteFX"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		_UVOffset_MoveDir("UVOffset_MoveDir", Vector) = (0,0,0,0)
		_UVOffset_Speed("UVOffset_Speed", Float) = 0
		_UVDistortion_Mask("UVDistortion_Mask", 2D) = "bump" {}
		_UVDistortion_Amount("UVDistortion_Amount", Vector) = (1,1,0,0)
		_UVMask_Mask("UVMask_Mask", 2D) = "white" {}
		_UVVortex_Force("UVVortex_Force", Float) = 0
		_UVVortex_Speed("UVVortex_Speed", Float) = 0
		_ColorBlend_BlendColor("ColorBlend_BlendColor", Color) = (1,1,1,1)
		[Enum(Additive,0,AdditiveWithoutAlpha,1,Multiply,2,MultiplyWithouAlpha,3,Min,4,Max,5)]_ColorBlend_Mode("ColorBlend_Mode", Int) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}

	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
		
		
		Pass
		{
		CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
			#include "UnityCG.cginc"
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
				float2 texcoord  : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				float4 ase_texcoord1 : TEXCOORD1;
			};
			
			uniform fixed4 _Color;
			uniform float _EnableExternalAlpha;
			uniform sampler2D _MainTex;
			uniform sampler2D _AlphaTex;
			uniform int _ColorBlend_Mode;
			uniform float _UVOffset_Speed;
			uniform float2 _UVOffset_MoveDir;
			uniform sampler2D _UVDistortion_Mask;
			uniform float4 _UVDistortion_Mask_ST;
			uniform float2 _UVDistortion_Amount;
			uniform float _UVVortex_Force;
			uniform float _UVVortex_Speed;
			uniform sampler2D _UVMask_Mask;
			uniform float4 _UVMask_Mask_ST;
			uniform float4 _ColorBlend_BlendColor;
			
			v2f vert( appdata_t IN  )
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
				float3 objectToViewPos = UnityObjectToViewPos(IN.vertex.xyz);
				float eyeDepth = -objectToViewPos.z;
				OUT.ase_texcoord1.x = eyeDepth;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				OUT.ase_texcoord1.yzw = 0;
				
				IN.vertex.xyz +=  float3(0,0,0) ; 
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

#if ETC1_EXTERNAL_ALPHA
				// get the color from an external texture (usecase: Alpha support for ETC1 on android)
				fixed4 alpha = tex2D (_AlphaTex, uv);
				color.a = lerp (color.a, alpha.r, _EnableExternalAlpha);
#endif //ETC1_EXTERNAL_ALPHA

				return color;
			}
			
			fixed4 frag(v2f IN  ) : SV_Target
			{
				int temp_output_8_0_g80 = _ColorBlend_Mode;
				float mulTime8_g65 = _Time.y * _UVOffset_Speed;
				float2 uv072 = IN.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner7_g65 = ( mulTime8_g65 * _UVOffset_MoveDir + uv072);
				float2 uv_UVDistortion_Mask = IN.texcoord.xy * _UVDistortion_Mask_ST.xy + _UVDistortion_Mask_ST.zw;
				float3 localUnpackNormal13 = UnpackNormal( tex2D( _UVDistortion_Mask, uv_UVDistortion_Mask ) );
				float2 appendResult16_g66 = (float2(localUnpackNormal13.x , localUnpackNormal13.y));
				float2 temp_output_3_0_g67 = ( ( panner7_g65 % float2( 1,1 ) ) + ( appendResult16_g66 * _UVDistortion_Amount ) );
				float2 temp_output_33_0_g67 = float2( 0.5,0.5 );
				float cos32_g67 = cos( ( distance( temp_output_3_0_g67 , temp_output_33_0_g67 ) * _UVVortex_Force ) );
				float sin32_g67 = sin( ( distance( temp_output_3_0_g67 , temp_output_33_0_g67 ) * _UVVortex_Force ) );
				float2 rotator32_g67 = mul( temp_output_3_0_g67 - temp_output_33_0_g67 , float2x2( cos32_g67 , -sin32_g67 , sin32_g67 , cos32_g67 )) + temp_output_33_0_g67;
				float mulTime40_g67 = _Time.y * _UVVortex_Speed;
				float cos39_g67 = cos( mulTime40_g67 );
				float sin39_g67 = sin( mulTime40_g67 );
				float2 rotator39_g67 = mul( rotator32_g67 - temp_output_33_0_g67 , float2x2( cos39_g67 , -sin39_g67 , sin39_g67 , cos39_g67 )) + temp_output_33_0_g67;
				float2 uv_UVMask_Mask = IN.texcoord.xy * _UVMask_Mask_ST.xy + _UVMask_Mask_ST.zw;
				float eyeDepth = IN.ase_texcoord1.x;
				float ES22_g71 = ( -1.0 / unity_FogParams.z );
				float End24_g71 = ( ES22_g71 * unity_FogParams.w );
				float Start23_g71 = ( End24_g71 - ES22_g71 );
				float clampResult3_g71 = clamp( (0.0 + (eyeDepth - Start23_g71) * (1.0 - 0.0) / (End24_g71 - Start23_g71)) , 0.0 , 1.0 );
				float4 lerpResult19_g71 = lerp( ( tex2D( _MainTex, ( rotator39_g67 * ( 1.0 - step( tex2D( _UVMask_Mask, uv_UVMask_Mask ).a , 0.0 ) ) ) ) * _Color ) , unity_FogColor , clampResult3_g71);
				float4 temp_output_1_0_g80 = lerpResult19_g71;
				float4 temp_output_4_0_g80 = _ColorBlend_BlendColor;
				float4 temp_output_16_0_g80 = (( (float)temp_output_8_0_g80 == 0.0 ) ? ( temp_output_1_0_g80 + temp_output_4_0_g80 ) :  temp_output_1_0_g80 );
				float4 temp_output_20_0_g80 = (( (float)temp_output_8_0_g80 == 1.0 ) ? ( temp_output_16_0_g80 + float4( (temp_output_4_0_g80).rgb , 0.0 ) ) :  temp_output_16_0_g80 );
				float4 temp_output_24_0_g80 = (( (float)temp_output_8_0_g80 == 2.0 ) ? ( temp_output_20_0_g80 * temp_output_4_0_g80 ) :  temp_output_20_0_g80 );
				float4 appendResult37_g80 = (float4((temp_output_4_0_g80).rgb , 1.0));
				float4 temp_output_27_0_g80 = (( (float)temp_output_8_0_g80 == 3.0 ) ? ( temp_output_24_0_g80 * appendResult37_g80 ) :  temp_output_24_0_g80 );
				float4 temp_output_32_0_g80 = (( (float)temp_output_8_0_g80 == 4.0 ) ? min( temp_output_27_0_g80 , temp_output_4_0_g80 ) :  temp_output_27_0_g80 );
				float4 temp_output_2_0_g81 = (( (float)temp_output_8_0_g80 == 5.0 ) ? max( temp_output_32_0_g80 , temp_output_4_0_g80 ) :  temp_output_32_0_g80 );
				float temp_output_6_0_g81 = max( max( (temp_output_2_0_g81).r , (temp_output_2_0_g81).g ) , (temp_output_2_0_g81).b );
				float4 appendResult7_g81 = (float4(temp_output_6_0_g81 , temp_output_6_0_g81 , temp_output_6_0_g81 , (temp_output_2_0_g81).a));
				
				fixed4 c = appendResult7_g81;
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=17000
91;558;1069;581;2318.173;111.5352;2.275297;True;False
Node;AmplifyShaderEditor.CommentaryNode;42;-3249.301,371.2279;Float;False;509.302;268.5091;UVOffset;3;45;44;96;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;67;-2706.683,368.4212;Float;False;521.8199;444.397;UVDistortion;3;62;78;80;;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector2Node;44;-3225.538,428.9354;Float;False;Property;_UVOffset_MoveDir;UVOffset_MoveDir;4;0;Create;True;0;0;False;0;0,0;0,-1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;72;-3497.415,371.2853;Float;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;45;-3214.757,554.3633;Float;False;Property;_UVOffset_Speed;UVOffset_Speed;5;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;62;-2674.964,472.3615;Float;True;Property;_UVDistortion_Mask;UVDistortion_Mask;6;0;Create;True;0;0;False;0;dd2fd2df93418444c8e280f1d34deeb5;dd2fd2df93418444c8e280f1d34deeb5;True;bump;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.FunctionNode;96;-2994.82,414.7219;Float;True;UVPanner;-1;;65;c00c1ac13f878f049b5bca0e356bf406;0;3;14;FLOAT2;0,0;False;18;FLOAT2;0,0;False;12;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;78;-2653.569,673.1474;Float;False;Property;_UVDistortion_Amount;UVDistortion_Amount;7;0;Create;True;0;0;False;0;1,1;1,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.CommentaryNode;88;-2153.428,372.4177;Float;False;479.9644;313.4833;Vortex;3;94;93;97;;1,1,1,1;0;0
Node;AmplifyShaderEditor.FunctionNode;80;-2432.571,412.4042;Float;True;UVDistortion;2;;66;0b46207477921624eb86bbe135d1ae9b;0;3;21;FLOAT2;0,0;False;12;SAMPLER2D;;False;29;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;94;-2107.24,569.5613;Float;False;Property;_UVVortex_Speed;UVVortex_Speed;10;0;Create;True;0;0;False;0;0;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;93;-2109.24,487.5615;Float;False;Property;_UVVortex_Force;UVVortex_Force;9;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;84;-1652.985,377.3036;Float;False;517.5752;310.0524;Mask;2;85;87;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TexturePropertyNode;85;-1617.406,480.5705;Float;True;Property;_UVMask_Mask;UVMask_Mask;8;0;Create;True;0;0;False;0;7b0842e3d0da6bf468f08b4a0ad9db9b;7b0842e3d0da6bf468f08b4a0ad9db9b;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.FunctionNode;97;-1870.99,419.3238;Float;False;UVVortex;-1;;67;ac61d1f1a75c8f949893b9f327f03dc1;0;4;3;FLOAT2;0,0;False;33;FLOAT2;0.5,0.5;False;38;FLOAT;1;False;41;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;10;-1303.341,250.651;Float;False;0;0;_MainTex;Shader;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;87;-1374.819,417.3185;Float;True;UVMask;0;;68;5d9733a763741cd4d8c166c2fb2c8299;0;2;3;FLOAT2;0,0;False;1;SAMPLER2D;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;15;-1089.673,249.9787;Float;False;508.4637;446.8318;Sprite;3;13;11;12;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;11;-950.7487,511.5646;Float;False;0;0;_Color;Shader;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;12;-1057.769,297.7584;Float;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;102;-550.2219,254.9642;Float;False;488.8;400;Fog;4;105;104;103;107;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-729.6656,303.3664;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;109;-32.27313,254.9384;Float;False;510.2064;316.884;Blend;2;111;110;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;110;-13.69531,319.4504;Float;False;Property;_ColorBlend_BlendColor;ColorBlend_BlendColor;14;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.IntNode;111;25.30469,491.4504;Float;False;Property;_ColorBlend_Mode;ColorBlend_Mode;15;1;[Enum];Create;True;6;Additive;0;AdditiveWithoutAlpha;1;Multiply;2;MultiplyWithouAlpha;3;Min;4;Max;5;0;False;0;0;2;0;1;INT;0
Node;AmplifyShaderEditor.FunctionNode;107;-278.8875,300.2674;Float;False;ColorFog;-1;;71;8f384f0cb329dcf46b997d6cedda6967;0;1;16;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;118;502.0144,257.4449;Float;False;231.9391;131.0716;BlackWhite;1;119;;1,1,1,1;0;0
Node;AmplifyShaderEditor.FunctionNode;122;247.5201,300.8739;Float;False;ColorBlend;-1;;80;a3dc1f070b4b5744db15b4eaf78e5e65;0;3;1;COLOR;1,1,1,1;False;4;COLOR;1,1,1,1;False;8;INT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;104;-487.6176,492.5429;Float;False;Property;_ColorFog_Near;ColorFog_Near;12;0;Create;True;0;0;False;0;5;50;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;103;-517.2255,320.507;Float;False;Property;_ColorFog_FogColor;ColorFog_FogColor;11;0;Create;True;0;0;False;0;1,0,0,1;1,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;105;-410.6176,572.5429;Float;False;Property;_ColorFog_Far;ColorFog_Far;13;0;Create;True;0;0;False;0;10;100;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;119;520.3835,308.2102;Float;False;ColorBlackWhite;-1;;81;9d72f2dcbdef0a140a60d78dcd029c31;0;1;2;COLOR;1,1,1,1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;9;760.5052,307.2175;Float;False;True;2;Float;ASEMaterialInspector;0;6;My/UISpriteFX;0f8ba0101102bb14ebf021ddadce9b49;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;3;1;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;True;2;False;-1;False;False;True;2;False;-1;False;False;True;5;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;CanUseSpriteAtlas=True;False;0;False;False;False;False;False;False;False;False;False;False;True;2;0;;0;0;Standard;0;0;1;True;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;0
WireConnection;96;14;72;0
WireConnection;96;18;44;0
WireConnection;96;12;45;0
WireConnection;80;21;96;0
WireConnection;80;12;62;0
WireConnection;80;29;78;0
WireConnection;97;3;80;0
WireConnection;97;38;93;0
WireConnection;97;41;94;0
WireConnection;87;3;97;0
WireConnection;87;1;85;0
WireConnection;12;0;10;0
WireConnection;12;1;87;0
WireConnection;13;0;12;0
WireConnection;13;1;11;0
WireConnection;107;16;13;0
WireConnection;122;1;107;0
WireConnection;122;4;110;0
WireConnection;122;8;111;0
WireConnection;119;2;122;0
WireConnection;9;0;119;0
ASEEND*/
//CHKSM=2E48D3DA8D0A6A2A06D5C291A10FD7E40B8083F0