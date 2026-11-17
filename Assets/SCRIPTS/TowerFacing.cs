using UnityEngine;

public class TowerFacing : MonoBehaviour
{
    public Transform model;
    [Min(1f)] public float turnSpeedDegreesPerSecond = 720f;
    private Vector3 desiredDirection = Vector3.back;
    [SerializeField, HideInInspector] private Vector3 neutralLocalPosition;
    [SerializeField, HideInInspector] private Quaternion neutralLocalRotation = Quaternion.identity;
    [SerializeField, HideInInspector] private bool poseInitialized;
    public Vector3 FacingDirection { get; private set; } = Vector3.back;

    public void Configure(Transform visual)
    {
        model = visual;
        poseInitialized = model != null;
        if (!poseInitialized) return;
        neutralLocalPosition = model.localPosition;
        neutralLocalRotation = model.localRotation;
    }

    void Awake()
    {
        if (EnsureModel()) ApplyRotation(Quaternion.LookRotation(Vector3.back, Vector3.up));
    }

    public void FaceDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.000001f) desiredDirection = direction.normalized;
    }

    bool EnsureModel()
    {
        if (model == null)
        {
            model = transform.Find("Model");
            poseInitialized = false;
        }
        if (model == null) return false;
        if (!poseInitialized) Configure(model);
        return true;
    }

    void LateUpdate() { RotateModel(Time.deltaTime); }

    void RotateModel(float deltaTime)
    {
        if (!EnsureModel()) return;
        Quaternion current = transform.rotation * model.localRotation * Quaternion.Inverse(neutralLocalRotation);
        Quaternion target = Quaternion.LookRotation(desiredDirection, Vector3.up);
        ApplyRotation(Quaternion.RotateTowards(current, target, turnSpeedDegreesPerSecond * Mathf.Max(0f, deltaTime)));
    }

    void ApplyRotation(Quaternion worldRotation)
    {
        FacingDirection = worldRotation * Vector3.forward;
        Quaternion localYaw = Quaternion.Inverse(transform.rotation) * worldRotation;
        // Rotate the authored offset too: the body's centre stays over the footprint.
        model.localPosition = localYaw * neutralLocalPosition;
        model.localRotation = localYaw * neutralLocalRotation;
    }
}
