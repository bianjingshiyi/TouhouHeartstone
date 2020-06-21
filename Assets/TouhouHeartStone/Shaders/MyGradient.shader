Shader "THH/MyGradient"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RangeX ("RangeX", Range(0, 0.5)) = 0.4
        _RangeY ("RangeY", Range(0, 0.5)) = 0.4
        _Magnitude ("Alpha Magnitude", Float) = 1
        _Color ("Color", Color) = (1,1,1,1)
        _Intensity ("Color Intensity", Float) = 1

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
                half _FillterRange;
                half _RangeX;
                half _RangeY;
                half _Magnitude;
                half _Intensity;
                half4 _Color;
            CBUFFER_END

            half FillterColorRange(half2 uv)
            {
                half uvX = abs(uv.x);
                half uvY = abs(uv.y);

                half max_X = step(_RangeX, uvX);
                half max_Y = step(_RangeY, uvY);
                return (1 - max_X) * (1 - max_Y);
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
                half len = length(dir) * 0.1;

                half fRange = FillterColorRange(dir);
                half alpha = lerp(1, fRange, saturate(len * _Magnitude));
                half3 color = lerp(col.rgb, _Color.rgb, (1 - fRange) * saturate(len * _Intensity));
                
                return half4(color, alpha);
            }
            ENDCG
        }
    }
}
