using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterController))]
public class NPCMovement : MonoBehaviour
{
    [Header("Komponenter")]
    private CharacterController characterController;
    private Animator animator;
    private NavMeshAgent navMeshAgent;

    [Header("R�relse-inst�llningar")]
    public float g�ngHastighet = 6f;
    public float springHastighet = 12f;
    public float gravitation = 10f;
    public float vandringsRadie = 10f;
    public float status�ndringsIntervall = 5f;
    public LayerMask hinderlager;

    //[Header("AI-tillst�nd")]
    public enum NPCStatus { Inaktiv, G�ende, Springande, Interagerande }
    public NPCStatus aktuellStatus = NPCStatus.Inaktiv;

    private Vector3 startPosition;
    private Vector3 r�relsesRiktning = Vector3.zero;
    private float timer;
    private float aktuelHastighet;
    private bool r�rSig = true;

    void Start()
    {
        // H�mta n�dv�ndiga komponenter
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (characterController == null)
            Debug.LogError("CharacterController saknas!");

        if (animator == null)
            Debug.LogError("Animator saknas p� child-objektet!");

        // Inaktivera NavMeshAgent eftersom vi anv�nder CharacterController f�r r�relse
        if (navMeshAgent != null)
            navMeshAgent.enabled = false;

        // Spara startposition
        startPosition = transform.position;

        // S�tt timer
        timer = status�ndringsIntervall;

        // B�rja med ett slumpm�ssigt tillst�nd
        V�ljNyttTillst�nd();
    }

    void Update()
    {
        // Uppdatera timer
        timer -= Time.deltaTime;

        // Kolla om det �r dags att byta tillst�nd
        if (timer <= 0)
        {
            V�ljNyttTillst�nd();
            timer = status�ndringsIntervall * Random.Range(0.8f, 1.2f);
        }

        // Hantera r�relse baserat p� status
        HanteraR�relse();

        // Uppdatera animationer
        UppdateraAnimationer();
    }

    void V�ljNyttTillst�nd()
    {
        // Slumpm�ssigt v�lj ett nytt tillst�nd med viktade sannolikheter
        float slumpm�ssigtV�rde = Random.value;

        if (slumpm�ssigtV�rde < 0.3f) // 30% chans f�r inaktiv
        {
            S�ttTillst�nd(NPCStatus.Inaktiv);
        }
        else if (slumpm�ssigtV�rde < 0.9f) // 60% chans f�r g�ende
        {
            S�ttTillst�nd(NPCStatus.G�ende);
            FlyttaTillSlumpm�ssigPosition();
        }
        else // 10% chans f�r springande
        {
            S�ttTillst�nd(NPCStatus.Springande);
            FlyttaTillSlumpm�ssigPosition();
        }
    }

    void S�ttTillst�nd(NPCStatus nyttTillst�nd)
    {
        aktuellStatus = nyttTillst�nd;

        // Uppdatera r�relsehastighet baserat p� tillst�nd
        switch (nyttTillst�nd)
        {
            case NPCStatus.Inaktiv:
                r�rSig = false;
                break;

            case NPCStatus.G�ende:
                r�rSig = true;
                aktuelHastighet = g�ngHastighet;
                break;

            case NPCStatus.Springande:
                r�rSig = true;
                aktuelHastighet = springHastighet;
                break;

            case NPCStatus.Interagerande:
                r�rSig = false;
                break;
        }
    }

    void FlyttaTillSlumpm�ssigPosition()
    {
        // F� en slumpm�ssig position inom vandringsRadie fr�n startpositionen
        Vector3 slumpm�ssigRiktning = Random.insideUnitSphere * vandringsRadie;
        slumpm�ssigRiktning.y = 0; // H�ll p� samma h�jd

        Vector3 m�lPosition = startPosition + slumpm�ssigRiktning;

        // S�tt riktning mot den nya positionen
        r�relsesRiktning = (m�lPosition - transform.position).normalized;
    }

    void HanteraR�relse()
    {
        if (!r�rSig)
            return;

        // Ber�kna �nskad r�relse
        Vector3 �nskadR�relse = r�relsesRiktning * aktuelHastighet;
        �nskadR�relse.y = 0; // H�ll r�relsen horisontell

        // Applicera gravitation om NPC:n inte st�r p� marken
        if (!characterController.isGrounded)
        {
            �nskadR�relse.y -= gravitation * Time.deltaTime;
        }

        // Rotera mot r�relseriktningen
        if (r�relsesRiktning != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(r�relsesRiktning),
                10f * Time.deltaTime
            );
        }

        // Flytta NPC:n
        characterController.Move(�nskadR�relse * Time.deltaTime);

        // Om NPC:n har kommit v�ldigt n�ra sin destination, v�lj ett nytt tillst�nd
        if (Vector3.Distance(transform.position, startPosition + r�relsesRiktning * vandringsRadie) < 1f)
        {
            timer = 0; // Tvinga ett nytt tillst�nd
        }
    }

    void UppdateraAnimationer()
    {
        if (animator == null) return;

        // S�tt animator parametrar baserat p� tillst�nd
        animator.SetBool("isWalking", r�rSig && aktuellStatus == NPCStatus.G�ende);
        animator.SetBool("isRunning", r�rSig && aktuellStatus == NPCStatus.Springande);
    }

    // Metod f�r att markera n�r en spelare interagerar med NPC:n
    public void StartaInteraktion()
    {
        S�ttTillst�nd(NPCStatus.Interagerande);
        timer = status�ndringsIntervall * 2; // Ger extra tid f�r interaktion
    }

    // Metod f�r att avsluta interaktion
    public void SlutaInteraktion()
    {
        V�ljNyttTillst�nd();
    }

    // F�r att visualisera vandringsradie i redigeraren
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, vandringsRadie);
    }
}