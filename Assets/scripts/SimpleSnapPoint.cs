using UnityEngine;
using System.Collections;

public class SimpleSnapPoint : MonoBehaviour
{
    [Header("Snap Configuration")]
    [SerializeField] private bool isOccupied = false;
    [SerializeField] private GameObject currentTube = null;
    [SerializeField] private float snapSpeed = 5f; // Mantengo por si se usa en el futuro

    [Header("Visual Feedback")]
    [SerializeField] private Color availableColor = Color.green;
    [SerializeField] private Color occupiedColor = Color.red;

    [Header("Tube Positioning")]
    [Tooltip("Ajusta la posición del tubo relativa al punto de snap")]
    [SerializeField] private Vector3 tubeOffset = new Vector3(0, 0.02f, 0);

    [Tooltip("Rotación del tubo cuando se snapea (0,0,0 = vertical)")]
    [SerializeField] private Vector3 tubeRotation = new Vector3(0, 0, 0);

    [Header("Debug Options")]
    [SerializeField] private bool showDebugMessages = true;

    private void Start()
    {
        if (!gameObject.CompareTag("SnapPoint"))
        {
            try { gameObject.tag = "SnapPoint"; }
            catch { Debug.LogWarning("Tag 'SnapPoint' no existe. Créalo en Project Settings > Tags"); }
        }
    }

    // Usamos OnTriggerStay para una detección más robusta
    private void OnTriggerStay(Collider other)
    {
        // Si ya está ocupado o el objeto no tiene el tag correcto, no hacemos nada.
        if (isOccupied || (!other.CompareTag("Vaso") && !other.CompareTag("Electrodo"))) return;
    
        // Verificamos si el objeto está siendo agarrado por la mano
        var grabInteractable = other.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable != null && !grabInteractable.isSelected)
        {
            // Si no está seleccionado (fue soltado), procedemos a anclarlo.
            AttemptSnap(other.gameObject);
        }
    }

    public void AttemptSnap(GameObject tube)
    {
        if (showDebugMessages)
            Debug.Log($"✓ Intentando anclar {tube.name} en {gameObject.name}");

        isOccupied = true;
        currentTube = tube;

        // Deshabilitamos temporalmente el collider para evitar rebotes extraños durante el anclaje
        Collider myCollider = GetComponent<Collider>();
        if (myCollider != null) myCollider.enabled = false;

        Rigidbody rb = tube.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeAll; // Congelamos para asegurar la posición
        }

        // Posicionamos y rotamos el tubo
        tube.transform.position = transform.position + tubeOffset;
        tube.transform.rotation = Quaternion.Euler(tubeRotation);

        // Reactivamos el collider después de un par de ciclos de física
        StartCoroutine(ReactivateCollider());

        if (showDebugMessages)
            Debug.Log($"✓ Tubo anclado en {gameObject.name}");
    }

    private IEnumerator ReactivateCollider()
    {
        // Esperamos dos ciclos de física para asegurar que todo se ha estabilizado
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        Collider myCollider = GetComponent<Collider>();
        if (myCollider != null) myCollider.enabled = true;
    }

    public void ReleaseTube()
    {
        if (currentTube != null)
        {
            Rigidbody rb = currentTube.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.None; // Liberamos las restricciones
                rb.useGravity = true; // Reactivamos la gravedad
            }
            if (showDebugMessages)
                Debug.Log($"Tubo liberado de {gameObject.name}");
        }

        isOccupied = false;
        currentTube = null;
    }

    // --- El resto de funciones (SetTubeOffset, SetTubeRotation, OnDrawGizmos, etc.) se mantienen igual ---
    // (Puedes copiarlas de tu script original si las eliminaste)
    public void SetTubeOffset(Vector3 newOffset) { /* ... */ }
    public void SetTubeRotation(Vector3 newRotation) { /* ... */ }
    private void OnDrawGizmos() { /* ... */ }
    private void OnDrawGizmosSelected() { /* ... */ }
}