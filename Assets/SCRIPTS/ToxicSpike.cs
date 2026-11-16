using UnityEngine;

public class ToxicSpike : MonoBehaviour
{
    private bool consumed;

    public bool TryPoisonEnemy(EnemyHealth enemy)
    {
        if (consumed || enemy == null || enemy.IsDead || !enemy.CompareTag("Enemy")) return false;
        // Check before consuming: poisoned enemies leave all subsequent spikes intact.
        var poison = enemy.GetComponent<ToxicStatus>();
        if (poison != null && poison.IsToxic) return false;
        if (poison == null) enemy.gameObject.AddComponent<ToxicStatus>();
        else poison.enabled = true;
        consumed = true;
        gameObject.SetActive(false);
        Destroy(gameObject);
        return true;
    }

    void OnTriggerEnter(Collider other)
    {
        TryPoisonEnemy(other.GetComponentInParent<EnemyHealth>());
    }
}
