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
    public float chainRadius = 4f;
    public LineRenderer lineRenderer;

    void Update()
    {
        enemiesInRange.RemoveAll(item => item == null);

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
    void FireChainLightning()
    {
        List<GameObject> hitEnemies = new List<GameObject>();
        GameObject currentTarget = GetTargetClosestToGoal(); // Uses same priority target logic
        
        if (currentTarget == null) return;

        List<Vector3> lightningPoints = new List<Vector3>();
        lightningPoints.Add(transform.position + new Vector3(0, 0.5f, 0)); // Start at Shinx

        int chainCount = 0;

        while (currentTarget != null && chainCount < maxChainTargets)
        {
            hitEnemies.Add(currentTarget);
            lightningPoints.Add(currentTarget.transform.position);

            EnemyHealth health = currentTarget.GetComponent<EnemyHealth>();
            if (health != null) health.TakeChainDamage(lightningDamage);

            chainCount++;
            currentTarget = GetClosestEnemyToTarget(currentTarget.transform.position, hitEnemies);
        }

        StartCoroutine(DrawLightning(lightningPoints));
    }

    GameObject GetClosestEnemyToTarget(Vector3 position, List<GameObject> exclude)
    {
        Collider[] hits = Physics.OverlapSphere(position, chainRadius);
        GameObject closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy") && !exclude.Contains(hit.gameObject))
            {
                float dist = Vector3.Distance(position, hit.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = hit.gameObject;
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
