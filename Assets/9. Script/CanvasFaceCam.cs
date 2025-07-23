using UnityEngine;

public class CanvasFaceCam : MonoBehaviour
{
    public Camera camera;               // Referens till spelarens Kamera
    void Start()
    {
        if(camera == null)              // Kolla om det redan finns en kamera assignad
        {
            camera = Camera.main;       // Om inte, l�gg till camera.main i f�ltet
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        // L�t healthbaren " titta mot kameran co h rotera kring sin y-axel.
        transform.LookAt(transform.position + camera.transform.rotation * Vector3.forward, 
            camera.transform.rotation * Vector3.up);
    }
}
