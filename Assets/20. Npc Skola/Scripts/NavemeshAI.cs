using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

public class NavemeshAI : MonoBehaviour
{
    NavMeshAgent agent;                         // Referens f�r navmesh-komponenten
    public Transform target;                    // Referens till spelaren
    public float pathUpdateTime = 1f;           // Det tidsintervall som avg�r hur ofta en path skapas till m�let
    float timer = 0f;                           // Timerv�rde
    Animator animator;                          // Referens till animatorkomponenten
    public Transform[] waypoints;               // Array inneh�llande v�ra waypoints
    public int waypointIndex;                   // Nuvarande patrullm�l f�r agenten
    public bool randomWp;                       // Bool som avg�r om patrullv�gen ska vara slumpm�ssig eller ej
    private FieldOfView fieldOfView;            // Referens till field of view-scriptet
    public MultiAimConstraint bodyAimRig;       // Referens f�r ryggradsriggen
    public MultiAimConstraint RhandAimRig;      // Referens f�r h�gerhandsriggen
    public bool aimModeActive;                  // Bool som �r sann n�r agenten siktar                                       

    // FSM
    public enum STATE { IDLE, PATROL, CHASE, ATTACK, DEAD } // De states FSM kan befinna sig i
    public STATE state; // Variabel som inneh�ller valet av state
    public float waypointDistance = 0.5f; // Stopping distancev�rde som anv�nds i patrolstate
    public float attackDistance = 5f; // Stopping distancev�rde som anv�nds i attackstate
    public float escapeDistance = 24f; // Det avst�nd d�r agenten "gl�mmer" spelaren

    // Shoot
    private RaycastHit hitInfo; // variabel som lagrar info om vad vi tr�ffar n�r vi skjuter
    public Transform bulletPrefab; // Projektil
    public ParticleSystem muzzleFlash; // Mynningsflamma
    public ParticleSystem hitEffect; // Partiklar som alstras n�r man tr�ffar ett objekt
    public Transform bulletSpawnpoint; // Den punkt d�r projektilerna skjuts ifr�n
    float shootTimer = 0f; // Timerv�rde f�r skjutning
    public float shootFreq = 0.15f; // Skjutfrekvens
    public Transform shootTarget; // Den punkt d�r npc'n egentligen siktar
    //public float weaponamage = 19f; 

    // Ljudhantering f�r fotsteg
    public AudioClip[] footstepSounds;
    private AudioSource audioSource;
    public float footstepVolume = 0.5f;

    //Annat
    //public PlayerHealth playerHealth;  // Refrens till spelarens healthscript   -------------------

    public Slider healthSlider;
    public Canvas healthBar;
    public EnemyHealth healthScript;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        fieldOfView = GetComponent<FieldOfView>();
        waypointIndex = 0; // Nollst�ll patrullm�let
        healthBar.enabled = true;
        healthScript = GetComponent<EnemyHealth>();


        // Konfigurera ljudk�lla
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;       // 3D ljud
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.maxDistance = 20f;         // Max avst�nd f�r att h�ra ljudet
            audioSource.volume = footstepVolume;
        }

        // Konfigurera mynningselden att INTE spela automatiskt
        if (muzzleFlash != null)
        {
            var main = muzzleFlash.main;
            main.loop = false;          // St�ng av looping
            main.playOnAwake = false;   // Spela inte vid start
            muzzleFlash.Stop();         // F�rs�kra oss om att den �r stoppad vid start
        }

        // Kontrollera om waypoints finns innan vi s�tter destination
        if (waypoints != null && waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[0].position); // S�tt f�rsta m�l till f�rsta wp
        }

        state = STATE.PATROL; // S�tt utg�ngsstate till patrol
    }

    void Update()
    {
        Aim(); // Kalla p� aimfunktionen

        switch (state) // Finite statemachine som switchar mellan olika states
        {
            case STATE.IDLE:
                Idle();
                break;
            case STATE.PATROL:
                FollowWaypoints();
                break;
            case STATE.CHASE:
                ChasePlayer();
                break;
            case STATE.ATTACK:
                Attack();
                break;
            case STATE.DEAD:
                Dead();
                break;
        }
        healthSlider.value = healthScript.currentHealth / 10;
        if (healthSlider != null && healthScript != null)
        {
            healthSlider.value = healthScript.currentHealth / 10;
        }
    }

    void ChasePlayer()
    {
        aimModeActive = false; // Sluta sikta
        float distanceToPlayer = Vector3.Distance(transform.position,
        target.transform.position); // R�kna ut avst�nd mellan agent och spelare

        if (fieldOfView.canSeePlayer) // Kolla om agenten kan se spelaren
        {
            // S�tt stoppingdistance till attackdistance f�r att kunna skjuta p� l�ngre avst�nd
            agent.stoppingDistance = attackDistance;
        }
        else
        {
            agent.stoppingDistance = 1.6f; // S�tt stopping till l�gt v�rde f�r att f�lja spelaren t�tt inp�
        }

        timer -= Time.deltaTime; // Bakl�ngestimer f�r att optimera pathgeneretion
        if (timer < 0.0f)
        {
            agent.destination = target.position;
            timer = pathUpdateTime; // Resetta timern
        }

        animator.SetFloat("Speed", agent.velocity.magnitude); // S�tt animationshastigheten
        transform.LookAt(target.transform.position); // Rotera npc mot spelaren

        // G� in i attackstate om npc �r n�ra spelaren och kan se denne
        if ((agent.remainingDistance <= attackDistance) && fieldOfView.canSeePlayer)
        {
            state = STATE.ATTACK;
        }

        if (distanceToPlayer > escapeDistance) // "Gl�m" spelaren om npc befinner sig l�ngre bort �n esc-distance
        {
            state = STATE.PATROL;
        }
    }

    void FollowWaypoints()
    {
        // S�kerst�ll att mynningselden �r stoppad i detta state
        if (muzzleFlash != null && muzzleFlash.isPlaying)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // Kontrollera om waypoints finns
        if (waypoints == null || waypoints.Length == 0)
        {
            state = STATE.IDLE;
            return;
        }

        agent.stoppingDistance = waypointDistance;

        if (randomWp)
        {
            if (agent.remainingDistance < agent.stoppingDistance)
            {
                int newIndex = Random.Range(0, waypoints.Length);
                agent.SetDestination(waypoints[newIndex].position);
            }
        }
        else
        {
            if (agent.remainingDistance < agent.stoppingDistance)
            {
                waypointIndex++;
                if (waypointIndex >= waypoints.Length)
                {
                    waypointIndex = 0;
                }
                agent.SetDestination(waypoints[waypointIndex].position);
            }
        }

        animator.SetFloat("Speed", agent.velocity.magnitude);

        if (fieldOfView.canSeePlayer)
        {
            state = STATE.CHASE;
        }
    }

    void Idle()
    {
        animator.SetBool("isShooting", false);   // V�xla aniamationen till rifle idle
        aimModeActive = false;                   // sluta sikta
        // S�kerst�ll att mynningselden �r stoppad i detta state
        if (muzzleFlash != null && muzzleFlash.isPlaying)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // St� stilla och v�nta
        animator.SetFloat("Speed", 0);

        // Om agenten kan se spelaren, b�rja jaga
        if (fieldOfView.canSeePlayer)
        {
            state = STATE.CHASE;
        }
    }

    void Attack()
    {
        aimModeActive = true; // Sl� p� siktfunktionen
        transform.LookAt(target.transform.position); // Titta alltid mot spelaren
        animator.SetFloat("Speed", agent.velocity.magnitude); // Uppdatera speedparametern i animatorn

        timer -= Time.deltaTime; // Bakl�ngestimer f�r pathfinding
        if (timer < 0.0f)
        {
            agent.destination = target.position; // R�kna ut en ny path till spelaren
            timer = pathUpdateTime; // Resetta timern
        }

        // Skjut-logik - Enbart n�r shootTimer n�r noll
        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0)
        {
            Shoot(); // Skjut och spela mynningseld enbart h�r
            shootTimer = shootFreq; // Reset timer med skjutfrekvensen
        }

        // Kolla om spelaren l�mnat attackzonen och vi fortfarande kan se denne
        if ((agent.remainingDistance > attackDistance) && fieldOfView.canSeePlayer)
        {
            state = STATE.CHASE; // S�tt i s� fall state till chase
        }

        // Om vi inte l�ngre kan se spelaren, �terg� till patrullering
        if (!fieldOfView.canSeePlayer)
        {
            state = STATE.PATROL;
        }
    }



    void Shoot()
    {
        // L�gg till dessa debug-loggar
        Debug.Log($"bulletSpawnpoint: {bulletSpawnpoint}");
        Debug.Log($"bulletSpawnpoint position: {bulletSpawnpoint.position}");
        Debug.Log($"Fiende position: {transform.position}");

        if (aimModeActive)
        {
            // Kontrollera att bulletSpawnpoint �r giltig
            if (bulletSpawnpoint == null)
            {
                Debug.LogError("KRITISK: Ingen bulletSpawnpoint hittad!");
                return;
            }

            Vector3 targetPosition = target.position + Vector3.up * 1.5f;

            Transform bulletTransform = Instantiate(
                bulletPrefab,
                bulletSpawnpoint.position,  // Anv�nd ALLTID bulletSpawnpoint.position
                bulletSpawnpoint.rotation   // Och bulletSpawnpoint.rotation
            );

            bulletTransform.tag = "EnemyBullet";
            bulletTransform.GetComponent<EnemyBullet>().Setup(targetPosition);

            muzzleFlash.Emit(1);
            animator.SetBool("isShooting", true);
        }
    }

    //Skapa en kula om bulletPrefab �r satt
    //if (bulletPrefab != null && bulletSpawnpoint != null)
    //{
    //    Transform bullet = Instantiate(bulletPrefab, bulletSpawnpoint.position, bulletSpawnpoint.rotation);

    //    L�gg till kraft p� kulan om den har en Rigidbody
    //    Rigidbody rb = bullet.GetComponent<Rigidbody>();
    //    if (rb != null)
    //    {
    //        rb.AddForce(bulletSpawnpoint.forward * 40f, ForceMode.Impulse);
    //    }

    //    F�rst�r kulan efter n�gra sekunder om den inte redan f�rst�rts
    //    Destroy(bullet.gameObject, 3f);
    //}

    //Utf�r raycast f�r att se om vi tr�ffar n�got
    //if (Physics.Raycast(bulletSpawnpoint.position, bulletSpawnpoint.forward, out hitInfo, 100f))
    //{
    //    Skapa tr�ffeffekt
    //    if (hitEffect != null)
    //    {
    //        Instantiate(hitEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
    //    }

    //    Skada spelaren eller annat objekt som tr�ffas
    //    if (hitInfo.transform != null && hitInfo.transform.CompareTag("Player"))
    //    {
    //        Skada spelaren
    //         hitInfo.transform.GetComponent<PlayerHealth>().TakeDamage(10);
    //    }
    //}


    void Dead()
    {
        // Implementera d�dslogik h�r
        agent.isStopped = true;
        animator.SetTrigger("Death");
        //ragdollSetup.enabled = false;
        this.enabled = false;
        agent.enabled = false;

        // Inaktivera skript efter en viss tid
        // Destroy(this, 3f);
    }

    void Aim()
    {
        if (aimModeActive) // Agenten ska skjuta
        {
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 1f,
            Time.deltaTime * 10f)); // Blenda in skjutanimationslagret 100%

            //if (RhandAimRig != null)
            RhandAimRig.weight = 1f; // Aktivera riggarna

            //  if (bodyAimRig != null)
            bodyAimRig.weight = 1f;
            if (Physics.Raycast(bulletSpawnpoint.transform.position, bulletSpawnpoint.transform.forward, out hitInfo, 1000f))
            {
                //Debug.DrawRay(bulletSpawnpoint.transform.position, bulletSpawnpoint.transform.forward *
                //hitInfo.distance);
            }
            else
            {
                animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f,
                Time.deltaTime * 10f)); // Blenda ut skjutanimationslagret

                //if (RhandAimRig != null)
                RhandAimRig.weight = 0f; // Inaktivera riggarna

                // if (bodyAimRig != null)
                bodyAimRig.weight = 0f;
            }
        }
    }

    // Metod som anropas av animationsh�ndelsen 'Footstep'
    public void Footstep()
    {
        if (footstepSounds != null && footstepSounds.Length > 0)
        {
            int index = Random.Range(0, footstepSounds.Length);
            audioSource.PlayOneShot(footstepSounds[index], footstepVolume);
        }
    }
}

//using UnityEngine;
//using UnityEngine.AI;
//using UnityEngine.Animations.Rigging;
//using UnityEngine.UI;


//public class NavemeshAI : MonoBehaviour
//{
//    NavMeshAgent agent; //Referens f�r navmesh-komponenten
//    public Transform target; //Referens till spelaren
//    public float pathUpdateTime = 1f; //Det tidsintervall som avg�r hur ofta en path skapas till m�let
//    float timer = 0f; //Timerv�rde
//    Animator animator; //Referens till animatorkomponenten
//    public Transform[] waypoints; // Array inneh�llande v�ra waypoints
//    int waypointIndex; //Nuvarande patrullm�l f�r agenten
//    public bool randomWp; //Bool som avg�r om patrullv�gen ska vara slumpm�ssig eller ej
//    private FieldOfView fieldOfView; //Referens till field of view-scriptet
//    public MultiAimConstraint bodyAimRig; //Referens f�r ryggradsriggen
//    public MultiAimConstraint RhandAimRig; //Referens f�r h�gerhandsriggen
//    public bool aimModeActive; //Bool som �r sann n�r agenten siktar
//    public PlayerHealth playerHealth; //Referens till spelarens healthscript
//    public EnemyHealth enemyHealth; //Referens till det egna healthscriptet
//    public RagdollSetup ragdollSetup; //Referens till ragdollscriptet
//    public Slider healthSlider; //Referens till healthbar
//    public Canvas healthBar; //Referens till healthbarens canvasobjekt


//    //FSM
//    public enum STATE { IDLE, PATROL, CHASE, ATTACK, DEAD } //De states FSM kan befinna sig i
//    public STATE state; //Variabel som inneh�ller valet av state
//    public float waypointDistance = 0.5f; //Stopping distancev�rde som anv�nds i patrolstate
//    public float attackDistance = 5f; //Stopping distancev�rde som anv�nds i attackstate
//    public float escapeDistance = 24f; //Det avst�nd d�r agenten "gl�mmer" spelaren

//    //Shoot
//    private RaycastHit hitInfo; //variabel som lagrar info om vad vi tr�ffar n�r vi skjuter
//    public Transform bulletPrefab; //Projektil
//    public ParticleSystem muzzleFlash; //Mynningsflamma
//    public ParticleSystem hitEffect; //Partiklar som alstras n�r man tr�ffar ett objekt
//    public Transform bulletSpawnpoint; //Den punkt d�r projektilerna skjuts ifr�n
//    float shootTimer = 0.5f; //Timerv�rde
//    float shootFreq = 0.15f; //Skjutfrekvens
//    public Transform shootTarget; //Den punkt d�r npc'n egentligen siktar
//    public float weaponDamage = 10f; //Den skada en tr�ff ger
//    public float shootInaccuracy = 50f; //Variabel som inf�r slumpen i tr�ffs�kerheten
//    public Vector3 shootOffset; //Roterar shootspawn med hj�lp av ovanst�ende v�rde


//    void Start()
//    {
//        agent = GetComponent<NavMeshAgent>();
//        animator = GetComponent<Animator>();
//        fieldOfView = GetComponent<FieldOfView>();
//        waypointIndex = 0; //Nollst�ll patrullm�let
//        playerHealth = target.gameObject.GetComponent<PlayerHealth>(); //S�k reda p� spelarens healthscript
//        enemyHealth = GetComponent<EnemyHealth>(); //H�mta fiendens healthscript
//        ragdollSetup = GetComponent<RagdollSetup>();

//        if (randomWp)
//        {
//            agent.SetDestination(waypoints[Random.Range(0, waypoints.Length)].transform.position); //S�tt f�rsta m�l random wp 
//        }
//        else
//        {
//            agent.SetDestination(waypoints[0].transform.position); //S�tt f�rsta m�l till f�rsta wp
//        }
//        state = STATE.PATROL; //S�tt utg�ngsstate till patrol

//    }


//    void Update()
//    {
//        Aim(); //Kalla p� aimfunktionen

//        switch (state) //Finite statemachine som switchar mellan olika states
//        {
//            case STATE.IDLE:
//                Idle();
//                break;
//            case STATE.PATROL:
//                FollowWaypoints();
//                break;
//            case STATE.CHASE:
//                ChasePlayer();
//                break;
//            case STATE.ATTACK:
//                Attack();
//                break;
//            case STATE.DEAD:
//                Dead();
//                break;
//        }

//        healthSlider.value = enemyHealth.currentHealth / 10;
//    }


//    void ChasePlayer()
//    {
//        aimModeActive = false; //Sluta sikta
//        float distanceToPlayer = Vector3.Distance(transform.position,
//        target.transform.position); //R�kna ut avst�nd mellan agent och spelare
//        if (fieldOfView.canSeePlayer) //Kolla om agenten kan se spelaren
//        {
//            //S�tt stoppingdistance till attackdistance f�r att kunna skjuta p� l�ngre avst�nd
//            agent.stoppingDistance = attackDistance;
//        }
//        else
//            agent.stoppingDistance = 1.6f; //S�tt stopping till l�gt v�rde f�r att f�lja spelaren t�tt inp�

//        timer -= Time.deltaTime; //Bakl�ngestimer f�r att optimera pathgeneretion
//        if (timer < 0.0f)
//        {
//            agent.destination = target.position; //F�lj spelaren
//            timer = pathUpdateTime; //Resetta timern
//        }
//        animator.SetFloat("Speed", agent.velocity.magnitude); //S�tt animationshastigheten
//        transform.LookAt(target.transform.position); //Rotera npc mot spelaren

//        //G� in i attackstate om npc �r n�ra spelaren och kan se denne
//        if ((agent.remainingDistance <= attackDistance) && fieldOfView.canSeePlayer)
//        {
//            state = STATE.ATTACK;
//        }
//        if (distanceToPlayer > escapeDistance) //"Gl�m" spelaren om npc befinner sig l�ngre bort �n esc-distance
//        {
//            state = STATE.PATROL; //�terg� till patrolstate
//        }

//        if (enemyHealth.isDead) //Kolla om agenten �r d�d
//        {
//            Dead(); //Byt state till dead
//        }
//    }

//    void FollowWaypoints()
//    {
//        agent.stoppingDistance = waypointDistance;//St�ll in stopping distance till ett l�gt v�rde

//        if (randomWp) //Best�mmer om patrullv�gen ska vara slumpm�ssig
//        {
//            if (agent.remainingDistance < agent.stoppingDistance)
//            {
//                agent.SetDestination(waypoints[Random.Range(0,
//                    waypoints.Length)].transform.position); //Slumpa en waypoint som patrullm�l
//            }
//        }
//        else
//        {
//            if (agent.remainingDistance < agent.stoppingDistance) //Kolla om agenten befinner sig n�ra aktuell wp
//            {
//                waypointIndex++; //�ka is s� fall index med 1
//                if (waypointIndex >= waypoints.Length)
//                    waypointIndex = 0; //Resetta index f�r att skapa en loopande patrullering
//            }
//            agent.SetDestination(waypoints[waypointIndex].transform.position); //S�tt agentens m�l till aktuell wp
//        }
//        animator.SetFloat("Speed", agent.velocity.magnitude); //S�tt agentens animationshastighet

//        if (fieldOfView.canSeePlayer) //Kolla om agenten kan se spelaren
//        {
//            state = STATE.CHASE; //G� in i chase-state
//        }

//        if (enemyHealth.isDead) //Kolla om agenten �r d�d
//        {
//            Dead(); //Byt state till dead
//        }
//    }

//    void Idle()
//    {
//        animator.SetBool("isShooting", false); //V�xla animation till rifle idle
//        aimModeActive = false; //Sluta sikta p� spelaren

//        if (enemyHealth.isDead) //Kolla om agenten �r d�d
//        {
//            Dead(); //Byt state till dead
//        }
//    }

//    void Attack()
//    {
//        shootTimer -= Time.deltaTime; //Bakl�ngestimer
//        if (shootTimer <= 0f)
//        {
//            Shoot(); //Skjut
//            shootTimer = shootFreq; //Resetta timern
//        }

//        aimModeActive = true; //Sl� p� siktfunktionen
//        transform.LookAt(target.transform.position); //Titta alltid mot spelaren
//        animator.SetFloat("Speed", agent.velocity.magnitude); //Uppdatera speedparametern i animatorn
//        timer -= Time.deltaTime; //Bakl�ngestimer
//        if (timer < 0.0f)
//        {
//            agent.destination = target.position; //R�kna ut en ny path till spelaren
//            timer = pathUpdateTime; //Resetta timern
//        }
//        //Kolla om spelaren l�mnat attackzonen och vi fortfarande kan se denne
//        if ((agent.remainingDistance > attackDistance) && fieldOfView.canSeePlayer)
//        {
//            state = STATE.CHASE; //S�tt i s� fall state till chase
//        }

//        if (playerHealth.isDead) //Kolla om spelaren �r d�d
//        {
//            state = STATE.IDLE; //Byt state till idle
//        }

//        if (enemyHealth.isDead) //Kolla om agenten �r d�d
//        {
//            Dead(); //Byt state till dead
//        }
//    }

//    void Dead()
//    {
//        animator.enabled = false; //Sl� av animatorn
//        ragdollSetup.ActivateRagdoll(); //Aktivera ragdoll
//        this.enabled = false; //Sl� av detta script
//        agent.enabled = false; //Sl� av navmesh agent-komponenten f�r att om�jligg�ra f�rflyttning
//        fieldOfView.enabled = false;
//        healthBar.enabled = false; //Sl� av healthbar
//    }

//    void Aim()
//    {
//        if (aimModeActive) //Agenten ska skjuta
//        {
//            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 1f,
//            Time.deltaTime * 10f)); //Blenda in skjutanimationslagret 100%
//            RhandAimRig.weight = 1f; //Aktivera riggarna
//            bodyAimRig.weight = 1f;
//            Vector3 shootRand = Random.insideUnitSphere * shootInaccuracy; //Slumpa skottets tr�ffpunkt
//            //Konvertera localspace till worldspace f�r att undvika problem vid raycasting
//            Vector3 shootDir = transform.TransformPoint(shootOffset + shootRand);

//            Ray ray = new Ray(bulletSpawnpoint.transform.position, shootDir * 1000f); //Ray som anv�nds vid raycast
//            //Skjut en ray mot m�let och lagra info om tr�ffat objekt i hitInfo
//            if (Physics.Raycast(ray, out hitInfo))
//            {
//                //Rita ut kulans v�g mot m�let
//                //Debug.DrawRay(bulletSpawnpoint.transform.position, shootDir * 1000f);
//            }
//        }
//        else
//        {
//            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f,
//            Time.deltaTime * 10f)); //Blenda ut skjutanimationslagret
//            RhandAimRig.weight = 0f; //Inaktivera riggarna
//            bodyAimRig.weight = 0f;
//        }
//    }

//    void Shoot()
//    {
//        if (aimModeActive) //Kolla om aim-mode �r aktivt och agenten kan se spelaren
//        {
//            //Instansiera ett skott
//            Transform bulletTransform = Instantiate(bulletPrefab, bulletSpawnpoint.position, Quaternion.identity);
//            bulletTransform.GetComponent<EnemyBullet>().Setup(shootTarget.position); //Skjut projektilen mot m�let
//            if (hitInfo.collider != null && hitInfo.collider.tag != "NotShootable")
//            {
//                hitEffect.transform.position = hitInfo.point; //Placera hiteffekten d�r skottet tr�ffar
//                hitEffect.transform.forward = hitInfo.normal; //Rotera hiteffekten i samma riktining som den tr�ffade ytans normal
//                hitEffect.Emit(1); //Starta hiteffekten 
//            }
//            muzzleFlash.Play(); //Starta muzzleflash-effekt
//            animator.SetBool("isShooting", true); //Starta shoot-animationen

//            if (hitInfo.transform != null && hitInfo.transform.GetComponent<HitboxPlayer>()) //Kolla om det tr�ffade objektet har ett hitboxscript
//            {
//                hitInfo.transform.GetComponent<HitboxPlayer>().OnRaycastHit(weaponDamage); //Dra skadev�rdet fr�n health                                                                     
//            }
//        }
//        else
//        {
//            animator.SetBool("isShooting", false); //Stoppa shoot-animationen
//            muzzleFlash.Stop(); //St�ng av muzzleflash
//            hitEffect.Emit(0); //St�ng av hiteffect
//        }
//    }
//}
