Shader "Pokemon/FakeOut Glove"
{
 Properties{_Color("Color",Color)=(1,1,1,1) _Opacity("Opacity",Float)=1 _Outline("Outline",Float)=0.012}
 SubShader{
 Tags{"Queue"="Transparent+20" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"}
 Pass{Cull Off ZWrite On Blend SrcAlpha OneMinusSrcAlpha
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment glove
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 float4 _Color;float _Opacity;float _Outline;
 CBUFFER_END
 struct A{float4 p:POSITION;float3 n:NORMAL;};
 struct V{float4 p:SV_POSITION;float3 normal:TEXCOORD0;float3 world:TEXCOORD1;};
 V vert(A v){V o;o.p=TransformObjectToHClip(v.p.xyz);o.normal=TransformObjectToWorldNormal(v.n);o.world=TransformObjectToWorld(v.p.xyz);return o;}
 half4 glove(V i):SV_Target{
 float facing=abs(dot(normalize(i.normal),normalize(GetWorldSpaceViewDir(i.world))));
 half3 color=facing<0.30?half3(0.035,0.025,0.025):_Color.rgb;
 return half4(color,_Color.a*_Opacity);
 }
 ENDHLSL
 }
 }
}
