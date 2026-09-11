using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    CharacterController characterController;
    Vector2 moveInput;
    [SerializeField] float moveSpeed;
    [SerializeField] float sprintMultiplier;
    float verticalVelocity;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float jumpHeight;
    bool sprinting;


    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Start()
    {
        
    }
    
    void Update()
    {
        if (characterController.isGrounded)
        {
            verticalVelocity = Mathf.Clamp(verticalVelocity, -0.1f, float.MaxValue);
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        float currentSpeed = sprinting ? moveSpeed * sprintMultiplier : moveSpeed;
        Vector3 forwardRelative = moveInput.y * new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        Vector3 rightRelative = moveInput.x * new Vector3(transform.right.x, 0, transform.right.z).normalized;
        Vector3 horizontalMovement = (forwardRelative + rightRelative).normalized * currentSpeed * Time.deltaTime;
        Vector3 netMovement = horizontalMovement + new Vector3(0, verticalVelocity * Time.deltaTime, 0);

        characterController.Move(netMovement);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (characterController.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(-2f * gravity * jumpHeight);
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            sprinting = true;
        } 
        else if (context.canceled)
        {
            sprinting = false;
        }
    }
}
