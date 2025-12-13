using UnityEngine;
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float mouseSensitivity = 2f;
    public float lookXLimit = 85f;
    public Camera playerCamera;

    [Header("Interaction Settings")]
    public float interactionDistance = 3f;
    public TMP_Text interactionText;
    public Backpack backpack;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (interactionText != null)
            interactionText.text = "";
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleInteraction();
    }

    void HandleMovement()
    {
        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        
        // Press Left Shift to run (optional)
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = (isRunning ? walkSpeed * 1.5f : walkSpeed) * Input.GetAxis("Vertical");
        float curSpeedY = (isRunning ? walkSpeed * 1.5f : walkSpeed) * Input.GetAxis("Horizontal");

        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Gravity
        if (!characterController.isGrounded)
        {
            moveDirection.y = movementDirectionY - 9.81f * Time.deltaTime;
        }
        else
        {
            moveDirection.y = -0.5f; // Small stick-to-ground force
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    void HandleRotation()
    {
        if (playerCamera == null) return;

        rotationX += -Input.GetAxis("Mouse Y") * mouseSensitivity;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * mouseSensitivity, 0);
    }

    void HandleInteraction()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            Weapon weapon = hit.collider.GetComponent<Weapon>();
            if (weapon != null)
            {
                if (interactionText != null)
                {
                    // "Take for E" as requested + weapon name
                    interactionText.text = "Press 'E' to take " + weapon.weaponName; 
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (backpack != null)
                    {
                        backpack.AddItem(weapon);
                        
                        // Clear text immediately or let the next frame handle it (next frame is safer if we destroy)
                        if (interactionText != null) interactionText.text = "";
                        
                        Destroy(weapon.gameObject);
                    }
                    else
                    {
                        Debug.LogWarning("Backpack is not assigned on FirstPersonController!");
                    }
                }
            }
            else
            {
                // Hit something else
                if (interactionText != null) interactionText.text = "";
            }
        }
        else
        {
            // Hit nothing
            if (interactionText != null) interactionText.text = "";
        }
    }
}
