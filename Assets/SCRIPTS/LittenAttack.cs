using UnityEngine;
using UnityEngine.Splines;

public class LittenAttack : MonoBehaviour
{
    [Min(0.01f)] public float detectionRange = 1f;
    [Min(0.01f)] public float attackRange = 1.5f;
    [Range(1f, 180f)] public float coneAngle = 60f;
    [Min(1)] public int damage = 1;
    [Min(0.01f)] public float ticksPerSecond = 4f;
    [Min(0.01f)] public float flameSpeed = 3f;
    public ParticleSystem flames;
    public MeshRenderer fireCone;
    public EnemyHealth Target { get; private set; }
    private float countdown;

    void Awake()
    {
        if (fireCone != null) fireCone.enabled = false;
        if (flames == null) return;
        var main = flames.main;
        main.startSpeed = flameSpeed;
        // Particles travel at most attackRange units before expiring.
        main.startLifetime = attackRange / flameSpeed;
        var shape = flames.shape;
        shape.angle = coneAngle * 0.5f;
        flames.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void Update()
    {
        Target = FindTarget();
        if (Target == null)
        {
            countdown = 0f;
            if (fireCone != null) fireCone.enabled = false;
            if (flames != null && flames.isEmitting) flames.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            return;
        }
        Vector3 direction = Target.transform.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.000001f) direction = transform.forward;
        if (fireCone != null)
        {
            fireCone.enabled = true;
            fireCone.transform.rotation = Quaternion.LookRotation(direction);
        }
        else if (flames != null)
        {
            flames.transform.rotation = Quaternion.LookRotation(direction);
            if (!flames.isEmitting) flames.Play();
        }
        countdown -= Time.deltaTime;
        if (countdown > 0f) return;
        DamageCone(direction);
        countdown = 1f / ticksPerSecond;
    }

    EnemyHealth FindTarget()
    {
        EnemyHealth best = null;
        float progress = float.NegativeInfinity;
        foreach (var enemy in FindObjectsByType<EnemyHealth>())
        {
            if (enemy.IsDead || !enemy.CompareTag("Enemy")) continue;
            Vector3 offset = enemy.transform.position - transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude > detectionRange * detectionRange) continue;
            var anim = enemy.GetComponent<SplineAnimate>();
            float candidate = anim != null ? anim.NormalizedTime : 0f;
            if (best != null && candidate <= progress) continue;
            best = enemy;
            progress = candidate;
        }
        return best;
    }

    public bool IsInsideCone(Vector3 position, Vector3 direction)
    {
        Vector3 offset = position - transform.position;
        offset.y = 0f;
        direction.y = 0f;
        if (offset.sqrMagnitude > attackRange * attackRange) return false;
        if (offset.sqrMagnitude < 0.000001f) return true;
        return Vector3.Angle(direction, offset) <= coneAngle * 0.5f;
    }

    public int DamageCone(Vector3 direction)
    {
        int hitCount = 0;
        foreach (var enemy in FindObjectsByType<EnemyHealth>())
        {
            if (enemy.IsDead || !enemy.CompareTag("Enemy") || !IsInsideCone(enemy.transform.position, direction)) continue;
            enemy.TakeDamage(damage);
            hitCount++;
        }
        return hitCount;
    }

    void OnDisable()
    {
        Target = null;
        if (fireCone != null) fireCone.enabled = false;
        if (flames != null) flames.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
