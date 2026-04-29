using System;
using UnityEngine;
using NaughtyAttributes;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public sealed class SaveId : MonoBehaviour
{
    [SerializeField] private string _id;

    public string Id => _id;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_id))
        {
            _id = Guid.NewGuid().ToString("N");
            EditorUtility.SetDirty(this);
        }
    }

    [Button("Regenerate Save Id")]
    private void Regenerate()
    {
        _id = Guid.NewGuid().ToString("N");
        EditorUtility.SetDirty(this);
    }
#endif
}