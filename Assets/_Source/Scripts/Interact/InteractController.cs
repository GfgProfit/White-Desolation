using UnityEngine;

public class InteractController : MonoBehaviour
{
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private LayerMask _layerMask;

    [Space]
    [SerializeField] private float _interactRange = 3.0f;

    [Inject] private IPlayerInput _playerInput;

    private void Update()
    {
        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, _interactRange, _layerMask))
        {
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                if (_playerInput.IsInteractPressed())
                {
                    interactable.Interact();
                }
            }
        }
    }
}