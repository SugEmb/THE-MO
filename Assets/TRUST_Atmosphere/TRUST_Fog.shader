Shader "TRUST/AtmosphericFog"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("UI Tint", Color) = (1,1,1,1)

        _FogColor ("Fog Color", Color) = (0.22,0.30,0.38,1)
        _FogStrength ("Fog Strength", Range(0,1)) = 0.85
        _FogScale ("Fog Scale", Range(1,8)) = 2.5
        _FogSpeed ("Fog Speed", Range(0,0.2)) = 0.035
        _FogContrast ("Fog Contrast", Range(0.2,2)) = 0.8

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
            Name "AtmosphericFog"

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
                half4 _FogColor;
                half _FogStrength;
                half _FogScale;
                half _FogSpeed;
                half _FogContrast;
            CBUFFER_END

            float4 _ClipRect;

            float UnityGet2DClipping(float2 position, float4 clipRect)
            {
                float2 inside = step(clipRect.xy, position)
                              * step(position, clipRect.zw);

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

            float noise2d(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                f = f * f * (3.0 - 2.0 * f);

                float a = hash21(i);
                float b = hash21(i + float2(1.0, 0.0));
                float c = hash21(i + float2(0.0, 1.0));
                float d = hash21(i + float2(1.0, 1.0));

                return lerp(
                    lerp(a, b, f.x),
                    lerp(c, d, f.x),
                    f.y
                );
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
                half4 baseTex = SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    i.uv
                ) * i.color;

                float2 uv = i.uv;
                float t = _Time.y * _FogSpeed;

                float n1 = noise2d(
                    uv * _FogScale
                    + float2(t, t * 0.28)
                );

                float n2 = noise2d(
                    uv * (_FogScale * 2.1)
                    + float2(-t * 0.65, t * 0.42)
                );

                float fog = n1 * 0.72 + n2 * 0.28;

                fog = saturate((fog - 0.28) * 2.0);
                fog = smoothstep(0.02, 0.88, fog);
                fog = pow(fog, _FogContrast);

                half alpha = (half)fog
                    * _FogStrength
                    * baseTex.a;

                #ifdef UNITY_UI_CLIP_RECT
                alpha *= UnityGet2DClipping(
                    i.localPosition,
                    _ClipRect
                );
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(alpha - 0.001);
                #endif

                return half4(_FogColor.rgb, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}