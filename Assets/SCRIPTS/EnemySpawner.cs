using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Splines;

public class EnemySpawner : MonoBehaviour
{
    public enum WaveState { PreWave, Running, Lost }
    public GameObject rattataPrefab;
    public SplineContainer trackSpline;
    public float spawnRate = 2f;
    public int startingCoins = 5;
    public int waveOneReward = 10;
    public int Coins { get; private set; }
    public TMP_Text coinLabel;
    public UnityEngine.RectTransform buyButtonRect;

    public TMP_Text centerMessageText;

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

    public void StartWave()
    {
        if (State != WaveState.PreWave || WaveNumber != 1) return;
        State = WaveState.Running;
        FindAnyObjectByType<TowerPlacement>()?.CancelPurchase();
        UpdateUI();
        StartCoroutine(SpawnWave());
        statusLabel.text = "DEFEND PHASE";
    }

    IEnumerator SpawnWave()
    {
        for (int i = 0; i < 25 && State == WaveState.Running; i++)
        {
            GameObject enemy = Instantiate(rattataPrefab, transform.position, Quaternion.identity);
            enemies.Add(enemy);
            SpawnedCount++;
            SplineAnimate anim = enemy.GetComponent<SplineAnimate>();
            if (anim != null)
            {
                anim.Container = trackSpline;
                anim.Restart(true);
            }
            if (i < 24) yield return new WaitForSeconds(spawnRate);
        }
    }

    void Update()
    {
    if (State != WaveState.Running) return;

    enemies.RemoveAll(enemy => enemy == null);

    if (SpawnedCount == 25 && enemies.Count == 0)
    {
        // 1. Flash the victory message for the CURRENT wave
        statusLabel.text = "PLANNING PHASE";
        StartCoroutine(FlashCenterMessage("WAVE " + WaveNumber + " COMPLETED!", 2.5f));

        // 2. Hasan's transition logic
        State = WaveState.PreWave;
        Coins += waveOneReward;
        WaveNumber = 2;
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
        if (startButton != null)
        {
            startButton.gameObject.SetActive(State == WaveState.PreWave);
            startButton.interactable = WaveNumber == 1;
        }
        if (coinLabel != null) coinLabel.text = "Coins: " + Coins;
        if (waveLabel != null) waveLabel.text = "Wave " + WaveNumber;
        if (statusLabel != null)
            statusLabel.text = State == WaveState.Lost ? "Game Over" :
                State == WaveState.Running ? "" :
                WaveNumber == 1 ? "Place your towers" : "Pre-wave: Wave 2 not implemented";
                statusLabel.text = "PLANNING PHASE";
    }



    public IEnumerator FlashCenterMessage(string message, float duration = 2f)
        {
            centerMessageText.text = message;
            centerMessageText.gameObject.SetActive(true);
            yield return new WaitForSeconds(duration);
            centerMessageText.gameObject.SetActive(false);
        }
}


