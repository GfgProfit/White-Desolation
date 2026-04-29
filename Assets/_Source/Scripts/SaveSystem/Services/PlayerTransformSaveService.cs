using UnityEngine;

public sealed class PlayerTransformSaveService
{
    private readonly Transform _playerTransform;

    public PlayerTransformSaveService(Transform playerTransform)
    {
        _playerTransform = playerTransform;
    }

    public void Capture(GameSaveData saveData)
    {
        if (_playerTransform == null || saveData == null)
        {
            return;
        }

        saveData.PlayerTransform.HasData = true;
        saveData.PlayerTransform.Position = new SerializableVector3(_playerTransform.position);
        saveData.PlayerTransform.Rotation = new SerializableQuaternion(_playerTransform.rotation);
    }

    public void Restore(GameSaveData saveData)
    {
        if (_playerTransform == null || saveData == null || saveData.PlayerTransform == null || !saveData.PlayerTransform.HasData)
        {
            return;
        }

        CharacterController characterController = _playerTransform.GetComponent<CharacterController>();

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        _playerTransform.SetPositionAndRotation(saveData.PlayerTransform.Position.ToVector3(), saveData.PlayerTransform.Rotation.ToQuaternion());

        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }
}