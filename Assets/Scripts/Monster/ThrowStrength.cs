using UnityEngine;

[System.Serializable]
public struct ThrowStrength
{
    private float _verticalStrength;
    private float _horizontalStrength;

    public ThrowStrength(float minStrength, float maxStrength)
    {
        _verticalStrength = Random.Range(minStrength, maxStrength);
        _horizontalStrength = Random.Range(minStrength, maxStrength);
    }

    public Vector3 GetForce()
    {
        Vector3 randomForce = Random.insideUnitSphere * _horizontalStrength;
        return new Vector3(randomForce.x, _verticalStrength, randomForce.z);
    }
}
