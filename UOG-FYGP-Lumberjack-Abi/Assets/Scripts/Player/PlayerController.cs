using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    NavMeshAgent agent;
    Animator animator;

    [Header("Movement")]
    [SerializeField] ParticleSystem clickEffect;
    [SerializeField] LayerMask clickableLayers;
    [SerializeField] float lookRotationSpeed = 8f;
    [SerializeField] float maxTapMovement = 20f; // pixels allowed for it to still count as a tap

    private Vector2 touchStartPos;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        ClickToMove();
        FaceTarget();
        SetAnimations();
    }

    void ClickToMove()
    {
        RaycastHit hit;
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100, clickableLayers))
            {
                MoveAgent(hit.point);
            }
        }
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position; // remember where finger started
            }

            if (touch.phase == TouchPhase.Ended)
            {
                // check if finger moved too much (ignore swipes/pans)
                if (Vector2.Distance(touch.position, touchStartPos) <= maxTapMovement)
                {
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(touch.position), out hit, 100, clickableLayers))
                    {
                        MoveAgent(hit.point);
                    }
                }
            }
        }
    }
    void MoveAgent(Vector3 point)
    {
        agent.destination = point;

        if (clickEffect != null)
        {
            Instantiate(clickEffect, point + Vector3.up * 0.1f, clickEffect.transform.rotation);
        }
    }
    void FaceTarget()
    {
        Vector3 direction = (agent.destination - transform.position).normalized;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookRotationSpeed);
        }
    }
    void SetAnimations()
    {
        if (agent.velocity.sqrMagnitude < 0.01f)
            animator.Play("Idle");
        else
            animator.Play("Walk");
    }
}
