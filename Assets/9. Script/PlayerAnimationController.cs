using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private CharacterController characterController;

    // Generisk lista för att hålla reda på pågående animationer
    private Dictionary<string, bool> activeAnimations = new Dictionary<string, bool>();

    // Spara originalvärden för CharacterController
    private bool originalControllerEnabled;

    // För att minnas rörelseinformation
    private bool shouldDisableController = false;

    // Debug-inställningar
    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = false;

    [Header("Animation Triggers")]
    [SerializeField]
    private List<string> animationTriggers = new List<string>() {
        "isEating", "isDrinking", "isCutting", "isMining", "isDigging",
        "isFishing", "isHammering", "isHoeing", "UseItem"
    };

    // Blockerande animationer som stoppar rörelse
    [SerializeField]
    private List<string> blockingAnimations = new List<string>() {
        "isEating", "isDrinking", "isMining", "isDigging"
    };

    // Callback för att meddela andra komponenter när animation slutförs
    public delegate void AnimationCompletedHandler(string animationName);
    public event AnimationCompletedHandler OnAnimationCompleted;

    void Start()
    {
        InitializeComponents();
        RegisterAnimationParameters();

        RegisterAnimationParameter("Pickup", false);
        RegisterAnimationParameter("isPickingUp", false);
    }

    private void InitializeComponents()
    {
        // Om animator inte är tilldelad via inspector, försök hitta den
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError("Kunde inte hitta Animator-komponenten!");
            return;
        }

        characterController = GetComponent<CharacterController>();

        if (characterController != null)
        {
            originalControllerEnabled = characterController.enabled;
        }
        else
        {
            Debug.LogWarning("CharacterController hittades inte - kan inte blockera rörelse automatiskt");
        }

        // Lägg till alla standardanimationer till activeAnimations och sätt dem till false
        foreach (var trigger in animationTriggers)
        {
            activeAnimations[trigger] = false;
        }

        if (debugMode)
        {
            Debug.Log("PlayerAnimationController initialiserad");
            ListAnimatorParameters();
        }
    }

    private void RegisterAnimationParameters()
    {
        // Kontrollera vilka animationsparametrar som faktiskt finns och logga dem
        if (animator != null)
        {
            foreach (var trigger in animationTriggers)
            {
                bool paramExists = false;
                foreach (var param in animator.parameters)
                {
                    if (param.name == trigger)
                    {
                        paramExists = true;
                        break;
                    }
                }

                if (!paramExists && debugMode)
                {
                    Debug.LogWarning($"Animationsparameter '{trigger}' finns inte i Animator!");
                }
            }
        }
    }

    private void ListAnimatorParameters()
    {
        if (animator == null) return;

        Debug.Log("Tillgängliga animationsparametrar:");
        foreach (var param in animator.parameters)
        {
            Debug.Log($"- {param.name} (Type: {param.type})");
        }
    }

    void Update()
    {
        if (animator == null) return;

        try
        {
            // Grundläggande rörelser om inga blockerande animationer spelas
            if (!IsPlayingBlockingAnimation())
            {
                float horizontalInput = Input.GetAxis("Horizontal");
                float verticalInput = Input.GetAxis("Vertical");
                bool isMoving = horizontalInput != 0 || verticalInput != 0;

                //animator.SetBool("isWalking", isMoving);
                //animator.SetBool("isRunning", isMoving && Input.GetKey(KeyCode.LeftShift));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ett fel uppstod i AnimationController: {e.Message}");
        }
    }

    void LateUpdate()
    {
        // Blockera alla försök att aktivera CharacterController under en animation
        if (shouldDisableController && characterController != null && characterController.enabled)
        {
            characterController.enabled = false;
        }
    }

    // Generell metod för att spela en animation
    public void PlayAnimation(string animationParam, float duration, bool blockMovement = true)
    {
        // Kontrollera om vi redan spelar en blockerande animation
        if (IsPlayingBlockingAnimation() && blockMovement)
        {
            Debug.Log($"Kan inte spela {animationParam}: Redan upptagen med en annan animation");
            return;
        }

        // Kontrollera om denna animation finns i Animator
        if (!CheckAnimatorHasParameter(animationParam))
        {
            Debug.LogWarning($"Animationsparameter '{animationParam}' finns inte i Animator!");
            return;
        }

        // Markera animationen som aktiv
        activeAnimations[animationParam] = true;

        // Blockera rörelse om det behövs
        if (blockMovement && characterController != null)
        {
            // Spara nuvarande state
            originalControllerEnabled = characterController.enabled;

            // Inaktivera controller för att blockera rörelse
            characterController.enabled = false;
            shouldDisableController = true;

            if (debugMode)
                Debug.Log("Rörelse blockerad för animation");
        }

        // Spela animationen
        animator.SetBool(animationParam, true);

        if (debugMode)
            Debug.Log($"Spelar animation: {animationParam}");

        // Schemalägg när animationen ska sluta
        StartCoroutine(StopAnimationAfterDuration(animationParam, duration));
    }

    // Stoppa animationen efter en viss tid
    private IEnumerator StopAnimationAfterDuration(string animationParam, float duration)
    {
        yield return new WaitForSeconds(duration);
        StopAnimation(animationParam);

        // Meddela eventuella lyssnare att animationen har slutförts
        OnAnimationCompleted?.Invoke(animationParam);
    }

    // Stoppa en animation
    public void StopAnimation(string animationParam)
    {
        if (activeAnimations.ContainsKey(animationParam) && activeAnimations[animationParam])
        {
            activeAnimations[animationParam] = false;

            if (animator != null && CheckAnimatorHasParameter(animationParam))
            {
                animator.SetBool(animationParam, false);

                if (debugMode)
                    Debug.Log($"Stoppade animation: {animationParam}");
            }

            // Om detta var den sista aktiva blockerande animationen, återställ rörelseförmåga
            if (!IsPlayingBlockingAnimation() && characterController != null)
            {
                characterController.enabled = originalControllerEnabled;
                shouldDisableController = false;

                if (debugMode)
                    Debug.Log("Rörelse återställd efter animation");
            }
        }
    }

    // Kontrollera om någon animation som blockerar rörelse spelas
    public bool IsPlayingBlockingAnimation()
    {
        foreach (var blockingAnim in blockingAnimations)
        {
            if (activeAnimations.ContainsKey(blockingAnim) && activeAnimations[blockingAnim])
            {
                return true;
            }
        }
        return false;
    }

    // Publik metod för att kontrollera om en specifik animation spelas
    public bool IsPlayingAnimation(string animationParam)
    {
        return activeAnimations.ContainsKey(animationParam) && activeAnimations[animationParam];
    }

    // Kontrollera om en parameter finns i Animator
    private bool CheckAnimatorHasParameter(string paramName)
    {
        if (animator == null) return false;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    // Publik metod för att triggra en animation från andra komponenter
    public bool TriggerAnimation(string triggerName)
    {
        if (animator == null) return false;

        if (CheckAnimatorHasParameter(triggerName))
        {
            animator.SetTrigger(triggerName);
            return true;
        }

        if (debugMode)
            Debug.LogWarning($"Kunde inte triggra animation '{triggerName}', parameter finns inte");

        return false;
    }

    // Metod för att registrera nya animationsparametrar
    public void RegisterAnimationParameter(string paramName, bool isBlocking = false)
    {
        if (!animationTriggers.Contains(paramName))
        {
            animationTriggers.Add(paramName);
            activeAnimations[paramName] = false;

            if (isBlocking && !blockingAnimations.Contains(paramName))
            {
                blockingAnimations.Add(paramName);
            }

            if (debugMode)
                Debug.Log($"Registrerade ny animationsparameter: {paramName} (Blockerande: {isBlocking})");
        }
    }

    // Om skriptet förstörs, återställ alltid original state för att undvika att spelaren blir låst
    private void OnDestroy()
    {
        if (characterController != null)
        {
            characterController.enabled = originalControllerEnabled;
        }
    }
}