using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurentScript : MonoBehaviour
{

    public float speed;
    public Transform lavett; // Referens till lavetten transform rotatioen komponent
    public Transform gun;
    private float mouseX; // variabel
    private float mouseY;
    private Quaternion lavettRotation;
    private Quaternion gunRotation;
    public float minRot = -40f;
    public float maxRot = -18;




    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        LavettRotation();  // h�r kallar man p� rotatioen funktionen   
        GunRotation();    // h�r kallar man p� rot funktionerna
    }


    void LavettRotation() // en funkion som bara h�nger om man inte kallar p� den i update eller s�
    {
        mouseX += Input.GetAxis("Mouse X") * speed;           // Ta in mouseX posititionen och laga sen i mouseX
        lavettRotation = Quaternion.Euler(0f, mouseX, 0f);   // Rotera lavetten i enbart Y-led -- euler x y z , man g�r om det f�r att det inte ska bli knas i quarernion och f�r att viska f�rts�
        lavett.rotation = lavettRotation;                   // H�r f�r vi �ver rotationen till lavetten
    }

    void GunRotation()
    {
        mouseY -= Input.GetAxis("Mouse Y") * speed;       // Ta in mouseY positioenenoch lagra den // lagras i mouseY variabeln - �ndra += till -= f�r att invertera musen
        mouseY = Mathf.Clamp(mouseY, minRot, maxRot);        // h�r l�ser man  rotatioen  - clamp �r h�rt
        gunRotation = Quaternion.Euler(mouseY, mouseX, 0f); // roterakanonen i enbart X.led - S�tter in mouse x v�rddt f�r att l�sa child till parent
        gun.rotation = gunRotation;                     // h�r f�r vi �ver rotationen till kanonen 
    }
}