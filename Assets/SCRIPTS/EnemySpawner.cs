using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.SceneManagement; // Added for UI Restart

public class EnemySpawner : MonoBehaviour
{
    public enum WaveState { PreWave, Running, Lost, Won }
    public GameObject rattataPrefab;
    public GameObject mausholdPrefab;
    public GameObject togedemaruPrefab;
    [UnityEngine.Serialization.FormerlySerializedAs("alolanRaticatePrefab")]
    public GameObject raticatePrefab;
    public SplineContainer trackSpline;
    public float spawnRate = 2f;
    public int startingCoins = 5;
    public int waveOneReward = 10;
    public int waveTwoReward = 20;
    public int waveThreeReward = 20;
    public int Coins { get; private set; }
    public TMP_Text coinLabel;
    public UnityEngine.RectTransform buyButtonRect;

    public TMP_Text centerMessageText;
    public GameObject gameOverOverlay;
    
    [Header("UI Panels")]
    public GameObject startMenuPanel;
    public GameObject instructionsPanel;

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
    public bool IsBossStage => WaveNumber == 4;
    public bool EverythingUnlocked => WaveNumber >= 4;
    public int SpawnedCount { get; private set; }
    public int AliveCount => enemies.Count;
    public UnityEngine.UI.Button startButton;
    public TMP_Text waveLabel;
    public TMP_Text statusLabel;
    public UnityEngine.RectTransform removeButtonRect;
    private readonly List<GameObject> enemies = new List<GameObject>();
    private readonly HashSet<GameObject> nonRattataEnemies = new HashSet<GameObject>();
    [Min(0)] public float spawnExitDistance = 1f;
    public event System.Action<EnemyHealth> FirstNonRattataLeftSpawn;
    private bool fakeOutTargetChosen;

    void Start() 
    { 
        Coins = startingCoins; 
        
        // Show start menu and pause on boot
        if (startMenuPanel != null) startMenuPanel.SetActive(true);
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        Time.timeScale = 0f;
        
        UpdateUI(); 
    }

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

    // Types: 0 = Rattata, 1 = Maushold, 2 = Togedemaru. Mimikyu is not implemented yet.
    private static readonly int[] waveThreeSpawns = BuildWaveThree();

    static int[] BuildWaveThree()
    {
        var types = new List<int>();
        for (int group = 0; group < 10; group++)
        {
            for (int i = 0; i < 10; i++) types.Add(0);
            for (int i = 0; i < (group % 2 == 0 ? 1 : 2); i++) types.Add(1);
            for (int i = 0; i < (group >= 8 ? 2 : 1); i++) types.Add(2);
        }
        return types.ToArray();
    }

    private int CurrentWaveSize => IsBossStage ? 81 : WaveNumber == 3 ? waveThreeSpawns.Length : WaveNumber == 2 ? 61 : 25;
    private float CurrentSpawnInterval => WaveNumber >= 2 ? 0.33f : spawnRate;

    public float SpawnDelayAfter(int index)
    {
        if (WaveNumber == 2 && index == 59) return 5f;
        if (WaveNumber == 2 && index % 6 == 5) return 1f;
        if (WaveNumber == 3 && index + 1 < waveThreeSpawns.Length
            && waveThreeSpawns[index] != 0 && waveThreeSpawns[index + 1] == 0) return 1f;
        return CurrentSpawnInterval;
    }

    public void StartWave()
    {
        if (State != WaveState.PreWave || WaveNumber > 4) return;
        SpawnedCount = 0;
        nonRattataEnemies.Clear();
        fakeOutTargetChosen = false;
        State = WaveState.Running;
        FindAnyObjectByType<TowerPlacement>()?.CancelPurchase();
        UpdateUI();
        StartCoroutine(SpawnWave());
        statusLabel.text = "DEFEND PHASE";
    }

    // Boss: one uninterrupted group, with Raticate after the first 40 enemies.
    // Wave 2: (5 Rattata + 1 Maushold) x 10, then Togedemaru.
    public GameObject PrefabForSpawn(int index)
    {
        if (IsBossStage)
        {
            if (index == 40) return raticatePrefab;
            int supportIndex = index > 40 ? index - 1 : index;
            int type = supportIndex % 4;
            return type == 2 ? mausholdPrefab : type == 3 ? togedemaruPrefab : rattataPrefab;
        }
        if (WaveNumber == 3)
        {
            int type = waveThreeSpawns[index];
            return type == 1 ? mausholdPrefab : type == 2 ? togedemaruPrefab : rattataPrefab;
        }
        if (WaveNumber != 2) return rattataPrefab;
        if (index == 60) return togedemaruPrefab;
        return index % 6 == 5 ? mausholdPrefab : rattataPrefab;
    }

    IEnumerator SpawnWave()
    {
        for (int i = 0; i < CurrentWaveSize && State == WaveState.Running; i++)
        {
            GameObject prefab = PrefabForSpawn(i);
            GameObject enemy = Instantiate(prefab, transform.position, Quaternion.identity);
            enemies.Add(enemy);
            if (prefab != rattataPrefab) nonRattataEnemies.Add(enemy);
            SpawnedCount++;
            SplineAnimate anim = enemy.GetComponent<SplineAnimate>();
            if (anim != null)
            {
                anim.Container = trackSpline;
                anim.Restart(true);
            }
            if (i < CurrentWaveSize - 1)
                yield return new WaitForSeconds(SpawnDelayAfter(i));
        }
    }

    void Update()
    {
        if (State != WaveState.Running) return;

        CheckFakeOutTarget();

        enemies.RemoveAll(enemy => enemy == null);

        if (SpawnedCount == CurrentWaveSize && enemies.Count == 0)
        {
            if (IsBossStage)
            {
                State = WaveState.Won;
                StartCoroutine(FlashCenterMessage("BOSS STAGE CLEARED!", 5f));
                UpdateUI();
                return;
            }
            // Flash the victory message for the current wave.
            statusLabel.text = "PLANNING PHASE";
            StartCoroutine(FlashCenterMessage("WAVE " + WaveNumber + " COMPLETED!", 5f));

            // 2. Hasan's transition logic
            State = WaveState.PreWave;
            if (WaveNumber == 1) Coins += waveOneReward;
            else if (WaveNumber == 2) Coins += waveTwoReward;
            else if (WaveNumber == 3) Coins += waveThreeReward;
            WaveNumber++;
            UpdateUI();
        }
    }

    void CheckFakeOutTarget()
    {
        if (fakeOutTargetChosen || trackSpline == null) return;
        Vector3 start = trackSpline.EvaluatePosition(0f);
        foreach (var enemy in enemies)
        {
            if (enemy == null || !nonRattataEnemies.Contains(enemy)) continue;
            var health = enemy.GetComponent<EnemyHealth>();
            if (health == null || health.IsDead) continue;
            Vector3 offset = enemy.transform.position - start; offset.y = 0f;
            if (offset.sqrMagnitude < spawnExitDistance * spawnExitDistance) continue;
            fakeOutTargetChosen = true;
            FirstNonRattataLeftSpawn?.Invoke(health);
            return;
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
            startButton.interactable = WaveNumber <= 4;
        }
        if (coinLabel != null) coinLabel.text = "Coins: " + Coins;
        if (waveLabel != null) waveLabel.text = IsBossStage ? "BOSS" : "Wave " + WaveNumber;
        if (statusLabel != null)
            statusLabel.text = State == WaveState.Lost ? "Game Over" :
                State == WaveState.Won ? "VICTORY" :
                State == WaveState.Running ? "DEFEND PHASE" :
                WaveNumber <= 4 ? "PLANNING PHASE" : "All stages completed";
    }

    public IEnumerator FlashCenterMessage(string message, float duration = 5f)
    {
        if (centerMessageText == null) yield break;
        centerMessageText.text = message;
        centerMessageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        if (centerMessageText != null) centerMessageText.gameObject.SetActive(false);
    }

    // --- NEW UI METHODS FOR BUTTONS ---

    public void StartNewGame()
    {
        if (startMenuPanel != null) startMenuPanel.SetActive(false);
        Time.timeScale = 1f; 
    }

    public void ShowInstructions()
    {
        if (instructionsPanel != null) instructionsPanel.SetActive(true);
    }

    public void HideInstructions()
    {
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Debug.Log("Game Quit!"); 
        Application.Quit(); 
    }
}