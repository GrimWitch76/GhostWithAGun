using UnityEngine;

public class ScreenDamageIndicator : MonoBehaviour
{
    [SerializeField] private GameObject _leftLeg, _leftArm, _rightLeg, _rightArm;
    [SerializeField] private GameObject _torsoOne, _torsoTwo, _torsoThree;
    [SerializeField] private GameObject _finalCrack;


    private void Start()
    {
        DayCycleManager.Instance.OnDayStart += ResetHealth;
    }

    private void OnDisable()
    {
        DayCycleManager.Instance.OnDayStart -= ResetHealth;
    }

    public void UpdateBaseHealth(float normilizedHealth)
    {
        if(normilizedHealth <= .75f)
        {
            _torsoOne.SetActive(true);
        }

        if(normilizedHealth <= .5f)
        {
            _torsoTwo.SetActive(true);
        }

        if (normilizedHealth <= .25f)
        {
            _torsoThree.SetActive(true);
        }
    }

    public void DamageLimb(BodyPart limb)
    {
        switch (limb)
        {
            case BodyPart.Head:
            case BodyPart.Torso:
                break;
            case BodyPart.Arm:
                if(_rightArm.activeInHierarchy)
                {
                    _leftArm.SetActive(true);
                }else
                {
                    _rightArm.SetActive(true);
                }
                break;
            case BodyPart.Leg:
                if (_leftLeg.activeInHierarchy)
                {
                    _rightLeg.SetActive(true);
                }
                else
                {
                    _leftLeg.SetActive(true);
                }
                break;
            default:
                break;
        }
    }

    public void ResetHealth(int i)
    {
        _torsoOne.SetActive(false);
        _torsoTwo.SetActive(false);
        _torsoThree.SetActive(false);

        _leftLeg.SetActive(false);
        _leftArm.SetActive(false);
        _rightLeg.SetActive(false);
        _rightArm.SetActive(false);
    }

    
}
