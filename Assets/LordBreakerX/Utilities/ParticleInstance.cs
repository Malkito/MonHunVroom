using UnityEngine;

[System.Serializable]
public class ParticleInstance
{
    [SerializeField]
    private ParticleSystem _effectPrefab;

    private ParticleSystem _cachedInstance;

    public ParticleInstance(ParticleSystem sourcePrefab)
    {
        _effectPrefab = sourcePrefab;
    }

    public ParticleSystem GetOrCreateInstance(Vector3 createPosition, Transform parent)
    {
        if (_cachedInstance == null)
        {
            _cachedInstance = Object.Instantiate(_effectPrefab, createPosition, Quaternion.identity, parent);
        }
        return _cachedInstance;
    }

    public ParticleSystem GetOrCreateInstance(Vector3 createPosition)
    {
        return GetOrCreateInstance(createPosition, null);
    }

    public ParticleSystem GetOrCreateInstance(Transform parent)
    {
        return GetOrCreateInstance(parent.position, parent);
    }
}
