using UnityEngine;

// Visual only: Fake Out's damage/slow are still applied by IncineroarFakeOut.
public class FakeOutClapEffect : MonoBehaviour
{
    public Transform leftHand, rightHand;
    public float lifetime = 0.75f;
    Transform target;
    Vector3 lastAnchor, right = Vector3.right, up = Vector3.up;
    Quaternion viewRotation = Quaternion.identity;
    float age, size = 1f;
    Renderer[] renderers;
    MaterialPropertyBlock block;
    public void Initialize(EnemyHealth enemy)
    {
        target=enemy!=null?enemy.transform:null;
        var camera=Camera.main;
        if(camera!=null){right=camera.transform.right;up=camera.transform.up;viewRotation=Quaternion.LookRotation(-camera.transform.forward,up);}
        if(target!=null)
        {
            Vector3 forward=target.forward;forward.y=0;if(forward.sqrMagnitude<0.001f)forward=Vector3.forward;
            lastAnchor=target.position+Vector3.up*0.6f+forward.normalized*0.35f;
        }
        renderers=GetComponentsInChildren<Renderer>();block=new MaterialPropertyBlock();
        ApplyPose(0);
    }
    public void ApplyPose(float seconds)
    {
        if(leftHand==null||rightHand==null)return;
        if(target!=null){Vector3 forward=target.forward;forward.y=0;lastAnchor=target.position+Vector3.up*0.6f+forward.normalized*0.35f;}
        transform.position=lastAnchor;
        // Fast close, brief contact, small release, then disappear.
        float close=seconds<0.18f?Mathf.SmoothStep(0,1,seconds/0.18f):seconds<0.30f?1f:Mathf.Lerp(1f,0.7f,Mathf.Clamp01((seconds-0.30f)/0.20f));
        float spread=Mathf.Lerp(0.72f,0.055f,close)*size;
        float yaw=Mathf.Lerp(22f,78f,close);
        leftHand.position=lastAnchor-right*spread+up*(Mathf.Sin(close*Mathf.PI)*0.05f);
        rightHand.position=lastAnchor+right*spread+up*(Mathf.Sin(close*Mathf.PI)*0.05f);
        leftHand.rotation=Quaternion.AngleAxis(yaw,up)*viewRotation;
        rightHand.rotation=Quaternion.AngleAxis(-yaw,up)*viewRotation;
        float alpha=seconds<0.42f?1f:Mathf.Clamp01(1-(seconds-0.42f)/(lifetime-0.42f));
        if(renderers!=null)foreach(var renderer in renderers)
        {
            block.SetFloat("_Opacity",alpha);renderer.SetPropertyBlock(block);
        }
    }
    void Update(){age+=Time.deltaTime;if(age>=lifetime){Destroy(gameObject);return;}ApplyPose(age);}
}
