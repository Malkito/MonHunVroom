using UnityEngine;

public class billboardUI : MonoBehaviour
{



    private void LateUpdate()
    {
        transform.LookAt(Camera.current.transform.position, Vector3.up);

    }



}
