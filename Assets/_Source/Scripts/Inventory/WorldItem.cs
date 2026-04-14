using UnityEngine;

public class WorldItem : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData _itemData;
    [SerializeField, Min(1)] private int _count = 1;

    public ItemData ItemData => _itemData;
    public int Count => _count;

    private void Awake()
    {
        if (_itemData == null)
        {
            Debug.LogError($"WorldItem on {gameObject.name} has no ItemData assigned.");
            return;
        }

        _itemData = ScriptableObject.Instantiate(_itemData);
    }

    public void Interact()
    {
        Debug.Log($"Interacted with: {ItemData.Id} x{Count}");
    }
}