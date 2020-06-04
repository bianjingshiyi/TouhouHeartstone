Shader "Effect/GaussBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurRadius ("Blur Radius", Range(3, 20)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float _BlurRadius;

            
            float GetGaussWeight(float x, float y, float sigma)
            {
                float sigma2 = pow(sigma, 2.0f);
                float left = 1 / (2 * sigma2 * 3.1415926f);
                float right = exp(-(x*x+y*y)/(2*sigma2));
                return left * right;
            }
            
            half4 GaussBlur(float2 uv)
            {
                float sigma = _BlurRadius / 3.0;
                float4 col = float4(0, 0, 0, 0);
                for (int x = - _BlurRadius; x <= _BlurRadius; ++x)
                {
                    for (int y = - _BlurRadius; y <= _BlurRadius; ++y)
                    {
                        float4 color = tex2D(_MainTex, uv + float2(x * _MainTex_TexelSize.x, y * _MainTex_TexelSize.y));
                        float weight = GetGaussWeight(x, y, sigma);
                        col += color * weight;
                    }
                }
                
                return col;
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
