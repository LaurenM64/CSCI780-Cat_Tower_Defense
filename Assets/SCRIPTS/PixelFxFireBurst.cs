using UnityEngine;

// One continuous visual stream: ticks refresh its hold without restarting the animation.
[DefaultExecutionOrder(100)]
public class PixelFxFireBurst : MonoBehaviour
{
    public const float HoldSeconds = 1f;
    public const float FadeSeconds = 0.35f;
    PixelFxAttackVisual visual;
    Vector3 direction;
    TowerFacing facing;
    float range, angle, age;
    public static float AlphaAt(float seconds)
    {
        return seconds <= HoldSeconds ? 1f : Mathf.Clamp01(1f-(seconds-HoldSeconds)/FadeSeconds);
    }
    public void Initialize(PixelFxAttackVisual effect, Vector3 aim, float radius, float wedge)
    {
        visual=effect;
        facing=GetComponentInParent<TowerFacing>();
        Refresh(aim,radius,wedge);
        visual.ShowFire(facing != null ? facing.FacingDirection : direction,range,angle);
    }
    public void Refresh(Vector3 aim, float radius, float wedge)
    {
        direction=aim;range=radius;angle=wedge;age=0f;
    }
    void LateUpdate()
    {
        age+=Time.deltaTime;
        if(age>=HoldSeconds+FadeSeconds){Destroy(gameObject);return;}
        visual.ShowFire(facing != null ? facing.FacingDirection : direction,range,angle,AlphaAt(age));
    }
}
