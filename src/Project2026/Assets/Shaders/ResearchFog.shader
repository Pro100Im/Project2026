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

            half4 Frag(Varyings input) : SV_Target
            {
                float2 pixel = float2(input.uv.x, 1.0 - input.uv.y) * _TargetSize.xy;
                float reveal = 0.0;
                int count = (int)_CircleCount;

                [loop]
                for (int i = 0; i < 32; i++)
                {
                    if (i >= count)
                        break;

                    float dist = distance(pixel, _Circles[i].xy);
                    float radius = _Circles[i].z;
                    float feather = _Circles[i].w;
                    reveal = max(reveal, smoothstep(radius, radius - feather, dist));
                }

                float alpha = _FogColor.a * (1.0 - reveal);
                return half4(_FogColor.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
