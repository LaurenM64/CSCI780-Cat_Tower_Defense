using UnityEngine;

// Presentation only. BulletSeed/MagicalLeaf retain movement, collision and damage.
public class PixelFxProjectileVisual : MonoBehaviour
{
    public Material material;
    public Material trailMaterial;
    public int columns=4, rows=3, frames=12;
    public float worldSize=0.65f;
    public float trailSeconds=0.035f;
    public float trailWidth=0.045f;
    Mesh mesh;
    MeshRenderer visual;
    Transform graphic;
    MaterialPropertyBlock properties;
    float clock;
    void Awake(){Build();}
    void Build()
    {
        if(graphic!=null || material==null)return;
        var go=new GameObject("PixelFX Projectile");go.transform.SetParent(transform,false);graphic=go.transform;
        mesh=new Mesh{name="PixelFX projectile quad"};
        mesh.vertices=new[]{new Vector3(-0.5f,-0.5f,0),new Vector3(0.5f,-0.5f,0),new Vector3(-0.5f,0.5f,0),new Vector3(0.5f,0.5f,0)};
        mesh.uv=new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(0,1),new Vector2(1,1)};
        mesh.triangles=new[]{0,2,1,2,3,1};mesh.RecalculateBounds();
        go.AddComponent<MeshFilter>().sharedMesh=mesh;visual=go.AddComponent<MeshRenderer>();visual.sharedMaterial=material;
        visual.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;visual.receiveShadows=false;
        if(trailMaterial!=null)
        {
            var trailGo=new GameObject("Short green trail");trailGo.transform.SetParent(transform,false);
            var trail=trailGo.AddComponent<TrailRenderer>();trail.sharedMaterial=trailMaterial;
            trail.time=trailSeconds;trail.minVertexDistance=0.05f;trail.widthMultiplier=trailWidth;
            trail.widthCurve=AnimationCurve.Linear(0,1,1,0);
            trail.startColor=new Color(0.8f,1f,0.5f,0.65f);trail.endColor=new Color(0.25f,0.8f,0.2f,0);
            trail.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;trail.receiveShadows=false;
        }
    }
    public void RefreshVisual(Camera camera)
    {
        Build();if(graphic==null)return;
        if(camera!=null)graphic.rotation=camera.transform.rotation;
        Vector3 scale=transform.lossyScale;
        graphic.localScale=new Vector3(worldSize/Mathf.Max(0.001f,Mathf.Abs(scale.x)),worldSize/Mathf.Max(0.001f,Mathf.Abs(scale.y)),worldSize/Mathf.Max(0.001f,Mathf.Abs(scale.z)));
        PixelFxAttackVisual.ApplyFrame(visual,material,Mathf.FloorToInt(clock*12f)%Mathf.Max(1,frames),columns,rows,1,ref properties);
    }
    void LateUpdate(){clock+=Time.deltaTime;RefreshVisual(Camera.main);}
    void OnDestroy(){if(mesh!=null)Destroy(mesh);}
}
