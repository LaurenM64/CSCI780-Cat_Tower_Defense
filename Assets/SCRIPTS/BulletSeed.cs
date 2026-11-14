using UnityEngine;

public class BulletSeed : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 2;
    private bool hitEnemy;

    public void SetMaximumRange(float distance)
    {
        Destroy(gameObject, distance / speed);
    }

    void Update()
    {
        // Flies straight forward based on the direction the tower was facing when it fired
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        var health = other.GetComponentInParent<EnemyHealth>();
        if (hitEnemy || health == null || health.IsDead || !health.CompareTag("Enemy")) return;
        hitEnemy = true;
        health.TakeDamage(damage);
        Destroy(gameObject);
    }
}
