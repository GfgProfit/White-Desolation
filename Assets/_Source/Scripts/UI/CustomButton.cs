using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CustomButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private CanvasGroup _rootImage;

    [Space]
    [SerializeField] private UnityEvent _onClick;

    public UnityEvent OnClick => _onClick;

    private void OnDisable()
    {
        _rootImage.DOKill();
        _rootImage.alpha = 1.0f;
    }

    private void OnDestroy()
    {
        _rootImage.DOKill();
        _rootImage.alpha = 1.0f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke();

        float animationDuration = 0.1f;

        _rootImage.DOFade(0.7f, animationDuration).SetEase(Ease.OutBack).OnComplete(() =>
        {
            _rootImage.DOFade(1.0f, animationDuration).SetEase(Ease.OutBack);
        });
    }
}