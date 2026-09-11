using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    Camera camera;
    Vector2 lookInput;
    float pitch;
    [SerializeField] float sensitivity;

    private void Awake()
    {
        camera = GetComponentInChildren<Camera>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        transform.Rotate(0, lookInput.x * sensitivity, 0);
        pitch -= lookInput.y * sensitivity;
        pitch = Mathf.Clamp(pitch, -90f, 90f);
        camera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
}
