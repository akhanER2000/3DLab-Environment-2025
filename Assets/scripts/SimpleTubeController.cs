using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections; // <-- ASEGÚRATE DE QUE ESTA LÍNEA EXISTA

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
[RequireComponent(typeof(AutoGrabPoint))]
public class SimpleTubeController : MonoBehaviour
{
    private Rigidbody rb;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private AutoGrabPoint autoGrabPoint;

    private SimpleSnapPoint currentSnapPoint = null;
    private bool isHeldByTongs = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        autoGrabPoint = GetComponent<AutoGrabPoint>();

        grabInteractable.selectEntered.AddListener(OnGrabbedByHand);
        grabInteractable.selectExited.AddListener(OnReleasedByHand);
    }

    void Update()
    {
        if (autoGrabPoint.IsGrabbed && !isHeldByTongs)
        {
            OnGrabbedByTongs();
        }
        else if (!autoGrabPoint.IsGrabbed && isHeldByTongs)
        {
            OnReleasedByTongs();
        }
    }

    private void OnGrabbedByHand(SelectEnterEventArgs args)
    {
        Debug.Log("Tubo agarrado por la mano.");
        if (rb != null) rb.constraints = RigidbodyConstraints.None; // Liberación incondicional

        if (currentSnapPoint != null)
        {
            currentSnapPoint.ReleaseTube();
            currentSnapPoint = null;
        }
    }

    private void OnReleasedByHand(SelectExitEventArgs args)
    {
        Debug.Log("Tubo soltado por la mano.");
        if (rb != null && currentSnapPoint == null)
        {
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
        }
    }

    private void OnGrabbedByTongs()
    {
        isHeldByTongs = true;
        grabInteractable.enabled = false;
        Debug.LogWarning("Tubo agarrado por PINZAS. Interacción manual DESACTIVADA.");

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.None;
        }

        if (currentSnapPoint != null)
        {
            currentSnapPoint.ReleaseTube();
            currentSnapPoint = null;
        }
    }

    private void OnReleasedByTongs()
    {
        isHeldByTongs = false;
        grabInteractable.enabled = true;
        Debug.LogWarning("Tubo soltado por PINZAS. Interacción manual REACTIVADA.");

        // En lugar de aplicar la física directamente, iniciamos la corrutina
        StartCoroutine(RestorePhysicsAfterTongsRelease());
    }

    // --- NUEVA CORRUTINA ---
    // Espera hasta el final del fotograma para aplicar los cambios de física.
    private IEnumerator RestorePhysicsAfterTongsRelease()
    {
        // Espera a que todas las demás actualizaciones del fotograma terminen.
        yield return new WaitForEndOfFrame();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
            Debug.Log("Físicas restauradas al final del fotograma por corrutina.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SnapPoint"))
        {
            currentSnapPoint = other.GetComponent<SimpleSnapPoint>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (currentSnapPoint != null && other.GetComponent<SimpleSnapPoint>() == currentSnapPoint)
        {
            currentSnapPoint = null;
        }
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbedByHand);
            grabInteractable.selectExited.RemoveListener(OnReleasedByHand);
        }
    }
}