using UnityEngine;
using System.Collections.Generic;

// A continuous flame stream whose outer layers retain their emission direction.
[DefaultExecutionOrder(100)]
public class PixelFxFireBurst : MonoBehaviour
{
    public const float TravelSeconds = 0.65f;
    public const float HoldSeconds = 1f;
    public const float FadeSeconds = 0.35f;
    PixelFxAttackVisual visual;
    Vector3 direction;
    TowerFacing facing;
    float range, angle, age, clock;
    struct AimSample { public float time; public Vector3 direction; }
    readonly List<AimSample> history = new List<AimSample>(128);
    public Vector3 DirectionAt(float secondsAgo)
    {
        float target=clock-secondsAgo;
        for(int i=history.Count-1;i>=0;i--)
        {
            if(history[i].time>target)continue;
            if(i==history.Count-1)return history[i].direction;
            float blend=Mathf.InverseLerp(history[i].time,history[i+1].time,target);
            return Vector3.Slerp(history[i].direction,history[i+1].direction,blend).normalized;
        }
        return history.Count>0?history[0].direction:direction;
    }
    public static float AlphaAt(float seconds)
    {
        return seconds <= HoldSeconds ? 1f : Mathf.Clamp01(1f-(seconds-HoldSeconds)/FadeSeconds);
    }
    public void Initialize(PixelFxAttackVisual effect, Vector3 aim, float radius, float wedge)
    {
        visual=effect;
        facing=GetComponentInParent<TowerFacing>();
        Refresh(aim,radius,wedge);
        Vector3 initial=facing != null ? facing.FacingDirection : direction;
        history.Add(new AimSample{time=-TravelSeconds,direction=initial});
        history.Add(new AimSample{time=0,direction=initial});
        visual.ShowTrailingFire(this,range,angle,1f);
    }
    public void Refresh(Vector3 aim, float radius, float wedge)
    {
        direction=aim;range=radius;angle=wedge;age=0f;
    }
    void LateUpdate()
    {
        age+=Time.deltaTime;clock+=Time.deltaTime;
        Vector3 aim=facing != null ? facing.FacingDirection : direction;
        if(history.Count==0 || clock-history[history.Count-1].time>=1f/60f)
            history.Add(new AimSample{time=clock,direction=aim});
        while(history.Count>2 && history[1].time<clock-TravelSeconds-0.1f)history.RemoveAt(0);
        if(age>=HoldSeconds+FadeSeconds){Destroy(gameObject);return;}
        visual.ShowTrailingFire(this,range,angle,AlphaAt(age));
    }
}
