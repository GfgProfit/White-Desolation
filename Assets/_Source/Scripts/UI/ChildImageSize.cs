using UnityEngine;

[ExecuteAlways]
public class ChildImageSize : MonoBehaviour
{
    [SerializeField] private RectTransform[] _objects;
    [SerializeField] private RectTransform _self;

    [Space]
    [SerializeField] private Vector2 _offset;

    private void Update()
    {
        if (_self == null || _objects == null)
        {
            return;
        }

        float maxWidth = 0f;

        for (int i = 0; i < _objects.Length; i++)
        {
            RectTransform rect = _objects[i];

            if (rect == null)
            {
                continue;
            }

            if (rect.gameObject.activeInHierarchy == false)
            {
                continue;
            }

            maxWidth = Mathf.Max(maxWidth, rect.sizeDelta.x);
        }

        _self.sizeDelta = new Vector2(maxWidth + _offset.x, _self.sizeDelta.y);
    }
}