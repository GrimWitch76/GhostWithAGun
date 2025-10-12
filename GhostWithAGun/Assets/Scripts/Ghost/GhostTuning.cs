using UnityEngine;

[CreateAssetMenu(fileName = "GhostTuning", menuName = "AI/Ghost Tuning")]
public class GhostTuning : ScriptableObject
{
    [Header("Suspicion")]
    public float suspicionMax = 100f;
    public float suspicionDecay = 5f;
    public float suspicionThresholdChase = 60f;
    public float suspicionPerPrimaryVision = 15f;
    public float suspicionPerSecondaryVision = 2f;

    [Header("Frustration")]
    public float frustrationMax = 100f;
    public float frustrationGainPerSecondSearching = 5f;
    public float frustrationGainPerSecondWandering = 1.25f;
    public float frustrationDecayPerSecondWhenStimulated = 8f;
    public float frustrationToDropGun = 70f;

    [Header("Vision")]
    public float primaryConeAngle = 90f;
    public float primaryRange = 10f;
    public float secondaryConeAngle = 160f;
    public float secondaryRange = 4f;
    public float crouchVisibilityMultiplier = 0.6f; // reduces suspicion gain
    public float darknessVisibilityMultiplier = 0.7f; // optional

    [Header("Investigation")]
    public float investigateRoomDuration = 60f; // L2 sound sweep
    public float lookAroundInterval = 1.5f;
    public float searchDuration = 5f;

    [Header("Gun/Combat")]
    public float shootRange = 15f;
    public float shootCooldown = 3f;

    [Header("Doors")]
    public float breakDoorSuspicionGate = 80f;
}