Shader "Project2026/ResearchFog"
{
    Properties
    {
        _FogColor ("Fog", Color) = (0.08, 0.09, 0.12, 0.9)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
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

            float4 _FogColor;
            float4 _TargetSize;
            float _CircleCount;
            float4 _Circles[32];

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float Noise(float2 p)
            {
                float2 cell = floor(p);
                float2 fraction = frac(p);
                float a = Hash(cell);
                float b = Hash(cell + float2(1.0, 0.0));
                float c = Hash(cell + float2(0.0, 1.0));
                float d = Hash(cell + float2(1.0, 1.0));
                float2 blend = fraction * fraction * (3.0 - 2.0 * fraction);
                return lerp(lerp(a, b, blend.x), lerp(c, d, blend.x), blend.y);
            }

            float Fbm(float2 p)
            {
                float value = 0.0;
                float weight = 0.5;

                [unroll]
                for (int octave = 0; octave < 4; octave++)
                {
                    value += weight * Noise(p);
                    p = p * 2.13 + float2(19.2, 7.7);
                    weight *= 0.5;
                }

                return value;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 pixel = float2(input.uv.x, 1.0 - input.uv.y) * _TargetSize.xy;
                float warp = (Fbm(pixel * 0.018) - 0.5) * 100.0;
                float reveal = 0.0;
                int count = (int)_CircleCount;

                [loop]
                for (int i = 0; i < 32; i++)
                {
                    if (i >= count)
                        break;

                    float2 delta = abs(pixel - _Circles[i].xy);
                    float dist = max(delta.x, delta.y) + warp;
                    float radius = _Circles[i].z;
                    reveal = max(reveal, smoothstep(radius, radius - 16.0, dist));
                }

                float grain = Fbm(pixel * 0.09);
                float speck = Hash(floor(pixel * 0.35));
                float cover = 1.0 - reveal;
                float alpha = _FogColor.a * cover * lerp(0.72, 1.0, grain) * lerp(0.88, 1.0, speck);
                float3 color = _FogColor.rgb * lerp(0.78, 1.08, grain);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}
