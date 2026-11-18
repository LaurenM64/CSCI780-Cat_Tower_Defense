using UnityEngine;

// Keep the regular clearing close; widen only enough for a living boss's bounds.
[DefaultExecutionOrder(200)]
public class BossCameraFraming : MonoBehaviour
{
    public Vector3 normalPosition;
    public float maxPullback = 10f;
    Camera view;
    float pullback;
    void Awake(){view=GetComponent<Camera>();}
    public void ReframeImmediately(EnemyHealth boss)
    {
        if(view==null)view=GetComponent<Camera>();
        float desired=0;
        if(boss!=null && !boss.IsDead)
        {
            var renderers=boss.GetComponentsInChildren<Renderer>();
            for(int step=0;step<30;step++)
            {
                view.transform.position=normalPosition-view.transform.forward*desired;
                bool fits=true;
                foreach(var renderer in renderers)
                {
                    if(!renderer.enabled||!renderer.gameObject.activeInHierarchy)continue;
                    var bounds=renderer.bounds;
                    for(int i=0;i<8;i++)
                    {
                        var p=view.WorldToViewportPoint(bounds.center+Vector3.Scale(bounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1)));
                        if(p.x<0.04f||p.x>0.96f||p.y<0.07f||p.y>0.94f||p.z<=0)fits=false;
                    }
                }
                if(fits)break;
                desired=Mathf.Min(maxPullback,desired+0.4f);
            }
        }
        pullback=desired;
        view.transform.position=normalPosition-view.transform.forward*pullback;
    }
    float scanCountdown;
    EnemyHealth activeBoss;
    void LateUpdate()
    {
        scanCountdown-=Time.unscaledDeltaTime;
        if(scanCountdown<=0){scanCountdown=.25f;activeBoss=null;foreach(var enemy in FindObjectsByType<EnemyHealth>())if(enemy.species==EnemyHealth.EnemySpecies.Raticate&&!enemy.IsDead){activeBoss=enemy;break;}}
        Vector3 previous=transform.position;
        ReframeImmediately(activeBoss);
        transform.position=Vector3.Lerp(previous,transform.position,1-Mathf.Exp(-10f*Time.unscaledDeltaTime));
    }
}
