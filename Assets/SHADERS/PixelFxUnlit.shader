Shader "Pokemon/PixelFX Unlit"
{
 Properties { _MainTex("Sheet",2D)="white" {} _Sheet("Frame",Vector)=(1,1,0,0) _Tint("Tint",Color)=(1,1,1,1) }
 SubShader {
 Tags { "Queue"="Transparent+10" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
 Pass {
 Blend SrcAlpha OneMinusSrcAlpha
 ZWrite Off
 Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
 CBUFFER_START(UnityPerMaterial)
 float4 _Sheet;float4 _Tint;
 CBUFFER_END
 struct Attributes{float4 positionOS:POSITION;float2 uv:TEXCOORD0;};
 struct Varyings{float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;};
 Varyings vert(Attributes v){Varyings o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.uv=v.uv*_Sheet.xy+_Sheet.zw;return o;}
 half4 frag(Varyings i):SV_Target{return SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*_Tint;}
 ENDHLSL
 }
 }
}
