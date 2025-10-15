using UnityEngine;

public class GhostAnimationController : MonoBehaviour
{
    [SerializeField] GhostBrain _brain;
    [SerializeField] int _currentGhost;
    [SerializeField] private Animator[] _animationController;

    [SerializeField] private GameObject _revolverGhost;
    [SerializeField] private GameObject _SMGGhost;
    [SerializeField] private GameObject _ShotGunGhost;
    [SerializeField] private GameObject _machineGunGhost;
    [SerializeField] private GameObject _sniperGhost;

    private Animator _currentAnimator;

    private void OnEnable()
    {
        DayCycleManager.Instance.OnGhostSpawned += SetModel;
    }

    private void OnDisable()
    {
        DayCycleManager.Instance.OnGhostSpawned += SetModel;
    }

    public void Shoot()
    {
        _currentAnimator.SetTrigger("shoot");
    }

    public void Reload()
    {
        _currentAnimator.SetTrigger("reload");
    }

    public void DropGun()
    {
        _currentAnimator.SetTrigger("dropGun");
    }

    public void PickupGun()
    {
        _currentAnimator.SetTrigger("pickupGun");
    }

    public void SetModel(int index)
    {
        _revolverGhost.SetActive(false);
        _SMGGhost.SetActive(false);
        _ShotGunGhost.SetActive(false);
        _machineGunGhost.SetActive(false);
        _sniperGhost.SetActive(false);

        _currentAnimator = _animationController[index];
        switch (index)
        {
            case 0:
                _revolverGhost.SetActive(true);
                break;
            case 1:
                _SMGGhost.SetActive(true);
                break;
            case 2:
                _ShotGunGhost.SetActive(true);
                break;
            case 3:
                _machineGunGhost.SetActive(true);
                break;
            case 4:
                _sniperGhost.SetActive(true);
                break;
            default:
                break;
        }
    }
}
