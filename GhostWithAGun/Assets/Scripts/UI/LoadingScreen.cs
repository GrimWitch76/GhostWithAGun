using System.Collections;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup _fadeCanvasGroup;
    [SerializeField] private CanvasGroup _screenCanvasGroup;
    [SerializeField] private float fadeTime = 0.5f;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject); // optional
    }

    public IEnumerator FadeIn()
    {
        _fadeCanvasGroup.alpha = 0f;
        while (_fadeCanvasGroup.alpha < 1f)
        {
            _fadeCanvasGroup.alpha += Time.deltaTime / fadeTime;
            yield return null;
        }
    }

    public IEnumerator FadeOut()
    {
        while (_fadeCanvasGroup.alpha > 0f)
        {
            _fadeCanvasGroup.alpha -= Time.deltaTime / fadeTime;
            yield return null;
        }
    }

    public void ShowScreen()
    {
        _screenCanvasGroup.alpha = 1;
    }

    public void HideScreen()
    {
        _screenCanvasGroup.alpha = 0;
    }
}