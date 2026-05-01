public partial class WorldItem
{
    private void Reset()
    {
        _saveId = GetComponent<SaveId>();
    }

    private void Awake()
    {
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