using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private Camera mainCamera;

    [Header("Movement Settings")]
    [Tooltip("Optional visual effect spawned where the player clicks on the ground.")]
    [SerializeField] private ParticleSystem clickEffect;
    [Tooltip("Rotation speed when turning to face the movement direction.")]
    [SerializeField] private float lookRotationSpeed = 8f;
    [Tooltip("Maximum distance in pixels a touch can move and still count as a tap.")]
    [SerializeField] private float maxTapMovement = 20f;

    private Vector2 touchStartPos;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleInput();
        FaceTarget();
        UpdateAnimationState();
    }

    /// <summary>
    /// Handles mouse and touch input for player movement.
    /// Ignores clicks on objects assigned to the 'Interactable' layer.
    /// </summary>
    private void HandleInput()
    {
        // --- Mouse input ---
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // First, check if we clicked the ground
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
            {
                MoveAgent(hit.point);
                return;
            }

            // If not, check if we clicked an interactable object
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Interactable")))
            {
                Debug.Log("[PlayerController] Clicked on Interactable: " + hit.collider.name);
                // Do not move toward interactables
                return;
            }
        }

        // --- Touch input (for mobile) ---
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
                touchStartPos = touch.position;

            if (touch.phase == TouchPhase.Ended)
            {
                // Only treat as a tap if the finger didn’t move far
                if (Vector2.Distance(touch.position, touchStartPos) <= maxTapMovement)
                {
                    Ray ray = mainCamera.ScreenPointToRay(touch.position);
                    RaycastHit hit;

                    // Ground
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
                    {
                        MoveAgent(hit.point);
                        return;
                    }

                    // Interactable
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Interactable")))
                    {
                        Debug.Log("[PlayerController] Tapped on Interactable: " + hit.collider.name);
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Commands the NavMeshAgent to move to a new position.
    /// </summary>
    private void MoveAgent(Vector3 destination)
    {
        if (agent == null)
        {
            Debug.LogWarning("[PlayerController] Missing NavMeshAgent component.");
            return;
        }

        agent.SetDestination(destination);

        // Optional click effect
        if (clickEffect != null)
        {
            Instantiate(clickEffect, destination + Vector3.up * 0.1f, Quaternion.identity);
        }
    }

    /// <summary>
    /// Rotates the player toward their movement direction smoothly.
    /// </summary>
    private void FaceTarget()
    {
        if (agent == null) return;

        Vector3 direction = (agent.destination - transform.position).normalized;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookRotationSpeed);
        }
    }

    /// <summary>
    /// Updates animations based on velocity.
    /// </summary>
    private void UpdateAnimationState()
    {
        if (animator == null || agent == null) return;

        if (agent.velocity.sqrMagnitude < 0.01f)
            animator.Play("Idle");
        else
            animator.Play("Walk");
    }
}
