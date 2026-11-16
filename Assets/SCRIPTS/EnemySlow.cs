using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class EnemySlow : MonoBehaviour
{
    private struct Effect { public float multiplier; public float expiresAt; }
    private readonly List<Effect> effects = new List<Effect>();
    public float SpeedMultiplier { get; private set; } = 1f;
    private float normalSpeed;
    private SplineAnimate movement;
    private EnemyHealth health;
    private EnemySpawner wave;
    private bool pausedForStop;
    private bool resumeAfterStop;

    void Awake()
    {
        movement = GetComponent<SplineAnimate>();
        health = GetComponent<EnemyHealth>();
        wave = FindAnyObjectByType<EnemySpawner>();
        normalSpeed = movement != null ? movement.MaxSpeed : 0f;
    }

    public void Apply(float multiplier, float duration)
    {
        if (duration <= 0f) return;
        effects.Add(new Effect { multiplier = Mathf.Clamp01(multiplier), expiresAt = Time.time + duration });
        Recalculate();
    }

    void Recalculate()
    {
        float multiplier = 1f;
        for (int i = effects.Count - 1; i >= 0; i--)
        {
            if (effects[i].expiresAt <= Time.time) effects.RemoveAt(i);
            else multiplier = Mathf.Min(multiplier, effects[i].multiplier);
        }
        if (SpeedMultiplier == multiplier) return;
        SpeedMultiplier = multiplier;
        RefreshSpeed();
    }

    void RefreshSpeed()
    {
        if (movement == null) return;
        // A zero speed alone gives the spline an infinite duration. Pause it as well
        // so stopping cannot reset the enemy to the start of the path.
        if (SpeedMultiplier == 0f && !pausedForStop)
        {
            resumeAfterStop = movement.IsPlaying;
            movement.Pause();
            pausedForStop = true;
        }
        if (health != null && health.initialMoveSpeed > 0f) health.UpdateMovementSpeed();
        else
        {
            float progress = movement.NormalizedTime;
            movement.MaxSpeed = normalSpeed * SpeedMultiplier;
            if (SpeedMultiplier > 0f) movement.NormalizedTime = progress;
        }
        if (SpeedMultiplier > 0f && pausedForStop)
        {
            if (resumeAfterStop && (health == null || !health.IsDead)
                && (wave == null || wave.State == EnemySpawner.WaveState.Running)) movement.Play();
            pausedForStop = false;
        }
    }

    void Update() { if (effects.Count > 0) Recalculate(); }
}
