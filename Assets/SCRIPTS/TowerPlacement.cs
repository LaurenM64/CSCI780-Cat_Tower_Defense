using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TowerPlacement : MonoBehaviour
{
    [System.Serializable]
    public class TowerOffer
    {
        public string towerName;
        public int cost;
        public UnityEngine.UI.Image button;
    }
    public TowerOffer[] offers;
    public TowerDetailsPanel detailsPanel;
    public TMPro.TMP_Text pokemonLabel;
    public int waveOneTowerLimit = 2;
    public int waveTwoTowerLimit = 5;
    public int CurrentTowerLimit => wave.WaveNumber == 1 ? waveOneTowerLimit : waveTwoTowerLimit;
    [Tooltip("Radius used to check clearance against other towers.")]
    public float unitClearanceRadius = 0.75f;
    [Tooltip("Horizontal model footprint used only for track and walls.")]
    public float modelFootprintRadius = 0.5f;
    private readonly List<GameObject> placedTowers = new List<GameObject>();
    public int PlacedTowerCount { get { placedTowers.RemoveAll(t => t == null); return placedTowers.Count; } }
    public bool AtTowerLimit => PlacedTowerCount >= CurrentTowerLimit;
    public bool IsOfferLocked(int index) => wave.WaveNumber == 1 && index >= 2;
    private EnemySpawner wave;
    private readonly Dictionary<int, Coroutine> flashes = new Dictionary<int, Coroutine>();
    public enum BuildMode { None, BuildSprigatito }
    public BuildMode currentMode = BuildMode.None;
    [Header("Prefabs & Settings")]
    public GameObject sprigatitoPrefab;
    public Camera mainCamera;
    public LayerMask blockingLayers;
    [Header("Preview Settings")]
    public GameObject previewPrefab;
    private GameObject currentPreview;
    private Renderer previewRenderer;
    private LineRenderer rangePreview;
    private Material rangeMaterial;
    public Color validColor = new Color(0, 1, 0, 0.5f);
    public Color invalidColor = new Color(1, 0, 0, 0.5f);
    [Header("UI Buttons")]
    public Image sprigatitoButton;
    public Image removeButton;
    public Color defaultColor = Color.white;
    public Color highlightColor = Color.yellow;

    public bool HasPlacementClearance(Vector3 position)
    {
        return !Physics.CheckSphere(position, unitClearanceRadius, LayerMask.GetMask("Tower"), QueryTriggerInteraction.Ignore)
            && !Physics.CheckSphere(position, modelFootprintRadius, LayerMask.GetMask("Track"), QueryTriggerInteraction.Ignore);
    }
    void RefreshOfferLocks()
    {
        for (int i = 0; i < offers.Length; i++)
        {
            bool locked = IsOfferLocked(i);
            offers[i].button.GetComponent<UnityEngine.UI.Button>().interactable = !locked;
            var overlay = offers[i].button.transform.Find("WaveLockedOverlay");
            if (overlay != null) overlay.gameObject.SetActive(locked);
        }
    }
    public void SelectTower(int index)
    {
        if (wave.State != EnemySpawner.WaveState.PreWave) return;
        if (IsOfferLocked(index) || AtTowerLimit) { CancelPurchase(); return; }
        if (wave.Coins < offers[index].cost) { CancelPurchase(); FlashUnaffordable(index); return; }
        if (index == 0) SetMode(currentMode == BuildMode.BuildSprigatito ? BuildMode.None : BuildMode.BuildSprigatito);
        else CancelPurchase();
    }
    void FlashUnaffordable(int index)
    {
        if (flashes.TryGetValue(index, out var running)) StopCoroutine(running);
        flashes[index] = StartCoroutine(FlashButton(index));
    }
    IEnumerator FlashButton(int index)
    {
        var overlay = offers[index].button.transform.Find("InsufficientCoinsFlash").gameObject;
        overlay.SetActive(true);
        yield return new WaitForSecondsRealtime(0.2f);
        overlay.SetActive(false);
        flashes.Remove(index);
    }
    public void CancelPurchase() { SetMode(BuildMode.None); }
    public bool TryPlaceTower(Vector3 position)
    {
        if (wave.State != EnemySpawner.WaveState.PreWave || currentMode != BuildMode.BuildSprigatito) return false;
        if (AtTowerLimit || !HasPlacementClearance(position)) return false;
        if (!wave.TrySpendCoins(offers[0].cost)) { FlashUnaffordable(0); CancelPurchase(); return false; }
        var tower = Instantiate(sprigatitoPrefab, position, Quaternion.identity);
        var purchase = tower.AddComponent<TowerPurchase>();
        purchase.purchasePrice = offers[0].cost;
        placedTowers.Add(tower);
        CancelPurchase();
        return true;
    }
    public bool SellTower(GameObject tower)
    {
        var purchase = tower.GetComponent<TowerPurchase>();
        if (purchase == null || !purchase.TrySell(wave)) return false;
        placedTowers.Remove(tower);
        if (detailsPanel != null && detailsPanel.SelectedTower == tower) detailsPanel.Close();
        return true;
    }
    void Start()
    {
        wave = FindAnyObjectByType<EnemySpawner>();
        RefreshOfferLocks();
        currentPreview = Instantiate(previewPrefab);
        Vector3 scale = currentPreview.transform.localScale;
        scale.x = scale.z = unitClearanceRadius * 2f;
        currentPreview.transform.localScale = scale;
        previewRenderer = currentPreview.GetComponentInChildren<Renderer>();
        var circle = new GameObject("RangePreview");
        circle.transform.SetParent(currentPreview.transform, false);
        rangePreview = circle.AddComponent<LineRenderer>();
        rangePreview.useWorldSpace = true;
        rangePreview.loop = true;
        rangePreview.positionCount = 96;
        rangePreview.widthMultiplier = 0.06f;
        rangeMaterial = new Material(Shader.Find("Sprites/Default"));
        rangePreview.sharedMaterial = rangeMaterial;
        rangePreview.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rangePreview.receiveShadows = false;
        currentPreview.SetActive(false);
    }
    void OnDestroy()
    {
        if (rangeMaterial != null) Destroy(rangeMaterial);
        if (currentPreview != null) Destroy(currentPreview);
    }
    public void ToggleSprigatitoMode() { SelectTower(0); }
    public void ToggleRemoveMode() { CancelPurchase(); }
    void SetMode(BuildMode newMode)
    {
        currentMode = newMode;
        if (newMode != BuildMode.None && detailsPanel != null) detailsPanel.Close();
        sprigatitoButton.color = currentMode == BuildMode.BuildSprigatito ? highlightColor : defaultColor;
        removeButton.color = defaultColor;
        if (currentMode == BuildMode.None && currentPreview != null) currentPreview.SetActive(false);
    }
    void LateUpdate()
    {
        if (pokemonLabel == null) return;
        pokemonLabel.text = "Pokemon " + PlacedTowerCount + "/" + CurrentTowerLimit;
        var coins = wave.coinLabel.rectTransform;
        var rect = pokemonLabel.rectTransform;
        rect.anchorMin = coins.anchorMin; rect.anchorMax = coins.anchorMax; rect.pivot = coins.pivot;
        rect.sizeDelta = new Vector2(coins.sizeDelta.x, 60f);
        rect.anchoredPosition = coins.anchoredPosition + new Vector2(0, coins.rect.height);
    }
    void Update()
    {
        RefreshOfferLocks();
        if (wave.State != EnemySpawner.WaveState.PreWave && currentMode == BuildMode.BuildSprigatito) CancelPurchase();
        if (currentMode == BuildMode.None && !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
        {
            Ray selectionRay = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(selectionRay, out RaycastHit selectionHit, Mathf.Infinity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
                && selectionHit.collider.gameObject.layer == LayerMask.NameToLayer("Tower")) detailsPanel?.Show(selectionHit.collider.gameObject);
            else detailsPanel?.Close();
        }
        if (currentMode == BuildMode.None || EventSystem.current.IsPointerOverGameObject()) { currentPreview.SetActive(false); return; }
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) { currentPreview.SetActive(false); return; }
        currentPreview.SetActive(true);
        currentPreview.transform.position = hit.point;
        rangePreview.enabled = currentMode == BuildMode.BuildSprigatito;
        if (currentMode == BuildMode.BuildSprigatito)
        {
            bool canAction = hit.collider.CompareTag("Off_Track") && !AtTowerLimit && HasPlacementClearance(hit.point);
            Color color = canAction ? validColor : invalidColor;
            previewRenderer.material.color = color;
            var range = sprigatitoPrefab.GetComponent<SphereCollider>();
            var scale = sprigatitoPrefab.transform.lossyScale;
            float radius = range.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            rangePreview.startColor = rangePreview.endColor = color;
            for (int i = 0; i < rangePreview.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2f / rangePreview.positionCount;
                rangePreview.SetPosition(i, hit.point + new Vector3(Mathf.Cos(angle) * radius, 0.04f, Mathf.Sin(angle) * radius));
            }
            if (Input.GetMouseButtonDown(0) && canAction) TryPlaceTower(hit.point);
        }
    }
}
