using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class JumpScript : MonoBehaviour
{
    private NavMeshAgent agent; //Referens till navmeshagent-komponenten
    public float jumpHeight = 2f; //variabel som styr hoppets höjd
    public float jumpDuration = 0.5f; //variabel som styr hoppets längd i tid
    private Animator animator;  //Referensfält för animatorn

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoTraverseOffMeshLink = false; //Stäng av funktionen för automatiskt användande av links 
        animator = GetComponentInChildren<Animator>();
    }


    void Update()
    {
        if(agent.isOnOffMeshLink)
        {
            StartCoroutine(Jump(jumpHeight,jumpDuration)); //Starta timerfunktionen
            agent.CompleteOffMeshLink(); //Avsluta hoppet
        }
        else
        {
            animator.SetTrigger("Landed"); //Byt state till run när hoppet är genomfört
        }
    }

    IEnumerator Jump(float height, float duration) //Coroutine som styr hur hoppet ser ut
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData; //Läs in information om aktuell offmeshlink
        Vector3 startPos = agent.transform.position; //lagra agentens nuvarande position som ska utgöra hoppets start
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset; //Utforma hoppet som en parabelkurva.
        float normalizedTime = 0.0f; //Lokal variabel som används för att avgöra om hoppet genomförts eller ej
        animator.SetTrigger("Jump"); //Starta jump-animationen

        while (normalizedTime < 1.0f) //Loop 
        {
            float yOffset = height * (normalizedTime - normalizedTime * normalizedTime); //Utforma hoppet som en parabelkurva.
            agent.transform.position = Vector3.Lerp(startPos, endPos, normalizedTime) + yOffset * Vector3.up; //Smootha mellan start och endpos över hoppets förlopp
            normalizedTime += Time.deltaTime / duration; //Uppdatera normalizedTime under hoppets gång
            yield return null; //Pausa coroutinen  
        }
            
    }
}
