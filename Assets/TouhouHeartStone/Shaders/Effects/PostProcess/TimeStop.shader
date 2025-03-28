Shader "THH/PostProcess/TimeStop"
{
    HLSLINCLUDE
    // StdLib.hlsl holds pre-configured vertex shaders (VertDefault), varying structs (VaryingsDefault), and most of the data you need to write common effects.
    #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
    TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
    float2 _Center; // 时间停止中心
    float  _Radius; // 时间停止半径
    float4 Frag(VaryingsDefault i) : SV_Target
    {
        float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
        float dist = distance(i.texcoord, _Center);

        if (dist > _Radius)
        {
            // 范围外反色
            color.rgb = 1 - color.rgb;
        }
        else
        {
            // 范围内黑白
            float grayscale = dot(color.rgb, float3(0.299, 0.587, 0.114));
            color.rgb = grayscale.xxx;
        }

        return color;
    }
    ENDHLSL
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment Frag
            ENDHLSL
        }
    }
}