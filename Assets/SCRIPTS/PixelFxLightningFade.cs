using System.Collections.Generic;
using UnityEngine;

public class PixelFxLightningFade : MonoBehaviour
{
    public const float Lifetime = 3f;
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
            vertices.Add(start-side);vertices.Add(start+side);vertices.Add(end-side);vertices.Add(end+side);
            uv.Add(new Vector2(0,0));uv.Add(new Vector2(0,1));uv.Add(new Vector2(1,0));uv.Add(new Vector2(1,1));
            indices.Add(n);indices.Add(n+1);indices.Add(n+2);indices.Add(n+2);indices.Add(n+1);indices.Add(n+3);
        }
        mesh=new Mesh{name="PixelFX chain links"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(indices,0);mesh.RecalculateBounds();
        gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
        visual=gameObject.AddComponent<MeshRenderer>();visual.sharedMaterial=material;
        visual.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;visual.receiveShadows=false;
        PixelFxAttackVisual.ApplyFrame(visual,material,0,2,3,1,ref properties);
    }
    void Update()
    {
        age+=Time.deltaTime;
        if(age>=Lifetime){Destroy(gameObject);return;}
        PixelFxAttackVisual.ApplyFrame(visual,material,Mathf.FloorToInt(age*12)%6,2,3,1-age/Lifetime,ref properties);
    }
    void OnDestroy(){if(mesh!=null)Destroy(mesh);}
}
