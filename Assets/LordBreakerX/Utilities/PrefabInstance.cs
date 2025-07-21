using UnityEngine;

[System.Serializable]
public class PrefabInstance<T> where T : Object
{
    [SerializeField]
    private T _sourcePrefab;

    private T _cachedInstance;

    public PrefabInstance(T sourcePrefab)
    {
        _sourcePrefab = sourcePrefab;
    }

    public T  GetOrCreateInstance(Vector3 createPosition, Transform parent)
    {
        if (_cachedInstance == null)
        {
            _cachedInstance = Object.Instantiate(_sourcePrefab, createPosition, Quaternion.identity, parent);
        }
        return _cachedInstance;
    }

    public T GetOrCreateInstance(Vector3 createPosition)
    {
        return GetOrCreateInstance(createPosition, null);
    }
}
