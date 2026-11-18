using UnityEngine;

public class TowerDetailsPanel : MonoBehaviour
{
    [Header("UI References")]
    public TMPro.TMP_Text infoLabel; // Slot for your new text component
    
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
        if (!evolution.IsUnlocked(wave)) return;
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
        {
            // Hide the remove button entirely if nothing is selected
            removeTowerButton.gameObject.SetActive(SelectedTower != null);
            removeTowerButton.interactable = SelectedTower != null && wave.State == EnemySpawner.WaveState.PreWave;
        }
        
        if (evolveButton == null || evolveLabel == null) return;
        var evolution = SelectedTower != null ? SelectedTower.GetComponent<TowerEvolution>() : null;
        bool available = evolution != null && evolution.CanEvolve;
        evolveButton.gameObject.SetActive(SelectedTower != null && evolution != null);
        
        if (SelectedTower == null) return; // Stop processing if empty
        
        evolveButton.interactable = available && evolution.IsUnlocked(wave) && wave.State == EnemySpawner.WaveState.PreWave;
        if (!available) { evolveLabel.text = "Fully evolved"; return; }
        var next = evolution.nextStagePrefab.GetComponent<TowerEvolution>();
        string name = next != null ? next.pokemonName : evolution.nextStagePrefab.name;
        evolveLabel.text = "Evolve: " + name + "\n(" + evolution.evolutionCost + " Coins)";
        if (!evolution.IsUnlocked(wave)) evolveLabel.text += "\nAfter Wave " + (evolution.earliestWave - 1);
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

    void HighlightFootprint(GameObject tower)
    {
        if (tower == null || selectionOutlineMaterial == null) return;
        var footprint = tower.transform.Find("PlacementFootprint");
        var placement = FindAnyObjectByType<TowerPlacement>();
        float radius = placement != null ? placement.modelFootprintRadius : 0.5f;
        Vector3 center = Vector3.up * 0.04f;
        if (footprint != null)
        {
            center = footprint.localPosition + Vector3.up * 0.015f;
            var mesh = footprint.GetComponent<MeshFilter>();
            if (mesh != null && mesh.sharedMesh != null)
            {
                var bounds = mesh.sharedMesh.bounds;
                radius = Mathf.Max(bounds.extents.x * Mathf.Abs(footprint.localScale.x),
                    bounds.extents.z * Mathf.Abs(footprint.localScale.z));
                center += Vector3.Scale(bounds.center, footprint.localScale);
            }
        }
        var ring = new GameObject("SelectionOutline");
        ring.layer = LayerMask.NameToLayer("Ignore Raycast");
        ring.transform.SetParent(tower.transform, false);
        ring.transform.localPosition = center;
        var line = ring.AddComponent<LineRenderer>();
        line.sharedMaterial = selectionOutlineMaterial;
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = 64;
        line.widthMultiplier = 0.025f;
        line.startColor = line.endColor = Color.white;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        for (int i = 0; i < line.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / line.positionCount;
            line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
        }
        outlineObjects.Add(ring);
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
        
        // Ensure the panel starts completely visible on screen
        panel.anchoredPosition = Vector2.zero;
        if (infoLabel != null) infoLabel.text = "Click a tower to see details";
    }

    public void Show(GameObject tower)
    {
        ClearEvolutionFlash();
        ClearOutline();
        SelectedTower = tower;
        IsOpen = tower != null;
        HighlightFootprint(tower);
        ShowRange(tower);
        transform.SetAsLastSibling();
        
        UpdateInfoText(tower);
        RefreshEvolutionButton();
    }

    public void Close()
    {
        previewEvolutionRange = false;
        ClearEvolutionFlash();
        ClearOutline();
        SelectedTower = null;
        IsOpen = false;
        
        if (infoLabel != null) infoLabel.text = "Click a tower to see details";
        RefreshEvolutionButton(); // This will hide the buttons
    }

    void UpdateInfoText(GameObject tower)
    {
        if (infoLabel == null || tower == null) return;
        string tName = tower.name.ToLower();

        if (tName.Contains("sprigatito"))
            infoLabel.text = "<b>SPRIGATITO</b>\nType: Grass\nDamage: Single\nMove: Bullet Seed\nSpeed: Slow\nSpecial: None";
        else if (tName.Contains("floragato"))
            infoLabel.text = "<b>FLORAGATO</b>\nType: Grass\nDamage: Piercing\nMove: Magical Leaf\nSpeed: Slow\nSpecial: Pierces 2 enemies";
        else if (tName.Contains("meowscarada"))
            infoLabel.text = "<b>MEOWSCARADA</b>\nType: Grass/Dark\nDamage: Piercing\nMove: Energy Ball\nSpeed: Medium\nSpecial: Pierces 4 enemies, sets Toxic Spikes";
        else if (tName.Contains("litten"))
            infoLabel.text = "<b>LITTEN</b>\nType: Fire\nDamage: Spread\nMove: Flamethrower\nSpeed: Continuous\nSpecial: None";
        else if (tName.Contains("torracat"))
            infoLabel.text = "<b>TORRACAT</b>\nType: Fire\nDamage: Spread\nMove: Heat Wave\nSpeed: Continuous\nSpecial: None";
        else if (tName.Contains("incineroar"))
            infoLabel.text = "<b>INCINEROAR</b>\nType: Fire/Dark\nDamage: Spread\nMove: Flare Blitz\nSpeed: Continuous\nSpecial: Uses Fake Out";
        else if (tName.Contains("shinx"))
            infoLabel.text = "<b>SHINX</b>\nType: Electric\nDamage: Chain Lightning\nMove: Shock Wave\nSpeed: Slow\nSpecial: Chains 3 enemies";
        else if (tName.Contains("luxio") || tName.Contains("luxray"))
        {
            // Checks if it's the final evolution stage (since the graphic labeled the 3rd stage "Luxio" as well)
            if (tName.Contains("luxray") || tower.GetComponent<TowerEvolution>() == null)
                infoLabel.text = "<b>LUXRAY</b>\nType: Electric\nDamage: Chain Lightning\nMove: Thunderbolt\nSpeed: Slow\nSpecial: Unlimited Chains / 0.5s Paralyze ALL";
            else
                infoLabel.text = "<b>LUXIO</b>\nType: Electric\nDamage: Chain Lightning\nMove: Discharge\nSpeed: Slow\nSpecial: Chains 6 / Paralyzes every other hit for 1.5s";
        }
        else
            infoLabel.text = "<b>" + tower.name.Replace("(Clone)", "") + "</b>\nNo data available.";
    }

    void LateUpdate()
    {
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
        
        // Target is now permanently 0f so the panel stays fixed on screen
        float width = panel.rect.width + 20f;
        float target = 0f; 
        panel.anchoredPosition = new Vector2(Mathf.MoveTowards(panel.anchoredPosition.x,
            target, width * Time.unscaledDeltaTime / Mathf.Max(0.01f, slideDuration)), 0f);
    }
}