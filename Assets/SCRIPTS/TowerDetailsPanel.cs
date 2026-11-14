using UnityEngine;

public class TowerDetailsPanel : MonoBehaviour
{
    private UnityEngine.UI.Button removeTowerButton;
    public UnityEngine.UI.Button evolveButton;
    public TMPro.TMP_Text evolveLabel;
    public GameObject evolveFlash;
    private Coroutine evolveFlashRoutine;

    public void EvolveSelectedTower()
    {
        if (SelectedTower == null || wave.State != EnemySpawner.WaveState.PreWave) return;
        var evolution = SelectedTower.GetComponent<TowerEvolution>();
        if (evolution == null || !evolution.CanEvolve) return;
        if (wave.Coins < evolution.evolutionCost)
        {
            if (evolveFlashRoutine != null) StopCoroutine(evolveFlashRoutine);
            evolveFlashRoutine = StartCoroutine(FlashEvolutionCost());
            return;
        }
        FindAnyObjectByType<TowerPlacement>().TryEvolveTower(SelectedTower, out _);
        RefreshEvolutionButton();
    }

    System.Collections.IEnumerator FlashEvolutionCost()
    {
        evolveFlash.SetActive(true);
        yield return new WaitForSecondsRealtime(0.2f);
        evolveFlash.SetActive(false);
        evolveFlashRoutine = null;
    }

    void ClearEvolutionFlash()
    {
        if (evolveFlashRoutine != null) StopCoroutine(evolveFlashRoutine);
        evolveFlashRoutine = null;
        if (evolveFlash != null) evolveFlash.SetActive(false);
    }

    void RefreshEvolutionButton()
    {
        if (removeTowerButton != null)
            removeTowerButton.interactable = SelectedTower != null && wave.State == EnemySpawner.WaveState.PreWave;
        if (evolveButton == null || evolveLabel == null) return;
        var evolution = SelectedTower != null ? SelectedTower.GetComponent<TowerEvolution>() : null;
        bool available = evolution != null && evolution.CanEvolve;
        evolveButton.gameObject.SetActive(SelectedTower != null && evolution != null);
        evolveButton.interactable = available && wave.State == EnemySpawner.WaveState.PreWave;
        if (!available) { evolveLabel.text = "Fully evolved"; return; }
        var next = evolution.nextStagePrefab.GetComponent<TowerEvolution>();
        string name = next != null ? next.pokemonName : evolution.nextStagePrefab.name;
        evolveLabel.text = "Evolve: " + name + "\n(" + evolution.evolutionCost + " Coins)";
    }

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
        selectedRange = null;
    }

    private bool previewEvolutionRange;

    public void SetEvolutionRangePreview(bool preview)
    {
        previewEvolutionRange = preview;
        UpdateSelectedRange();
    }

    private LineRenderer selectedRange;
    private Material selectedRangeMaterial;

    void ShowRange(GameObject tower)
    {
        if (tower == null || tower.GetComponent<SphereCollider>() == null) return;
        if (selectedRangeMaterial == null) selectedRangeMaterial = new Material(Shader.Find("Sprites/Default"));
        var circle = new GameObject("SelectedTowerRange");
        circle.layer = LayerMask.NameToLayer("Ignore Raycast");
        selectedRange = circle.AddComponent<LineRenderer>();
        selectedRange.sharedMaterial = selectedRangeMaterial;
        selectedRange.useWorldSpace = true;
        selectedRange.loop = true;
        selectedRange.positionCount = 96;
        selectedRange.widthMultiplier = 0.06f;
        selectedRange.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        selectedRange.receiveShadows = false;
        var placement = FindAnyObjectByType<TowerPlacement>();
        Color color = placement != null ? placement.validColor : new Color(0, 1, 0, 0.5f);
        selectedRange.startColor = selectedRange.endColor = color;
        outlineObjects.Add(circle);
        UpdateSelectedRange();
    }

    void UpdateSelectedRange()
    {
        if (selectedRange == null || SelectedTower == null) return;
        GameObject rangeSource = SelectedTower;
        var evolution = SelectedTower.GetComponent<TowerEvolution>();
        if (previewEvolutionRange && evolution != null && evolution.CanEvolve)
            rangeSource = evolution.nextStagePrefab;
        var range = rangeSource.GetComponent<SphereCollider>();
        if (range == null) { selectedRange.enabled = false; return; }
        selectedRange.enabled = true;
        Vector3 scale = rangeSource.transform.lossyScale;
        float radius = range.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        Vector3 center = rangeSource == SelectedTower ? SelectedTower.transform.TransformPoint(range.center) :
            SelectedTower.transform.position + SelectedTower.transform.rotation * Vector3.Scale(range.center, scale);
        center.y = SelectedTower.transform.position.y + 0.04f;
        for (int i = 0; i < selectedRange.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / selectedRange.positionCount;
            selectedRange.SetPosition(i, center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius));
        }
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
        if (SelectedTower == null || wave.State != EnemySpawner.WaveState.PreWave) return;
        var placement = FindAnyObjectByType<TowerPlacement>();
        if (placement != null && placement.SellTower(SelectedTower)) Close();
    }

    void OnDestroy()
    {
        ClearOutline();
        if (selectedRangeMaterial != null) Destroy(selectedRangeMaterial);
    }

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
        if (evolveButton != null)
        {
            var hover = evolveButton.GetComponent<EvolutionRangeHover>();
            if (hover == null) hover = evolveButton.gameObject.AddComponent<EvolutionRangeHover>();
            hover.viewer = this;
        }
        removeTowerButton = transform.Find("RemoveSelectedTower").GetComponent<UnityEngine.UI.Button>();
        panel.anchoredPosition = new Vector2(panel.rect.width + 20f, 0f);
    }

    public void Show(GameObject tower)
    {
        ClearEvolutionFlash();
        ClearOutline();
        SelectedTower = tower;
        IsOpen = tower != null;
        OutlineTower(tower);
        ShowRange(tower);
        transform.SetAsLastSibling();
        RefreshEvolutionButton();
    }

    public void Close()
    {
        previewEvolutionRange = false;
        ClearEvolutionFlash();
        ClearOutline();
        SelectedTower = null;
        IsOpen = false;
    }

    void LateUpdate()
    {
        UpdateOutline();
        UpdateSelectedRange();
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
        RefreshEvolutionButton();
        if (IsOpen && SelectedTower == null) Close();
        float width = panel.rect.width + 20f;
        float target = IsOpen ? 0f : width;
        panel.anchoredPosition = new Vector2(Mathf.MoveTowards(panel.anchoredPosition.x,
            target, width * Time.unscaledDeltaTime / Mathf.Max(0.01f, slideDuration)), 0f);
    }
}
