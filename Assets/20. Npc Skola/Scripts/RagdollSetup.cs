using UnityEngine;
using UnityEngine.Animations.Rigging; // Lägg till för att hantera Animation Rigging komponenter
using System.Collections;

public class RagdollSetup : MonoBehaviour
{
    public enum CharacterType
    {
        Player,
        Enemy
    }

    public enum RagdollState
    {
        Deactivated,
        Active,
        Positioning
    }

    public CharacterType characterType;
    private Rigidbody[] rigidbodies;
    private Collider[] colliders;
    private Animator animator;
    private CharacterController characterController;
    private RagdollState currentState = RagdollState.Deactivated;

    // Animation Rigging-relaterade komponenter
    private RigBuilder rigBuilder;
    private bool rigBuilderWasEnabled;

    // Spara originalpositioner för att kunna återställa vid behov
    private Vector3[] originalPositions;
    private Quaternion[] originalRotations;

    void Start()
    {
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        // Hitta RigBuilder-komponenten
        rigBuilder = GetComponentInChildren<RigBuilder>();
        rigBuilderWasEnabled = rigBuilder != null && rigBuilder.enabled;

        // Spara originalpositioner och rotationer
        SaveOriginalTransforms();

        DeactivateRagdoll();
        SetupHitboxes();
    }

    void Update()
    {
        // Exempel på knappstyrning för att byta läge
        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleRagdollState();
        }
    }

    // Spara alla kroppsdelars ursprungliga positioner
    private void SaveOriginalTransforms()
    {
        originalPositions = new Vector3[rigidbodies.Length];
        originalRotations = new Quaternion[rigidbodies.Length];

        for (int i = 0; i < rigidbodies.Length; i++)
        {
            originalPositions[i] = rigidbodies[i].transform.position;
            originalRotations[i] = rigidbodies[i].transform.rotation;
        }
    }

    // Växla mellan olika ragdoll-tillstånd
    public void ToggleRagdollState()
    {
        switch (currentState)
        {
            case RagdollState.Deactivated:
                ActivateRagdoll();
                break;
            case RagdollState.Active:
                EnablePositioningMode();
                break;
            case RagdollState.Positioning:
                DeactivateRagdoll();
                break;
        }
    }

    // Inaktivera RigBuilder när vi aktiverar ragdoll/positionering
    private void DisableRigBuilder()
    {
        if (rigBuilder != null)
        {
            rigBuilderWasEnabled = rigBuilder.enabled;
            rigBuilder.enabled = false;
        }
    }

    // Återaktivera RigBuilder när vi stänger av ragdoll/positionering
    private IEnumerator EnableRigBuilder()
    {
        yield return new WaitForEndOfFrame();

        if (rigBuilder != null && rigBuilderWasEnabled)
        {
            rigBuilder.enabled = true;

            try
            {
                rigBuilder.Clear();
                rigBuilder.Build();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Fel vid återaktivering av RigBuilder: " + e.Message);
            }
        }
    }

    // Aktivera positioneringsläge
    public void EnablePositioningMode()
    {
        currentState = RagdollState.Positioning;

        // Inaktivera RigBuilder först
        DisableRigBuilder();

        // Inaktivera animator och karaktärskontroller
        if (animator != null)
            animator.enabled = false;

        if (characterController != null)
            characterController.enabled = false;

        // Sätt alla rigidbodies till kinematisk för manuell positionering
        foreach (var rigidBody in rigidbodies)
        {
            if (rigidBody.gameObject != gameObject)
            {
                rigidBody.isKinematic = true;
                rigidBody.detectCollisions = false;
            }
        }

        // Aktivera alla kolliders för att kunna välja dem
        foreach (var col in colliders)
        {
            if (col.gameObject != gameObject)
            {
                col.enabled = true;
            }
        }

        Debug.Log("Positioneringsläge aktiverat - du kan nu placera om kroppsdelarna manuellt");
    }

    public void ActivateRagdoll()
    {
        currentState = RagdollState.Active;

        // Inaktivera RigBuilder först
        DisableRigBuilder();

        // Inaktivera animator och karaktärskontroller
        if (animator != null)
            animator.enabled = false;

        if (characterController != null)
            characterController.enabled = false;

        // Aktivera alla ragdoll-rigidbodies
        foreach (var rigidBody in rigidbodies)
        {
            if (rigidBody.gameObject != gameObject)
            {
                rigidBody.isKinematic = false;
                rigidBody.detectCollisions = true;
            }
        }

        // Aktivera alla kolliders för ragdoll
        foreach (var col in colliders)
        {
            // Undanta huvudobjektets kollider
            if (col.gameObject != gameObject)
            {
                col.enabled = true;
            }
        }
    }

    public void DeactivateRagdoll()
    {
        currentState = RagdollState.Deactivated;

        // Inaktivera alla ragdoll-rigidbodies först
        foreach (var rigidBody in rigidbodies)
        {
            if (rigidBody.gameObject != gameObject)
            {
                rigidBody.isKinematic = true;
                rigidBody.linearVelocity = Vector3.zero;
                rigidBody.angularVelocity = Vector3.zero;
            }
        }

        // Inaktivera alla extra kolliders
        foreach (var col in colliders)
        {
            // Undanta huvudobjektets kollider
            if (col.gameObject != gameObject)
            {
                col.enabled = false;
            }
        }

        // Återaktivera animator och karaktärskontroller
        if (animator != null)
            animator.enabled = true;

        if (characterController != null)
            characterController.enabled = true;

        // Återaktivera RigBuilder med fördröjning
        StartCoroutine(EnableRigBuilder());
    }

    // Metod för att återställa alla kroppsdelar till originalpositioner
    public void ResetToOriginalPositions()
    {
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            if (rigidbodies[i].gameObject != gameObject)
            {
                rigidbodies[i].transform.position = originalPositions[i];
                rigidbodies[i].transform.rotation = originalRotations[i];
            }
        }
    }

    // Metod för att programmatiskt positionera en specifik kroppsdel
    public void PositionBodyPart(string bodyPartName, Vector3 newPosition, Quaternion newRotation)
    {
        foreach (var rigidBody in rigidbodies)
        {
            if (rigidBody.gameObject.name.Contains(bodyPartName))
            {
                rigidBody.transform.position = newPosition;
                rigidBody.transform.rotation = newRotation;
                break;
            }
        }
    }

    void SetupHitboxes()
    {
        foreach (var rigidBody in rigidbodies)
        {
            // Bestäm hitbox-typ baserat på namnet eller position
            if (characterType == CharacterType.Enemy)
            {
                var hitbox = rigidBody.gameObject.AddComponent<HitboxEnemy>();
                hitbox.hitboxType = DetermineEnemyHitboxType(rigidBody.gameObject);

                // Försök länka till EnemyHealth
                var enemyHealth = GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    hitbox.enemyHealth = enemyHealth;
                }
            }
            else if (characterType == CharacterType.Player)
            {
                var hitbox = rigidBody.gameObject.AddComponent<HitboxPlayer>();
                hitbox.hitboxType = DeterminePlayerHitboxType(rigidBody.gameObject);

                // Försök länka till PlayerHealth
                var playerHealth = GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    hitbox.playerHealth = playerHealth;
                }
            }
        }
    }

    // Metoder för att bestämma hitbox-typ (samma som tidigare)
    private HitboxEnemy.HitboxType DetermineEnemyHitboxType(GameObject bodyPart)
    {
        string name = bodyPart.name.ToLower();
        if (name.Contains("head"))
            return HitboxEnemy.HitboxType.Head;
        if (name.Contains("arm") || name.Contains("leg"))
            return HitboxEnemy.HitboxType.Limbs;
        return HitboxEnemy.HitboxType.Body;
    }

    private HitboxPlayer.HitboxType DeterminePlayerHitboxType(GameObject bodyPart)
    {
        string name = bodyPart.name.ToLower();
        if (name.Contains("head"))
            return HitboxPlayer.HitboxType.Head;
        if (name.Contains("arm") || name.Contains("leg"))
            return HitboxPlayer.HitboxType.Limbs;
        return HitboxPlayer.HitboxType.Body;
    }

    // Lägg till för att visa ragdoll-läge i inspektorn (valfritt)
    void OnGUI()
    {
        if (currentState == RagdollState.Positioning)
        {
            GUI.Box(new Rect(10, 10, 200, 25), "Positioneringsläge aktivt");

            if (GUI.Button(new Rect(10, 40, 200, 25), "Spara positioner"))
            {
                SaveOriginalTransforms();
                Debug.Log("Positioner sparade");
            }
        }
    }
}