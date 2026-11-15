using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Splines;

public class EnemySpawner : MonoBehaviour
{
    public enum WaveState { PreWave, Running, Lost }
    public GameObject rattataPrefab;
    public GameObject mausholdPrefab;
    public GameObject togedemaruPrefab;
    public SplineContainer trackSpline;
    public float spawnRate = 2f;
    public int startingCoins = 5;
    public int waveOneReward = 10;
    public int Coins { get; private set; }
    public TMP_Text coinLabel;
    public UnityEngine.RectTransform buyButtonRect;

    public TMP_Text centerMessageText;
    public GameObject gameOverOverlay;

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        Coins += amount;
        UpdateUI();
    }

    public bool TrySpendCoins(int cost)
    {
        if (State != WaveState.PreWave || cost < 0 || Coins < cost) return false;
        Coins -= cost;
        UpdateUI();
        return true;
    }
    public WaveState State { get; private set; } = WaveState.PreWave;
    public int WaveNumber { get; private set; } = 1;
    public int SpawnedCount { get; private set; }
    public int AliveCount => enemies.Count;
    public UnityEngine.UI.Button startButton;
    public TMP_Text waveLabel;
    public TMP_Text statusLabel;
    public UnityEngine.RectTransform removeButtonRect;
    private readonly List<GameObject> enemies = new List<GameObject>();

    void Start() { Coins = startingCoins; UpdateUI(); }

    void LateUpdate()
    {
        if (startButton == null || removeButtonRect == null) return;
        var canvas = startButton.GetComponentInParent<Canvas>();
        var canvasRect = canvas.GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        removeButtonRect.GetWorldCorners(corners);
        Vector3 topLeft = canvasRect.InverseTransformPoint(corners[1]);
        Vector3 topRight = canvasRect.InverseTransformPoint(corners[2]);
        var rect = startButton.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0);
        rect.sizeDelta = new Vector2(Mathf.Min(360f, topRight.x - topLeft.x), 90f);
        rect.anchoredPosition = new Vector2((topLeft.x + topRight.x) * 0.5f - canvasRect.rect.xMin,
            topLeft.y - canvasRect.rect.yMin + 12f);
        if (coinLabel != null && buyButtonRect != null)
        {
            buyButtonRect.GetWorldCorners(corners);
            topLeft = canvasRect.InverseTransformPoint(corners[1]);
            topRight = canvasRect.InverseTransformPoint(corners[2]);
            var coinRect = coinLabel.rectTransform;
            coinRect.anchorMin = coinRect.anchorMax = Vector2.zero;
            coinRect.pivot = new Vector2(0.5f, 0);
            coinRect.sizeDelta = new Vector2(topRight.x - topLeft.x, 90f);
            coinRect.anchoredPosition = new Vector2((topLeft.x + topRight.x) * 0.5f - canvasRect.rect.xMin,
                topLeft.y - canvasRect.rect.yMin + 12f);
        }
    }

    private int CurrentWaveSize => WaveNumber == 2 ? 61 : 25;
    private float CurrentSpawnInterval => WaveNumber == 2 ? 0.33f : spawnRate;

    public void StartWave()
    {
        if (State != WaveState.PreWave || WaveNumber > 2) return;
        SpawnedCount = 0;
        State = WaveState.Running;
        FindAnyObjectByType<TowerPlacement>()?.CancelPurchase();
        UpdateUI();
        StartCoroutine(SpawnWave());
        statusLabel.text = "DEFEND PHASE";
    }

    // Wave 2: (5 Rattata + 1 Maushold) x 10, then Togedemaru.
    public GameObject PrefabForSpawn(int index)
    {
        if (WaveNumber != 2) return rattataPrefab;
        if (index == 60) return togedemaruPrefab;
        return index % 6 == 5 ? mausholdPrefab : rattataPrefab;
    }

    IEnumerator SpawnWave()
    {
        for (int i = 0; i < CurrentWaveSize && State == WaveState.Running; i++)
        {
            GameObject enemy = Instantiate(PrefabForSpawn(i), transform.position, Quaternion.identity);
            enemies.Add(enemy);
            SpawnedCount++;
            SplineAnimate anim = enemy.GetComponent<SplineAnimate>();
            if (anim != null)
            {
                anim.Container = trackSpline;
                anim.Restart(true);
            }
            if (i < CurrentWaveSize - 1)
                yield return new WaitForSeconds(WaveNumber == 2 && i == 59 ? 5f :
                    WaveNumber == 2 && i % 6 == 5 ? 1f : CurrentSpawnInterval);
        }
    }

    void Update()
    {
    if (State != WaveState.Running) return;

    enemies.RemoveAll(enemy => enemy == null);

    if (SpawnedCount == CurrentWaveSize && enemies.Count == 0)
    {
        // 1. Flash the victory message for the CURRENT wave
        statusLabel.text = "PLANNING PHASE";
        StartCoroutine(FlashCenterMessage("WAVE " + WaveNumber + " COMPLETED!", 2.5f));

        // 2. Hasan's transition logic
        State = WaveState.PreWave;
        if (WaveNumber == 1) Coins += waveOneReward;
        WaveNumber++;
        UpdateUI();
    }
    }

    public void Lose()
    {
        if (State != WaveState.Running) return;
        State = WaveState.Lost;
        StopAllCoroutines();
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;
            SplineAnimate anim = enemy.GetComponent<SplineAnimate>();
            if (anim != null) anim.Pause();
        }
        UpdateUI();
    }

    void UpdateUI()
    {
        if (gameOverOverlay != null)
        {
            gameOverOverlay.SetActive(State == WaveState.Lost);
            if (State == WaveState.Lost) gameOverOverlay.transform.SetAsLastSibling();
        }
        if (startButton != null)
        {
            startButton.gameObject.SetActive(State == WaveState.PreWave);
            startButton.interactable = WaveNumber <= 2;
        }
        if (coinLabel != null) coinLabel.text = "Coins: " + Coins;
        if (waveLabel != null) waveLabel.text = "Wave " + WaveNumber;
        if (statusLabel != null)
            statusLabel.text = State == WaveState.Lost ? "Game Over" :
                State == WaveState.Running ? "DEFEND PHASE" :
                WaveNumber <= 2 ? "PLANNING PHASE" : "Pre-wave: Wave 3 not implemented";
    }



    public IEnumerator FlashCenterMessage(string message, float duration = 2f)
        {
            centerMessageText.text = message;
            centerMessageText.gameObject.SetActive(true);
            yield return new WaitForSeconds(duration);
            centerMessageText.gameObject.SetActive(false);
        }
}


