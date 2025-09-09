Shader "Custom/HappySky"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.53, 0.81, 0.92, 1) // Sky blue
        _BottomColor ("Bottom Color", Color) = (1.0, 0.91, 0.66, 1) // Peachy yellow
        _SunColor ("Sun Color", Color) = (1.0, 0.97, 0.84, 1)
        _SunDirection ("Sun Direction", Vector) = (0,1,0,0)
        _SunSize ("Sun Size", Range(0.1,5.0)) = 1.0
        _CloudIntensity ("Cloud Intensity", Range(0,1)) = 0.3
        _CloudSpeed ("Cloud Speed", Range(0,2)) = 0.2
        _CloudScale ("Cloud Scale", Range(1,10)) = 4.0
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

            float4 _TopColor, _BottomColor, _SunColor, _SunDirection;
            float _SunSize, _CloudIntensity, _CloudSpeed, _CloudScale;

            // Simple hash & noise
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

                // Use view direction for sky mapping
                float3 dir = mul((float3x3)UNITY_MATRIX_V, IN.positionOS.xyz);
                OUT.dirWS = normalize(dir);

                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // Vertical gradient
                float t = saturate(IN.dirWS.y * 0.5 + 0.5);
                float3 col = lerp(_BottomColor.rgb, _TopColor.rgb, t);

                // Sun glow
                float sunDot = saturate(dot(normalize(IN.dirWS), normalize(_SunDirection.xyz)));
                float sun = pow(sunDot, 500 / _SunSize);
                col += _SunColor.rgb * sun;

                // Clouds (animated with _Time.y)
                float2 uv = IN.dirWS.xz * _CloudScale + _Time.y * _CloudSpeed;
                float c = noise(uv);
                col = lerp(col, col + float3(1,1,1) * _CloudIntensity, c);

                return float4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
