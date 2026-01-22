Shader "Unlit/IceSpell"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Color ("Ice Color", Color) = (0.5, 0.8, 1, 0.5)
        _FogIntensity ("Fog Intensity", Range(0, 1)) = 0
        _FogColor ("Fog Color", Color) = (0.7, 0.9, 1, 0.8)
        _NoiseScale ("Noise Scale", Float) = 5.0
        _NoiseSpeed ("Noise Speed", Float) = 1.0
        _FogThickness ("Fog Thickness", Range(0, 2)) = 1.0
        _EmissionIntensity ("Emission Intensity", Range(0, 5)) = 1.0
        _MinNoiseVisibility ("Min Noise Visibility", Range(0, 1)) = 0.3
        _NoiseDistortion ("Noise Distortion", Range(0, 1)) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _MainTex_ST;
            float4 _NoiseTex_ST;
            float4 _Color;
            float _FogIntensity;
            float4 _FogColor;
            float _NoiseScale;
            float _NoiseSpeed;
            float _FogThickness;
            float _EmissionIntensity;
            float _MinNoiseVisibility;
            float _NoiseDistortion;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 noiseUV = i.worldPos.xy * _NoiseScale * 0.1 + _Time.y * _NoiseSpeed * 0.1;
                float noiseValue = tex2D(_NoiseTex, noiseUV).r;

                float2 noiseUV2 = i.worldPos.yz * _NoiseScale * 0.07 - _Time.y * _NoiseSpeed * 0.05;
                float noiseValue2 = tex2D(_NoiseTex, noiseUV2).r;

                float fogNoise = (noiseValue + noiseValue2) * 0.5;
                
                float2 distortionUV = i.worldPos.xz * _NoiseScale * 0.05 + _Time.y * _NoiseSpeed * 0.08;
                float distortion = tex2D(_NoiseTex, distortionUV).r * _NoiseDistortion;

                float2 distortedUV = noiseUV + distortion;
                float distortedNoise = tex2D(_NoiseTex, distortedUV).r;
                fogNoise = (fogNoise + distortedNoise) * 0.5;

                float noiseInfluence = lerp(_MinNoiseVisibility, 1.0, _FogIntensity);

                float fogMask = smoothstep(0.2, 0.9, fogNoise * noiseInfluence + _FogIntensity * _FogThickness * 0.5);

                fixed4 baseColor = _Color * i.color;

                fixed4 fogEffect = _FogColor * (fogMask * 0.7 + fogNoise * 0.3);
                
                float blendFactor = _FogIntensity * 0.8;
                fixed4 col = lerp(baseColor, fogEffect, blendFactor);
                
                col.a = lerp(baseColor.a, fogEffect.a * (0.8 + fogNoise * 0.4), _FogIntensity * 0.9);

                float emissionNoise = fogNoise * 0.5 + 0.5;
                col.rgb += _FogColor.rgb * _FogIntensity * _EmissionIntensity * fogMask * emissionNoise;

                UNITY_APPLY_FOG(i.fogCoord, col);
                
                return col;
            }
            ENDCG
        }
    }
}