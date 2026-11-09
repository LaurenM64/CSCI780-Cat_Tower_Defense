using UnityEngine;
using UnityEngine.Splines;

public class EnemySpawner : MonoBehaviour
{
    public GameObject rattataPrefab;
    public SplineContainer trackSpline;
    public float spawnRate = 2f;

    void Start()
    {
        InvokeRepeating(nameof(SpawnEnemy), 0f, spawnRate);
    }

    void SpawnEnemy()
    {
        GameObject newEnemy = Instantiate(rattataPrefab, transform.position, Quaternion.identity);
        
        // Re-link the spline path since prefabs lose scene references
        SplineAnimate splineAnim = newEnemy.GetComponent<SplineAnimate>();
        if (splineAnim != null)
        {
            splineAnim.Container = trackSpline;
            splineAnim.Restart(true);
        }
    }
}