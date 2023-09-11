Shader "ERB/Particles/Blend_Tornado"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		[Toggle]_UpCutoff("Up Cutoff", Float) = 0
		[HDR]_Color("Color", Color) = (1,0.6,0,1)
		_SpeedXYFresnelEmission("Speed XY + Fresnel + Emission", Vector) = (-0.3,-0.7,2,2)
		[Toggle]_Fresnel("Fresnel", Float) = 0
		[HDR]_Fresnelcolor("Fresnel color", Color) = (1,1,1,1)
		_Numberofwaves("Number of waves", Float) = 1
		_WavesspeedsizeXYTwistspeedsizeZW("Waves speed-size XY Twist speed-size ZW", Vector) = (-1,0.2,4,0.6)
		[HideInInspector] _tex3coord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha noshadow vertex:vertexDataFunc 
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float4 vertexColor : COLOR;
			float2 uv_texcoord;
			float3 uv_tex3coord;
			float3 worldPos;
			float3 worldNormal;
			half ASEVFace : VFACE;
		};

		uniform float _Numberofwaves;
		uniform float4 _WavesspeedsizeXYTwistspeedsizeZW;
		uniform float _Fresnel;
		uniform float4 _Color;
		uniform sampler2D _MainTex;
		uniform float4 _SpeedXYFresnelEmission;
		uniform float4 _MainTex_ST;
		uniform float4 _Fresnelcolor;
		uniform float _UpCutoff;
		uniform float _Cutoff = 0.5;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertexNormal = v.normal.xyz;
			float V162 = v.texcoord.xyz.y;
			float3 ase_vertex3Pos = v.vertex.xyz;
			float mulTime136 = _Time.y * _WavesspeedsizeXYTwistspeedsizeZW.x;
			float mulTime194 = _Time.y * _WavesspeedsizeXYTwistspeedsizeZW.z;
			float temp_output_186_0 = ( V162 * 1.0 );
			float3 appendResult215 = (float3(( sin( ( ase_vertex3Pos.y + mulTime194 ) ) * temp_output_186_0 ) , 0.0 , ( temp_output_186_0 * sin( ( mulTime194 + ase_vertex3Pos.y + ( UNITY_PI / 2.0 ) ) ) )));
			v.vertex.xyz += ( ( ase_vertexNormal * ( V162 * sin( ( _Numberofwaves * ( ase_vertex3Pos.y + mulTime136 ) * UNITY_PI ) ) ) * _WavesspeedsizeXYTwistspeedsizeZW.y ) + ( _WavesspeedsizeXYTwistspeedsizeZW.w * appendResult215 ) );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 appendResult105 = (float2(_SpeedXYFresnelEmission.x , _SpeedXYFresnelEmission.y));
			float2 uv0_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float2 panner102 = ( 1.0 * _Time.y * appendResult105 + ( uv0_MainTex + i.uv_tex3coord.z ));
			float4 tex2DNode13 = tex2D( _MainTex, panner102 );
			float3 temp_output_111_0 = (( _Color * i.vertexColor * tex2DNode13 * _SpeedXYFresnelEmission.w )).rgb;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV92 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode92 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV92, _SpeedXYFresnelEmission.z ) );
			float3 clampResult116 = clamp( ( fresnelNode92 * fresnelNode92 * (_Fresnelcolor).rgb ) , float3( 0,0,0 ) , float3( 1,1,1 ) );
			float3 switchResult126 = (((i.ASEVFace>0)?(clampResult116):(float3( 0,0,0 ))));
			o.Emission = lerp(temp_output_111_0,( temp_output_111_0 + switchResult126 ),_Fresnel);
			o.Alpha = 1;
			float V162 = i.uv_tex3coord.y;
			float clampResult131 = clamp( pow( V162 , 20.0 ) , 0.0 , 1.0 );
			float clampResult129 = clamp( ( i.vertexColor.a - clampResult131 ) , 0.0 , 1.0 );
			clip( ( tex2DNode13.r + (-0.5 + (lerp(i.vertexColor.a,clampResult129,_UpCutoff) - 0.0) * (0.5 - -0.5) / (1.0 - 0.0)) ) - _Cutoff );
		}
		ENDCG
	}
}