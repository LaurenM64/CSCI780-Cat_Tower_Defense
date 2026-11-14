using UnityEngine;

public class TowerEvolution : MonoBehaviour
{
    public string pokemonName;
    public GameObject nextStagePrefab;
    [Min(0)] public int evolutionCost;
    public bool CanEvolve => nextStagePrefab != null;
}
