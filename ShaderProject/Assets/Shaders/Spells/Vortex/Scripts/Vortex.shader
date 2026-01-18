Shader "Custom/VoidVortex"
{
    Properties
    {
        _VortexColor ("Vortex Color", Color) = (0.6, 0, 1, 1) //couleur vortex overall
        _InnerGlow ("Inner Glow Color", Color) = (0.8, 0.2, 1, 1) //vortex interieur couleur
        _EdgeColor ("Edge Color", Color) = (1, 0, 1, 1)
        _ChargeAmount ("Charge Amount", Range(0, 1)) = 0 // pour son spawn
        _VortexSpeed ("Vortex Speed", Float) = 2.0
        _SpiralTightness ("Spiral Tightness", Float) = 5.0 // à quel point la spiral est recroqueviller sur elle même 
        _EmissionStrength ("Emission Strength", Float) = 8.0 
        _NoiseTexture ("Noise Texture", 2D) = "white" {}
        _PulseSpeed ("Pulse Speed", Float) = 3.0 //vitesse du pulse 
        _InnerBrightness ("Inner Brightness", Float) = 15.0 // luminosité interieure
        _DepthLayers ("Depth Layers", Range(1, 10)) = 5 //experimental pour l'instant ne marche pas comme voulu
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
                float4 _VortexColor, _InnerGlow, _EdgeColor;
                float _ChargeAmount, _VortexSpeed, _SpiralTightness, _EmissionStrength;
                float _PulseSpeed, _InnerBrightness, _DepthLayers;
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
                float2 centered = input.uv - 0.5;
                float radius = length(centered) * 2.0;
                float angle = atan2(centered.y, centered.x);

                float depthRot = (1.0 - radius) * time * _VortexSpeed * 2.0;
                float spiral1 = sin((angle + radius * _SpiralTightness - time * _VortexSpeed + depthRot) * 8.0) * 0.5 + 0.5;
                float spiral2 = sin((angle - radius * _SpiralTightness * 0.7 + time * _VortexSpeed * 0.5) * 12.0) * 0.5 + 0.5;

                float2 noiseUV = input.uv * 3.0 + time * 0.1;
                float noise = SAMPLE_TEXTURE2D(_NoiseTexture, sampler_NoiseTexture, noiseUV).r * 0.6 +
                              SAMPLE_TEXTURE2D(_NoiseTexture, sampler_NoiseTexture, noiseUV * 2.0).r * 0.4;

                float radial = pow(1.0 - saturate(radius), 1.2);
                float charge = pow(_ChargeAmount, 1.2);
                float pulse = sin(time * _PulseSpeed) * 0.5 + 0.5;

                float bands = pow(sin((radius - time * 0.5) * 30.0 * charge) * 0.5 + 0.5, 3.0);

                float pattern = (spiral1 * 0.6 + spiral2 * 0.4) * radial * lerp(1.0, 1.4, pulse * charge);
                pattern = lerp(pattern, bands, 0.3);

                float centerRing = smoothstep(0.15, 0.05, radius) * smoothstep(0.0, 0.08, radius) * charge * (sin(time * 8.0) * 0.3 + 0.7);
                float deepRing = smoothstep(0.1, 0.0, radius) * pow(1.0 - radius, 3.0) * (sin(time * 5.0) * 0.5 + 0.5);
                
                float fresnel = pow(1.0 - saturate(dot(normalize(input.normalWS), normalize(input.viewDirWS))), 4.0) * charge * 2.0;

                float3 color = _VortexColor.rgb * pattern * 1.5;
                color += _InnerGlow.rgb * pow(radial, 4.0) * charge * _InnerBrightness;
                color += _EdgeColor.rgb * (centerRing * 10.0 + deepRing * 20.0);
                color += _VortexColor.rgb * fresnel;
                color *= 1.5 * (1.0 + _EmissionStrength * pow(charge, 2.0));

                float alpha = saturate(pattern + centerRing + deepRing) * charge;
                alpha = lerp(alpha, alpha * 0.6, smoothstep(0.2, 0.0, radius) * (1.0 - charge));

                return float4(color, alpha);
            }
            ENDHLSL
        }
    }
}