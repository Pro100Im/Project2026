// World map cloud veil in the Tiny Swords pixel-art style: round flat puffs, three tones, flat drop shadow.
// Coordinates are map UI pixels (origin top-left, y down), matching LevelNodeDefinition.Position.
// Pass 0 bakes the clearing field (signed distance to the clear area) into a small float texture.
// Pass 1 draws the puffs into a point-filtered texture where one texel is one art pixel.
Shader "Project2026/LevelMapClouds"
{
    Properties
    {
        _LightColor ("Light", Color) = (0.988, 0.996, 0.937, 1)
        _MidColor ("Mid", Color) = (0.898, 0.914, 0.804, 1)
        _DarkColor ("Dark", Color) = (0.859, 0.847, 0.745, 1)
        _ShadowColor ("Shadow", Color) = (0.141, 0.165, 0.231, 0.333)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        ZWrite Off
        ZTest Always
        Cull Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        #define MAX_HOLES 64

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

        float4 _MapSize;
        float _BorderWidth;
        float _HoleCount;
        float4 _Holes[MAX_HOLES];

        Varyings Vert(Attributes input)
        {
            Varyings output;
            output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
            output.uv = input.uv;
            return output;
        }

        float2 MapPoint(float2 uv)
        {
            return float2(uv.x, 1.0 - uv.y) * _MapSize.xy;
        }

        float Hash(float2 p)
        {
            p = frac(p * float2(123.34, 456.21));
            p += dot(p, p + 45.32);
            return frac(p.x * p.y);
        }

        float Noise(float2 p)
        {
            float2 cell = floor(p);
            float2 f = frac(p);
            float2 u = f * f * (3.0 - 2.0 * f);
            return lerp(
                lerp(Hash(cell), Hash(cell + float2(1.0, 0.0)), u.x),
                lerp(Hash(cell + float2(0.0, 1.0)), Hash(cell + float2(1.0, 1.0)), u.x),
                u.y);
        }
        ENDHLSL

        // Pass 0: clearing field. Negative inside a clearing, positive under the clouds.
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragField

            float SmoothMin(float a, float b, float k)
            {
                float h = saturate(0.5 + 0.5 * (b - a) / k);
                return lerp(b, a, h) - k * h * (1.0 - h);
            }

            float4 FragField(Varyings input) : SV_Target
            {
                float2 p = MapPoint(input.uv);
                float d = 100000.0;
                int count = (int)_HoleCount;

                [loop]
                for (int i = 0; i < MAX_HOLES; i++)
                {
                    if (i >= count)
                        break;

                    d = SmoothMin(d, length(p - _Holes[i].xy) - _Holes[i].z, 90.0);
                }

                // Wavy frame: a straight border would show as straight seams between the puffs.
                float edge = min(min(p.x, _MapSize.x - p.x), min(p.y, _MapSize.y - p.y));
                float wave = (Noise(p * 0.008) * 0.65 + Noise(p * 0.021) * 0.35 - 0.5) * _BorderWidth * 0.8;
                return float4(max(d, _BorderWidth - edge + wave), 0.0, 0.0, 1.0);
            }
            ENDHLSL
        }

        // Pass 1: pixel-art puffs.
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragClouds

            TEXTURE2D(_FieldTex);
            SAMPLER(sampler_FieldTex);

            float4 _LightColor;
            float4 _MidColor;
            float4 _DarkColor;
            float4 _ShadowColor;
            float _PuffSize;
            float _ShadowOffset;
            float _CloudTime;
            float _BreathAmount;
            float _BreathSpeed;

            // Highlight sits up-left, the shaded crescent down-right, as on the pack's cloud sprites.
            static const float2 ShadeDir = float2(0.543, 0.840);

            float2 Hash2(float2 p)
            {
                return float2(Hash(p), Hash(p + float2(37.1, 91.7)));
            }

            float Field(float2 p)
            {
                float2 uv = float2(p.x / _MapSize.x, 1.0 - p.y / _MapSize.y);
                return SAMPLE_TEXTURE2D_LOD(_FieldTex, sampler_FieldTex, uv, 0).r;
            }

            struct Puff
            {
                float height; // r - distance, > 0 when the pixel is inside
                float2 center;
                float radius;
                float key;    // stable per puff, unlike the swaying center
            };

            // One layer of puffs on a jittered grid. Whether a puff exists is decided from its resting
            // center and radius only, so the breathing below never makes puffs pop in or out:
            // field at the rest center in [fieldMin + radius * fieldMinPerRadius, fieldMax], hash under
            // density, and (thinning) fewer puffs the deeper they sit past thinStart over thinLength.
            Puff Layer(float2 p, float cell, float rMin, float rMax, float seed,
                       float fieldMin, float fieldMinPerRadius, float fieldMax, float density,
                       float thinStart, float thinLength)
            {
                Puff best;
                best.height = -1.0;
                best.center = 0.0;
                best.radius = 1.0;
                best.key = 0.0;

                float2 home = floor(p / cell);
                float t = _CloudTime * _BreathSpeed;

                [unroll]
                for (int y = -1; y <= 1; y++)
                {
                    [unroll]
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 id = home + float2(x, y);
                        float2 h = Hash2(id + seed);
                        float extra = Hash(id + seed + 17.3);

                        if (extra > density)
                            continue;

                        float2 rest = (id + 0.25 + 0.5 * h) * cell;
                        float baseRadius = lerp(rMin, rMax, frac(extra * 7.13 + h.x)) * cell;

                        // Each puff slowly swells and sways with its own phase, so the edge keeps living.
                        float phase = (h.x + h.y) * 6.2832;
                        float2 sway = float2(sin(t * 0.8 + phase), cos(t * 0.65 + phase * 1.3)) * _BreathAmount * cell;
                        float2 center = rest + sway;
                        float radius = baseRadius * (1.0 + _BreathAmount * sin(t + phase * 2.0));
                        float height = radius - length(p - center);

                        if (height <= best.height)
                            continue;

                        float field = Field(rest);

                        if (field < fieldMin + fieldMinPerRadius * baseRadius || field > fieldMax)
                            continue;

                        if (thinLength > 0.0 && h.y < saturate((field - thinStart) / thinLength))
                            continue;

                        best.height = height;
                        best.center = center;
                        best.radius = radius;
                        best.key = h.y;
                    }
                }

                return best;
            }

            // Small puffs along the edge, poking out by up to ~0.8 of their radius. Deeper in they thin
            // out gradually and uncover the big puffs, so there is no visible border between the two.
            Puff EdgePuffs(float2 p)
            {
                return Layer(p, _PuffSize, 0.7, 1.0, 0.0, 0.0, 0.2, 100000.0, 1.0, _PuffSize * 1.5, _PuffSize * 3.5);
            }

            // Big puffs filling the inside of the veil.
            Puff BigPuffs(float2 p)
            {
                return Layer(p, _PuffSize * 2.4, 0.75, 1.05, 11.0, _PuffSize * 0.8, 0.0, 100000.0, 1.0, 0.0, 0.0);
            }

            // Small loose dots floating just outside the cloud edge.
            Puff Dots(float2 p)
            {
                return Layer(p, _PuffSize * 0.5, 0.14, 0.26, 23.0, _PuffSize * -1.5, 0.0, _PuffSize * -0.7, 0.35, 0.0, 0.0);
            }

            bool InCloud(float2 p)
            {
                if (Field(p) > _PuffSize * 0.5)
                    return true;

                return EdgePuffs(p).height > 0.0 || Dots(p).height > 0.0;
            }

            // Body light, a thin crescent down-right mid. Dark only on the outer bottom rim of puffs
            // that stick out over a clearing.
            float3 Tone(float2 p, Puff puff, float field, float crescent)
            {
                float2 inner = puff.center - ShadeDir * puff.radius * 0.16;
                float t = length(p - inner) / puff.radius;

                if (t < crescent)
                    return _LightColor.rgb;

                if (field < 0.0 && t > 1.04)
                    return _DarkColor.rgb;

                return _MidColor.rgb;
            }

            half4 FragClouds(Varyings input) : SV_Target
            {
                float2 p = MapPoint(input.uv);
                float field = Field(p);

                Puff edge = EdgePuffs(p);

                if (edge.height > 0.0)
                    return half4(Tone(p, edge, field, 0.86), 1.0);

                Puff dots = Dots(p);

                if (dots.height > 0.0)
                    return half4(Tone(p, dots, field, 0.86), 1.0);

                if (field > _PuffSize * 0.5)
                {
                    Puff big = BigPuffs(p);

                    // Relatively thinner crescent so it is about as many pixels wide as on the small puffs.
                    if (big.height > 0.0)
                        return half4(Tone(p, big, field, 0.9), 1.0);

                    // Gaps: light near the edge so no seam shows, mid deeper in.
                    return half4(field < _PuffSize * 1.5 ? _LightColor.rgb : _MidColor.rgb, 1.0);
                }

                // Flat drop shadow on the clearing below the clouds.
                if (InCloud(p - float2(0.0, _ShadowOffset)))
                    return _ShadowColor;

                return half4(0.0, 0.0, 0.0, 0.0);
            }
            ENDHLSL
        }
    }
}
