using UnityEngine;

public class CameraRotationTest : MonoBehaviour
{
    void Update()
    {
        float mouseY = Input.GetAxis("Mouse Y");
        if (Mathf.Abs(mouseY) > 0.01f)
        {
            Debug.Log($"Muser�relse Y: {mouseY}");
        }
    }
}