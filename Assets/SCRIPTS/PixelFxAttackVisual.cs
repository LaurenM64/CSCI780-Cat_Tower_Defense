using System.Collections.Generic;
using UnityEngine;

// Sprite sheets rendered from patch-gg/pixelfx; visual only, never applies damage.
public class PixelFxAttackVisual : MonoBehaviour
{
    public Material fireMaterial;
    public Material lightningMaterial;
    MeshRenderer fireRenderer;
    Mesh fireMesh;
    Vector3[] trailVertices;
    const int FireArcs = 24, FireRings = 24;
    MaterialPropertyBlock properties;
    float fireClock;
    PixelFxFireBurst activeFire;
    static readonly int Sheet = Shader.PropertyToID("_Sheet");
    static readonly int Tint = Shader.PropertyToID("_Tint");

    public void ShowFire(Vector3 direction, float range, float angle, float alpha = 1f)
    {
        if (fireMaterial == null) return;
        if (fireRenderer == null) BuildFire(range, angle);
        fireRenderer.enabled = true;
        fireRenderer.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        fireClock += Time.deltaTime;
        ApplyFrame(fireRenderer, fireMaterial, Mathf.FloorToInt(fireClock * 12f) % 18, 4, 5, alpha, ref properties);
    }

    public void EmitFire(Vector3 direction, float range, float angle)
    {
        if (fireMaterial == null) return;
        if (activeFire != null)
        {
            activeFire.Refresh(direction, range, angle);
            return;
        }
        var effect = new GameObject("PixelFX Fire Stream (1s hold)");
        effect.transform.SetParent(transform, false);
        var visual = effect.AddComponent<PixelFxAttackVisual>();
        visual.fireMaterial = fireMaterial;
        activeFire = effect.AddComponent<PixelFxFireBurst>();
        activeFire.Initialize(visual, direction, range, angle);
    }

    public void HideFire()
    {
        if (fireRenderer != null) fireRenderer.enabled = false;
    }

    void BuildFire(float range, float angle)
    {
        var go = new GameObject("PixelFX Fire");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.up * 0.35f;
        var filter = go.AddComponent<MeshFilter>();
        fireRenderer = go.AddComponent<MeshRenderer>();
        fireRenderer.sharedMaterial = fireMaterial;
        fireRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        fireRenderer.receiveShadows = false;
        const int arcs = FireArcs, rings = FireRings;
        var vertices = new Vector3[(arcs+1)*(rings+1)];
        var uv = new Vector2[vertices.Length];
        var indices = new List<int>();
        for (int r=0;r<=rings;r++) for(int a=0;a<=arcs;a++)
        {
            float radial=(float)r/rings, across=(float)a/arcs;
            float theta=(across-0.5f)*angle*Mathf.Deg2Rad;
            int i=r*(arcs+1)+a;
            vertices[i]=new Vector3(Mathf.Sin(theta),0,Mathf.Cos(theta))*radial*range;
            uv[i]=new Vector2((12f+96f*radial)/160f, (116f-(72f-26f*radial+(across-0.5f)*68f))/116f);
            if(r<rings && a<arcs)
            {
                int j=i+arcs+1;
                indices.Add(i);indices.Add(j);indices.Add(i+1);
                indices.Add(i+1);indices.Add(j);indices.Add(j+1);
            }
        }
        trailVertices=vertices;
        fireMesh=new Mesh {name="PixelFX fire fan"};
        fireMesh.MarkDynamic();
        fireMesh.vertices=vertices;fireMesh.uv=uv;fireMesh.SetTriangles(indices,0);fireMesh.RecalculateBounds();
        filter.sharedMesh=fireMesh;
    }

    public void ShowTrailingFire(PixelFxFireBurst trail, float range, float angle, float alpha)
    {
        ShowFire(Vector3.forward,range,angle,alpha);
        // Each radial layer remembers the direction when that flame left the mouth.
        // Near layers track the model now; the outer layers retain older directions.
        for(int r=0;r<=FireRings;r++)
        {
            float radial=(float)r/FireRings;
            Vector3 aim=trail.DirectionAt(radial*PixelFxFireBurst.TravelSeconds);
            aim=fireRenderer.transform.InverseTransformDirection(aim);
            for(int a=0;a<=FireArcs;a++)
            {
                float degrees=((float)a/FireArcs-0.5f)*angle;
                trailVertices[r*(FireArcs+1)+a]=Quaternion.AngleAxis(degrees,Vector3.up)*aim*range*radial;
            }
        }
        fireMesh.vertices=trailVertices;
        fireMesh.RecalculateBounds();
    }

    public void Lightning(IReadOnlyList<Vector3> points)
    {
        if(lightningMaterial==null || points.Count<2)return;
        var effect=new GameObject("PixelFX Lightning (1.5s)");
        effect.AddComponent<PixelFxLightningFade>().Initialize(points,lightningMaterial);
    }

    internal static void ApplyFrame(MeshRenderer renderer, Material material, int frame, int columns, int rows, float alpha, ref MaterialPropertyBlock block)
    {
        if(block==null)block=new MaterialPropertyBlock();
        block.SetVector(Sheet,new Vector4(1f/columns,1f/rows,(frame%columns)/(float)columns,1f-(frame/columns+1f)/rows));
        block.SetColor(Tint,new Color(1,1,1,alpha));
        renderer.SetPropertyBlock(block);
    }
    void OnDisable(){HideFire();}
    void OnDestroy(){if(fireMesh!=null)Destroy(fireMesh);}
}
