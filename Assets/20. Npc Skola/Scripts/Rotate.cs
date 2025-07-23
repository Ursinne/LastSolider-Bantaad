using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float speed = 0.2f;
    public bool started;

    void Update()
    {
        if (started)
        {
            transform.Rotate(0f, speed, 0f); 
        }
    }
}
