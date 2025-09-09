Shader "Custom/CreepySky"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.23, 0.0, 0.28, 1) // Deep purple
        _BottomColor ("Bottom Color", Color) = (0.05, 0.0, 0.1, 1) // Near black
        _FogColor ("Fog Color", Color) = (0.2, 0.8, 0.4, 1) // Sickly green
        _NoiseScale ("Noise Scale", Range(1,10)) = 3.0
        _NoiseSpeed ("Noise Speed", Range(0,2)) = 0.5
        _Distortion ("Distortion", Range(0,1)) = 0.3
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionHCS : SV_POSITION; float3 dirWS : TEXCOORD0; };

            float4 _TopColor, _BottomColor, _FogColor;
            float _NoiseScale, _NoiseSpeed, _Distortion;

            float hash21(float2 p) {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p+45.32);
                return frac(p.x*p.y);
            }
            float noise(float2 uv) {
                float2 i = floor(uv);
                float2 f = frac(uv);
                float a = hash21(i);
                float b = hash21(i+float2(1,0));
                float c = hash21(i+float2(0,1));
                float d = hash21(i+float2(1,1));
                float2 u = f*f*(3.0-2.0*f);
                return lerp(lerp(a,b,u.x), lerp(c,d,u.x), u.y);
            }

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.dirWS = normalize(mul((float3x3)UNITY_MATRIX_M, IN.positionOS.xyz));
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float t = saturate(IN.dirWS.y * 0.5 + 0.5);
                float3 baseCol = lerp(_BottomColor.rgb, _TopColor.rgb, t);

                // Creepy noise fog
                float2 uv = IN.dirWS.xz * _NoiseScale + _Time.y * _NoiseSpeed;
                uv += sin(uv.yx * 5.0 + _Time.y) * _Distortion;
                float n = noise(uv);

                baseCol = lerp(baseCol, _FogColor.rgb, n * 0.6);

                return float4(baseCol, 1.0);
            }
            ENDHLSL
        }
    }
}
