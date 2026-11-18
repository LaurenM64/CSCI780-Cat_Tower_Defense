# Experimental environment

Branch: experimental-environment, based on submission-ready main 8b9d4d7.

- Restored the original 67.2-degree tilt / 0.344-degree yaw and 60-degree perspective, with camera moved back for wide framing to keep the unchanged full-size Raticate visible. Offline check sampled 65 positions along the existing spline, using its travel direction.
- Hid all four wall renderers, retaining their original enabled colliders, layers and placement clearance.
- Added a continuous visual grass plane, without a collider or buildable tag. Main playable ground and spline colliders remain unchanged. ContinuousGrass.shader preserves the existing red/white/black track pixels over a shared procedural grass field, removing the original rectangular texture seams.
- Created two original low-poly tree prototypes in Blender; 1,123 tree instances are baked into one forest mesh with 3 material slots. No tree physics, scripts, per-tree updates or external asset pack dependency. Openings at spawn and goal prevent nearby trees hiding the boss/end zone.
- Replaced the goal's visible cube with a pile of three wheels, three wedges, three blocks and a rind detail. Shallow cheese holes are actual low-poly geometry. The GoalZone object, position and trigger remain unchanged; cheese has no collision.
- Used the user-supplied others_0009_1k_9wCDfD.zip color and OpenGL normal textures. Roughness source is retained for future tuning. Blender source packs the color image. Texture rights remain those of the user-supplied source; no external license claim is made.

Geometry budget: forest 87,258 triangles; cheese 1,962; outer plane 200; total added 89,420. Forest is 3 material submeshes, cheese 2. This is a geometry/draw-structure check, not a benchmark or FPS guarantee.

Editable Blender source: EnvironmentModels.blend (Experimental_Environment scene). Existing hand-model scene was preserved. Assets/EnvironmentExperimental/Models contains FBX source and baked Unity meshes. Preview.png shows the experimental view with a temporary full-size boss; that temporary boss was removed after capture.

Verification: 65/65 boss positions in view, 4 invisible walls retain collision, goal trigger intact, added outer ground has no collider, grass shader has no errors. No tower/enemy stats, paths, economy or wave data changed.

Dense surround update: 1,036 additional trees fill the nonplayable surroundings, with varied rotation/scale and spawn/goal clearings. DenseForestSurround.asset uses 32-bit indices; there are still three forest material groups, no individual tree colliders or runtime scripts. The original 87-tree border mesh is retained as source. Camera translated 0.4u south with rotation, FOV and projection unchanged. Rechecked all 65 boss path samples. No FPS delta has been claimed or benchmarked.
