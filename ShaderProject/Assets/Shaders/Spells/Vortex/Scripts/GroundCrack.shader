Shader "Custom/GroundCrack"
{
    Properties
    {
        _CrackColor ("Crack Color", Color) = (0.8, 0, 1, 1)
        _CrackTexture ("Crack Texture", 2D) = "white" {}
        _Intensity ("Intensity", Range(0, 1)) = 0
        _CrackGlow ("Crack Glow", Float) = 1.0
        _EmissionStrength ("Emission Strength", Float) = 5.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_CrackTexture);
            SAMPLER(sampler_CrackTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _CrackColor;
                float _Intensity, _CrackGlow, _EmissionStrength;
                float4 _CrackTexture_ST;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _CrackTexture);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float crack = SAMPLE_TEXTURE2D(_CrackTexture, sampler_CrackTexture, input.uv).r;
                float pulse = sin(_Time.y * 4.0) * 0.3 + 0.7;
                float glow = pow(crack, 2.0) * _CrackGlow * pulse;
                
                float3 color = _CrackColor.rgb * (crack + glow * _EmissionStrength);
                float alpha = crack * _Intensity;
                
                return float4(color, alpha);
            }
            ENDHLSL
        }
    }
}