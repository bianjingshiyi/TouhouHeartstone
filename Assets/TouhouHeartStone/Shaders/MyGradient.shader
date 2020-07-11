Shader "THH/MyGradient"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RangeX ("RangeX", Range(0, 0.5)) = 0.4
        _RangeY ("RangeY", Range(0, 0.5)) = 0.4
        _Magnitude ("Alpha Magnitude", Float) = 1
        _Color ("Color", Color) = (0,0,0,0)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags 
        { 
        	"Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp] 
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        ColorMask [_ColorMask]
		

        LOD 100

        Pass
        {
        	Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZTest Always

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            CBUFFER_START(UnityPerMaterial)
                half _RangeX;
                half _RangeY;
                half _Magnitude;
                half4 _Color;
            CBUFFER_END


            half FillterColorAlpha(half2 dir)
            {
                half uvX = abs(dir.x);
                half uvY = abs(dir.y);
                half maxX = 0.5 - _RangeX;
                half maxY = 0.5 - _RangeY;
                half2 a = half2(clamp(0.5 - uvX, 0, maxX), clamp(0.5 - uvY, 0, maxY));
                a = abs(a - half2(maxX, maxY));
                half dis = length(a);
                return clamp(1 - dis, 0, max(maxX, maxY)) * dis * _Magnitude;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half2 uv = i.uv;
                half4 col = tex2D(_MainTex, uv);
                half2 center = half2(0.5, 0.5);
                half2 dir = uv - center;
                
                return half4(_Color.xyz, FillterColorAlpha(dir));

                
            }
            ENDCG
        }
    }
}
