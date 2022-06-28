using UnityEngine;
using System.Collections.Generic;

public class SimpleCharacterControl : MonoBehaviour
{
    CharacterController controller;
    [SerializeReference] Transform camera;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float maxSpeed = 4f;
    [SerializeField] float drag = 0.01f;
    [SerializeField] float jumpForce = 4f;
    [SerializeField] float gravityForce = 9.81f;
    [SerializeField] float turnSmoothness = .1f;
    [SerializeField] private Animator animator = null;
    float turnSmoothVelocity;
    const float movEpsilon = 0.1f;
    private bool wasGrounded;

    Vector3 velocity;

    private void Awake()
    {
        if (!animator) { gameObject.GetComponent<Animator>(); }
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        wasGrounded = controller.isGrounded;
    }

    private float ApplyDrag(float veloc)
    {
        float modifiedDrag = drag * Time.deltaTime;
        float sign = Mathf.Sign(veloc);
        if (Mathf.Abs(veloc) - modifiedDrag < 0)
        {
            veloc = 0f;
        }
        else
        {
            veloc -= sign * drag;
        }
        return veloc;
    }

    private void HandlePlayerMovement()
    {
        float movX = Input.GetAxisRaw("Horizontal");
        float movZ = Input.GetAxisRaw("Vertical");

        Vector3 dir = new Vector3(movX, 0f, movZ).normalized;

        if (controller.isGrounded)
        {
            float modifiedDrag = drag * Time.deltaTime;

            velocity.x = ApplyDrag(velocity.x);
            velocity.z = ApplyDrag(velocity.z);
        }

        if (dir.magnitude >= movEpsilon)
        {
            float target = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + camera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, target, ref turnSmoothVelocity, turnSmoothness);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 movDir = (Quaternion.Euler(0f, target, 0f) * Vector3.forward).normalized;
            Vector3 movement = movDir * moveSpeed * Time.deltaTime;
            
            velocity.x = movement.x;
            velocity.z = movement.z;
            controller.Move(movDir * moveSpeed * Time.deltaTime);
        }

        animator.SetFloat("MoveSpeed", dir.magnitude);
    }

    private void HandlePlayerJump()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        if (Input.GetKey(KeyCode.Space) && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * 2f * gravityForce);
        }

        velocity.y -= gravityForce * Time.deltaTime;
        controller.Move(Vector3.up * velocity.y * Time.deltaTime);

        if (!wasGrounded && controller.isGrounded)
        {
            animator.SetTrigger("Land");
        }

        if (!controller.isGrounded && wasGrounded)
        {
            animator.SetTrigger("Jump");
        }
    }

    private void Update()
    {
        animator.SetBool("Grounded", controller.isGrounded);
        HandlePlayerMovement();
        HandlePlayerJump();
        wasGrounded = controller.isGrounded;
    }
}