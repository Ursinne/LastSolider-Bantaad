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
        LavettRotation();  // här kallar man på rotatioen funktionen   
        GunRotation();    // här kallar man på rot funktionerna
    }


    void LavettRotation() // en funkion som bara hänger om man inte kallar på den i update eller så
    {
        mouseX += Input.GetAxis("Mouse X") * speed;           // Ta in mouseX posititionen och laga sen i mouseX
        lavettRotation = Quaternion.Euler(0f, mouseX, 0f);   // Rotera lavetten i enbart Y-led -- euler x y z , man gör om det för att det inte ska bli knas i quarernion och för att viska förtså
        lavett.rotation = lavettRotation;                   // Här för vi över rotationen till lavetten
    }

    void GunRotation()
    {
        mouseY -= Input.GetAxis("Mouse Y") * speed;       // Ta in mouseY positioenenoch lagra den // lagras i mouseY variabeln - ändra += till -= för att invertera musen
        mouseY = Mathf.Clamp(mouseY, minRot, maxRot);        // här låser man  rotatioen  - clamp är hårt
        gunRotation = Quaternion.Euler(mouseY, mouseX, 0f); // roterakanonen i enbart X.led - Sätter in mouse x värddt för att låsa child till parent
        gun.rotation = gunRotation;                     // här för vi över rotationen till kanonen 
    }
}