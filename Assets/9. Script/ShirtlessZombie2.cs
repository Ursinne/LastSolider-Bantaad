    using UnityEngine;
    using UnityEngine.AI;
    using System.Collections;
    using System.Collections.Generic;

    public class NPCShirtlessZombie2 : MonoBehaviour
    {
        [Header("Zombie Settings")]
        public float detectionRange = 10f;      // Räckvidd där zombien upptäcker spelaren
        public float chaseRange = 20f;          // Utökad räckvidd när zombien redan jagar spelaren
        public float attackRange = 1.5f;        // Räckvidd där zombien kan attackera spelaren
        public float zombieSpeed = 2f;          // Zombiens normala rörelsehastighet
        public float chaseSpeed = 3.5f;         // Zombiens rörelsehastighet när den jagar
        public float wanderSpeed = 1f;          // Hastighet när zombien vandrar
        public float attackCooldown = 2f;       // Tid mellan attacker
        public float wanderRadius = 10f;        // Hur långt zombien vandrar från sin startposition
        public float minWanderWaitTime = 3f;    // Minsta tid zombien väntar mellan vandringar
        public float maxWanderWaitTime = 8f;    // Längsta tid zombien väntar mellan vandringar
        public float memoryDuration = 8f;       // Hur länge zombien minns spelaren efter att den försvunnit

        [Header("Health Settings")]
        public float maxHealth = 100f;          // Zombiens maximala hälsa
        public float currentHealth;             // Zombiens nuvarande hälsa
        public float attackDamage = 10f;        // Skada som zombien gör vid attack

        [Header("Death Settings")]
        public float deathAnimationTime = 3f;   // Hur lång tid dödsanimationen spelas innan objekt förstörs
        public GameObject[] dropItems;          // Möjliga items som zombien kan släppa när den dör
        [Range(0, 1)] public float dropChance = 0.3f; // Chans att zombien släpper något item

        [Header("Scream Settings")]
        public float screamRange = 30f;         // Räckvidd för skriket (hur långt andra zombier kan höra)
        public float screamCooldown = 10f;      // Tid mellan skrik
        public bool canScream = true;           // Om zombien kan skrika
        public float screamAnimationDuration = 1.5f; // Längd på skrikanimationen
        public AudioClip screamSound;           // Ljudet för skriket

        [Header("References")]
        private CharacterController controller;  // Referens till CharacterController-komponenten
        private Animator animator;               // Referens till Animator-komponenten
        private Transform player;                // Referens till spelarens transform
        private AudioSource audioSource;         // Referens till AudioSource-komponenten

        // Status-variabler
        private bool playerDetected = false;
        private float lastAttackTime;
        private float lastScreamTime;           // Tid för senaste skriket
        private Vector3 startPosition;          // Zombiens startposition
        private Vector3 targetPosition;         // Destination för zombiens vandring
        private float wanderTimer;              // Timer för vandringsintervall
        private bool isWandering = false;       // Om zombien för närvarande vandrar
        private bool isDead = false;
        private bool canAttack = true;          // Om zombien kan attackera (för att förhindra attackspam)
        private bool hasScreamedForPlayer = false; // Om zombien redan har skrikit för denna spelare

        // Nya variabler för förbättrad jaktlogik
        private bool isChasing = false;         // Om zombien aktivt jagar spelaren
        private float chaseTimer = 0f;          // Timer för hur länge zombien ska fortsätta jaga efter att spelaren försvunnit
        private Vector3 lastKnownPlayerPosition; // Senast kända position för spelaren
        private bool playerInSight = false;     // Om zombien faktiskt kan se spelaren
        private float playerDistanceLastFrame = float.MaxValue; // För att bestämma om spelaren närmar sig eller flyr
        private bool isScreaming = false;       // Om zombien för närvarande skriker

        // Variabler för dödsspelarbeteende
        public bool isPlayerDead = false;
        public float bitingDistance = 1.0f;
        private Vector3 deadPlayerPosition;
        private bool isEating = false;          // Om zombien äter på en död spelare

        void Start()
        {
            // Spara startposition
            startPosition = transform.position;

            // Initialisera hälsa
            currentHealth = maxHealth;

            // Hitta nödvändiga komponenter
            controller = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
        
            // Om det inte finns en AudioSource, lägg till en
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

            // Starta zombien med ett idle-läge
            wanderTimer = Random.Range(minWanderWaitTime, maxWanderWaitTime);
        }

        void Update()
        {
            // Om zombien är död, avbryt uppdateringen
            if (isDead || controller == null) return;

            // Nyckel-tester för debugging
            if (Input.GetKeyDown(KeyCode.R))
            {
                // Manuellt aktivera/inaktivera spring
                if (animator != null)
                {
                    bool currentRunState = animator.GetBool("isRunning");
                    animator.SetBool("isRunning", !currentRunState);
                    animator.SetBool("isWalking", currentRunState);
                    Debug.Log($"Manuellt växlade isRunning till {!currentRunState}");
                }
            }

            // Om zombien skriker, avvakta tills skriket är klart
            if (isScreaming)
            {
                // När zombien skriker, se till att bara skrikanimationen spelas
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

            // Om spelaren är död och zombien vet om det
            if (isPlayerDead)
            {
                GoToDeadPlayer();
                return;
            }

            // Om spelaren inte hittats, försök igen (för fall där spelaren spawnas efter zombien)
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

                    // VIKTIGT: Anropa inte UpdateAnimations() här, låt IdleOrWander sköta det
                    return;
                }
            }

            // Beräkna avstånd till spelaren
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Debug-info om spelaravstånd
            //Debug.Log($"Avstånd till spelare: {distanceToPlayer}, detektionsräckvidd: {detectionRange}");

            // Uppdatera tidigare-frame data
            if (playerInSight)
            {
                playerDistanceLastFrame = distanceToPlayer;
                lastKnownPlayerPosition = player.position;
            }

            // Kontrollera om spelaren är inom detektionsräckvidden
            float effectiveRange = isChasing ? chaseRange : detectionRange;
            bool wasPlayerInSight = playerInSight;
            playerInSight = distanceToPlayer <= effectiveRange;

            // Om vi just upptäckte spelaren, logga detta
            if (playerInSight && !wasPlayerInSight)
            {
                Debug.Log("Zombie upptäckte spelaren!");
            }

            // Logik för att hantera jaktläge
            if (playerInSight)
            {
                // Om vi just upptäckt spelaren och inte skrikit än, skrik
                if (!playerDetected && canScream && !hasScreamedForPlayer)
                {
                    Debug.Log("Zombie börjar skrika!");
                    Scream();
                    hasScreamedForPlayer = true;

                    // Lägg till en fördröjning innan jakt börjar
                    StartCoroutine(StartChasingAfterScream());
                }
                else if (!playerDetected)
                {
                    Debug.Log($"Upptäckte spelare men skriker inte. canScream={canScream}, hasScreamedForPlayer={hasScreamedForPlayer}");
                }

                playerDetected = true;
                isChasing = true;
                chaseTimer = memoryDuration; // Återställ minnestimern

                // Jaga spelaren när den är inom syn
                ChasePlayer();

                // Attackera bara om inom räckhåll
                if (distanceToPlayer <= attackRange && canAttack && Time.time > lastAttackTime + attackCooldown)
                {
                    AttackPlayer();
                }
            }
            else if (isChasing)
            {
                // Spelaren är inte synlig men zombien är fortfarande i jaktläge
                chaseTimer -= Time.deltaTime;
                if (chaseTimer > 0)
                {
                    // Fortsätt jaga spelaren baserat på senast kända position
                    ChaseLastKnownPosition();
                }
                else
                {
                    // Minnet om spelaren har bleknat, återgå till vandringsläge
                    isChasing = false;
                    playerDetected = false;
                    hasScreamedForPlayer = false; // Återställ skrikflaggan när vi glömmer spelaren
                    IdleOrWander();
                }
            }
            else
            {
                // Zombien ser inte spelaren och jagar inte, vandra slumpmässigt
                IdleOrWander();
            }

            // VIKTIGT: Anropa INTE UpdateAnimations() här
            // Vi låter varje metod (ChasePlayer, IdleOrWander, etc.) hantera sina egna animationer
            // UpdateAnimations();
        }

        private void Scream()
        {
            if (isScreaming || Time.time < lastScreamTime + screamCooldown) return;

            isScreaming = true;
            lastScreamTime = Time.time;

            // Spela skrikanimation - använd både trigger och bool för säkerhets skull
            if (animator != null)
            {
                // Använd alla möjliga varianter av namnet för att täcka alla basis
                animator.SetTrigger("isScreaming");
                animator.SetTrigger("Scream");
            
                // Sätt även bool-värdet
                animator.SetBool("isScreaming", true);
            
                // Stäng av andra animationer
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", false);
                animator.SetBool("isZombieAttacing", false);
                animator.SetBool("isBiting", false);
            
                Debug.Log("Zombie aktiverar skrikanimation!");
            }
            else
            {
                Debug.LogError("Animator saknas på zombie!");
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

            // Alarmera alla zombier inom räckhåll
            AlertNearbyZombies();

            // Återställ skrikstatus efter animationen
            StartCoroutine(ResetScream());
        }

        private IEnumerator ResetScream()
        {
            yield return new WaitForSeconds(screamAnimationDuration);
            isScreaming = false;
        
            // Återställ skrikanimatorn
            if (animator != null)
            {
                animator.SetBool("isScreaming", false);
            }
        
            Debug.Log("Zombie slutar skrika och fortsätter jaga");
        }

        private void AlertNearbyZombies()
        {
            // Hitta alla zombier inom räckhåll
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, screamRange);
            foreach (var hitCollider in hitColliders)
            {
                NPCShirtlessZombie2 otherZombie = hitCollider.GetComponent<NPCShirtlessZombie2>();
            
                // Om det är en annan zombie än den här och den inte är död
                if (otherZombie != null && otherZombie != this && !otherZombie.isDead)
                {
                    // Alarmera zombien om spelaren
                    otherZombie.AlertedByScream(lastKnownPlayerPosition);
                }
            }

            Debug.Log("Zombie skrek och alarmerade närliggande zombier!");
        }

        public void AlertedByScream(Vector3 suspiciousPosition)
        {
            // Om zombien redan jagar eller är död, ignorera larmet
            if (isChasing || isDead) return;

            // Sätt zombien i jaktläge mot den misstänkta positionen
            isChasing = true;
            chaseTimer = memoryDuration;
            lastKnownPlayerPosition = suspiciousPosition;
            playerDetected = true;

            Debug.Log("Zombie hörde ett skrik och rör sig mot platsen!");
        }

        private void ChaseLastKnownPosition()
        {
            if (Vector3.Distance(transform.position, lastKnownPlayerPosition) < 1.5f)
            {
                // När zombien når den senast kända positionen men inte hittar spelaren
                // börja vandra slumpmässigt i området
                isChasing = false;
                wanderTimer = 0; // Tvinga omedelbar vandring
                return;
            }

            // Rikta mot den senast kända positionen
            Vector3 direction = (lastKnownPlayerPosition - transform.position).normalized;
            direction.y = 0;

            // Rotera mot målet
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);

            // Flytta mot den senast kända positionen
            if (controller != null && controller.enabled)
            {
                Vector3 movement = direction * zombieSpeed * Time.deltaTime;

                // Lägg till gravitation
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

            // Om vi redan äter, fortsätt med det
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

            // Beräkna avstånd till den döda spelaren
            float distanceToBody = Vector3.Distance(transform.position, deadPlayerPosition);

            if (distanceToBody > bitingDistance)
            {
                // Gå mot kroppen
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

                // Uppdatera animationer för att visa att zombien går
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
                // Bitanimation när zombien når kroppen
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

            // Avbryt vandring när vi jagar
            isWandering = false;

            // Beräkna avstånd till spelaren
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Vänd zombien mot spelaren - alltid!
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0; // Ignorera höjdskillnad för rotation

            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);

            // ÄNDRING: Använd alltid chaseSpeed när vi jagar spelaren
            // Detta gör att zombien alltid springer mot spelaren
            float currentSpeed = chaseSpeed;
            bool isRunningAfterPlayer = true;

            // Debug-utskrift för hastighet
            Debug.Log($"Zombie jagar spelare med hastighet: {currentSpeed}");

            // Sätt animations-booleans för att matcha beteendet
            if (animator != null)
            {
                animator.SetBool("isRunning", isRunningAfterPlayer);
                animator.SetBool("isWalking", false); // Aldrig gå när vi jagar
                animator.SetBool("isScreaming", false);
            
                // Debug för animationsstatus
                Debug.Log($"Animations-state: isRunning={animator.GetBool("isRunning")}, isWalking={animator.GetBool("isWalking")}");
            }
            else
            {
                Debug.LogError("Animator saknas på zombie!");
            }

            // Flytta zombien mot spelaren
            if (controller != null && controller.enabled)
            {
                Vector3 movement = direction * currentSpeed * Time.deltaTime;

                // Lägg till gravitation om det behövs
                if (!controller.isGrounded)
                {
                    movement.y = Physics.gravity.y * Time.deltaTime;
                }

                controller.Move(movement);
            }
            else
            {
                //Debug.LogError("CharacterController saknas eller är inaktiverad på zombie!");
            }
        }

        private void AttackPlayer()
        {
            if (player == null) return;

            // Registrera tid för attacken
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

                // Återställ attack-animationen efter en kort stund
                StartCoroutine(ResetAttackAnimation(1.0f));
            }

            // Gör skadekontroll först efter en kort fördröjning så att det matchar animationen
            StartCoroutine(DealDamageAfterDelay(0.5f));

            // Återställ attack-flaggan efter cooldown
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

            // Kontrollera igen att spelaren finns och är inom räckhåll
            if (player != null && !isDead)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                if (distanceToPlayer <= attackRange * 1.5f) // Lite större marginal för attacken
                {
                    // Kontrollera om spelaren har ett hälsosystem och orsaka skada
                    PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(attackDamage);
                        Debug.Log($"Zombie orsakade {attackDamage} skada på spelaren.");
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

            // Zombien springer mot den döda spelaren
            zombieSpeed *= 1.2f; // Snabbare för att nå kroppen
        }

        private void IdleOrWander()
        {
            // Om zombien redan vandrar, fortsätt tills den når målet
            if (isWandering)
            {
                if (controller != null && controller.enabled)
                {
                    // Beräkna avstånd till målpunkten
                    float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

                    // Om zombien har nått sin destination eller är nära nog
                    if (distanceToTarget < 1.0f)
                    {
                        isWandering = false;
                        wanderTimer = Random.Range(minWanderWaitTime, maxWanderWaitTime);

                        // Zombien står stilla - uppdatera animator
                        if (animator != null)
                        {
                            animator.SetBool("isWalking", false);
                            animator.SetBool("isRunning", false);
                        }
                    }
                    else
                    {
                        // Fortsätt förflytta zombien mot målet
                        Vector3 direction = (targetPosition - transform.position).normalized;
                        direction.y = 0;

                        // Rotera mot målet
                        transform.rotation = Quaternion.Slerp(transform.rotation,
                                                             Quaternion.LookRotation(direction),
                                                             2f * Time.deltaTime);

                        // Flytta mot målet
                        Vector3 movement = direction * wanderSpeed * Time.deltaTime;

                        // Lägg till gravitation
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
                // Minska väntetimern
                wanderTimer -= Time.deltaTime;

                // När timern når noll, välj en ny plats att vandra till
                if (wanderTimer <= 0)
                {
                    // 70% chans att vandra, 30% chans att bara stå stilla
                    if (Random.value < 0.7f)
                    {
                        targetPosition = GetRandomWanderPoint();
                        isWandering = true;

                        // Börja gå-animation
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
                        // Bara stå stilla ett tag till
                        wanderTimer = Random.Range(minWanderWaitTime, maxWanderWaitTime);

                        // Försäkra att zombien står stilla
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
                    // Zombien står stilla
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
            // Beräkna en slumpmässig destination inom vandringsradien från startpunkten
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection.y = 0;
            Vector3 randomPosition = startPosition + randomDirection;

            // Försäkra att destinationen är på NavMesh (om NavMesh används)
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(randomPosition, out navHit, wanderRadius, NavMesh.AllAreas))
            {
                return navHit.position;
            }

            // Om ingen NavMesh-position hittades, använd bara den slumpmässiga positionen
            return randomPosition;
        }

        private void UpdateAnimations()
        {
            // Denna metod är nu mer överflödig eftersom vi sätter
            // animatorns booleans direkt i respektive metoder,
            // men behåller den för att säkerställa att animationsläget är konsekvent

            if (animator == null || isDead) return;

            // Säkerställ att animatorns tillstånd matchar zombiens beteende
            bool isAttacking = Vector3.Distance(transform.position, player?.position ?? Vector3.positiveInfinity) <= attackRange && canAttack;

            // Avstå från att ändra tillstånd om zombien attackerar, skriker eller biter
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

        // Metod för att hantera när zombien tar skada
        public void TakeDamage(float damageAmount)
        {
            if (isDead) return;

            currentHealth -= damageAmount;
            Debug.Log($"Zombie tog {damageAmount} skada. Återstående hälsa: {currentHealth}");

            // Aktivera en skadad-animation om det finns
            if (animator != null)
            {
                animator.SetTrigger("TakeDamage");
            }

            // När zombien tar skada blir den medveten om spelaren
            if (player != null && !isChasing)
            {
                isChasing = true;
                chaseTimer = memoryDuration;
                lastKnownPlayerPosition = player.position;
            }

            // Kontrollera om zombien har dött
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            isDead = true;

            // Inaktivera CharacterController för att stoppa rörelse
            if (controller != null)
            {
                controller.enabled = false;
            }

            // Aktivera dödsanimation
            if (animator != null)
            {
                animator.SetTrigger("Die");
                animator.SetBool("IsDead", true);

                // Återställ eventuella pågående animationer
                animator.SetBool("isBiting", false);
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", false);
                animator.SetBool("isZombieAttacing", false);
                animator.SetBool("isScreaming", false);
            }

            // Slumpmässigt släpp items vid död
            DropItems();

            // Ta bort eventuella colliders
            Collider[] colliders = GetComponents<Collider>();
            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }

            // Förstör zombie-objektet efter en fördröjning
            StartCoroutine(DestroyAfterDelay());
        }

        private void DropItems()
        {
            if (dropItems == null || dropItems.Length == 0) return;

            // Slumpa om zombien ska släppa något
            if (Random.value <= dropChance)
            {
                // Slumpa vilket item som ska släppas
                int randomIndex = Random.Range(0, dropItems.Length);
                GameObject itemToSpawn = dropItems[randomIndex];

                if (itemToSpawn != null)
                {
                    // Skapa item med en liten offset från zombiens position
                    Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0.1f, Random.Range(-0.5f, 0.5f));
                    Instantiate(itemToSpawn, spawnPos, Quaternion.identity);
                    Debug.Log($"Zombien släppte: {itemToSpawn.name}");
                }
            }
        }

        private IEnumerator DestroyAfterDelay()
        {
            // Vänta på att dödsanimationen ska spelas klart
            yield return new WaitForSeconds(deathAnimationTime);

            // Förstör zombie-objektet
            Destroy(gameObject);
        }

        // Visualisera detektions- och attackområdet i Unity-editorn
        void OnDrawGizmosSelected()
        {
            // Rita detektionsområdet
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Rita utökat jaktområde
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Orange, halvtransparent
            Gizmos.DrawWireSphere(transform.position, chaseRange);

            // Rita attackområdet
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

            // Rita skrikområdet
            Gizmos.color = new Color(1f, 0f, 1f, 0.3f); // Lila, halvtransparent
            Gizmos.DrawWireSphere(transform.position, screamRange);

            // Om vi vandrar, visa målpunkten
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
            // Vänta på att skrik-animationen ska slutföras
            yield return new WaitForSeconds(screamAnimationDuration);

            // Aktivera jaktläge
            playerDetected = true;
            isChasing = true;
            chaseTimer = memoryDuration;
        }
    }