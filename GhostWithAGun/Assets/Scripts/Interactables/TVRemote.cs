using UnityEngine;

public class TVRemote : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject _screen;
    [SerializeField] private SoundEmitter _soundEmitter;

    private bool _isOn;

    public void GhostInteract(GameObject ghost)
    {
    }

    public void Interact(PlayerInteraction interactor)
    {
        ToggleTv();
    }

    private void ToggleTv()
    {
        _isOn = !_isOn;

        if(_screen != null)
        {
            _screen.SetActive(_isOn);
            if(_isOn)
            {
                _soundEmitter.PlayLooped();
            }
            else
            {
                _soundEmitter.Stop();
            }
        }
    }
}
