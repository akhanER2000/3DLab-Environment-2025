// CalculatorUIController.cs
// frab+
using UnityEngine;

[DisallowMultipleComponent]
public class CalculatorUIController : MonoBehaviour
{
    [Header("UI Root (GameObject que contiene el Canvas en World Space)")]
    [Tooltip("Arrastra aquí el GameObject raíz que contiene el Canvas (CalculatorUI_Root).")]
    [SerializeField] private GameObject uiRoot;

    [Header("Posicionamiento relativo a la cámara")]
    [Tooltip("Distancia hacia delante desde la cámara (m).")]
    [SerializeField] private float forwardDistance = 0.9f;
    [Tooltip("Desplazamiento hacia la izquierda (m). Si quieres a la derecha, usa valor negativo).")]
    [SerializeField] private float leftOffset = 0.45f;
    [Tooltip("Ajuste vertical (m). Valores positivos suben, negativos bajan).")]
    [SerializeField] private float verticalOffset = -0.05f;

    [Header("Suavizado")]
    [Tooltip("Velocidad de interpolación de la rotación (más alto = reacción más rápida).")]
    [SerializeField] private float rotateSmooth = 12f;

    // estado interno
    private Transform cam;
    private bool isOpen = false;

    void Awake()
    {
        // si no asignaste uiRoot, asumimos que el script está en el mismo GameObject raíz
        if (uiRoot == null) uiRoot = this.gameObject;

        // aseguramos que la UI comience desactivada
        if (uiRoot.activeSelf) uiRoot.SetActive(false);
    }

    void Start()
    {
        // buscamos la cámara principal (XR Origin debe tener su cámara con tag MainCamera)
        cam = Camera.main?.transform;

        if (cam == null)
        {
            Camera anyCam = FindObjectOfType<Camera>();
            if (anyCam != null)
            {
                cam = anyCam.transform;
                Debug.LogWarning("[CalculatorUIController] Camera.main no encontrada. Usando cámara: " + anyCam.name + ". Recomiendo taggear la cámara VR como 'MainCamera'.");
            }
            else
            {
                Debug.LogError("[CalculatorUIController] No hay cámara en la escena. La UI no podrá posicionarse.");
            }
        }
    }

    void LateUpdate()
    {
        if (isOpen && cam != null)
        {
            UpdatePositionAndRotation();
        }
    }

    private void UpdatePositionAndRotation()
    {
        // base de vectores de cámara
        Vector3 forward = cam.forward.normalized;
        Vector3 right = cam.right.normalized;

        // posición objetivo: delante + a la izquierda + offset vertical
        Vector3 targetPos = cam.position + forward * forwardDistance - right * leftOffset + Vector3.up * verticalOffset;
        uiRoot.transform.position = targetPos;

        // orientamos la UI para mirar al jugador en yaw (sin pitch)
        Vector3 toCam = (cam.position - uiRoot.transform.position);
        toCam.y = 0f; // evitar inclinación vertical (para que no "mire hacia arriba/abajo")
        if (toCam.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(toCam.normalized, Vector3.up);
            uiRoot.transform.rotation = Quaternion.Slerp(uiRoot.transform.rotation, targetRot, Time.deltaTime * rotateSmooth);
        }
    }

    // métodos públicos que puedes llamar desde otros scripts o desde eventos (XR Interactable)
    public void Open()
    {
        if (cam == null) Start(); // intento obtener cámara si no está
        isOpen = true;
        uiRoot.SetActive(true);
        UpdatePositionAndRotation(); // colocación inmediata en el mismo frame
    }

    public void Close()
    {
        isOpen = false;
        uiRoot.SetActive(false);
    }

    public void Toggle()
    {
        if (isOpen) Close(); else Open();
    }
}
