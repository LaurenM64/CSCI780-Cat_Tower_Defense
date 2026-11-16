using UnityEngine;

// Toxic lasts for the enemy's lifetime and does not stack.
public class ToxicStatus : MonoBehaviour
{
    public const float DistancePerDamage = 1f;
    private EnemyHealth health;
    private EnemySpawner wave;
    private Vector3 previousPosition;
    private float distanceTravelled;
    public bool IsToxic => enabled && health != null && !health.IsDead;

    void Awake()
    {
        health = GetComponent<EnemyHealth>();
        wave = FindAnyObjectByType<EnemySpawner>();
        previousPosition = transform.position;
    }

    void LateUpdate()
    {
        Vector3 position = transform.position;
        float distance = Vector3.Distance(previousPosition, position);
        previousPosition = position;
        if (!IsToxic || (wave != null && wave.State != EnemySpawner.WaveState.Running)) return;
        distanceTravelled += distance;
        while (distanceTravelled >= DistancePerDamage && !health.IsDead)
        {
            distanceTravelled -= DistancePerDamage;
            health.TakeTrueDamage(1);
        }
    }
}
