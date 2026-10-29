Shader "TRUST/FilmGrainVignette"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("UI Tint", Color) = (1,1,1,1)

        _VignetteStrength ("Vignette Strength", Range(0,1)) = 0.55
        _VignetteSize ("Vignette Size", Range(0.1,1)) = 0.65
        _VignetteSoftness ("Vignette Softness", Range(0.01,1)) = 0.45

        _GrainStrength ("Film Grain Strength", Range(0,0.3)) = 0.07
        _GrainScale ("Film Grain Scale", Range(100,1200)) = 500
        _GrainSpeed ("Film Grain Speed", Range(0,30)) = 12

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        [Toggle(UNITY_UI_ALPHACLIP)]
        _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGBA

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                float _VignetteStrength;
                float _VignetteSize;
                float _VignetteSoftness;
                float _GrainStrength;
                float _GrainScale;
                float _GrainSpeed;
            CBUFFER_END

            float4 _ClipRect;

            float UnityGet2DClipping(float2 position, float4 clipRect)
            {
                float2 inside =
                    step(clipRect.xy, position) *
                    step(position, clipRect.zw);

                return inside.x * inside.y;
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 localPosition : TEXCOORD1;
            };

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            Varyings vert(Attributes v)
            {
                Varyings o;

                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                o.localPosition = v.positionOS.xy;

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 uv = i.uv;

                half4 baseTex = SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    uv
                ) * i.color;

                // 1. Vignette: darken the screen corners
                float2 centeredUV = (uv - 0.5) * float2(1.0, 1.0);
                float distanceFromCenter = length(centeredUV);

                float vignette = smoothstep(
                    _VignetteSize * 0.5,
                    _VignetteSize * 0.5 + _VignetteSoftness,
                    distanceFromCenter
                );

                vignette *= _VignetteStrength;

                // 2. Film grain: animated fine noise
                float2 grainUV = floor(uv * _GrainScale);
                float frame = floor(_Time.y * _GrainSpeed);

                float noise = hash21(grainUV + frame);
                float grain = (noise - 0.5) * 2.0;

                // Subtle monochrome noise over the image
                float grainAlpha = abs(grain) * _GrainStrength;

                float3 grainColor = (grain > 0.0)
                    ? float3(1.0, 1.0, 1.0)
                    : float3(0.0, 0.0, 0.0);

                // Combine both effects into one transparent overlay
                float vignetteAlpha = vignette * baseTex.a;
                float finalAlpha = max(vignetteAlpha, grainAlpha * baseTex.a);

                float3 finalColor = lerp(
                    grainColor,
                    float3(0.0, 0.0, 0.0),
                    saturate(vignetteAlpha)
                );

                #ifdef UNITY_UI_CLIP_RECT
                finalAlpha *= UnityGet2DClipping(
                    i.localPosition,
                    _ClipRect
                );
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(finalAlpha - 0.001);
                #endif

                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}