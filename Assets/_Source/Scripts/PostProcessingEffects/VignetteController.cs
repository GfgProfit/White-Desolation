using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VignetteController : MonoBehaviour
{
    [SerializeField] private Volume _globalVolume;
    
    [Space]
    [SerializeField] private float _crouchIntensity;
    
    private Vignette _vignette;
    private float _baseIntensity;

    private void Start()
    {
        _globalVolume.profile = Instantiate(_globalVolume.profile);

        if (!_globalVolume.profile.TryGet(out _vignette))
        {
            _vignette = _globalVolume.profile.Add<Vignette>(true);
        }

        _baseIntensity = _vignette.intensity.value;
    }

    public void AnimateIntensityCrouch()
    {
        DOTween.To(
            () => _vignette.intensity.value,
            x => _vignette.intensity.value = x,
            _crouchIntensity,
            0.5f
        ).SetEase(Ease.InOutSine);
    }

    public void AnimateIntensityBase()
    {
        DOTween.To(
            () => _vignette.intensity.value,
            x => _vignette.intensity.value = x,
            _baseIntensity,
            0.5f
        ).SetEase(Ease.InOutSine);
    }
}