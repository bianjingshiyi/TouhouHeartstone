Shader "THH/GaussBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurRadius ("Blur Radius", Range(0, 20)) = 1
        _Sigma ("Sigma", Range(0.5, 20)) = 1

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
            "RenderType"="Transparent" 
            "IgnoreProjector"="True"
            "Queue" = "Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        LOD 100
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp] 
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        ColorMask [_ColorMask]

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest Always 
        ZClip True

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                half2 uv      : TEXCOORD0;
            };

            struct v2f
            {
                half2 uv      : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MainTex_TexelSize;
                float _BlurRadius;
                float _Sigma;
            CBUFFER_END

            
            float GetGaussWeight(float x, float y, float sigma2, float left)
            {
                float right = exp(-(x*x+y*y)/(2*sigma2));
                return left * right;
            }
            
            half4 GaussBlur(float2 uv)
            {
                float sigma = _Sigma;
                float sigma2 = pow(sigma, 2.0f);
                float left = 1 / (2 * sigma2 * 3.1415926f);
                float4 col = float4(0, 0, 0, 0);
                float sumWeight = 0;
                for (int x = - _BlurRadius; x <= _BlurRadius; ++x)
                {
                    for (int y = - _BlurRadius; y <= _BlurRadius; ++y)
                    {
                        float4 color = tex2D(_MainTex, uv + float2(x * _MainTex_TexelSize.x, y * _MainTex_TexelSize.y));
                        float weight = GetGaussWeight(x, y, sigma2, left);
                        col += color * weight;
                        sumWeight += weight;
                    }
                }

                return col / sumWeight;

            }


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half2 uv = i.uv;
                half4 col = GaussBlur(uv);
                return col;
            }
            ENDCG
        }
    }
}
