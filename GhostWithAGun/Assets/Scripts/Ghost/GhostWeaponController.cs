using System.Collections;
using UnityEditor;
using UnityEngine;
public enum AmmoType
{
    Normal,
    Sniper,
    Shotgun
}

public class GhostWeaponController : MonoBehaviour
{
    [SerializeField] WeaponTuning _currentWeaponTuning;

    [SerializeField] private GameObject tracerPrefab;
    [SerializeField] private float tracerLife = 0.05f; // how long it stays visible

    [Header("VFX/SFX")]
    [SerializeField] private AudioSource gunAudio;
    [SerializeField] private AudioClip shotClip;
    [SerializeField] private AudioClip reloadClip;
    [SerializeField] private AudioClip pickUpClip;
    [SerializeField] private Light muzzleFlash;
    [SerializeField] private float flashDuration = 0.05f;

    private int currentAmmo;
    private bool hasMissedOnce = false;
    private bool reloading = false;
    private bool hasPlayedEquipSound = false;
    private void Awake()
    {
        currentAmmo = _currentWeaponTuning.maxAmmo;
        if (muzzleFlash != null) muzzleFlash.enabled = false;
    }

    public void PlayPickUpSfx()
    {
        if (hasPlayedEquipSound)
            return;

        hasPlayedEquipSound = true;
        Debug.Log("Cocking Gun");
        gunAudio.PlayOneShot(pickUpClip);
    }
    public bool CanFire => !reloading && currentAmmo > 0;

    public void Fire(Vector3 origin, Vector3 target)
    {
        if (!CanFire) return;

        currentAmmo--;

        // Play SFX & flash
        if (gunAudio && shotClip) gunAudio.PlayOneShot(shotClip);
        if (muzzleFlash != null) StartCoroutine(FlashLight());

        // Miss-first-shot logic
        bool shouldMiss = _currentWeaponTuning.missFirstShot && !hasMissedOnce;
        if (shouldMiss)
        {
            hasMissedOnce = true;
            Vector3 missDir = (target - origin).normalized;
            missDir = Quaternion.Euler(0, Random.Range(-10f, 10f), 0) * missDir;
            Debug.DrawRay(origin, missDir * _currentWeaponTuning.range, Color.yellow, 1f);
            if (Physics.Raycast(origin, missDir, out RaycastHit missHit, _currentWeaponTuning.range))
            {
                // optional: spawn decal or particle where it hits
            }
            return;
        }

        switch (_currentWeaponTuning.ammoType)
        {
            case AmmoType.Normal:
                Vector3 dir = ApplySpread((target - origin).normalized, _currentWeaponTuning.spreadAngle);
                RaycastBullet(origin, dir, _currentWeaponTuning.range, Color.red);
                break;

            case AmmoType.Sniper:
                RaycastBullet(origin, (target - origin).normalized, _currentWeaponTuning.range * 2f, Color.cyan);
                break;

            case AmmoType.Shotgun:
                for (int i = 0; i < _currentWeaponTuning.shotgunPellets; i++)
                {
                    Vector3 pelletDir = ApplySpread((target - origin).normalized, _currentWeaponTuning.spreadAngle * 3f);
                    RaycastBullet(origin, pelletDir, _currentWeaponTuning.range * 0.7f, Color.magenta);
                }
                break;
        }
    }

    private void RaycastBullet(Vector3 origin, Vector3 dir, float bulletRange, Color debugColor)
    {
        Debug.DrawRay(origin, dir * bulletRange, debugColor, 1f);

        if (Physics.Raycast(origin, dir, out RaycastHit hit, bulletRange))
        {
            // Check blockable objects first
            IDestructable blockable = hit.collider.GetComponent<IDestructable>();
            if (blockable != null)
            {
                SpawnTracer(muzzleFlash.transform.position, hit.point);
                blockable.TakeDamage(_currentWeaponTuning.damage);
                return; // bullet stopped
            }

            // Then check player
            if (hit.collider.CompareTag("Player"))
            {
                PlayerHealth health = hit.collider.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    SpawnTracer(muzzleFlash.transform.position, hit.point);
                    BodyPart part = RollHitLocation();
                    health.ApplyDamage(part, _currentWeaponTuning.damage);
                }
            }
            else
            {
                // TODO: impact effects for walls etc.
            }

            SpawnTracer(muzzleFlash.transform.position, hit.point);
        }
    }

    private void SpawnTracer(Vector3 start, Vector3 end)
    {
        if (tracerPrefab != null)
        {
            GameObject tracer = Instantiate(tracerPrefab);
            LineRenderer lr = tracer.GetComponent<LineRenderer>();
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            Destroy(tracer, tracerLife);
        }
    }

    public void Reload()
    {
        if (reloading || currentAmmo == _currentWeaponTuning.maxAmmo) return;
        StartCoroutine(ReloadRoutine());
    }

    private BodyPart RollHitLocation()
    {
        // Example weights: Head 5%, Torso 50%, Arm 25%, Leg 20%
        float roll = Random.value;

        if (roll < 0.50f) return BodyPart.Torso;
        else if (roll < 0.75f) return BodyPart.Arm;
        else return BodyPart.Leg;
    }

    private IEnumerator ReloadRoutine()
    {
        reloading = true;
        if (gunAudio && reloadClip) gunAudio.PlayOneShot(reloadClip);

        float wait = Random.Range(_currentWeaponTuning.reloadDelayRange.x, _currentWeaponTuning.reloadDelayRange.y);
        yield return new WaitForSeconds(wait);

        currentAmmo = _currentWeaponTuning.maxAmmo;
        reloading = false;
    }

    private System.Collections.IEnumerator FlashLight()
    {
        muzzleFlash.enabled = true;
        yield return new WaitForSeconds(flashDuration);
        muzzleFlash.enabled = false;
    }

    private Vector3 ApplySpread(Vector3 dir, float angle)
    {
        return Quaternion.Euler(
            Random.Range(-angle, angle),
            Random.Range(-angle, angle),
            0
        ) * dir;
    }
}