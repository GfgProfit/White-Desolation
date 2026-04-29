using NaughtyAttributes;
using UnityEngine;

public partial class WorldItem
{
    [Button]
    private void AssignObjectName()
    {
        gameObject.name = $"[Item] - ID: {_itemData.Id}";
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * _stickingOffsetY, 0.1f);
    }
}