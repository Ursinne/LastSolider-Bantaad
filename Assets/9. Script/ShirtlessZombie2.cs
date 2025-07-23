    using UnityEngine;
    using UnityEngine.AI;
    using System.Collections;
    using System.Collections.Generic;

    public class NPCShirtlessZombie2 : MonoBehaviour
    {
        [Header("Zombie Settings")]
        public float detectionRange = 10f;      // R�ckvidd d�r zombien uppt�cker spelaren
        public float chaseRange = 20f;          // Ut�kad r�ckvidd n�r zombien redan jagar spelaren
        public float attackRange = 1.5f;        // R�ckvidd d�r zombien kan attackera spelaren
        public float zombieSpeed = 2f;          // Zombiens normala r�relsehastighet
        public float chaseSpeed = 3.5f;         // Zombiens r�relsehastighet n�r den jagar
        public float wanderSpeed = 1f;          // Hastighet n�r zombien vandrar
        public float attackCooldown = 2f;       // Tid mellan attacker
        public float wanderRadius = 10f;        // Hur l�ngt zombien vandrar fr�n sin startposition
        public float minWanderWaitTime = 3f;    // Minsta tid zombien v�ntar mellan vandringar
        public float maxWanderWaitTime = 8f;    // L�ngsta tid zombien v�ntar mellan vandringar
        public float memoryDuration = 8f;       // Hur l�nge zombien minns spelaren efter att den f�rsvunnit

        [Header("Health Settings")]
        public float maxHealth = 100f;          // Zombiens maximala h�lsa
        public float currentHealth;             // Zombiens nuvarande h�lsa
        public float attackDamage = 10f;        // Skada som zombien g�r vid attack

        [Header("Death Settings")]
        public float deathAnimationTime = 3f;   // Hur l�ng tid d�dsanimationen spelas innan objekt f�rst�rs
        public GameObject[] dropItems;          // M�jliga items som zombien kan sl�ppa n�r den d�r
        [Range(0, 1)] public float dropChance = 0.3f; // Chans att zombien sl�pper n�got item

        [Header("Scream Settings")]
        public float screamRange = 30f;         // R�ckvidd f�r skriket (hur l�ngt andra zombier kan h�ra)
        public float screamCooldown = 10f;      // Tid mellan skrik
        public bool canScream = true;           // Om zombien kan skrika
        public float screamAnimationDuration = 1.5f; // L�ngd p� skrikanimationen
        public AudioClip screamSound;           // Ljudet f�r skriket

        [Header("References")]
        private CharacterController controller;  // Referens till CharacterController-komponenten
        private Animator animator;               // Referens till Animator-komponenten
        private Transform player;                // Referens till spelarens transform
        private AudioSource audioSource;         // Referens till AudioSource-komponenten

        // Status-variabler
        private bool playerDetected = false;
        private float lastAttackTime;
        private float lastScreamTime;           // Tid f�r senaste skriket
        private Vector3 startPosition;          // Zombiens startposition
        private Vector3 targetPosition;         // Destination f�r zombiens vandring
        private float wanderTimer;              // Timer f�r vandringsintervall
        private bool isWandering = false;       // Om zombien f�r n�rvarande vandrar
        private bool isDead = false;
        private bool canAttack = true;          // Om zombien kan attackera (f�r att f�rhindra attackspam)
        private bool hasScreamedForPlayer = false; // Om zombien redan har skrikit f�r denna spelare

        // Nya variabler f�r f�rb�ttrad jaktlogik
        private bool isChasing = false;         // Om zombien aktivt jagar spelaren
        private float chaseTimer = 0f;          // Timer f�r hur l�nge zombien ska forts�tta jaga efter att spelaren f�rsvunnit
        private Vector3 lastKnownPlayerPosition; // Senast k�nda position f�r spelaren
        private bool playerInSight = false;     // Om zombien faktiskt kan se spelaren
        private float playerDistanceLastFrame = float.MaxValue; // F�r att best�mma om spelaren n�rmar sig eller flyr
        private bool isScreaming = false;       // Om zombien f�r n�rvarande skriker

        // Variabler f�r d�dsspelarbeteende
        public bool isPlayerDead = false;
        public float bitingDistance = 1.0f;
        private Vector3 deadPlayerPosition;
        private bool isEating = false;          // Om zombien �ter p� en d�d spelare

        void Start()
        {
            // Spara startposition
            startPosition = transform.position;

            // Initialisera h�lsa
            currentHealth = maxHealth;

            // Hitta n�dv�ndiga komponenter
            controller = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
        
            // Om det inte finns en AudioSource, l�gg till en
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1.0f; // 3D-ljud
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                audioSource.maxDistance = screamRange;
            }

            // Hitta spelaren med tag
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                Debug.LogWarning("Ingen spelare med taggen 'Player' hittades!");
            }

            // Starta zombien med ett idle-l�ge
            wanderTimer = Random.Range(minWanderWaitTime, maxWanderWaitTime);
        }

        void Update()
        {
            // Om zombien �r d�d, avbryt uppdateringen
            if (isDead || controller == null) return;

            // Nyckel-tester f�r debugging
            if (Input.GetKeyDown(KeyCode.R))
            {
                // Manuellt aktivera/inaktivera spring
                if (animator != null)
                {
                    bool currentRunState = animator.GetBool("isRunning");
                    animator.SetBool("isRunning", !currentRunState);
                    animator.SetBool("isWalking", currentRunState);
                    Debug.Log($"Manuellt v�xlade isRunning till {!currentRunState}");
                }
            }

            // Om zombien skriker, avvakta tills skriket �r klart
            if (isScreaming)
            {
                // N�r zombien skriker, se till att bara skrikanimationen spelas
                if (animator != null)
                {
                    animator.SetBool("isScreaming", true);
                    animator.SetBool("isWalking", false);
                    animator.SetBool("isRunning", false);
                    animator.SetBool("isZombieAttacing", false);
                    animator.SetBool("isBiting", false);
                }
                return;
            }

            // Om spelaren �r d�d och zombien vet om det
            if (isPlayerDead)
            {
                GoToDeadPlayer();
                return;
            }

            // Om spelaren inte hittats, f�rs�k igen (f�r fall d�r spelaren spawnas efter zombien)
            if (player == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                {
                    player = playerObject.transform;
                    Debug.Log("Hittade spelaren: " + player.name);
                }
                else
                {
                    Debug.LogWarning("Ingen spelare med taggen 'Player' hittades!");
                    // Om ingen spelare finns, bara vandra runt
                    IdleOrWander();

                    // VIKTIGT: Anropa inte UpdateAnimations() h�r, l�t IdleOrWander sk�ta det
                    return;
                }
            }

            // Ber�kna avst�nd till spelaren
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Debug-info om spelaravst�nd
            //Debug.Log($"Avst�nd till spelare: {distanceToPlayer}, detektionsr�ckvidd: {detectionRange}");

            // Uppdatera tidigare-frame data
            if (playerInSight)
            {
                playerDistanceLastFrame = distanceToPlayer;
                lastKnownPlayerPosition = player.position;
            }

            // Kontrollera om spelaren �r inom detektionsr�ckvidden
            float effectiveRange = isChasing ? chaseRange : detectionRange;
            bool wasPlayerInSight = playerInSight;
            playerInSight = distanceToPlayer <= effectiveRange;

            // Om vi just uppt�ckte spelaren, logga detta
            if (playerInSight && !wasPlayerInSight)
            {
                Debug.Log("Zombie uppt�ckte spelaren!");
            }

            // Logik f�r att hantera jaktl�ge
            if (playerInSight)
            {
                // Om vi just uppt�ckt spelaren och inte skrikit �n, skrik
                if (!playerDetected && canScream && !hasScreamedForPlayer)
                {
                    Debug.Log("Zombie b�rjar skrika!");
                    Scream();
                    hasScreamedForPlayer = true;

                    // L�gg till en f�rdr�jning innan jakt b�rjar
                    StartCoroutine(StartChasingAfterScream());
                }
                else if (!playerDetected)
                {
                    Debug.Log($"Uppt�ckte spelare men skriker inte. canScream={canScream}, hasScreamedForPlayer={hasScreamedForPlayer}");
                }

                playerDetected = true;
                isChasing = true;
                chaseTimer = memoryDuration; // �terst�ll minnestimern

                // Jaga spelaren n�r den �r inom syn
                ChasePlayer();

                // Attackera bara om inom r�ckh�ll
                if (distanceToPlayer <= attackRange && canAttack && Time.time > lastAttackTime + attackCooldown)
                {
                    AttackPlayer();
                }
            }
            else if (isChasing)
            {
                // Spelaren �r inte synlig men zombien �r fortfarande i jaktl�ge
                chaseTimer -= Time.deltaTime;
                if (chaseTimer > 0)
                {
                    // Forts�tt jaga spelaren baserat p� senast k�nda position
                    ChaseLastKnownPosition();
                }
                else
                {
                    // Minnet om spelaren har bleknat, �terg� till vandringsl�ge
                    isChasing = false;
                    playerDetected = false;
                    hasScreamedForPlayer = false; // �terst�ll skrikflaggan n�r vi gl�mmer spelaren
                    IdleOrWander();
                }
            }
            else
            {
                // Zombien ser inte spelaren och jagar inte, vandra slumpm�ssigt
                IdleOrWander();
            }

            // VIKTIGT: Anropa INTE UpdateAnimations() h�r
            // Vi l�ter varje metod (ChasePlayer, IdleOrWander, etc.) hantera sina egna animationer
            // UpdateAnimations();
        }

        private void Scream()
        {
            if (isScreaming || Time.time < lastScreamTime + screamCooldown) return;

            isScreaming = true;
            lastScreamTime = Time.time;

            // Spela skrikanimation - anv�nd b�de trigger och bool f�r s�kerhets skull
            if (animator != null)
            {
                // Anv�nd alla m�jliga varianter av namnet f�r att t�cka alla basis
                animator.SetTrigger("isScreaming");
                animator.SetTrigger("Scream");
            
                // S�tt �ven bool-v�rdet
                animator.SetBool("isScreaming", true);
            
                // St�ng av andra animationer
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", false);
                animator.SetBool("isZombieAttacing", false);
                animator.SetBool("isBiting", false);
            
                Debug.Log("Zombie aktiverar skrikanimation!");
            }
            else
            {
                Debug.LogError("Animator saknas p� zombie!");
            }

            // Spela skrikljud
            if (audioSource != null && screamSound != null)
            {
                audioSource.clip = screamSound;
                audioSource.Play();
            }
            else
            {
                Debug.Log("AudioSource eller screamSound saknas - inget ljud spelas");
            }

            // Alarmera alla zombier inom r�ckh�ll
            AlertNearbyZombies();

            // �terst�ll skrikstatus efter animationen
            StartCoroutine(ResetScream());
        }

        private IEnumerator ResetScream()
        {
            yield return new WaitForSeconds(screamAnimationDuration);
            isScreaming = false;
        
            // �terst�ll skrikanimatorn
            if (animator != null)
            {
                animator.SetBool("isScreaming", false);
            }
        
            Debug.Log("Zombie slutar skrika och forts�tter jaga");
        }

        private void AlertNearbyZombies()
        {
            // Hitta alla zombier inom r�ckh�ll
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, screamRange);
            foreach (var hitCollider in hitColliders)
            {
                NPCShirtlessZombie2 otherZombie = hitCollider.GetComponent<NPCShirtlessZombie2>();
            
                // Om det �r en annan zombie �n den h�r och den inte �r d�d
                if (otherZombie != null && otherZombie != this && !otherZombie.isDead)
                {
                    // Alarmera zombien om spelaren
                    otherZombie.AlertedByScream(lastKnownPlayerPosition);
                }
            }

            Debug.Log("Zombie skrek och alarmerade n�rliggande zombier!");
        }

        public void AlertedByScream(Vector3 suspiciousPosition)
        {
            // Om zombien redan jagar eller �r d�d, ignorera larmet
            if (isChasing || isDead) return;

            // S�tt zombien i jaktl�ge mot den misst�nkta positionen
            isChasing = true;
            chaseTimer = memoryDuration;
            lastKnownPlayerPosition = suspiciousPosition;
            playerDetected = true;

            Debug.Log("Zombie h�rde ett skrik och r�r sig mot platsen!");
        }

        private void ChaseLastKnownPosition()
        {
            if (Vector3.Distance(transform.position, lastKnownPlayerPosition) < 1.5f)
            {
                // N�r zombien n�r den senast k�nda positionen men inte hittar spelaren
                // b�rja vandra slumpm�ssigt i omr�det
                isChasing = false;
                wanderTimer = 0; // Tvinga omedelbar vandring
                return;
            }

            // Rikta mot den senast k�nda positionen
            Vector3 direction = (lastKnownPlayerPosition - transform.position).normalized;
            direction.y = 0;

            // Rotera mot m�let
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);

            // Flytta mot den senast k�nda positionen
            if (controller != null && controller.enabled)
            {
                Vector3 movement = direction * zombieSpeed * Time.deltaTime;

                // L�gg till gravitation
                if (!controller.isGrounded)
                {
                    movement.y = Physics.gravity.y * Time.deltaTime;
                }

                controller.Move(movement);
            }
        }

        private void GoToDeadPlayer()
        {
            if (isDead) return;

            // Om vi redan �ter, forts�tt med det
            if (isEating)
            {
                if (animator != null)
                {
                    animator.SetBool("isBiting", true);
                    animator.SetBool("isWalking", false);
                    animator.SetBool("isRunning", false);
                    animator.SetBool("isZombieAttacing", false);
                    animator.SetBool("isScreaming", false);
                }
                return;
            }

            // Ber�kna avst�nd till den d�da spelaren
            float distanceToBody = Vector3.Distance(transform.position, deadPlayerPosition);

            if (distanceToBody > bitingDistance)
            {
                // G� mot kroppen
                Vector3 direction = (deadPlayerPosition - transform.position).normalized;
                direction.y = 0;

                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);

                if (controller != null && controller.enabled)
                {
                    Vector3 movement = direction * zombieSpeed * Time.deltaTime;
                    if (!controller.isGrounded)
                    {
                        movement.y = Physics.gravity.y * Time.deltaTime;
                    }
                    controller.Move(movement);
                }

                // Uppdatera animationer f�r att visa att zombien g�r
                if (animator != null)
                {
                    animator.SetBool("isWalking", true);
                    animator.SetBool("isRunning", false);
                    animator.SetBool("isBiting", false);
                    animator.SetBool("isZombieAttacing", false);
                    animator.SetBool("isScreaming", false);
                }
            }
            else
            {
                // Bitanimation n�r zombien n�r kroppen
                isEating = true;
                if (animator != null)
                {
                    animator.SetBool("isBiting", true);
                    animator.SetBool("isWalking", false);
                    animator.SetBool("isRunning", false);
                    animator.SetBool("isZombieAttacing", false);
                    animator.SetBool("isScreaming", false);
                }
            }
        }

        private void ChasePlayer()
        {
            if (player == null) return;

            // Avbryt vandring n�r vi jagar
            isWandering = false;

            // Ber�kna avst�nd till spelaren
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // V�nd zombien mot spelaren - alltid!
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0; // Ignorera h�jdskillnad f�r rotation

            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);

            // �NDRING: Anv�nd alltid chaseSpeed n�r vi jagar spelaren
            // Detta g�r att zombien alltid springer mot spelaren
            float currentSpeed = chaseSpeed;
            bool isRunningAfterPlayer = true;

            // Debug-utskrift f�r hastighet
            Debug.Log($"Zombie jagar spelare med hastighet: {currentSpeed}");

            // S�tt animations-booleans f�r att matcha beteendet
            if (animator != null)
            {
                animator.SetBool("isRunning", isRunningAfterPlayer);
                animator.SetBool("isWalking", false); // Aldrig g� n�r vi jagar
                animator.SetBool("isScreaming", false);
            
                // Debug f�r animationsstatus
                Debug.Log($"Animations-state: isRunning={animator.GetBool("isRunning")}, isWalking={animator.GetBool("isWalking")}");
            }
            else
            {
                Debug.LogError("Animator saknas p� zombie!");
            }

            // Flytta zombien mot spelaren
            if (controller != null && controller.enabled)
            {
                Vector3 movement = direction * currentSpeed * Time.deltaTime;

                // L�gg till gravitation om det beh�vs
                if (!controller.isGrounded)
                {
                    movement.y = Physics.gravity.y * Time.deltaTime;
                }

                controller.Move(movement);
            }
            else
            {
                //Debug.LogError("CharacterController saknas eller �r inaktiverad p� zombie!");
            }
        }

        private void AttackPlayer()
        {
            if (player == null) return;

            // Registrera tid f�r attacken
            lastAttackTime = Time.time;
            canAttack = false;

            // Spela attackanimation
            if (animator != null)
            {
                animator.SetTrigger("Attack");
                animator.SetBool("isZombieAttacing", true);
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", false);
                animator.SetBool("isBiting", false);
                animator.SetBool("isScreaming", false);

                // �terst�ll attack-animationen efter en kort stund
                StartCoroutine(ResetAttackAnimation(1.0f));
            }

            // G�r skadekontroll f�rst efter en kort f�rdr�jning s� att det matchar animationen
            StartCoroutine(DealDamageAfterDelay(0.5f));

            // �terst�ll attack-flaggan efter cooldown
            StartCoroutine(ResetAttackFlag());
        }

        private IEnumerator ResetAttackAnimation(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (animator != null && !isDead)
            {
                animator.SetBool("isZombieAttacing", false);
            }
        }

        private IEnumerator DealDamageAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            // Kontrollera igen att spelaren finns och �r inom r�ckh�ll
            if (player != null && !isDead)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                if (distanceToPlayer <= attackRange * 1.5f) // Lite st�rre marginal f�r attacken
                {
                    // Kontrollera om spelaren har ett h�lsosystem och orsaka skada
                    PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(attackDamage);
                        Debug.Log($"Zombie orsakade {attackDamage} skada p� spelaren.");
                    }
                }
            }
        }

        private IEnumerator ResetAttackFlag()
        {
            yield return new WaitForSeconds(attackCooldown * 0.8f);
            canAttack = true;
        }

        public void PlayerDied(Vector3 playerPos)
        {
            isPlayerDead = true;
            deadPlayerPosition = playerPos;
            isEating = false;

            // Zombien springer mot den d�da spelaren
            zombieSpeed *= 1.2f; // Snabbare f�r att n� kroppen
        }

        private void IdleOrWander()
        {
            // Om zombien redan vandrar, forts�tt tills den n�r m�let
            if (isWandering)
            {
                if (controller != null && controller.enabled)
                {
                    // Ber�kna avst�nd till m�lpunkten
                    float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

                    // Om zombien har n�tt sin destination eller �r n�ra nog
                    if (distanceToTarget < 1.0f)
                    {
                        isWandering = false;
                        wanderTimer = Random.Range(minWanderWaitTime, maxWanderWaitTime);

                        // Zombien st�r stilla - uppdatera animator
                        if (animator != null)
                        {
                            animator.SetBool("isWalking", false);
                            animator.SetBool("isRunning", false);
                        }
                    }
                    else
                    {
                        // Forts�tt f�rflytta zombien mot m�let
                        Vector3 direction = (targetPosition - transform.position).normalized;
                        direction.y = 0;

                        // Rotera mot m�let
                        transform.rotation = Quaternion.Slerp(transform.rotation,
                                                             Quaternion.LookRotation(direction),
                                                             2f * Time.deltaTime);

                        // Flytta mot m�let
                        Vector3 movement = direction * wanderSpeed * Time.deltaTime;

                        // L�gg till gravitation
                        if (!controller.isGrounded)
                        {
                            movement.y = Physics.gravity.y * Time.deltaTime;
                        }

                        controller.Move(movement);

                        // Animera vandring
                        if (animator != null)
                        {
                            animator.SetBool("isWalking", true);
                            animator.SetBool("isRunning", false);
                        }
                    }
                }
            }
            else
            {
                // Minska v�ntetimern
                wanderTimer -= Time.deltaTime;

                // N�r timern n�r noll, v�lj en ny plats att vandra till
                if (wanderTimer <= 0)
                {
                    // 70% chans att vandra, 30% chans att bara st� stilla
                    if (Random.value < 0.7f)
                    {
                        targetPosition = GetRandomWanderPoint();
                        isWandering = true;

                        // B�rja g�-animation
                        if (animator != null)
                        {
                            animator.SetBool("isWalking", true);
                            animator.SetBool("isRunning", false);
                            animator.SetBool("isBiting", false);
                            animator.SetBool("isZombieAttacing", false);
                            animator.SetBool("isScreaming", false);
                        }
                    }
                    else
                    {
                        // Bara st� stilla ett tag till
                        wanderTimer = Random.Range(minWanderWaitTime, maxWanderWaitTime);

                        // F�rs�kra att zombien st�r stilla
                        if (animator != null)
                        {
                            animator.SetBool("isWalking", false);
                            animator.SetBool("isRunning", false);
                            animator.SetBool("isBiting", false);
                            animator.SetBool("isZombieAttacing", false);
                            animator.SetBool("isScreaming", false);
                        }
                    }
                }
                else
                {
                    // Zombien st�r stilla
                    if (animator != null)
                    {
                        animator.SetBool("isWalking", false);
                        animator.SetBool("isRunning", false);
                        animator.SetBool("isBiting", false);
                        animator.SetBool("isZombieAttacing", false);
                        animator.SetBool("isScreaming", false);
                    }
                }
            }
        }

        private Vector3 GetRandomWanderPoint()
        {
            // Ber�kna en slumpm�ssig destination inom vandringsradien fr�n startpunkten
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection.y = 0;
            Vector3 randomPosition = startPosition + randomDirection;

            // F�rs�kra att destinationen �r p� NavMesh (om NavMesh anv�nds)
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(randomPosition, out navHit, wanderRadius, NavMesh.AllAreas))
            {
                return navHit.position;
            }

            // Om ingen NavMesh-position hittades, anv�nd bara den slumpm�ssiga positionen
            return randomPosition;
        }

        private void UpdateAnimations()
        {
            // Denna metod �r nu mer �verfl�dig eftersom vi s�tter
            // animatorns booleans direkt i respektive metoder,
            // men beh�ller den f�r att s�kerst�lla att animationsl�get �r konsekvent

            if (animator == null || isDead) return;

            // S�kerst�ll att animatorns tillst�nd matchar zombiens beteende
            bool isAttacking = Vector3.Distance(transform.position, player?.position ?? Vector3.positiveInfinity) <= attackRange && canAttack;

            // Avst� fr�n att �ndra tillst�nd om zombien attackerar, skriker eller biter
            if (!animator.GetBool("isZombieAttacing") && !animator.GetBool("isBiting") && !animator.GetBool("isScreaming"))
            {
                if (isChasing)
                {
                    bool isRunningAfterPlayer = Vector3.Distance(transform.position, player.position) > playerDistanceLastFrame &&
                                              Vector3.Distance(transform.position, player.position) > attackRange * 1.5f;

                    animator.SetBool("isRunning", isRunningAfterPlayer);
                    animator.SetBool("isWalking", !isRunningAfterPlayer);
                }
                else if (isWandering)
                {
                    animator.SetBool("isWalking", true);
                    animator.SetBool("isRunning", false);
                }
                else
                {
                    animator.SetBool("isWalking", false);
                    animator.SetBool("isRunning", false);
                }
            }
        }

        // Metod f�r att hantera n�r zombien tar skada
        public void TakeDamage(float damageAmount)
        {
            if (isDead) return;

            currentHealth -= damageAmount;
            Debug.Log($"Zombie tog {damageAmount} skada. �terst�ende h�lsa: {currentHealth}");

            // Aktivera en skadad-animation om det finns
            if (animator != null)
            {
                animator.SetTrigger("TakeDamage");
            }

            // N�r zombien tar skada blir den medveten om spelaren
            if (player != null && !isChasing)
            {
                isChasing = true;
                chaseTimer = memoryDuration;
                lastKnownPlayerPosition = player.position;
            }

            // Kontrollera om zombien har d�tt
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            isDead = true;

            // Inaktivera CharacterController f�r att stoppa r�relse
            if (controller != null)
            {
                controller.enabled = false;
            }

            // Aktivera d�dsanimation
            if (animator != null)
            {
                animator.SetTrigger("Die");
                animator.SetBool("IsDead", true);

                // �terst�ll eventuella p�g�ende animationer
                animator.SetBool("isBiting", false);
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", false);
                animator.SetBool("isZombieAttacing", false);
                animator.SetBool("isScreaming", false);
            }

            // Slumpm�ssigt sl�pp items vid d�d
            DropItems();

            // Ta bort eventuella colliders
            Collider[] colliders = GetComponents<Collider>();
            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }

            // F�rst�r zombie-objektet efter en f�rdr�jning
            StartCoroutine(DestroyAfterDelay());
        }

        private void DropItems()
        {
            if (dropItems == null || dropItems.Length == 0) return;

            // Slumpa om zombien ska sl�ppa n�got
            if (Random.value <= dropChance)
            {
                // Slumpa vilket item som ska sl�ppas
                int randomIndex = Random.Range(0, dropItems.Length);
                GameObject itemToSpawn = dropItems[randomIndex];

                if (itemToSpawn != null)
                {
                    // Skapa item med en liten offset fr�n zombiens position
                    Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0.1f, Random.Range(-0.5f, 0.5f));
                    Instantiate(itemToSpawn, spawnPos, Quaternion.identity);
                    Debug.Log($"Zombien sl�ppte: {itemToSpawn.name}");
                }
            }
        }

        private IEnumerator DestroyAfterDelay()
        {
            // V�nta p� att d�dsanimationen ska spelas klart
            yield return new WaitForSeconds(deathAnimationTime);

            // F�rst�r zombie-objektet
            Destroy(gameObject);
        }

        // Visualisera detektions- och attackomr�det i Unity-editorn
        void OnDrawGizmosSelected()
        {
            // Rita detektionsomr�det
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Rita ut�kat jaktomr�de
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Orange, halvtransparent
            Gizmos.DrawWireSphere(transform.position, chaseRange);

            // Rita attackomr�det
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Rita vandringsradien
            Gizmos.color = Color.blue;
            if (Application.isPlaying)
            {
                Gizmos.DrawWireSphere(startPosition, wanderRadius);
            }
            else
            {
                Gizmos.DrawWireSphere(transform.position, wanderRadius);
            }

            // Rita skrikomr�det
            Gizmos.color = new Color(1f, 0f, 1f, 0.3f); // Lila, halvtransparent
            Gizmos.DrawWireSphere(transform.position, screamRange);

            // Om vi vandrar, visa m�lpunkten
            if (Application.isPlaying && isWandering)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(targetPosition, 0.3f);
                Gizmos.DrawLine(transform.position, targetPosition);
            }

            // Om zombien minns senaste spelarposition
            if (Application.isPlaying && isChasing && !playerInSight)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(lastKnownPlayerPosition, 0.5f);
                Gizmos.DrawLine(transform.position, lastKnownPlayerPosition);
            }
        }

        private IEnumerator StartChasingAfterScream()
        {
            // V�nta p� att skrik-animationen ska slutf�ras
            yield return new WaitForSeconds(screamAnimationDuration);

            // Aktivera jaktl�ge
            playerDetected = true;
            isChasing = true;
            chaseTimer = memoryDuration;
        }
    }