using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;
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
    [Header("Revolver")]
    [SerializeField] private AudioClip RevolvershotClip;
    [SerializeField] private AudioClip RevolverreloadClip;
    [SerializeField] private AudioClip RevolverpickUpClip;
    [Header("SMG")]
    [SerializeField] private AudioClip SMGshotClip;
    [SerializeField] private AudioClip SMGreloadClip;
    [SerializeField] private AudioClip SMGpickUpClip;
    [Header("Shotgun")]
    [SerializeField] private AudioClip ShotgunshotClip;
    [SerializeField] private AudioClip ShotgunreloadClip;
    [SerializeField] private AudioClip ShotgunpickUpClip;
    [Header("LMG")]
    [SerializeField] private AudioClip LMGshotClip;
    [SerializeField] private AudioClip LMGreloadClip;
    [SerializeField] private AudioClip LMGpickUpClip;
    [Header("Sniper")]
    [SerializeField] private AudioClip SnipershotClip;
    [SerializeField] private AudioClip SniperreloadClip;
    [SerializeField] private AudioClip SniperpickUpClip;


    [SerializeField] private Light revolvermuzzleFlash;
    [SerializeField] private Light SMGmuzzleFlash;
    [SerializeField] private Light shotgunmuzzleFlash;
    [SerializeField] private Light LMGmuzzleFlash;
    [SerializeField] private Light snipermuzzleFlash;

    [SerializeField] private VisualEffect revolvershotEffect;
    [SerializeField] private VisualEffect smgshotEffect;
    [SerializeField] private VisualEffect shotgunshotEffect;
    [SerializeField] private VisualEffect LMGshotEffect;
    [SerializeField] private VisualEffect SnipershotEffect;

    [SerializeField] private float flashDuration = 0.05f;

    public int currentGunIndex = 0;
    private int currentAmmo;
    private bool hasMissedOnce = false;
    private bool reloading = false;
    private bool hasPlayedEquipSound = false;
    private void Awake()
    {
        currentAmmo = _currentWeaponTuning.maxAmmo;
        revolvermuzzleFlash.enabled = false;
        SMGmuzzleFlash.enabled = false;
        shotgunmuzzleFlash.enabled = false;
        LMGmuzzleFlash.enabled = false;
        snipermuzzleFlash.enabled = false;
    }

    public void SetWeaponInt(int weapon)
    {
        currentGunIndex = weapon;
    }

    public void PlayPickUpSfx()
    {
        if (hasPlayedEquipSound)
            return;

        hasPlayedEquipSound = true;
        Debug.Log("Cocking Gun");

        //switch (currentGunIndex)
        //{
        //    case 0:
        //        gunAudio.PlayOneShot(RevolverpickUpClip);
        //        break;
        //    case 1:
        //        gunAudio.PlayOneShot(SMGpickUpClip);
        //        break;
        //    case 2:
        //        gunAudio.PlayOneShot(ShotgunpickUpClip);
        //        break;
        //    case 3:
        //        gunAudio.PlayOneShot(LMGpickUpClip);
        //        break;
        //    case 4:
        //        gunAudio.PlayOneShot(SniperpickUpClip);
        //        break;
        //    default:
        //        break;
        //}
    }
    public bool CanFire => !reloading && currentAmmo > 0;

    public void Fire(Vector3 origin, Vector3 target)
    {
        if (!CanFire) return;

        currentAmmo--;

        // Play SFX & flash
        switch (currentGunIndex)
        {
            case 0:
                gunAudio.PlayOneShot(RevolvershotClip);
                StartCoroutine(FlashLight());
                break;
            case 1:
                gunAudio.PlayOneShot(SMGshotClip);
                StartCoroutine(FlashLight());
                break;
            case 2:
                gunAudio.PlayOneShot(ShotgunshotClip);
                StartCoroutine(FlashLight());
                break;
            case 3:
                gunAudio.PlayOneShot(LMGshotClip);
                StartCoroutine(FlashLight());
                break;
            case 4:
                gunAudio.PlayOneShot(SnipershotClip);
                StartCoroutine(FlashLight());
                break;
            default:
                break;
        }
        

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
                blockable.TakeDamage(_currentWeaponTuning.damage);
                return; // bullet stopped
            }

            // Then check player
            if (hit.collider.CompareTag("Player"))
            {
                PlayerHealth health = hit.collider.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    BodyPart part = RollHitLocation();
                    health.ApplyDamage(part, _currentWeaponTuning.damage);
                }
            }
            if (hit.collider.CompareTag("Interactable"))
            {
                IDestructable health = hit.collider.GetComponent<IDestructable>();
                if (health != null)
                {
                    health.TakeDamage(_currentWeaponTuning.damage);
                }
            }

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

        if (roll < 0.05f) return BodyPart.Head;
        else if (roll < 0.55f) return BodyPart.Torso;
        else if (roll < 0.80f) return BodyPart.Arm;
        else return BodyPart.Leg;
    }

    private IEnumerator ReloadRoutine()
    {
        reloading = true;
        switch (currentGunIndex)
        {
            case 0:
                gunAudio.PlayOneShot(RevolverreloadClip);
                break;
            case 1:
                gunAudio.PlayOneShot(SMGreloadClip);
                break;
            case 2:
                gunAudio.PlayOneShot(ShotgunreloadClip);
                break;
            case 3:
                gunAudio.PlayOneShot(LMGreloadClip);
                break;
            case 4:
                gunAudio.PlayOneShot(SniperreloadClip);
                break;
            default:
                break;
        }

        float wait = Random.Range(_currentWeaponTuning.reloadDelayRange.x, _currentWeaponTuning.reloadDelayRange.y);
        yield return new WaitForSeconds(wait);

        currentAmmo = _currentWeaponTuning.maxAmmo;
        reloading = false;
    }

    private System.Collections.IEnumerator FlashLight()
    {
        switch (currentGunIndex)
        {
            case 0:
                revolvermuzzleFlash.enabled = true;
                revolvershotEffect.Play();
                break;
            case 1:
                SMGmuzzleFlash.enabled = true;
                smgshotEffect.Play();
                break;
            case 2:
                shotgunmuzzleFlash.enabled = true;
                shotgunshotEffect.Play();
                break;
            case 3:
                LMGmuzzleFlash.enabled = true;
                LMGshotEffect.Play();
                break;
            case 4:
                snipermuzzleFlash.enabled = true;
                SnipershotEffect.Play();
                break;
            default:
                break;
        }
        yield return new WaitForSeconds(flashDuration);
        switch (currentGunIndex)
        {
            case 0:
                revolvermuzzleFlash.enabled = false;
                break;
            case 1:
                SMGmuzzleFlash.enabled = false;
                break;
            case 2:
                shotgunmuzzleFlash.enabled = false;
                break;
            case 3:
                LMGmuzzleFlash.enabled = false;
                break;
            case 4:
                snipermuzzleFlash.enabled = false;
                break;
            default:
                break;
        }
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