using UnityEngine;

// Each damage tick leaves a visual only snapshot, independent of later aiming.
public class PixelFxFireBurst : MonoBehaviour
{
    public const float HoldSeconds = 1f;
    public const float FadeSeconds = 0.35f;
    PixelFxAttackVisual visual;
    Vector3 direction;
    float range, angle, age;
    public static float AlphaAt(float seconds)
    {
        return seconds <= HoldSeconds ? 1f : Mathf.Clamp01(1f-(seconds-HoldSeconds)/FadeSeconds);
    }
    public void Initialize(PixelFxAttackVisual effect, Vector3 aim, float radius, float wedge)
    {
        visual=effect;direction=aim;range=radius;angle=wedge;
        visual.ShowFire(direction,range,angle);
    }
    void Update()
    {
        age+=Time.deltaTime;
        if(age>=HoldSeconds+FadeSeconds){Destroy(gameObject);return;}
        visual.ShowFire(direction,range,angle,AlphaAt(age));
    }
}
