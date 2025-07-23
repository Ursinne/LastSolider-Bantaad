using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class JumpScript : MonoBehaviour
{
    private NavMeshAgent agent; //Referens till navmeshagent-komponenten
    public float jumpHeight = 2f; //variabel som styr hoppets h�jd
    public float jumpDuration = 0.5f; //variabel som styr hoppets l�ngd i tid
    private Animator animator;  //Referensf�lt f�r animatorn

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoTraverseOffMeshLink = false; //St�ng av funktionen f�r automatiskt anv�ndande av links 
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
            animator.SetTrigger("Landed"); //Byt state till run n�r hoppet �r genomf�rt
        }
    }

    IEnumerator Jump(float height, float duration) //Coroutine som styr hur hoppet ser ut
    {
        OffMeshLinkData data = agent.currentOffMeshLinkData; //L�s in information om aktuell offmeshlink
        Vector3 startPos = agent.transform.position; //lagra agentens nuvarande position som ska utg�ra hoppets start
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset; //Utforma hoppet som en parabelkurva.
        float normalizedTime = 0.0f; //Lokal variabel som anv�nds f�r att avg�ra om hoppet genomf�rts eller ej
        animator.SetTrigger("Jump"); //Starta jump-animationen

        while (normalizedTime < 1.0f) //Loop 
        {
            float yOffset = height * (normalizedTime - normalizedTime * normalizedTime); //Utforma hoppet som en parabelkurva.
            agent.transform.position = Vector3.Lerp(startPos, endPos, normalizedTime) + yOffset * Vector3.up; //Smootha mellan start och endpos �ver hoppets f�rlopp
            normalizedTime += Time.deltaTime / duration; //Uppdatera normalizedTime under hoppets g�ng
            yield return null; //Pausa coroutinen  
        }
            
    }
}
