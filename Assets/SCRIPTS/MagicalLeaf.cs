using System.Collections.Generic;
using UnityEngine;

public class MagicalLeaf : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 2;
    [Min(1)] public int maxTargets = 2;
    private readonly HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>();

    public void SetMaximumRange(float distance)
    {
        Destroy(gameObject, distance / speed);
    }

    void Update()
    {
        if (hitEnemies.Count >= Mathf.Max(1, maxTargets)) return;
        Vector3 start = transform.position;
        Vector3 direction = transform.forward;
        float distance = speed * Time.deltaTime;
        if (distance <= 0f) return;
        var sphere = GetComponent<SphereCollider>();
        Vector3 scale = transform.lossyScale;
        float radius = sphere.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        var hits = Physics.SphereCastAll(transform.TransformPoint(sphere.center), radius, direction,
            distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (var hit in hits)
        {
            HitEnemy(hit.collider.GetComponentInParent<EnemyHealth>());
            if (hitEnemies.Count < Mathf.Max(1, maxTargets)) continue;
            transform.position = start + direction * hit.distance;
            return;
        }
        transform.position = start + direction * distance;
    }

    void OnTriggerEnter(Collider other)
    {
        HitEnemy(other.GetComponentInParent<EnemyHealth>());
    }

    void HitEnemy(EnemyHealth health)
    {
        if (hitEnemies.Count >= Mathf.Max(1, maxTargets) || health == null || health.IsDead || !health.CompareTag("Enemy")) return;
        if (!hitEnemies.Add(health)) return;
        health.TakePiercingDamage(damage);
        // Continue travelling after the first hit; expire after the second distinct enemy.
        if (hitEnemies.Count >= Mathf.Max(1, maxTargets)) Destroy(gameObject);
    }
}
