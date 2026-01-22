Shader "Custom/RevealSphere"
{
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        
        Pass
        {
            ZWrite Off
            ColorMask 0
            Cull Off
            
            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }
        }
    }
}