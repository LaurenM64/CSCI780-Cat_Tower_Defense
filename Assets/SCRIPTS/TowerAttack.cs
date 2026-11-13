using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class TowerAttack : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float fireRate = 1f; 
    public float bulletSpeed = 20f; // CRITICAL: This must match the speed on your BulletSeed script!
    private float fireCountdown = 0f;

    private List<GameObject> enemiesInRange = new List<GameObject>();

    void Update()
    {
        enemiesInRange.RemoveAll(item => item == null);

        if (enemiesInRange.Count > 0)
        {
            fireCountdown -= Time.deltaTime;
            if (fireCountdown <= 0f)
            {
                Shoot(GetTargetClosestToGoal());
                fireCountdown = 1f / fireRate;
            }
        }
    }

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
        
        // 3. Solve the quadratic equation to find the impact coordinate
        Vector3 interceptPoint = CalculateInterceptPoint(spawnPos, target.transform.position, enemyVelocity, bulletSpeed);
        
        // 4. Look at the future coordinate and fire
        transform.LookAt(interceptPoint);
        Instantiate(bulletPrefab, spawnPos, transform.rotation);
    }

    Vector3 CalculateInterceptPoint(Vector3 towerPos, Vector3 targetPos, Vector3 targetVelocity, float projectileSpeed)
    {
        Vector3 targetDirection = targetPos - towerPos;
        
        // Quadratic equation coefficients (at^2 + bt + c = 0)
        float a = targetVelocity.sqrMagnitude - (projectileSpeed * projectileSpeed);
        float b = 2f * Vector3.Dot(targetVelocity, targetDirection);
        float c = targetDirection.sqrMagnitude;

        float discriminant = (b * b) - (4f * a * c);

        // If discriminant is negative, the bullet is too slow to catch the enemy
        if (discriminant < 0f) return targetPos; 

        // Quadratic formula for time (t)
        float t1 = (-b + Mathf.Sqrt(discriminant)) / (2f * a);
        float t2 = (-b - Mathf.Sqrt(discriminant)) / (2f * a);

        // We want the smallest positive time of impact
        float t = -1f;
        if (t1 > 0f && t2 > 0f) t = Mathf.Min(t1, t2);
        else if (t1 > 0f) t = t1;
        else if (t2 > 0f) t = t2;

        // If time is negative, the enemy is moving away faster than the bullet can travel
        if (t < 0f) return targetPos;

        // Future Position = Current Position + (Velocity * Time)
        return targetPos + (targetVelocity * t);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy")) enemiesInRange.Add(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy")) enemiesInRange.Remove(other.gameObject);
    }
}