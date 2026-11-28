// Shared sprite shader for every SpriteRenderer / TilemapRenderer in the game (based on URP Sprite-Unlit-Default).
// Unlit (no 2D light passes) and SRP Batcher compatible. The hit flash is a per-material value: a flashing
// renderer temporarily swaps to one of a few shared "flash step" materials (see HitFlashMaterials), so no
// renderer ever needs a MaterialPropertyBlock and everything at _FlashAmount = 0 batches together.
Shader "Project2026/SpriteUnlitFlash"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _FlashAmount("Flash Amount", Range(0, 1)) = 0
        [MaterialToggle] _ZWrite("ZWrite", Float) = 0

        [HideInInspector] _Color("Tint", Color) = (1,1,1,1)
        [HideInInspector] PixelSnap("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite [_ZWrite]

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex FlashVertex
            #pragma fragment FlashFragment

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_OUTPUTS
                half4 color : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/2DCommon.hlsl"

            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE

            // Do not ifdef the properties: the SRP Batcher needs one layout.
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _FlashAmount;
            CBUFFER_END

            Varyings FlashVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                Varyings o = CommonUnlitVertex(input);
                o.color = input.color * _Color * unity_SpriteColor;
                return o;
            }

            half4 FlashFragment(Varyings input) : SV_Target
            {
                half4 color = CommonUnlitFragment(input, input.color);
                color.rgb = lerp(color.rgb, half3(1.0h, 1.0h, 1.0h), saturate(_FlashAmount));
                return color;
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
