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
        animator.SetBool("isMoving", agent.velocity.magnitude > 0.2f); //Växla till runanimation om agentens hastighet överskrider tröskelvärdet 

        if(Input.GetMouseButtonDown(0)) //Kolla om vänster musknapp trycks in
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //Skapa en ray 
            RaycastHit hitInfo; //Variabel som lagrar info om objektet som rayen träffar
            if(Physics.Raycast(ray.origin,ray.direction,out hitInfo)) //Skjut en ray från kameran
            {
                agent.destination = hitInfo.point; //Sätt agentens mål till den punkt där rayen träffar
            }
        }
    }
}
