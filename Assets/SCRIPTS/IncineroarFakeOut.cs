using UnityEngine;

public class IncineroarFakeOut : MonoBehaviour
{
    public int damage = 20;
    [Range(0f, 1f)] public float speedMultiplier = 0.2f;
    public float slowDuration = 5f;
    public GameObject impactPrefab;
    private EnemySpawner wave;
    private int lastUsedWave = -1;

    void OnEnable()
    {
        wave = FindAnyObjectByType<EnemySpawner>();
        if (wave != null) wave.FirstNonRattataLeftSpawn += UseFakeOut;
    }

    void OnDisable()
    {
        if (wave != null) wave.FirstNonRattataLeftSpawn -= UseFakeOut;
    }

    public void UseFakeOut(EnemyHealth enemy)
    {
        if (wave == null || wave.State != EnemySpawner.WaveState.Running || lastUsedWave == wave.WaveNumber
            || enemy == null || enemy.IsDead) return;
        lastUsedWave = wave.WaveNumber;
        var facing = GetComponent<TowerFacing>();
        if (facing != null) facing.FaceDirection(enemy.transform.position - transform.position);
        if (impactPrefab != null)
        {
            var effect = Instantiate(impactPrefab, enemy.transform.position + Vector3.up * 0.5f, Quaternion.identity);
            var clap=effect.GetComponent<FakeOutClapEffect>();
            if(clap!=null)clap.Initialize(enemy);
            else Destroy(effect, 0.3f);
        }
        enemy.TakeDamage(damage);
        if (enemy.IsDead) return;
        var slow = enemy.GetComponent<EnemySlow>();
        if (slow == null) slow = enemy.gameObject.AddComponent<EnemySlow>();
        slow.Apply(speedMultiplier, slowDuration);
    }
}
