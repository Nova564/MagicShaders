Shader "Custom/EnergyPillar"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (0.6, 0, 1, 1)
        _RimColor ("Rim Color", Color) = (1, 0, 1, 1)
        _ChargeAmount ("Charge Amount", Range(0, 1)) = 0
        _RiseAmount ("Rise Amount", Range(0, 1)) = 0
        _PillarIntensity ("Pillar Intensity", Float) = 1.0
        _NoiseTexture ("Noise Texture", 2D) = "white" {}
        _ScrollSpeed ("Scroll Speed", Float) = 2.0
        _RimPower ("Rim Power", Float) = 3.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : NORMAL;
                float3 viewDirWS : TEXCOORD1;
            };

            TEXTURE2D(_NoiseTexture);
            SAMPLER(sampler_NoiseTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainColor, _RimColor;
                float _ChargeAmount, _RiseAmount, _PillarIntensity, _ScrollSpeed, _RimPower;
                float4 _NoiseTexture_ST;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.uv = input.uv;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float time = _Time.y;
                float2 scrollUV = input.uv + float2(0, time * _ScrollSpeed);
                
                float pattern = SAMPLE_TEXTURE2D(_NoiseTexture, sampler_NoiseTexture, scrollUV * 2.0).r * 0.5 +
                                SAMPLE_TEXTURE2D(_NoiseTexture, sampler_NoiseTexture, scrollUV * 3.0 - time * 0.5).r * 0.3 +
                                (sin(input.uv.y * 30.0 - time * 5.0) * 0.5 + 0.5) * 0.2;
                
                float rim = pow(1.0 - saturate(dot(normalize(input.normalWS), normalize(input.viewDirWS))), _RimPower);
                
                float verticalFade = smoothstep(0.0, 0.3, input.uv.y) * smoothstep(1.0, 0.7, input.uv.y) * _RiseAmount;
                
                float3 color = lerp(_MainColor.rgb, _RimColor.rgb, rim) * pattern + _RimColor.rgb * rim * 2.0;
                color *= _PillarIntensity * _ChargeAmount;
                
                float alpha = saturate((pattern * 0.5 + rim * 0.5) * verticalFade * _ChargeAmount);
                
                return float4(color, alpha);
            }
            ENDHLSL
        }
    }
}