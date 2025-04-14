Shader "THH/PostProcess/TimeStop"
{
    HLSLINCLUDE
    // StdLib.hlsl holds pre-configured vertex shaders (VertDefault), varying structs (VaryingsDefault), and most of the data you need to write common effects.
    #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
    TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
    float2 _Center; // 半径
    float  _Radius; // 范围
    float  _Fade; // 渐变范围
    float  _Ratio; // 比例
    float4 Frag(VaryingsDefault i) : SV_Target
    {
        float dist = distance(i.texcoord, _Center);
        float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);

        float3 inverted = 1 - color.rgb; // 反色
        float grayscale = dot(color.rgb, float3(0.299, 0.587, 0.114)); // 灰度
        float fade = clamp((dist - _Radius + _Fade) / _Fade, 0, 1);
        color.rgb = lerp(color.rgb, lerp(inverted.rgb, grayscale.xxx, fade), _Ratio);

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