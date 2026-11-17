Shader "Pokemon/PixelFX Outlined Projectile"
{
 Properties { _MainTex("Sheet",2D)="white" {} _Sheet("Frame",Vector)=(1,1,0,0) _Tint("Tint",Color)=(1,1,1,1) _Glow("Soft glow",Float)=0 _TailFade("Use vertex fade",Float)=0 }
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
 float4 _Sheet;float4 _Tint;float _Glow;float _TailFade;
 float4 _MainTex_TexelSize;
 CBUFFER_END
 struct Attributes{float4 positionOS:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
 struct Varyings{float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
 Varyings vert(Attributes v){Varyings o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.uv=v.uv*_Sheet.xy+_Sheet.zw;o.color=v.color;return o;}
 half4 frag(Varyings i):SV_Target
 {
     half4 c=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
     float2 px=_MainTex_TexelSize.xy;
     half solid=step(0.35,c.a), neighbour=0;
     [unroll] for(int x=-1;x<=1;x++) [unroll] for(int y=-1;y<=1;y++)
         neighbour=max(neighbour,step(0.35,SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+float2(x,y)*px).a));
     half halo=(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+float2(3*px.x,0)).a
         +SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv-float2(3*px.x,0)).a
         +SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+float2(0,3*px.y)).a
         +SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv-float2(0,3*px.y)).a)*0.35;
     half3 rgb=solid>0?c.rgb:(neighbour>0?half3(0,0,0):half3(0.35,1,0.08));
     half alpha=max(neighbour,halo);
     return half4(rgb,alpha)*_Tint;
 }

 ENDHLSL
 }
 }
}
