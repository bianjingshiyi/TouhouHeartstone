// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "THH/MasterSpark"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_Main("Main", 2D) = "white" {}
		_Gradient("Gradient", 2D) = "white" {}
		[HDR]_Emission("Emission", Color) = (1,1,1,1)
		_Reduce("Reduce", Float) = 5
		_DirectionalReduce("DirectionalReduce", Float) = 1
		_UVSpeed("UVSpeed", Vector) = (0,0,0,0)
		[Toggle]_InverseDirection("InverseDirection", Float) = 0
	}


	Category 
	{
		SubShader
		{
			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend One One
			ColorMask RGB
			Cull Off
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
			Pass {
			
				CGPROGRAM
				
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#include "UnityShaderVariables.cginc"


				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					float3 ase_normal : NORMAL;
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
					float4 ase_texcoord3 : TEXCOORD3;
					float4 ase_texcoord4 : TEXCOORD4;
					float3 ase_normal : NORMAL;
				};
				
				
				#if UNITY_VERSION >= 560
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				#else
				uniform sampler2D_float _CameraDepthTexture;
				#endif

				//Don't delete this comment
				// uniform sampler2D_float _CameraDepthTexture;

				uniform sampler2D _MainTex;
				uniform fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				uniform float _InvFade;
				uniform sampler2D _Main;
				uniform float4 _UVSpeed;
				uniform float4 _Main_ST;
				uniform sampler2D _Gradient;
				uniform float4 _Gradient_ST;
				uniform float4 _Emission;
				uniform float _Reduce;
				uniform float _InverseDirection;
				uniform float _DirectionalReduce;

				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					o.ase_texcoord3.xyz = ase_worldPos;
					float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
					o.ase_texcoord4.xyz = ase_worldNormal;
					
					o.ase_normal = v.ase_normal;
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord3.w = 0;
					o.ase_texcoord4.w = 0;

					v.vertex.xyz +=  float3( 0, 0, 0 ) ;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos (o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag ( v2f i  ) : SV_Target
				{
					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						float fade = saturate (_InvFade * (sceneZ-partZ));
						i.color.a *= fade;
					#endif

					float2 appendResult62 = (float2(_UVSpeed.x , _UVSpeed.y));
					float2 uv0_Main = i.texcoord.xy * _Main_ST.xy + _Main_ST.zw;
					float2 panner10 = ( 1.0 * _Time.y * appendResult62 + uv0_Main);
					float2 appendResult63 = (float2(_UVSpeed.z , _UVSpeed.w));
					float2 uv0_Gradient = i.texcoord.xy * _Gradient_ST.xy + _Gradient_ST.zw;
					float2 panner28 = ( 1.0 * _Time.y * appendResult63 + uv0_Gradient);
					float3 ase_worldPos = i.ase_texcoord3.xyz;
					float3 ase_worldViewDir = UnityWorldSpaceViewDir(ase_worldPos);
					ase_worldViewDir = normalize(ase_worldViewDir);
					float3 ase_worldNormal = i.ase_texcoord4.xyz;
					float dotResult21 = dot( ase_worldViewDir , ase_worldNormal );
					float4 transform67 = mul(unity_ObjectToWorld,float4( 0,1,0,0 ));
					float dotResult54 = dot( transform67 , float4( ase_worldNormal , 0.0 ) );
					float4 transform70 = mul(unity_ObjectToWorld,float4( 0,0,1,0 ));
					float dotResult71 = dot( transform70 , float4( i.ase_normal , 0.0 ) );
					float lerpResult69 = lerp( dotResult54 , dotResult71 , _InverseDirection);
					

					fixed4 col = ( tex2D( _Main, panner10 ) * tex2D( _Gradient, panner28 ) * i.color * _Emission * _TintColor * ( pow( saturate( dotResult21 ) , _Reduce ) * pow( saturate( lerpResult69 ) , _DirectionalReduce ) ) * i.color.a );
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=17000
1076;499;1628;724;2137.528;-1351.305;1.562586;True;True
Node;AmplifyShaderEditor.CommentaryNode;45;-1394.033,1216.431;Float;False;1424.853;1324.421;Alpha;18;74;70;66;54;71;67;68;69;58;30;56;52;60;59;51;21;41;20;;1,1,1,1;0;0
Node;AmplifyShaderEditor.NormalVertexDataNode;74;-1305.567,2302.417;Float;True;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ObjectToWorldTransfNode;70;-1264.873,2123.811;Float;False;1;0;FLOAT4;0,0,1,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldNormalVector;66;-1289.948,1902.915;Float;True;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ObjectToWorldTransfNode;67;-1262.607,1718.922;Float;False;1;0;FLOAT4;0,1,0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;71;-1048.916,2103.297;Float;True;2;0;FLOAT4;0,0,1,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;41;-1163.843,1491.96;Float;True;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;54;-1034.783,1777.799;Float;True;2;0;FLOAT4;0,0,1,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;20;-1202.195,1267.206;Float;True;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;68;-1031.777,2010.187;Float;False;Property;_InverseDirection;InverseDirection;6;1;[Toggle];Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;37;-1391.624,-202.1133;Float;False;1337.054;1280.2;Coloring;15;65;14;7;15;13;17;10;28;6;12;9;27;63;62;61;;1,1,1,1;0;0
Node;AmplifyShaderEditor.DotProductOpNode;21;-878.0433,1280.151;Float;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;69;-805.5193,1760.553;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;61;-1355.337,310.4013;Float;False;Property;_UVSpeed;UVSpeed;5;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;63;-1128.337,425.4013;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;9;-1282.66,136.9417;Float;False;0;6;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;27;-1252.752,568.3872;Float;False;0;12;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;62;-1120.337,312.4013;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SaturateNode;51;-662.4482,1297.835;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;52;-675.4213,1573.018;Float;False;Property;_Reduce;Reduce;3;0;Create;True;0;0;False;0;5;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;60;-703.3884,1667.635;Float;False;Property;_DirectionalReduce;DirectionalReduce;4;0;Create;True;0;0;False;0;1;1.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;59;-634.2731,1760.286;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;30;-435.8994,1290.818;Float;True;2;0;FLOAT;0;False;1;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;56;-420.5063,1752.225;Float;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;12;-979.9572,382.6469;Float;True;Property;_Gradient;Gradient;1;0;Create;True;0;0;False;0;5d6bbe2534e697f4d88a3627162c6afa;5d6bbe2534e697f4d88a3627162c6afa;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;6;-998.8142,-97.46268;Float;True;Property;_Main;Main;0;0;Create;True;0;0;False;0;663006ce24aca6745ac9a2dd7901ce15;dbec9039c70e09e47b8ff15d508f39ac;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.PannerNode;28;-957.8895,595.0733;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;10;-963.6615,136.9417;Float;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;65;-510.7924,897.2686;Float;False;0;0;_TintColor;Shader;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;-199.8,1483.382;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;13;-655.903,263.078;Float;True;Property;_TextureSample1;Texture Sample 1;3;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;17;-572.2782,720.7866;Float;False;Property;_Emission;Emission;2;1;[HDR];Create;True;0;0;False;0;1,1,1,1;2,2,2,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;15;-547.8815,500.833;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;7;-670.0842,-100.3838;Float;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-293.6487,242.9562;Float;True;7;7;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;FLOAT;0;False;6;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;463.4201,125.91;Float;False;True;2;Float;ASEMaterialInspector;0;7;THH/MasterSpark;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;4;1;False;-1;1;False;-1;0;1;False;-1;10;False;-1;False;False;True;2;False;-1;True;True;True;True;False;0;False;-1;False;True;2;False;-1;True;3;False;-1;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;0;False;False;False;False;False;False;False;False;False;False;True;0;0;;0;0;Standard;0;0;1;True;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;0
WireConnection;71;0;70;0
WireConnection;71;1;74;0
WireConnection;54;0;67;0
WireConnection;54;1;66;0
WireConnection;21;0;20;0
WireConnection;21;1;41;0
WireConnection;69;0;54;0
WireConnection;69;1;71;0
WireConnection;69;2;68;0
WireConnection;63;0;61;3
WireConnection;63;1;61;4
WireConnection;62;0;61;1
WireConnection;62;1;61;2
WireConnection;51;0;21;0
WireConnection;59;0;69;0
WireConnection;30;0;51;0
WireConnection;30;1;52;0
WireConnection;56;0;59;0
WireConnection;56;1;60;0
WireConnection;28;0;27;0
WireConnection;28;2;63;0
WireConnection;10;0;9;0
WireConnection;10;2;62;0
WireConnection;58;0;30;0
WireConnection;58;1;56;0
WireConnection;13;0;12;0
WireConnection;13;1;28;0
WireConnection;7;0;6;0
WireConnection;7;1;10;0
WireConnection;14;0;7;0
WireConnection;14;1;13;0
WireConnection;14;2;15;0
WireConnection;14;3;17;0
WireConnection;14;4;65;0
WireConnection;14;5;58;0
WireConnection;14;6;15;4
WireConnection;0;0;14;0
ASEEND*/
//CHKSM=0C94A9E2F3345AC2DACA6F497CD4AF80044BB106