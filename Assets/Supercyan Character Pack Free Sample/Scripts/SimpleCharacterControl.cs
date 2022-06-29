using UnityEngine;
using System.Collections.Generic;

public class SimpleCharacterControl : MonoBehaviour
{
    CharacterController controller;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float drag = 0.01f;
    [SerializeField] float jumpForce = 4f;
    [SerializeField] float gravityForce = 9.81f;
    [SerializeField] float turnSmoothness = .1f;
    [SerializeField] float acceleration = .25f;
    [SerializeField] private Animator animator = null;
    [SerializeReference] private LayerMask terrainMask;
    private Transform cameraTransform;
    float halfHeight;
    float turnSmoothVelocity;
    float procAcc;
    const float movEpsilon = 0.1f;
    private bool isGrounded;
    private bool wasGrounded;
    private bool canJump;

    Vector3 velocity;

    private void Awake()
    {
        if (!animator) { gameObject.GetComponent<Animator>(); }
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        isGrounded = CheckGrounded();
        wasGrounded = isGrounded;
        canJump = isGrounded;
        halfHeight = controller.skinWidth + controller.height / 2f;
        procAcc = 0f;
        cameraTransform = Camera.main.transform;
    }

    private bool CheckGrounded()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position + Vector3.up * halfHeight, 0.2f, Vector3.down, out hit, halfHeight, terrainMask))
        {
            float groundSlopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            if (groundSlopeAngle > controller.slopeLimit)
            {
                canJump = false;
            }
            else
            {
                canJump = true;
            }
            return true;
        }
        canJump = false;
        return false;
    }

    private void HandlePlayerMovement()
    {
        float movX = Input.GetAxisRaw("Horizontal");
        float movZ = Input.GetAxisRaw("Vertical");

        Vector3 dir = new Vector3(movX, 0f, movZ).normalized;

        if (isGrounded)
        {
            float modifiedDrag = drag * Time.deltaTime;
            procAcc = Mathf.Max(0f, procAcc - modifiedDrag);
        }

        if (dir.magnitude >= movEpsilon)
        {
            float target = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, target, ref turnSmoothVelocity, turnSmoothness);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 movDir = (Quaternion.Euler(0f, target, 0f) * Vector3.forward).normalized;
            Vector3 movement = movDir * moveSpeed * Time.deltaTime;

            velocity.x = movement.x;
            velocity.z = movement.z;
            procAcc = Mathf.Min(procAcc + acceleration, 1f);
        }
        Vector3 mov = new Vector3(velocity.x * procAcc, 0.0f, velocity.z * procAcc);
        controller.Move(mov);
        animator.SetFloat("MoveSpeed", moveSpeed * procAcc);
    }

    private void HandlePlayerJump()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        if (Input.GetKey(KeyCode.Space) && canJump)
        {
            velocity.y = Mathf.Sqrt(jumpForce * 2f * gravityForce);
        }

        velocity.y -= gravityForce * Time.deltaTime;
        controller.Move(Vector3.up * velocity.y * Time.deltaTime);

        if (!wasGrounded && isGrounded)
        {
            animator.SetTrigger("Land");
        }

        if (!isGrounded && wasGrounded)
        {
            animator.SetTrigger("Jump");
        }
    }

    private void Update()
    {
        if (!PauseMenu.gameIsPaused)
        {
            animator.SetBool("Grounded", isGrounded);
            isGrounded = CheckGrounded();
            HandlePlayerMovement();
            HandlePlayerJump();
            wasGrounded = isGrounded;
        }
    }
}