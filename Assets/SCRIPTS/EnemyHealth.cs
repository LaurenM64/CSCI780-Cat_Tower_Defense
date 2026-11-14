using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Min(1)] public int maxHealth = 4;
    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;

    void Awake() { CurrentHealth = maxHealth; }

    public void TakeDamage(int damageAmount)
    {
        if (IsDead || damageAmount <= 0) return;
        CurrentHealth = Mathf.Max(0, CurrentHealth - damageAmount);
        if (!IsDead) return;
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
