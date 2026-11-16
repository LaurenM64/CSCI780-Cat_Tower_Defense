using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class MausholdVisualStages : MonoBehaviour
{
    public Transform[] mice;
    [Min(1)] public int healthPerMouse = 4;
    private EnemyHealth health;
    private int lastHealth = -1;

    void Awake() { health = GetComponent<EnemyHealth>(); }
    void OnEnable() { lastHealth = -1; Refresh(); }
    void LateUpdate() { Refresh(); }

    void Refresh()
    {
        if (health == null || mice == null || health.CurrentHealth == lastHealth) return;
        lastHealth = health.CurrentHealth;
        int visibleCount = Mathf.Clamp(Mathf.CeilToInt(lastHealth / (float)healthPerMouse), 0, mice.Length);
        for (int i = 0; i < mice.Length; i++)
            if (mice[i] != null && mice[i].gameObject.activeSelf != (i < visibleCount))
                mice[i].gameObject.SetActive(i < visibleCount);
    }
}
