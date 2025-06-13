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

    public T  GetOrCreateInstance(Vector3 createPosition)
    {
        if (_cachedInstance == null)
        {
            _cachedInstance = Object.Instantiate(_sourcePrefab, createPosition, Quaternion.identity);
        }
        return _cachedInstance;
    }
}
