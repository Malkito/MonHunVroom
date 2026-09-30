// Radial cooldown wedge for uGUI.
// Drawn on top of an ability icon: the area from 12 o'clock, sweeping clockwise,
// up to _Fill is tinted with the Image color; everything else is transparent.
// _Fill: 1 = full circle dimmed (just used), 0 = fully clear (ready).
// Structure mirrors "Universal Render Pipeline/2D/Sprite-Unlit-Default" so it
// compiles under the same URP version as the shipped shaders.
Shader "NikiUI/RadialCooldown"
{
    Properties
    {
        [MainColor] _Color ("Tint", Color) = (1, 1, 1, 1)
        [PerMaterial] _Fill ("Fill (1 = full, 0 = clear)", Range(0, 1)) = 0
        _MainTex ("Sprite Texture", 2D) = "white"
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreMaterial" = "True"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            ZTest Always

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 color : COLOR;
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _Fill;
                float4 _MainTex_ST;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.pos = TransformObjectToHClip(input.positionOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Polar coordinates centered on the UV, in the range [-1, 1].
                float2 p = (input.uv - 0.5) * 2.0;

                // Clip to a circle (also hides the square edges of the sprite).
                if (dot(p, p) > 1.0)
                    return float4(0, 0, 0, 0);

                // t: 0 at 12 o'clock, increasing clockwise, wrapping at 1.
                float ang = atan2(p.y, p.x);
                float t = fmod(1.5707963267948966 - ang, 6.2831853071795865);
                if (t < 0.0)
                    t += 6.2831853071795865;
                t /= 6.2831853071795865;

                // Dim the "remaining cooldown" wedge; everything else is transparent.
                if (t < _Fill)
                {
                    float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                    return input.color * tex;
                }

                return float4(0, 0, 0, 0);
            }
            ENDHLSL
        }
    }

    Fallback "Hidden/Default"
}
