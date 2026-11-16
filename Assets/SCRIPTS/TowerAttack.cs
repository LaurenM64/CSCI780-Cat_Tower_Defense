using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class TowerAttack : MonoBehaviour
{
    [Header("General Attack Settings")]
    public bool isChainLightning = false; // Check this ONLY on the Shinx prefab!
    public float fireRate = 1f; 
    private float fireCountdown = 0f;
    private List<GameObject> enemiesInRange = new List<GameObject>();

    [Header("Projectile Settings (Sprigatito)")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 20f; 

    [Header("Chain Lightning Settings (Shinx)")]
    public int lightningDamage = 2;
    public int maxChainTargets = 3;
    [Tooltip("Hit every enemy connected by chain jumps, with no target cap.")]
    public bool unlimitedBranchingChain;
    public bool paralyzeAllChainTargets;
    public float chainRadius = 4f;
    public LineRenderer lineRenderer;
    [Min(0f)] public float strongestTargetParalysisDuration;
    [Min(1)] public int paralysisEveryNAttacks = 1;
    private int chainAttackCount;
    public EnemyHealth LastParalyzedTarget { get; private set; }

    void Update()
    {
        enemiesInRange.RemoveAll(item => item == null || !item.activeInHierarchy);

        if (enemiesInRange.Count > 0)
        {
            fireCountdown -= Time.deltaTime;
            if (fireCountdown <= 0f)
            {
                if (isChainLightning)
                {
                    FireChainLightning();
                }
                else
                {
                    Shoot(GetTargetClosestToGoal());
                }
                fireCountdown = 1f / fireRate;
            }
        }
    }

    // --- SPRIGATITO PROJECTILE LOGIC ---
    GameObject GetTargetClosestToGoal()
    {
        GameObject bestTarget = null;
        float highestProgress = -1f;

        foreach (GameObject enemy in enemiesInRange)
        {
            SplineAnimate anim = enemy.GetComponent<SplineAnimate>();
            if (anim != null && anim.NormalizedTime > highestProgress)
            {
                highestProgress = anim.NormalizedTime;
                bestTarget = enemy;
            }
        }
        return bestTarget;
    }

    void Shoot(GameObject target)
    {
        if (target == null) return;
        
        // 1. Calculate the enemy's velocity vector using its SplineAnimate speed
        SplineAnimate anim = target.GetComponent<SplineAnimate>();
        Vector3 enemyVelocity = target.transform.forward * anim.MaxSpeed;
        
        // 2. Set the bullet spawn point slightly elevated
        Vector3 spawnPos = transform.position + new Vector3(0, 0.5f, 0);
        
        // Piercing leaves must remain above the track after crossing their first target.
        // Predict on the horizontal firing plane instead of aiming down at the enemy root.
        Vector3 targetPosition = target.transform.position;
        if (bulletPrefab.GetComponent<MagicalLeaf>() != null)
        {
            targetPosition.y = spawnPos.y;
            enemyVelocity.y = 0f;
        }
        Vector3 interceptPoint = CalculateInterceptPoint(spawnPos, targetPosition, enemyVelocity, bulletSpeed);
        
        // Aim the projectile from its actual muzzle position
        Vector3 direction = interceptPoint - spawnPos;
        if (direction.sqrMagnitude < 0.000001f) return;
        Quaternion shotRotation = Quaternion.LookRotation(direction);
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, shotRotation);
        BulletSeed seed = bullet.GetComponent<BulletSeed>();
        MagicalLeaf leaf = bullet.GetComponent<MagicalLeaf>();
        if (seed != null || leaf != null)
        {
            SphereCollider range = GetComponent<SphereCollider>();
            Vector3 scale = transform.lossyScale;
            float radius = range.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            float initialDistance = radius + Vector3.Distance(spawnPos, transform.TransformPoint(range.center));
            float projectileSpeed = seed != null ? seed.speed : leaf.speed;
            float maximumTime = initialDistance / Mathf.Max(0.01f, projectileSpeed - enemyVelocity.magnitude);
            if (seed != null) seed.SetMaximumRange(maximumTime * projectileSpeed);
            else leaf.SetMaximumRange(maximumTime * projectileSpeed);
        }
    }

    Vector3 CalculateInterceptPoint(Vector3 towerPos, Vector3 targetPos, Vector3 targetVelocity, float projectileSpeed)
    {
        Vector3 targetDirection = targetPos - towerPos;
        float a = targetVelocity.sqrMagnitude - (projectileSpeed * projectileSpeed);
        float b = 2f * Vector3.Dot(targetVelocity, targetDirection);
        float c = targetDirection.sqrMagnitude;
        float discriminant = (b * b) - (4f * a * c);

        if (discriminant < 0f) return targetPos; 

        float t1 = (-b + Mathf.Sqrt(discriminant)) / (2f * a);
        float t2 = (-b - Mathf.Sqrt(discriminant)) / (2f * a);

        float t = -1f;
        if (t1 > 0f && t2 > 0f) t = Mathf.Min(t1, t2);
        else if (t1 > 0f) t = t1;
        else if (t2 > 0f) t = t2;

        if (t < 0f) return targetPos;
        return targetPos + (targetVelocity * t);
    }

    // --- SHINX CHAIN LIGHTNING LOGIC ---
    public void FireChainLightning()
    {
        List<GameObject> hitEnemies = new List<GameObject>();
        GameObject currentTarget = GetTargetClosestToGoal(); // Uses same priority target logic
        
        if (currentTarget == null) return;

        List<Vector3> lightningPoints = new List<Vector3>();
        lightningPoints.Add(transform.position + new Vector3(0, 0.5f, 0)); // Start at Shinx

        int chainCount = 0;

        if (unlimitedBranchingChain)
            BuildBranchingChain(currentTarget, hitEnemies, lightningPoints);
        else while (currentTarget != null && chainCount < maxChainTargets)
        {
            hitEnemies.Add(currentTarget);
            lightningPoints.Add(currentTarget.transform.position);

            EnemyHealth health = currentTarget.GetComponent<EnemyHealth>();
            if (health != null) health.TakeChainDamage(lightningDamage);

            chainCount++;
            currentTarget = GetClosestEnemyToTarget(currentTarget.transform.position, hitEnemies);
        }

        LastParalyzedTarget = null;
        chainAttackCount++;
        if (strongestTargetParalysisDuration > 0f && chainAttackCount % Mathf.Max(1, paralysisEveryNAttacks) == 0)
        {
            if (paralyzeAllChainTargets)
            {
                foreach (var hit in hitEnemies)
                {
                    if (hit == null) continue;
                    var health = hit.GetComponent<EnemyHealth>();
                    if (health != null && !health.IsDead) ParalyzeTarget(health);
                }
            }
            else
            {
            EnemyHealth strongest = null;
            foreach (var hit in hitEnemies)
            {
                if (hit == null) continue;
                var health = hit.GetComponent<EnemyHealth>();
                if (health == null || health.IsDead) continue;
                if (strongest == null || (int)health.species > (int)strongest.species) strongest = health;
            }
            if (strongest != null)
            {
                ParalyzeTarget(strongest);
            }
            }
        }
        StartCoroutine(DrawLightning(lightningPoints));
    }

    void ParalyzeTarget(EnemyHealth health)
    {
        var slow = health.GetComponent<EnemySlow>();
        if (slow == null) slow = health.gameObject.AddComponent<EnemySlow>();
        slow.Apply(0f, strongestTargetParalysisDuration);
        LastParalyzedTarget = health;
    }

    void BuildBranchingChain(GameObject firstTarget, List<GameObject> hitEnemies, List<Vector3> points)
    {
        var first = firstTarget.GetComponent<EnemyHealth>();
        if (first == null || first.IsDead) return;
        var visited = new HashSet<EnemyHealth> { first };
        var queue = new Queue<EnemyHealth>();
        var children = new Dictionary<EnemyHealth, List<EnemyHealth>>();
        queue.Enqueue(first);
        children[first] = new List<EnemyHealth>();
        // Build the connected target graph before dealing damage, so killed enemies
        // still carry this attack's lightning through to their neighbours.
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            hitEnemies.Add(current.gameObject);
            foreach (var collider in Physics.OverlapSphere(current.transform.position, chainRadius))
            {
                var next = collider.GetComponentInParent<EnemyHealth>();
                if (next == null || next.IsDead || !next.CompareTag("Enemy") || visited.Contains(next)) continue;
                if ((next.transform.position - current.transform.position).sqrMagnitude > chainRadius * chainRadius) continue;
                visited.Add(next);
                children[current].Add(next);
                children[next] = new List<EnemyHealth>();
                queue.Enqueue(next);
            }
        }
        points.Add(first.transform.position);
        AppendBranchPath(first, children, points);
        foreach (var hit in hitEnemies) hit.GetComponent<EnemyHealth>().TakeChainDamage(lightningDamage);
    }

    static void AppendBranchPath(EnemyHealth current, Dictionary<EnemyHealth, List<EnemyHealth>> children, List<Vector3> points)
    {
        foreach (var child in children[current])
        {
            points.Add(child.transform.position);
            AppendBranchPath(child, children, points);
            points.Add(current.transform.position);
        }
    }

    GameObject GetClosestEnemyToTarget(Vector3 position, List<GameObject> exclude)
    {
        Collider[] hits = Physics.OverlapSphere(position, chainRadius);
        GameObject closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            var health = hit.GetComponentInParent<EnemyHealth>();
            if (health != null && !health.IsDead && health.CompareTag("Enemy") && !exclude.Contains(health.gameObject))
            {
                float dist = Vector3.Distance(position, health.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = health.gameObject;
                }
            }
        }
        return closest;
    }

    IEnumerator DrawLightning(List<Vector3> points)
    {
        if (lineRenderer == null) yield break;
        
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
        lineRenderer.enabled = true;
        
        yield return new WaitForSeconds(0.1f); 
        
        lineRenderer.enabled = false;
    }

    // --- COMMON DETECTION ---
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy")) enemiesInRange.Add(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy")) enemiesInRange.Remove(other.gameObject);
    }
}
