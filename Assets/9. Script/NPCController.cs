using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterController))]
public class NPCMovement : MonoBehaviour
{
    [Header("Komponenter")]
    private CharacterController characterController;
    private Animator animator;
    private NavMeshAgent navMeshAgent;

    [Header("Rörelse-inställningar")]
    public float gångHastighet = 6f;
    public float springHastighet = 12f;
    public float gravitation = 10f;
    public float vandringsRadie = 10f;
    public float statusÄndringsIntervall = 5f;
    public LayerMask hinderlager;

    //[Header("AI-tillstånd")]
    public enum NPCStatus { Inaktiv, Gående, Springande, Interagerande }
    public NPCStatus aktuellStatus = NPCStatus.Inaktiv;

    private Vector3 startPosition;
    private Vector3 rörelsesRiktning = Vector3.zero;
    private float timer;
    private float aktuelHastighet;
    private bool rörSig = true;

    void Start()
    {
        // Hämta nödvändiga komponenter
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (characterController == null)
            Debug.LogError("CharacterController saknas!");

        if (animator == null)
            Debug.LogError("Animator saknas på child-objektet!");

        // Inaktivera NavMeshAgent eftersom vi använder CharacterController för rörelse
        if (navMeshAgent != null)
            navMeshAgent.enabled = false;

        // Spara startposition
        startPosition = transform.position;

        // Sätt timer
        timer = statusÄndringsIntervall;

        // Börja med ett slumpmässigt tillstånd
        VäljNyttTillstånd();
    }

    void Update()
    {
        // Uppdatera timer
        timer -= Time.deltaTime;

        // Kolla om det är dags att byta tillstånd
        if (timer <= 0)
        {
            VäljNyttTillstånd();
            timer = statusÄndringsIntervall * Random.Range(0.8f, 1.2f);
        }

        // Hantera rörelse baserat på status
        HanteraRörelse();

        // Uppdatera animationer
        UppdateraAnimationer();
    }

    void VäljNyttTillstånd()
    {
        // Slumpmässigt välj ett nytt tillstånd med viktade sannolikheter
        float slumpmässigtVärde = Random.value;

        if (slumpmässigtVärde < 0.3f) // 30% chans för inaktiv
        {
            SättTillstånd(NPCStatus.Inaktiv);
        }
        else if (slumpmässigtVärde < 0.9f) // 60% chans för gående
        {
            SättTillstånd(NPCStatus.Gående);
            FlyttaTillSlumpmässigPosition();
        }
        else // 10% chans för springande
        {
            SättTillstånd(NPCStatus.Springande);
            FlyttaTillSlumpmässigPosition();
        }
    }

    void SättTillstånd(NPCStatus nyttTillstånd)
    {
        aktuellStatus = nyttTillstånd;

        // Uppdatera rörelsehastighet baserat på tillstånd
        switch (nyttTillstånd)
        {
            case NPCStatus.Inaktiv:
                rörSig = false;
                break;

            case NPCStatus.Gående:
                rörSig = true;
                aktuelHastighet = gångHastighet;
                break;

            case NPCStatus.Springande:
                rörSig = true;
                aktuelHastighet = springHastighet;
                break;

            case NPCStatus.Interagerande:
                rörSig = false;
                break;
        }
    }

    void FlyttaTillSlumpmässigPosition()
    {
        // Få en slumpmässig position inom vandringsRadie från startpositionen
        Vector3 slumpmässigRiktning = Random.insideUnitSphere * vandringsRadie;
        slumpmässigRiktning.y = 0; // Håll på samma höjd

        Vector3 målPosition = startPosition + slumpmässigRiktning;

        // Sätt riktning mot den nya positionen
        rörelsesRiktning = (målPosition - transform.position).normalized;
    }

    void HanteraRörelse()
    {
        if (!rörSig)
            return;

        // Beräkna önskad rörelse
        Vector3 önskadRörelse = rörelsesRiktning * aktuelHastighet;
        önskadRörelse.y = 0; // Håll rörelsen horisontell

        // Applicera gravitation om NPC:n inte står på marken
        if (!characterController.isGrounded)
        {
            önskadRörelse.y -= gravitation * Time.deltaTime;
        }

        // Rotera mot rörelseriktningen
        if (rörelsesRiktning != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(rörelsesRiktning),
                10f * Time.deltaTime
            );
        }

        // Flytta NPC:n
        characterController.Move(önskadRörelse * Time.deltaTime);

        // Om NPC:n har kommit väldigt nära sin destination, välj ett nytt tillstånd
        if (Vector3.Distance(transform.position, startPosition + rörelsesRiktning * vandringsRadie) < 1f)
        {
            timer = 0; // Tvinga ett nytt tillstånd
        }
    }

    void UppdateraAnimationer()
    {
        if (animator == null) return;

        // Sätt animator parametrar baserat på tillstånd
        animator.SetBool("isWalking", rörSig && aktuellStatus == NPCStatus.Gående);
        animator.SetBool("isRunning", rörSig && aktuellStatus == NPCStatus.Springande);
    }

    // Metod för att markera när en spelare interagerar med NPC:n
    public void StartaInteraktion()
    {
        SättTillstånd(NPCStatus.Interagerande);
        timer = statusÄndringsIntervall * 2; // Ger extra tid för interaktion
    }

    // Metod för att avsluta interaktion
    public void SlutaInteraktion()
    {
        VäljNyttTillstånd();
    }

    // För att visualisera vandringsradie i redigeraren
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, vandringsRadie);
    }
}