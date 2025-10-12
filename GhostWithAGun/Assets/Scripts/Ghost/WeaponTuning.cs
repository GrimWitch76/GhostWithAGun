using UnityEngine;

[CreateAssetMenu(fileName = "WeaponTuning", menuName = "AI/Weapon Tuning")]
public class WeaponTuning : ScriptableObject
{
    [Header("Gun Settings")]
    [SerializeField] public int maxAmmo = 6;
    [SerializeField] public float reloadTime = 2f;
    [SerializeField] public bool missFirstShot = true;  // can toggle later nights
    [SerializeField] public float damage = 25f;
    [SerializeField] public float range = 30f;
    [SerializeField] public float spreadAngle = 2f;     // degrees for normal gun
    [SerializeField] public int shotgunPellets = 6;     // number of pellets
    [SerializeField] public Vector2 reloadDelayRange = new Vector2(1.5f, 3.5f);
    [SerializeField] public AmmoType ammoType = AmmoType.Normal;
}
