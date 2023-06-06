Shader "THH/Bezierline"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _Fade("Fade", Range(0.0, 1.0)) = 0

        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255

        _MainTex("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True" 
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }
        Stencil
        {
            Ref[_Stencil]
            Comp[_StencilComp]
            Pass[_StencilOp]
            ReadMask[_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            sampler2D _MainTex;
            fixed4 _Color;
            fixed _Fade;

            struct appdata_t {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR0;
                float2 texcoord : TEXCOORD0;
            };

            float4 _MainTex_ST;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.texcoord = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed alpha = i.color.a;
                fixed fadeValue = abs(i.texcoord.y - 0.5) * 2;
                fixed alphaReduction = 0;
                if (_Fade != 0)
                {
                    alphaReduction = saturate(1 - (1 - fadeValue) / _Fade);
                }
                alpha = saturate(alpha - alphaReduction);

                fixed4 color = fixed4(i.color.rgb, alpha);
                return color * tex2D(_MainTex, i.texcoord);
            }
            ENDCG
        }
    }
    FallBack "Default/Sprite"
}
