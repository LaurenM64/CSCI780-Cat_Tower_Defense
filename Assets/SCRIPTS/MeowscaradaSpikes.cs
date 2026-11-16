using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class MeowscaradaSpikes : MonoBehaviour
{
    public GameObject spikePrefab;
    public float deployInterval = 2f;
    public int maxSpikes = 5;
    private readonly List<GameObject> spikes = new List<GameObject>();
    private EnemySpawner wave;
    private float countdown;
    public int ActiveSpikeCount { get { spikes.RemoveAll(s => s == null || !s.activeSelf); return spikes.Count; } }

    void Awake() { wave = FindAnyObjectByType<EnemySpawner>(); countdown = deployInterval; }

    void Update()
    {
        if (wave != null && (wave.State == EnemySpawner.WaveState.Lost || wave.State == EnemySpawner.WaveState.Won)) return;
        countdown -= Time.deltaTime;
        if (countdown > 0f) return;
        countdown = deployInterval;
        TryDeploySpike();
    }

    public bool TryDeploySpike()
    {
        if (spikePrefab == null || wave == null || wave.trackSpline == null || ActiveSpikeCount >= maxSpikes) return false;
        var range = GetComponent<SphereCollider>();
        float radius = range.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y), Mathf.Abs(transform.lossyScale.z));
        Vector3 best = default;
        float bestDistance = float.PositiveInfinity;
        bool found = false;
        // Search the track in world space, choosing the nearest free spot to the tower.
        for (int i = 0; i <= 512; i++)
        {
            Vector3 point = wave.trackSpline.EvaluatePosition(i / 512f);
            Vector3 offset = point - transform.position; offset.y = 0f;
            float distance = offset.sqrMagnitude;
            if (distance > radius * radius || distance >= bestDistance) continue;
            bool occupied = false;
            foreach (var spike in spikes)
            {
                Vector3 gap = spike.transform.position - point; gap.y = 0f;
                if (gap.sqrMagnitude < 0.4f * 0.4f) { occupied = true; break; }
            }
            if (occupied) continue;
            best = point; bestDistance = distance; found = true;
        }
        if (!found) return false;
        int trackLayer = LayerMask.NameToLayer("Track");
        if (trackLayer >= 0 && Physics.Raycast(best + Vector3.up * 5f, Vector3.down, out var hit, 10f, 1 << trackLayer, QueryTriggerInteraction.Ignore))
            best.y = hit.point.y;
        best.y += 0.02f;
        spikes.Add(Instantiate(spikePrefab, best, Quaternion.identity));
        return true;
    }

    void OnDisable()
    {
        foreach (var spike in spikes) if (spike != null) { spike.SetActive(false); Destroy(spike); }
        spikes.Clear();
    }
}
