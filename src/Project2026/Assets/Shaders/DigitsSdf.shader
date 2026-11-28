// SDF glyph quads (damage numbers) sampled from a TextMeshPro font atlas. Used by BatchedQuadMesh: all numbers
// are one draw call. Face color and fade come from the vertex color; the outline is a material setting.
// Edge at 0.5 like TMP; the outline straddles it by _OutlineWidth (in SDF units, TMP _OutlineWidth * _ScaleRatioA).
Shader "Project2026/DigitsSdf"
{
    Properties
    {
        _MainTex("Font Atlas", 2D) = "white" {}
        _OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth("Outline Width", Range(0, 0.5)) = 0.225
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _OutlineColor;
                half _OutlineWidth;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(input.positionOS);
                o.color = input.color;
                o.uv = input.uv;
                return o;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half d = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                half w = max(fwidth(d), 1e-4h) * 0.5h;

                half face = smoothstep(0.5h + _OutlineWidth - w, 0.5h + _OutlineWidth + w, d);
                half shape = smoothstep(0.5h - _OutlineWidth - w, 0.5h - _OutlineWidth + w, d);

                half4 color = lerp(_OutlineColor, half4(input.color.rgb, 1.0h), face);
                color.a *= shape * input.color.a;
                return color;
            }
            ENDHLSL
        }
    }
}
