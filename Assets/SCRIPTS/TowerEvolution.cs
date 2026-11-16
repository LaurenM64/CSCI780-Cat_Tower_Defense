using UnityEngine;

public class TowerEvolution : MonoBehaviour
{
    public string pokemonName;
    public GameObject nextStagePrefab;
    [Min(0)] public int evolutionCost;
    [Min(1)] public int earliestWave = 1;
    public bool CanEvolve => nextStagePrefab != null;
    public bool IsUnlocked(EnemySpawner wave) => wave != null && (wave.EverythingUnlocked || wave.WaveNumber >= earliestWave);
}
