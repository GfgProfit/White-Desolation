using UnityEngine;

public partial class WorldItem
{
    private void Reset()
    {
        _saveId = GetComponent<SaveId>();
    }

    private void Awake()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity))
        {
            transform.position = hit.point - Vector3.up * _stickingOffsetY;
        }

        if (_saveId == null)
        {
            _saveId = GetComponent<SaveId>();
        }

        if (_pickedUp)
        {
            gameObject.SetActive(false);
        }
    }
}