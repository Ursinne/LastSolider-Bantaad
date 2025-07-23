using UnityEngine;

public class Mutant1 : MonoBehaviour

{
    private CharacterController controller;  // Referens till char-Controllerkomponenten.
    private Animator animator;               // Teferens till animatorn.
    private float playerSpeed = 4f;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();


    }


    void Update()
    {
        //Move();
    }

    //private void Move()
    //{
    //    Vector3 move = transform.right * Input.GetAxis("Horizontal") + transform.forward *
    //    Input.GetAxis("Vertical");                                                          // Läs spelarinput och lagra i en vector3-variabel.
    //    controller.Move(move * playerSpeed * Time.deltaTime);                               // Sätt characterkontrollens hastighet
    //    float x = Input.GetAxis("Horizontal");
    //    float y = Input.GetAxis("Vertical");                                                // Ta inputen som separata floatvärden

    //    animator.SetFloat("VelocityX", x);                                                  //Skicka in horisontal movement till animatorn
    //    animator.SetFloat("VelocityY", y);                                                  //Skicka in Vertical movement till animatorn
    //}
}
