Shader "Pokemon/PixelFX Unlit"
{
 Properties { _MainTex("Sheet",2D)="white" {} _Sheet("Frame",Vector)=(1,1,0,0) _Tint("Tint",Color)=(1,1,1,1) _Glow("Soft glow",Float)=0 }
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
 float4 _Sheet;float4 _Tint;float _Glow;
 float4 _MainTex_TexelSize;
 CBUFFER_END
 struct Attributes{float4 positionOS:POSITION;float2 uv:TEXCOORD0;};
 struct Varyings{float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;};
 Varyings vert(Attributes v){Varyings o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.uv=v.uv*_Sheet.xy+_Sheet.zw;return o;}
 half4 frag(Varyings i):SV_Target
 {
     half4 c=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
     if(_Glow>0)
     {
         float2 pixel=_MainTex_TexelSize.xy*2;
         half halo=(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+float2(pixel.x,0)).a
             +SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv-float2(pixel.x,0)).a
             +SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+float2(0,pixel.y)).a
             +SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv-float2(0,pixel.y)).a)*0.25*_Glow;
         half alpha=saturate(c.a+halo);
         c.rgb=(c.rgb*c.a*1.25+half3(1,0.38,0.06)*halo)/max(alpha,0.001);
         c.a=alpha;
     }
     return c*_Tint;
 }
 ENDHLSL
 }
 }
}
