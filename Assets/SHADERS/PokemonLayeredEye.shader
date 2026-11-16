Shader "Custom/PokemonLayeredEye"
{
    Properties
    {
        _BaseMap("Eye Albedo", 2D) = "white" {}
        _LayerMask("Eye Color Masks", 2D) = "black" {}
        _Layer1("Layer 1", Color) = (1,1,1,1)
        _Layer2("Layer 2", Color) = (1,1,1,1)
        _Layer3("Layer 3", Color) = (1,1,1,1)
        _Layer4("Layer 4", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_LayerMask); SAMPLER(sampler_LayerMask);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _Layer1, _Layer2, _Layer3, _Layer4;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb;
                half4 mask = SAMPLE_TEXTURE2D(_LayerMask, sampler_LayerMask, input.uv);
                half3 tint = _Layer1.rgb * mask.r + _Layer2.rgb * mask.g
                    + _Layer3.rgb * mask.b + _Layer4.rgb * mask.a;
                tint += 1 - saturate(mask.r + mask.g + mask.b + mask.a);
                return half4(albedo * tint, 1);
            }
            ENDHLSL
        }
    }
}
