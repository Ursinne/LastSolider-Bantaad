using UnityEngine;

public class SimpleAnimalAI : MonoBehaviour
{
    public enum AnimalType { Herbivore, Predator }
    public enum AnimalState { Idle, Walking, Running, Eating, Sleeping, Fleeing, Hunting, Dead }

    [Header("Animal Settings")]
    public AnimalType animalType = AnimalType.Herbivore;
    public float walkSpeed = 1.5f;
    public float runSpeed = 4f;
    public float rotationSpeed = 2f;
    public float stateChangeInterval = 5f;
    public float detectionRadius = 10f;
    public float fleeDistance = 15f;
    public LayerMask obstacleLayer;
    public float groundCheckDistance = 0.5f;

    [Header("Resources")]
    public GameObject meatPrefab;
    public GameObject peltPrefab;
    public GameObject toothPrefab;
    public int meatDropCount = 2;
    public int peltDropCount = 1;
    public int toothDropCount = 2;

    [Header("Animal Stats")]
    public float health = 100f;
    public float hunger = 0f;
    public float hungerRate = 0.1f;

    [Header("References")]
    public Animator animator;

    // Privata variabler
    private AnimalState currentState = AnimalState.Idle;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 moveDirection;
    private float stateTimer;
    private bool isMoving = false;
    private float currentSpeed;
    private CharacterController characterController;
    private Collider animalCollider;

    void Start()
    {
        // Initialisera komponenter
        characterController = GetComponent<CharacterController>();
        animalCollider = GetComponent<Collider>();
        animator = GetComponentInChildren<Animator>();

        // Spara startposition
        startPosition = transform.position;
        targetPosition = startPosition;
        stateTimer = stateChangeInterval;

        // Börja med vandringstillstånd
        ChangeState(AnimalState.Walking);
    }

    void Update()
    {
        // Uppdatera grundläggande statistik
        hunger += hungerRate * Time.deltaTime;
        hunger = Mathf.Clamp(hunger, 0f, 100f);

        // Kontrollera för hot eller byte i närheten
        CheckForTargets();

        // Uppdatera nuvarande tillstånd
        UpdateCurrentState();

        // Hantera rörelse
        HandleMovement();

        // Uppdatera animationer
        UpdateAnimations();
    }

    void CheckForTargets()
    {
        // Om redan flyr eller jagar, minska timern först
        if (currentState == AnimalState.Fleeing || currentState == AnimalState.Hunting)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0)
            {
                ChangeState(AnimalState.Walking);
                return;
            }
        }

        // Sök efter spelare och reagera baserat på djurtyp
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            if (distanceToPlayer < detectionRadius)
            {
                if (animalType == AnimalType.Herbivore || health < 30f)
                {
                    // Växtätare eller skadade rovdjur flyr från spelaren
                    FleeFrom(player.transform.position);
                }
                else if (animalType == AnimalType.Predator && hunger > 50f)
                {
                    // Hungriga rovdjur attackerar spelaren
                    HuntTarget(player.transform.position);
                }
            }
        }

        // Rovdjur kan också söka efter bytesdjur
        if (animalType == AnimalType.Predator && hunger > 70f && currentState != AnimalState.Fleeing)
        {
            // Sök efter bytesdjur genom att hitta andra SimpleAnimalAI med Herbivore-typ
            SimpleAnimalAI[] possiblePrey = FindObjectsOfType<SimpleAnimalAI>();

            foreach (var prey in possiblePrey)
            {
                if (prey != this && prey.animalType == AnimalType.Herbivore && prey.currentState != AnimalState.Dead)
                {
                    float distanceToPrey = Vector3.Distance(transform.position, prey.transform.position);

                    if (distanceToPrey < detectionRadius)
                    {
                        HuntTarget(prey.transform.position);
                        break;
                    }
                }
            }
        }
    }

    void UpdateCurrentState()
    {
        stateTimer -= Time.deltaTime;

        switch (currentState)
        {
            case AnimalState.Idle:
                isMoving = false;
                if (stateTimer <= 0)
                {
                    ChangeState(Random.value < 0.7f ? AnimalState.Walking : AnimalState.Eating);
                }
                break;

            case AnimalState.Walking:
                isMoving = true;
                currentSpeed = walkSpeed;

                if (Vector3.Distance(transform.position, targetPosition) < 1f || stateTimer <= 0)
                {
                    float rand = Random.value;
                    if (rand < 0.4f)
                        ChangeState(AnimalState.Idle);
                    else if (rand < 0.7f)
                        ChangeState(AnimalState.Walking);
                    else if (rand < 0.85f)
                        ChangeState(AnimalState.Eating);
                    else
                        ChangeState(AnimalState.Sleeping);
                }
                break;

            case AnimalState.Running:
                isMoving = true;
                currentSpeed = runSpeed;

                if (Vector3.Distance(transform.position, targetPosition) < 1f || stateTimer <= 0)
                {
                    ChangeState(AnimalState.Walking);
                }
                break;

            case AnimalState.Eating:
                isMoving = false;
                hunger = Mathf.Max(0, hunger - 0.2f * Time.deltaTime);

                if (stateTimer <= 0 || hunger <= 10f)
                {
                    ChangeState(AnimalState.Walking);
                }
                break;

            case AnimalState.Sleeping:
                isMoving = false;
                health = Mathf.Min(100f, health + 0.1f * Time.deltaTime);

                if (stateTimer <= 0)
                {
                    ChangeState(AnimalState.Idle);
                }
                break;

            case AnimalState.Fleeing:
                isMoving = true;
                currentSpeed = runSpeed * 1.2f;

                if (Vector3.Distance(transform.position, targetPosition) < 1f || stateTimer <= 0)
                {
                    ChangeState(AnimalState.Walking);
                }
                break;

            case AnimalState.Hunting:
                isMoving = true;
                currentSpeed = runSpeed;

                if (Vector3.Distance(transform.position, targetPosition) < 2f)
                {
                    // Attackera
                    Debug.Log("Djur attackerar spelaren!");
                    hunger = Mathf.Max(0, hunger - 10f);
                    ChangeState(AnimalState.Walking);
                }
                break;

            case AnimalState.Dead:
                isMoving = false;
                break;
        }
    }

    void HandleMovement()
    {
        if (!isMoving || currentState == AnimalState.Dead)
            return;

        // Beräkna riktning till målet
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Håll rörelsen horisontell

        if (direction != Vector3.zero)
        {
            // Rotera mot riktningen
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Endast rör sig om det inte finns hinder
            if (!IsObstacleAhead())
            {
                // Flytta djuret
                Vector3 movement = transform.forward * currentSpeed * Time.deltaTime;

                // Använd CharacterController om den finns
                if (characterController != null)
                {
                    // Applicera gravitation på djuret
                    Vector3 gravity = new Vector3(0, -9.81f, 0);
                    characterController.Move(gravity * Time.deltaTime);

                    // Flytta djuret med CharacterController
                    characterController.Move(movement);
                }
                else
                {
                    // Fallback: använd transform direkt
                    transform.position += movement;

                    // Håll djuret på marken
                    AdjustHeightToGround();
                }
            }
            else
            {
                // Om det finns hinder, välj en ny slumpmässig destination
                targetPosition = GetRandomPosition();
            }
        }
    }

    void AdjustHeightToGround()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance + 0.5f, obstacleLayer))
        {
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
        }
    }

    bool IsObstacleAhead()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out hit, 1f, obstacleLayer))
        {
            return true;
        }
        return false;
    }

    void ChangeState(AnimalState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case AnimalState.Idle:
                stateTimer = Random.Range(3f, 6f);
                break;

            case AnimalState.Walking:
                targetPosition = GetRandomPosition();
                stateTimer = Random.Range(5f, 10f);
                break;

            case AnimalState.Running:
                targetPosition = GetRandomPosition();
                stateTimer = Random.Range(3f, 5f);
                break;

            case AnimalState.Eating:
                stateTimer = Random.Range(5f, 8f);
                break;

            case AnimalState.Sleeping:
                stateTimer = Random.Range(8f, 15f);
                break;

            case AnimalState.Fleeing:
                stateTimer = Random.Range(5f, 8f);
                break;

            case AnimalState.Hunting:
                stateTimer = Random.Range(8f, 12f);
                break;
        }
    }

    Vector3 GetRandomPosition()
    {
        // Definiera banans gränser (anpassa efter dina behov)
        float minX = -50f;
        float maxX = 50f;
        float minZ = -50f;
        float maxZ = 50f;

        // Välj en slumpmässig position inom banans gränser
        Vector3 randomDirection = Random.insideUnitSphere;
        randomDirection.y = 0f;
        randomDirection.Normalize();

        float randomDistance = Random.Range(10f, 20f);
        Vector3 randomPosition = transform.position + randomDirection * randomDistance;

        // Begränsa positionen inom banans gränser
        randomPosition.x = Mathf.Clamp(randomPosition.x, minX, maxX);
        randomPosition.z = Mathf.Clamp(randomPosition.z, minZ, maxZ);
        randomPosition.y = transform.position.y;

        return randomPosition;
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        // Sätt animator parametrar baserat på tillstånd
        animator.SetBool("isWalking", isMoving && currentState != AnimalState.Running);
        animator.SetBool("isRunning", currentState == AnimalState.Running);
        animator.SetBool("isEating", currentState == AnimalState.Eating);
        animator.SetBool("isSleeping", currentState == AnimalState.Sleeping);
        animator.SetBool("isDead", currentState == AnimalState.Dead);
    }

    public void TakeDamage(float amount)
    {
        health -= amount;

        if (health <= 0)
        {
            Die();
        }
        else
        {
            // 75% chans att fly när skadad
            if (Random.value < 0.75f)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    FleeFrom(player.transform.position);
                }
            }
        }
    }

    public void FleeFrom(Vector3 dangerPosition)
    {
        // Riktning bort från faran
        Vector3 fleeDirection = (transform.position - dangerPosition).normalized;
        targetPosition = transform.position + fleeDirection * fleeDistance;
        targetPosition.y = transform.position.y; // Behåll samma höjd

        currentState = AnimalState.Fleeing;
        stateTimer = Random.Range(4f, 7f);
    }

    public void HuntTarget(Vector3 targetPos)
    {
        targetPosition = targetPos;
        currentState = AnimalState.Hunting;
        stateTimer = Random.Range(10f, 15f);
    }

    void Die()
    {
        currentState = AnimalState.Dead;

        if (animator != null)
        {
            animator.SetTrigger("die");
        }

        // Droppa resurser
        DropResources();

        // Avaktivera collider om det finns någon
        if (animalCollider != null)
            animalCollider.enabled = false;
        if (characterController != null)
            characterController.enabled = false;

        // Lämna kadavret i 30 sekunder och ta sedan bort det
        Destroy(gameObject, 30f);
    }

    void DropResources()
    {
        Vector3 dropPosition = transform.position;

        if (animalType == AnimalType.Predator)
        {
            // Droppa rovdjursresurser (varg/björn)
            DropItem(meatPrefab, dropPosition, meatDropCount);
            DropItem(peltPrefab, dropPosition, peltDropCount);
            DropItem(toothPrefab, dropPosition, toothDropCount);
        }
        else
        {
            // Droppa växtätarresurser (hjort)
            DropItem(meatPrefab, dropPosition, meatDropCount);
            DropItem(peltPrefab, dropPosition, peltDropCount);
        }
    }

    void DropItem(GameObject itemPrefab, Vector3 position, int count)
    {
        if (itemPrefab == null) return;

        for (int i = 0; i < count; i++)
        {
            // Slumpmässig position inom en liten radie
            Vector3 randomOffset = Random.insideUnitSphere * 0.5f;
            randomOffset.y = 0.1f; // Lite ovanför marken

            Instantiate(itemPrefab, position + randomOffset, Quaternion.identity);
        }
    }
}

    // För att rita hjälplinjer i editorn för debugging

//using UnityEngine;

//public class SimpleAnimalAI : MonoBehaviour
//{
//    public enum AnimalType { Herbivore, Predator }
//    public enum AnimalState { Idle, Walking, Running, Eating, Sleeping, Fleeing, Hunting, Dead }

//    [Header("Animal Settings")]
//    public AnimalType animalType = AnimalType.Herbivore;
//    public float walkSpeed = 1.5f;
//    public float runSpeed = 4f;
//    public float rotationSpeed = 2f;
//    public float stateChangeInterval = 5f;
//    public float detectionRadius = 10f;
//    public float attackRange = 2f;
//    public float fleeDistance = 15f;
//    public LayerMask obstacleLayer;
//    public float groundCheckDistance = 0.5f;

//    [Header("Resources")]
//    public GameObject meatPrefab;
//    public GameObject peltPrefab;
//    public GameObject toothPrefab;
//    public int meatDropCount = 2;
//    public int peltDropCount = 1;
//    public int toothDropCount = 2;

//    [Header("Animal Stats")]
//    public float health = 100f;
//    public float hunger = 0f;
//    public float hungerRate = 0.1f;

//    [Header("References")]
//    public Animator animator;

//    // Privata variabler
//    public AnimalState currentState = AnimalState.Idle;
//    private Vector3 startPosition;
//    private Vector3 targetPosition;
//    private Vector3 moveDirection;
//    private float stateTimer;
//    public bool isMoving = false;
//    public bool isRunning = false;
//    private float currentSpeed;
//    private CharacterController characterController;
//    private Collider animalCollider;

//    void Start()
//    {
//        // Initialisera komponenter
//        characterController = GetComponent<CharacterController>();
//        animalCollider = GetComponent<Collider>();
//        animator = GetComponentInChildren<Animator>();

//        // Spara startposition
//        startPosition = transform.position;
//        targetPosition = startPosition;
//        stateTimer = stateChangeInterval;

//        // Börja med vandringstillstånd
//        ChangeState(AnimalState.Walking);

//        characterController = GetComponent<CharacterController>();

//        if (characterController == null)
//        {
//            Debug.LogWarning("CharacterController not found on " + gameObject.name);
//        }
//    }

//    void Update()
//    {
//        // Uppdatera grundläggande statistik
//        hunger += hungerRate * Time.deltaTime;
//        hunger = Mathf.Clamp(hunger, 0f, 100f);

//        // Kontrollera för hot eller byte i närheten
//        CheckForTargets();

//        // Uppdatera nuvarande tillstånd
//        UpdateCurrentState();

//        // Hantera rörelse
//        HandleMovement();

//        // Uppdatera animationer
//        UpdateAnimations();
//    }

//    void CheckForTargets()
//    {
//        // Om redan flyr eller jagar, minska timern först
//        if (currentState == AnimalState.Fleeing || currentState == AnimalState.Hunting)
//        {
//            stateTimer -= Time.deltaTime;
//            if (stateTimer <= 0)
//            {
//                ChangeState(AnimalState.Walking);
//                return;
//            }
//        }

//        // Sök efter spelare och reagera baserat på djurtyp
//        GameObject player = GameObject.FindGameObjectWithTag("Player");
//        if (player != null)
//        {
//            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

//            if (distanceToPlayer < detectionRadius)
//            {
//                if (animalType == AnimalType.Herbivore || health < 30f)
//                {
//                    // Växtätare eller skadade rovdjur flyr från spelaren
//                    FleeFrom(player.transform.position);
//                }
//                else if (animalType == AnimalType.Predator && hunger > 50f)
//                {
//                    // Hungriga rovdjur attackerar spelaren
//                    HuntTarget(player.transform.position);
//                }
//            }
//        }

//        // Rovdjur kan också söka efter bytesdjur
//        if (animalType == AnimalType.Predator && hunger > 70f && currentState != AnimalState.Fleeing)
//        {
//            // Sök efter bytesdjur genom att hitta andra SimpleAnimalAI med Herbivore-typ
//            SimpleAnimalAI[] possiblePrey = FindObjectsOfType<SimpleAnimalAI>();

//            foreach (var prey in possiblePrey)
//            {
//                if (prey != this && prey.animalType == AnimalType.Herbivore && prey.currentState != AnimalState.Dead)
//                {
//                    float distanceToPrey = Vector3.Distance(transform.position, prey.transform.position);

//                    if (distanceToPrey < detectionRadius)
//                    {
//                        HuntTarget(prey.transform.position);
//                        break;
//                    }
//                }
//            }
//        }
//    }

//    void UpdateCurrentState()
//    {
//        stateTimer -= Time.deltaTime;

//        GameObject player = GameObject.FindGameObjectWithTag("Player");
//        if (player != null)
//        {
//            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

//            if (distanceToPlayer < detectionRadius)
//            {
//                // Gå mot spelaren
//                if (distanceToPlayer > attackRange)
//                {
//                    isRunning = false;
//                    isMoving = true;
//                    currentSpeed = walkSpeed;
//                }
//                // Sprint/attack när nära
//                else
//                {
//                    isRunning = true;
//                    isMoving = true;
//                    currentSpeed = runSpeed;
//                }
//            }
//            else
//            {
//                // Återgå till normal vandring
//                switch (currentState)
//                {
//                    case AnimalState.Idle:
//                        isMoving = false;
//                        isRunning = false;
//                        if (stateTimer <= 0)
//                        {
//                            ChangeState(Random.value < 0.7f ? AnimalState.Walking : AnimalState.Eating);
//                        }
//                        break;

//                    case AnimalState.Walking:
//                        isMoving = true;
//                        isRunning = false;
//                        currentSpeed = walkSpeed;

//                        if (Vector3.Distance(transform.position, targetPosition) < 1f || stateTimer <= 0)
//                        {
//                            float rand = Random.value;
//                            if (rand < 0.4f)
//                                ChangeState(AnimalState.Idle);
//                            else if (rand < 0.7f)
//                                ChangeState(AnimalState.Walking);
//                            else if (rand < 0.85f)
//                                ChangeState(AnimalState.Eating);
//                            else
//                                ChangeState(AnimalState.Sleeping);
//                        }
//                        break;

//                    case AnimalState.Running:
//                        isMoving = true;
//                        isRunning = true;
//                        currentSpeed = runSpeed;

//                        if (Vector3.Distance(transform.position, targetPosition) < 1f || stateTimer <= 0)
//                        {
//                            ChangeState(AnimalState.Walking);
//                        }
//                        break;
//                }
//            }
//        }
//    }

//    void HandleMovement()
//    {
//        if (!isMoving || currentState == AnimalState.Dead)
//            return;

//        // Beräkna riktning till målet
//        Vector3 direction = (targetPosition - transform.position).normalized;
//        direction.y = 0; // Håll rörelsen horisontell

//        if (direction != Vector3.zero)
//        {
//            // Rotera mot riktningen
//            Quaternion targetRotation = Quaternion.LookRotation(direction);
//            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

//            // Endast rör sig om det inte finns hinder
//            if (!IsObstacleAhead())
//            {
//                // Flytta djuret
//                Vector3 movement = transform.forward * currentSpeed * Time.deltaTime;

//                // Använd CharacterController om den finns och är påslagen
//                if (characterController != null && characterController.enabled)
//                {
//                    // Lägg till gravitationseffekt
//                    movement.y = -9.81f * Time.deltaTime;
//                    characterController.Move(movement);
//                }
//                else
//                {
//                    // Fallback när CharacterController inte är tillgänglig: använd transform direkt
//                    transform.position += movement;

//                    // Håll djuret på marken
//                    AdjustHeightToGround();
//                }
//            }
//            else
//            {
//                // Om det finns hinder, välj en ny slumpmässig destination
//                targetPosition = GetRandomPosition();
//            }
//        }
//        if (characterController != null && characterController.enabled)
//        {
//            Vector3 gravity = new Vector3(0, -9.81f, 0);
//            characterController.Move(gravity * Time.deltaTime);
//        }

//    //void HandleMovement()
//    //{
//    //    if (!isMoving || currentState == AnimalState.Dead)
//    //        return;

//    //    // Beräkna riktning till målet
//    //    Vector3 direction = (targetPosition - transform.position).normalized;
//    //    direction.y = 0; // Håll rörelsen horisontell

//    //    if (direction != Vector3.zero)
//    //    {
//    //        // Rotera mot riktningen
//    //        Quaternion targetRotation = Quaternion.LookRotation(direction);
//    //        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

//    //        // Endast rör sig om det inte finns hinder
//    //        if (!IsObstacleAhead())
//    //        {
//    //            // Flytta djuret
//    //            Vector3 movement = transform.forward * currentSpeed * Time.deltaTime;

//    //            // Använd CharacterController om den finns, annars transform
//    //            if (characterController != null)
//    //            {
//    //                // Lägg till gravitationseffekt
//    //                movement.y = -9.81f * Time.deltaTime;
//    //                characterController.Move(movement);
//    //            }
//    //            else
//    //            {
//    //                transform.position += movement;

//    //                // Håll djuret på marken
//    //                AdjustHeightToGround();
//    //            }
//    //        }
//    //        else
//    //        {
//    //            // Om det finns hinder, välj en ny slumpmässig destination
//    //            targetPosition = GetRandomPosition();
//    //        }
//    //    }
//    //}

//    void UpdateAnimations()
//    {
//        if (animator == null) return;

//        // Uppdatera animationer baserat på rörelse och tillstånd
//        animator.SetBool("isWalking", isMoving && !isRunning);
//        animator.SetBool("isRunning", isRunning);
//        animator.SetBool("isEating", currentState == AnimalState.Eating);
//        animator.SetBool("isSleeping", currentState == AnimalState.Sleeping);
//        animator.SetBool("isDead", currentState == AnimalState.Dead);
//    }

//    void ChangeState(AnimalState newState)
//    {
//        currentState = newState;

//        switch (newState)
//        {
//            case AnimalState.Idle:
//                stateTimer = Random.Range(3f, 6f);
//                break;

//            case AnimalState.Walking:
//                targetPosition = GetRandomPosition();
//                stateTimer = Random.Range(5f, 10f);
//                break;

//            case AnimalState.Running:
//                targetPosition = GetRandomPosition();
//                stateTimer = Random.Range(3f, 5f);
//                break;

//            case AnimalState.Eating:
//                stateTimer = Random.Range(5f, 8f);
//                break;

//            case AnimalState.Sleeping:
//                stateTimer = Random.Range(8f, 15f);
//                break;

//            case AnimalState.Fleeing:
//                stateTimer = Random.Range(5f, 8f);
//                break;

//            case AnimalState.Hunting:
//                stateTimer = Random.Range(8f, 12f);
//                break;
//        }
//    }

//    Vector3 GetRandomPosition()
//    {
//        // Definiera banans gränser (anpassa efter dina behov)
//        float minX = -50f;
//        float maxX = 50f;
//        float minZ = -50f;
//        float maxZ = 50f;

//        // Välj en slumpmässig position inom banans gränser
//        float randomX = Random.Range(minX, maxX);
//        float randomZ = Random.Range(minZ, maxZ);

//        // Sätt Y-positionen till djurets nuvarande höjd
//        float currentY = transform.position.y;

//        return new Vector3(randomX, currentY, randomZ);
//    }

//    //Vector3 GetRandomPosition()
//    //{
//    //    float radius = Random.Range(5f, 15f);
//    //    Vector3 randomDirection = Random.insideUnitSphere * radius;
//    //    randomDirection.y = 0;

//    //    Vector3 newPos = startPosition + randomDirection;

//    //    // Raycast för att säkerställa att positionen är på marken
//    //    RaycastHit hit;
//    //    if (Physics.Raycast(newPos + Vector3.up * 10f, Vector3.down, out hit, 20f, obstacleLayer))
//    //    {
//    //        return hit.point;
//    //    }

//    //    return newPos;
//    //}

//    void AdjustHeightToGround()
//    {
//        RaycastHit hit;
//        Vector3 rayStart = transform.position + Vector3.up * 0.5f;

//        if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance + 0.5f, obstacleLayer))
//        {
//            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
//        }
//    }

//    bool IsObstacleAhead()
//    {
//        RaycastHit hit;
//        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out hit, 1f, obstacleLayer))
//        {
//            return true;
//        }
//        return false;
//    }

//    public void TakeDamage(float amount)
//    {
//        health -= amount;

//        if (health <= 0)
//        {
//            Die();
//        }
//        else
//        {
//            // 75% chans att fly när skadad
//            if (Random.value < 0.75f)
//            {
//                GameObject player = GameObject.FindGameObjectWithTag("Player");
//                if (player != null)
//                {
//                    FleeFrom(player.transform.position);
//                }
//            }
//        }
//    }

//    public void FleeFrom(Vector3 dangerPosition)
//    {
//        // Riktning bort från faran
//        Vector3 fleeDirection = (transform.position - dangerPosition).normalized;
//        targetPosition = transform.position + fleeDirection * fleeDistance;
//        targetPosition.y = transform.position.y; // Behåll samma höjd

//        currentState = AnimalState.Fleeing;
//        stateTimer = Random.Range(4f, 7f);
//    }

//    public void HuntTarget(Vector3 targetPos)
//    {
//        targetPosition = targetPos;
//        currentState = AnimalState.Hunting;
//        stateTimer = Random.Range(10f, 15f);
//    }

//    void Die()
//    {
//        currentState = AnimalState.Dead;
//        isMoving = false;
//        isRunning = false;

//        // Droppa resurser
//        DropResources();

//        // Avaktivera collider om det finns någon
//        if (animalCollider != null)
//            animalCollider.enabled = false;
//        if (characterController != null)
//            characterController.enabled = false;

//        // Lämna kadavret i 30 sekunder och ta sedan bort det
//        Destroy(gameObject, 30f);
//    }

//    void DropResources()
//    {
//        Vector3 dropPosition = transform.position;

//        if (animalType == AnimalType.Predator)
//        {
//            // Droppa rovdjursresurser (varg/björn)
//            DropItem(meatPrefab, dropPosition, meatDropCount);
//            DropItem(peltPrefab, dropPosition, peltDropCount);
//            DropItem(toothPrefab, dropPosition, toothDropCount);
//        }
//        else
//        {
//            // Droppa växtätarresurser (hjort)
//            DropItem(meatPrefab, dropPosition, meatDropCount);
//            DropItem(peltPrefab, dropPosition, peltDropCount);
//        }
//    }

//    void DropItem(GameObject itemPrefab, Vector3 position, int count)
//    {
//        if (itemPrefab == null) return;

//        for (int i = 0; i < count; i++)
//        {
//            // Slumpmässig position inom en liten radie
//            Vector3 randomOffset = Random.insideUnitSphere * 0.5f;
//            randomOffset.y = 0.1f; // Lite ovanför marken

//            Instantiate(itemPrefab, position + randomOffset, Quaternion.identity);
//        }
//    }
//}