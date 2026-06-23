Shader "Custom/HologramAura"
{
    Properties
    {
        [HDR] _Color ("Glow Color", Color) = (0, 0.8, 1, 0.6)
        _ScanlineCount ("Scanline Count", Float) = 24
        _ScanlineSpeed ("Scanline Speed", Float) = 3
        _PulseSpeed ("Pulse Speed", Float) = 2
        _EdgeWidth ("Edge Width", Range(0.01, 0.5)) = 0.15
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha One
        Cull Off
        ZWrite Off

        Pass
        {
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
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float _ScanlineCount;
            float _ScanlineSpeed;
            float _PulseSpeed;
            float _EdgeWidth;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float2 centered = uv - 0.5;
                float dist = length(centered);

                // Ring shape: bright edge with hollow center
                float ring = 1 - smoothstep(0.35, 0.5, dist);
                float innerFade = smoothstep(0, 0.35, dist);
                ring *= innerFade;

                // Scanlines
                float scanline = sin(uv.y * _ScanlineCount + _Time.y * _ScanlineSpeed) * 0.5 + 0.5;
                scanline = lerp(0.3, 1.0, scanline);

                // Pulse
                float pulse = sin(_Time.y * _PulseSpeed) * 0.15 + 0.85;

                // Inner glow fade toward center
                float glow = exp(-dist * 6) * 0.3;

                half4 color = _Color;
                color.a *= (ring * scanline + glow) * pulse;

                return color;
            }
            ENDHLSL
        }
    }
}
