Shader "Pokemon/Continuous Grass"
{
Properties{_BaseMap("Grass",2D)="white"{} _FillColor("Gap fill",Color)=(0.22,0.6,0.12,1) _EdgeOnly("Map edge only",Float)=0}
SubShader{Tags{"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}
Pass{HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;float4 _FillColor;float _EdgeOnly;
CBUFFER_END
struct A{float4 p:POSITION;float2 uv:TEXCOORD0;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;float3 world:TEXCOORD1;};
V vert(A a){V o;o.p=TransformObjectToHClip(a.p.xyz);o.world=TransformObjectToWorld(a.p.xyz);o.uv=a.uv;return o;}
float hash(float2 p){p=frac(p*float2(123.34,456.21));p+=dot(p,p+45.32);return frac(p.x*p.y);}
float noise(float2 p){float2 cell=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(cell),hash(cell+float2(1,0)),f.x),lerp(hash(cell+float2(0,1)),hash(cell+1),f.x),f.y);}
half4 frag(V i):SV_Target{
 float n=noise(i.world.xz*.5)*.65+noise(i.world.xz*2)*.35;
 half3 grass=lerp(half3(.18,.40,.075),half3(.28,.54,.12),n);
 float2 blades=frac(i.world.xz*5),cell=floor(i.world.xz*5);
 float h=hash(cell);float blade=(1-smoothstep(.018,.045,abs(blades.x-.35-(blades.y-.5)*.22)))*(1-smoothstep(.15,.7,blades.y));grass*=1-blade*.12;
 half3 c=grass;
 if(_EdgeOnly>0.5){
  half3 ink=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).rgb;
  float red=step(.45,ink.r)*step(ink.g*1.7,ink.r)*step(ink.b*1.7,ink.r);
  float white=step(.65,min(ink.r,min(ink.g,ink.b)))*step(max(ink.r,max(ink.g,ink.b))-min(ink.r,min(ink.g,ink.b)),.14);
  float black=1-smoothstep(.04,.14,max(ink.r,max(ink.g,ink.b)));
  float centerStrip=step(abs(i.uv.y-.4705),.0085)*step(.125,i.uv.x)*step(i.uv.x,.9);
  float centerRing=step(.397,i.uv.x)*step(i.uv.x,.627)*step(.36,i.uv.y)*step(i.uv.y,.605);
  float track=saturate(red+white+black*max(centerStrip,centerRing));c=lerp(grass,ink,track);
 }
 Light light=GetMainLight(TransformWorldToShadowCoord(i.world));return half4(c*(.85+.15*light.shadowAttenuation),1);
}

ENDHLSL}}
}
