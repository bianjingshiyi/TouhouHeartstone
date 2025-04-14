Shader "THH/PostProcess/TimeStop"
{
    HLSLINCLUDE
    // StdLib.hlsl holds pre-configured vertex shaders (VertDefault), varying structs (VaryingsDefault), and most of the data you need to write common effects.
    #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
    TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
    float2 _Center; // ʱ��ֹͣ����
    float  _Radius; // ʱ��ֹͣ�뾶
    float  _Ratio; // ����
    float4 Frag(VaryingsDefault i) : SV_Target
    {
        float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
        float dist = distance(i.texcoord, _Center);

        if (dist > _Radius)
        {
            // ��Χ�ⷴɫ
            color.rgb = lerp(color.rgb, 1 - color.rgb, _Ratio);
        }
        else
        {
            // ��Χ�ںڰ�
            float grayscale = dot(color.rgb, float3(0.299, 0.587, 0.114));
            color.rgb = lerp(color.rgb, grayscale.xxx, _Ratio);
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