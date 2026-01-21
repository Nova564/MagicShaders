Shader "Unlit/FlameSpell"
{
    Properties
    {
        [Header(Colors)]
        _CoreColor ("Core Color", Color) = (1, 0.3, 0, 1)
        _MidColor ("Mid Color", Color) = (1, 0.5, 0, 1)
        _EdgeColor ("Edge Color", Color) = (1, 0.7, 0.1, 1)
        
        [Header(Noise Textures)]
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _DistortionTex ("Distortion Texture", 2D) = "white" {}
        
        [Header(Animation)]
        _FlameSpeed ("Flame Speed", Float) = 3.0
        _TurbulenceSpeed ("Turbulence Speed", Float) = 2.0
        
        [Header(Intensity)]
        _Intensity ("Overall Intensity", Range(0, 10)) = 8.0
        _EmissionStrength ("Emission Strength", Range(1, 30)) = 15.0
        _ChargeAmount ("Charge Amount", Range(0, 1)) = 1.0
        
        [Header(Flame Shape)]
        _FlameHeight ("Flame Height Power", Range(0.5, 3)) = 1.5
        _NoiseScale ("Noise Scale", Float) = 4.0
        _Distortion ("Distortion Amount", Range(0, 1)) = 0.4
        _EdgeSharpness ("Edge Sharpness", Range(0.01, 0.3)) = 0.08
        
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "IgnoreProjector"="True"
        }
        LOD 100
        
        Blend SrcAlpha One
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };

            float4 _CoreColor;
            float4 _MidColor;
            float4 _EdgeColor;
            
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            sampler2D _DistortionTex;
            float4 _DistortionTex_ST;
            
            float _FlameSpeed;
            float _TurbulenceSpeed;
            float _Intensity;
            float _EmissionStrength;
            float _ChargeAmount;
            float _FlameHeight;
            float _NoiseScale;
            float _Distortion;
            float _EdgeSharpness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y;
                
                float2 distortUV = i.uv * 3.0 + float2(0, time * _TurbulenceSpeed);
                float distortionNoise = tex2D(_DistortionTex, distortUV).r - 0.5;
                float2 uv = i.uv + float2(distortionNoise * _Distortion, 0);
                
                float2 flameUV1 = uv * _NoiseScale + float2(0, time * _FlameSpeed);
                float2 flameUV2 = uv * _NoiseScale * 1.5 + float2(0.5, time * _FlameSpeed * 1.3);
                
                float noise1 = tex2D(_NoiseTex, flameUV1).r;
                float noise2 = tex2D(_NoiseTex, flameUV2).r;
                float combinedNoise = (noise1 * 0.6 + noise2 * 0.4);
                
                float heightFactor = 1.0 - i.uv.y;
                float heightGradient = pow(heightFactor, _FlameHeight);
                
                float flameMask = combinedNoise * heightGradient;

                flameMask = smoothstep(0.25, 0.55, flameMask);

                float edgeTurbulence = tex2D(_NoiseTex, i.uv * 6.0 + time * 1.5).r;
                flameMask *= smoothstep(0.2, 0.6, edgeTurbulence + heightFactor * 0.5);

                float coreZone = smoothstep(0.6, 1.0, combinedNoise * heightFactor);
                float edgeZone = smoothstep(0.3, 0.6, combinedNoise);
                
                float3 coreToMid = lerp(_MidColor.rgb, _CoreColor.rgb, coreZone);
                float3 finalColor = lerp(_EdgeColor.rgb, coreToMid, edgeZone);
                
                finalColor = saturate(finalColor);
                
                float colorNoise = tex2D(_NoiseTex, i.uv * 4.0 + time * 0.8).r;
                finalColor *= 0.9 + colorNoise * 0.2; 
                
                
                float chargeMultiplier = _ChargeAmount;
                float finalIntensity = _Intensity * chargeMultiplier;
                
                float3 emission = finalColor * _EmissionStrength * finalIntensity;
                
                float alpha = flameMask * chargeMultiplier;
                alpha = smoothstep(0.0, _EdgeSharpness, alpha);
                
                //éviter trous
                alpha = saturate(alpha * 1.5);
                
                fixed4 col = fixed4(emission, alpha);
                
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}