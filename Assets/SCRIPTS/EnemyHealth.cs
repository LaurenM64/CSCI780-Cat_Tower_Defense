using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Min(1)] public int maxHealth = 4;
    [Tooltip("Damage stops at each multiple of this HP amount. Zero disables health gates.")]
    [Min(0)] public int healthGateInterval;
    [Min(1)] public int chainDamageMultiplier = 1;
    public bool chainBypassesHealthGates;
    [Tooltip("Zero preserves the prefab's movement speed.")]
    [Min(0)] public float initialMoveSpeed;
    [Min(0)] public float speedPerHealthGate;
    public bool immuneToNonPiercing;
    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;

    void Awake() { CurrentHealth = maxHealth; UpdateMovementSpeed(); }

    void UpdateMovementSpeed()
    {
        if (initialMoveSpeed <= 0f) return;
        var movement = GetComponent<UnityEngine.Splines.SplineAnimate>();
        if (movement == null) return;
        int crossedGates = healthGateInterval > 0
            ? (maxHealth - 1) / healthGateInterval - Mathf.Max(0, CurrentHealth - 1) / healthGateInterval
            : 0;
        movement.MaxSpeed = initialMoveSpeed + crossedGates * speedPerHealthGate;
    }

    public void TakeChainDamage(int damageAmount)
    {
        if (damageAmount <= 0) return;
        ApplyDamage(damageAmount * chainDamageMultiplier, chainBypassesHealthGates);
    }

    public void TakeDamage(int damageAmount)
    {
        ApplyDamage(damageAmount, false);
    }

    public void TakePiercingDamage(int damageAmount)
    {
        ApplyDamage(damageAmount, false, true);
    }

    void ApplyDamage(int damageAmount, bool bypassHealthGates, bool piercing = false)
    {
        if (IsDead || damageAmount <= 0 || (immuneToNonPiercing && !piercing)) return;
        int minimumHealth = healthGateInterval > 0 && !bypassHealthGates
            ? ((CurrentHealth - 1) / healthGateInterval) * healthGateInterval
            : 0;
        CurrentHealth = Mathf.Max(minimumHealth, CurrentHealth - damageAmount);
        UpdateMovementSpeed();
        if (!IsDead) return;
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
