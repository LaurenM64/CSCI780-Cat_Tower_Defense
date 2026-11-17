using System.Collections.Generic;
using UnityEngine;

public class PixelFxLightningFade : MonoBehaviour
{
    public const float Lifetime = 1.5f;
    float age;
    Mesh mesh;
    MeshRenderer visual;
    Material material;
    MaterialPropertyBlock properties;
    public void Initialize(IReadOnlyList<Vector3> points,Material sharedMaterial)
    {
        material=sharedMaterial;
        var vertices=new List<Vector3>();var uv=new List<Vector2>();var indices=new List<int>();
        for(int i=1;i<points.Count;i++)
        {
            Vector3 start=points[i-1]+Vector3.up*0.4f,end=points[i]+Vector3.up*0.4f;
            Vector3 direction=end-start;if(direction.sqrMagnitude<0.0001f)continue;
            Vector3 side=Vector3.Cross(direction.normalized,Vector3.up).normalized*0.4f;
            if(side.sqrMagnitude<0.0001f)side=Vector3.right*0.4f;
            int n=vertices.Count;
            // Pointed caps avoid the square ends of a textured quad.
            for(int cap=0;cap<4;cap++)
            {
                float t=cap==0?0f:cap==1?0.06f:cap==2?0.94f:1f;
                float width=(cap==0||cap==3)?0f:1f;
                Vector3 center=Vector3.Lerp(start,end,t);
                vertices.Add(center-side*width);vertices.Add(center+side*width);
                uv.Add(new Vector2(t,0));uv.Add(new Vector2(t,1));
                if(cap<3){int j=n+cap*2;indices.Add(j);indices.Add(j+1);indices.Add(j+2);indices.Add(j+2);indices.Add(j+1);indices.Add(j+3);}
            }
        }
        mesh=new Mesh{name="PixelFX chain links"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(indices,0);mesh.RecalculateBounds();
        gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
        visual=gameObject.AddComponent<MeshRenderer>();visual.sharedMaterial=material;
        visual.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;visual.receiveShadows=false;
        PixelFxAttackVisual.ApplyFrame(visual,material,Random.Range(0,6),2,3,1,ref properties);
    }
    void Update()
    {
        age+=Time.deltaTime;
        if(age>=Lifetime){Destroy(gameObject);return;}
        // Only opacity changes: the sprite frame and geometry stay fixed for this strike.
        properties.SetColor("_Tint",new Color(1,1,1,1-age/Lifetime));
        visual.SetPropertyBlock(properties);
    }
    void OnDestroy(){if(mesh!=null)Destroy(mesh);}
}
