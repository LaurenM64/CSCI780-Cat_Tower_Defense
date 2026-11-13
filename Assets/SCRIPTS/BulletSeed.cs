using UnityEngine;

public class BulletSeed : MonoBehaviour
{
    public float speed = 20f;

    void Update()
    {
        // Flies straight forward based on the direction the tower was facing when it fired
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Destroy(other.gameObject); // Pop the Rattata
            Destroy(gameObject); // Destroy the bullet
        }
    }
}