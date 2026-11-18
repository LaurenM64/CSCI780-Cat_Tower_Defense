# Fake Out clap hands

Original low-poly glove mesh modeled in Blender for this project. Editable source: FakeOutHands.blend. The user's original scene was preserved as a separate scene; hand modeling uses FakeOut_Hands.

Reference supplied by Hasan: https://www.youtube.com/watch?v=YII7wXP352g (#252 Fake Out, generation comparison). The visible Gen 5 pose shows white hands flanking the target; no assets were downloaded from the video.

Unity imports FakeOutHand.fbx and uses normalized LeftHand/RightHand mesh assets (0.9u tall, ~2016 triangles per hand), mirrored with corrected winding. Hand material is unlit white with dark seams and a simple black rim.

FakeOutClap.prefab: hands appear around a point just in front of the target, close in 0.18s, briefly hold, release and fade; self-cleanup at 0.75s. Effect follows the living target and retains the last position if it dies. Animation is presentation only. IncineroarFakeOut still applies 20 damage, 0.2 speed multiplier for 5s, once per wave. Old gold impact prefab is retained as unused fallback.
