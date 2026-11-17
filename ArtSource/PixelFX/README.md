# PixelFX attack experiment

Source: https://github.com/patch-gg/pixelfx
Revision: 5294518ce7c72e48b6100a744596e0acf57f2826 (MIT; see LICENSE and NOTICE.md).

Fire uses fire/fire-breath. Lightning uses tower-link, a small adapter built from PixelFX's zigzag/stroke helpers, with no forks, props or impact particles. Only the necessary source modules are included. Patch's gallery fonts are not included.

Regenerate from this directory with Node 18+ (no install required):

    node tools/render.mjs fire/fire-breath lightning/tower-link --sheet --fps 12 --no-actor --bg none --out rendered

Copy each PNG and JSON to Assets/VFX/PixelFX. Retain Point filtering, no mipmaps, uncompressed RGBA. Fire has 18 frames in a 4-column/5-row atlas; lightning has 6 frames in a 2-column/3-row atlas. The Unity adapter uses these fixed layouts at 12 fps.

The Unity fan maps the fire texture onto the existing attack area. It does not apply damage. Each chain strike creates a single mesh containing all its links, immediately visible with a fixed randomly chosen sprite frame and pointed end caps, fading over 1.5 seconds at its original hit positions. Strikes overlap independently. No lights, postprocessing, screen effects or runtime JavaScript are added.

Branch: codex/pixelfx-experiment, based on model-rotation (d79d1a5). Old wedge and LineRenderer references remain as fallback when the PixelFxAttackVisual component/material is removed.

Fire readability update: each real damage tick now creates an independent visual burst. It remains fully opaque for 1 second, then fades over 0.35 seconds; lingering visuals never apply extra damage. UVs crop to the bright flame body so it reaches the full attack radius. The fire material adds a small four-tap warm halo, with no scene lights or bloom requirement. Lightning glow stays disabled.

Fire blending update: ticks refresh a single continuous stream instead of layering separate fans. Animation does not restart per tick. The stream follows TowerFacing.FacingDirection after model rotation in LateUpdate; damage still uses the gameplay target direction. The last tick holds the stream for 1 second before its 0.35-second fade. This reduces mesh count and eliminates distinct overlapping cones.

Directional trail update: fire emission now runs continuously while a target is acquired, independently of damage ticks. A bounded direction history drives 24 radial flame layers: near-mouth layers follow the model now, while layers at maximum range retain the direction from 0.65 seconds earlier. These layers form one animated, bending mesh (625 vertices), rather than detached rigid cones. The last emission still holds for 1 second before fading over 0.35 seconds. Damage cones and all attack stats remain unchanged.
