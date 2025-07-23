using UnityEngine;
using UnityEngine.AI;

public class AgentMove : MonoBehaviour
{
    private NavMeshAgent agent; //Referens till navmesh agentkomponenten
    private Animator animator; //Referens till navmesh animatorn


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

   
    void Update()
    {
        animator.SetBool("isMoving", agent.velocity.magnitude > 0.2f); //V�xla till runanimation om agentens hastighet �verskrider tr�skelv�rdet 

        if(Input.GetMouseButtonDown(0)) //Kolla om v�nster musknapp trycks in
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //Skapa en ray 
            RaycastHit hitInfo; //Variabel som lagrar info om objektet som rayen tr�ffar
            if(Physics.Raycast(ray.origin,ray.direction,out hitInfo)) //Skjut en ray fr�n kameran
            {
                agent.destination = hitInfo.point; //S�tt agentens m�l till den punkt d�r rayen tr�ffar
            }
        }
    }
}
