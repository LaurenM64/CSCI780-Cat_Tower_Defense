using UnityEngine;

public class TowerDetailsPanel : MonoBehaviour
{
    public Material selectionOutlineMaterial;
    private readonly System.Collections.Generic.List<GameObject> outlineObjects = new System.Collections.Generic.List<GameObject>();

    void ClearOutline()
    {
        foreach (var outline in outlineObjects)
        {
            if (outline == null) continue;
            outline.SetActive(false);
            Destroy(outline);
        }
        outlineObjects.Clear();
        outlinedMesh = null;
        outlineLine = null;
    }

    private MeshFilter outlinedMesh;
    private LineRenderer outlineLine;
    public float outlineWidthPixels = 2f;

    void OutlineTower(GameObject tower)
    {
        if (tower == null || selectionOutlineMaterial == null) return;
        outlinedMesh = tower.GetComponentInChildren<MeshFilter>();
        if (outlinedMesh == null || outlinedMesh.sharedMesh == null) return;
        var outline = new GameObject("SelectionOutline");
        outline.layer = LayerMask.NameToLayer("Ignore Raycast");
        outlineLine = outline.AddComponent<LineRenderer>();
        outlineLine.sharedMaterial = selectionOutlineMaterial;
        outlineLine.useWorldSpace = true;
        outlineLine.loop = true;
        outlineLine.startColor = outlineLine.endColor = Color.white;
        outlineLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        outlineLine.receiveShadows = false;
        outlineObjects.Add(outline);
        UpdateOutline();
    }

    static float Cross(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.x-a.x)*(c.y-a.y)-(b.y-a.y)*(c.x-a.x);
    }

    void UpdateOutline()
    {
        if (outlinedMesh == null || outlineLine == null || !IsOpen) return;
        var camera = Camera.main;
        if (camera == null) return;
        var points = new System.Collections.Generic.List<Vector2>();
        // Clip the outline to the ground so buried model geometry does not create a floating contour.
        var vertices = outlinedMesh.sharedMesh.vertices;
        var triangles = outlinedMesh.sharedMesh.triangles;
        float ground = SelectedTower.transform.position.y;
        var worldVertices = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++) worldVertices[i] = outlinedMesh.transform.TransformPoint(vertices[i]);
        var visiblePoints = new System.Collections.Generic.List<Vector3>();
        foreach (var vertex in worldVertices) if (vertex.y >= ground) visiblePoints.Add(vertex);
        for (int i = 0; i < triangles.Length; i += 3)
        {
            for (int edge = 0; edge < 3; edge++)
            {
                Vector3 a = worldVertices[triangles[i + edge]];
                Vector3 b = worldVertices[triangles[i + (edge + 1) % 3]];
                if ((a.y < ground) == (b.y < ground)) continue;
                visiblePoints.Add(Vector3.Lerp(a, b, (ground - a.y) / (b.y - a.y)));
            }
        }
        foreach (var vertex in visiblePoints)
        {
            Vector3 point = camera.WorldToScreenPoint(vertex);
            if (point.z <= camera.nearClipPlane) { outlineLine.enabled = false; return; }
            points.Add(new Vector2(point.x, point.y));
        }
        points.Sort((a,b)=>a.x == b.x ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
        var hull = new System.Collections.Generic.List<Vector2>();
        foreach (var point in points)
        {
            while (hull.Count >= 2 && Cross(hull[hull.Count-2],hull[hull.Count-1],point) <= 0) hull.RemoveAt(hull.Count-1);
            hull.Add(point);
        }
        int lower = hull.Count;
        for (int i=points.Count-2;i>=0;i--)
        {
            while (hull.Count > lower && Cross(hull[hull.Count-2],hull[hull.Count-1],points[i]) <= 0) hull.RemoveAt(hull.Count-1);
            hull.Add(points[i]);
        }
        if (hull.Count < 4) { outlineLine.enabled = false; return; }
        hull.RemoveAt(hull.Count-1);
        float depth = camera.nearClipPlane + 1f;
        outlineLine.enabled = true;
        outlineLine.positionCount = hull.Count;
        for(int i=0;i<hull.Count;i++) outlineLine.SetPosition(i,camera.ScreenToWorldPoint(new Vector3(hull[i].x,hull[i].y,depth)));
        outlineLine.widthMultiplier = Vector3.Distance(camera.ScreenToWorldPoint(new Vector3(0,0,depth)),
            camera.ScreenToWorldPoint(new Vector3(outlineWidthPixels,0,depth)));
    }

    public void RemoveSelectedTower()
    {
        if (SelectedTower == null) return;
        var placement = FindAnyObjectByType<TowerPlacement>();
        if (placement != null && placement.SellTower(SelectedTower)) Close();
    }

    void OnDestroy() { ClearOutline(); }

    public float slideDuration = 0.25f;
    public GameObject SelectedTower { get; private set; }
    public bool IsOpen { get; private set; }
    private RectTransform panel;
    private EnemySpawner wave;
    private readonly Vector3[] corners = new Vector3[4];

    void Awake()
    {
        panel = GetComponent<RectTransform>();
        wave = FindAnyObjectByType<EnemySpawner>();
        panel.anchoredPosition = new Vector2(panel.rect.width + 20f, 0f);
    }

    public void Show(GameObject tower)
    {
        ClearOutline();
        SelectedTower = tower;
        IsOpen = tower != null;
        OutlineTower(tower);
        transform.SetAsLastSibling();
    }

    public void Close()
    {
        ClearOutline();
        SelectedTower = null;
        IsOpen = false;
    }

    void LateUpdate()
    {
        UpdateOutline();
        if (wave == null) return;
        var canvasRect = panel.parent.GetComponent<RectTransform>();
        wave.startButton.GetComponent<RectTransform>().GetWorldCorners(corners);
        float bottom = canvasRect.InverseTransformPoint(corners[1]).y + 16f;
        wave.statusLabel.rectTransform.GetWorldCorners(corners);
        float top = canvasRect.InverseTransformPoint(corners[0]).y - 16f;
        var bounds = canvasRect.rect;
        panel.anchorMin = new Vector2(0.75f, Mathf.Clamp01((bottom - bounds.yMin) / bounds.height));
        panel.anchorMax = new Vector2(1f, Mathf.Clamp01((top - bounds.yMin) / bounds.height));
        panel.sizeDelta = Vector2.zero;
    }

    void Update()
    {
        if (IsOpen && SelectedTower == null) Close();
        float width = panel.rect.width + 20f;
        float target = IsOpen ? 0f : width;
        panel.anchoredPosition = new Vector2(Mathf.MoveTowards(panel.anchoredPosition.x,
            target, width * Time.unscaledDeltaTime / Mathf.Max(0.01f, slideDuration)), 0f);
    }
}
