using UnityEngine;
using UnityEngine.InputSystem; // Nuevo Input System

[DisallowMultipleComponent]
public class InteractWithCalculator : MonoBehaviour
{
    [Header("Referencia al controlador de UI")]
    public CalculatorUIController uiController;

    [Header("Parámetros de interacción (PC con nuevo Input System)")]
    [Tooltip("Distancia máxima del raycast desde la cámara hacia adelante.")]
    public float interactDistance = 3.0f;

    [Tooltip("Tecla de interacción usando el nuevo Input System (Keyboard).")]
    public Key interactKey = Key.E; // <-- Nuevo: enum del Input System

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        if (uiController == null)
        {
            Debug.LogWarning("[InteractWithCalculator] Asigna el CalculatorUIController en el Inspector.");
        }

        if (cam == null)
        {
            var anyCam = FindObjectOfType<Camera>();
            if (anyCam != null)
            {
                cam = anyCam;
                Debug.LogWarning("[InteractWithCalculator] Camera.main no encontrada. Usando: " + anyCam.name + ". Etiqueta la cámara XR como 'MainCamera'.");
            }
            else
            {
                Debug.LogError("[InteractWithCalculator] No hay ninguna cámara en la escena.");
            }
        }
    }

    void Update()
    {
        // Usar nuevo Input System (Keyboard)
        if (Keyboard.current != null)
        {
            var keyControl = Keyboard.current[interactKey];
            if (keyControl != null && keyControl.wasPressedThisFrame)
            {
                TryToggleFromCenterRay();
            }
        }
        // Si quieres soportar también gamepad, podríamos mapear un InputAction; dímelo y te lo dejo listo.
    }

    /// <summary>
    /// Lanza un raycast desde la cámara hacia delante y, si golpea este mismo objeto, hace Toggle de la UI.
    /// </summary>
    private void TryToggleFromCenterRay()
    {
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                uiController?.Toggle();
            }
        }
    }

    // Permite click directo en Editor (útil para pruebas con ratón). No usa la API vieja de Input.
    // Requiere que este objeto tenga un Collider y que el EventSystem permita clics en mundo (para selección).
    private void OnMouseDown()
    {
        uiController?.Toggle();
    }
}
