using UnityEngine;

public class ValveInteractionController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GasValve gasValve;

    [Header("Mouse/Keyboard Controls")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private bool useScrollWheel = true;
    [SerializeField] private float scrollSensitivity = 10f;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask interactionLayer = -1;

    private bool isGrabbed = false;
    private Camera playerCamera;
    private Vector3 lastMousePosition;

    void Start()
    {
        if (!gasValve)
            gasValve = GetComponent<GasValve>();

        playerCamera = Camera.main;
        if (!playerCamera)
        {
            Debug.LogError("No se encontró cámara principal!");
        }
    }

    void Update()
    {
        HandleMouseInput();

        if (isGrabbed)
        {
            HandleRotation();
        }

        // Mostrar prompt de interacción
        if (!isGrabbed && IsLookingAtValve())
        {
            ShowInteractionPrompt();
        }
    }

    void HandleMouseInput()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (IsLookingAtValve() && !isGrabbed)
            {
                StartGrab();
            }
        }

        if (Input.GetKeyUp(interactKey) && isGrabbed)
        {
            EndGrab();
        }

        // Salida de emergencia
        if (Input.GetKeyDown(KeyCode.Escape) && isGrabbed)
        {
            EndGrab();
        }
    }

    bool IsLookingAtValve()
    {
        if (!playerCamera) return false;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange, interactionLayer))
        {
            return hit.transform == transform || hit.transform.IsChildOf(transform);
        }

        return false;
    }

    void StartGrab()
    {
        isGrabbed = true;
        gasValve.StartInteraction();
        lastMousePosition = Input.mousePosition;
        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log("Agarrando válvula - mueve el mouse para girar");
    }

    void EndGrab()
    {
        isGrabbed = false;
        gasValve.EndInteraction();
        Cursor.lockState = CursorLockMode.None;
        Debug.Log("Válvula soltada");
    }

    void HandleRotation()
    {
        float rotationInput = 0f;

        if (useScrollWheel && Input.mouseScrollDelta.y != 0)
        {
            rotationInput = Input.mouseScrollDelta.y * scrollSensitivity;
        }
        else
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            rotationInput = mouseDelta.x * mouseSensitivity;
            lastMousePosition = Input.mousePosition;
        }

        if (Mathf.Abs(rotationInput) > 0.01f)
        {
            gasValve.RotateValve(rotationInput);
        }
    }

    void ShowInteractionPrompt()
    {
        // Aquí puedes mostrar un UI element
        Debug.Log($"Presiona [{interactKey}] para interactuar con la válvula");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}