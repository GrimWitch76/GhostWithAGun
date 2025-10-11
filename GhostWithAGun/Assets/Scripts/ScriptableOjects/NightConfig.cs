using UnityEngine;

[CreateAssetMenu(fileName = "NightConfig", menuName = "ScriptableObjects/Night Config")]
public class NightConfig : ScriptableObject
{
    [Header("Spawn Timing")]
    [Tooltip("Seconds after night begins before the ghost spawns.")]
    public float SpawnDelay = 20f;

    [Tooltip("Optional random variance added to spawnDelay (±).")]
    public float SpawnDelayVariance = 0f;

    [Header("Ghost Behavior Params")]
    //Ty just add whatever tuning peramiters we've got in here for the desingers to tweak the ghost behaviour each night. 

    [Header("Overrides (Optional)")]
    [Tooltip("If true, during day time will not progress (used for day 1).")]
    public bool PauseDayTime = true; 
}
