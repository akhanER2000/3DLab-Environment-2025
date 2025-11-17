using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;

public class RotateOnGrabFixed : MonoBehaviour
{
    public Vector3 initialRotation = new Vector3(-91.641f, 86.807f, -87.37299f);
    public Vector3 grabbedRotation = new Vector3(-69.765f, 89.637f, -90.217f);

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        // Aplicar rotación inicial y bloquear posición y rotación
        transform.localEulerAngles = initialRotation;
        rb.isKinematic = true;  // Que no reaccione a físicas
        rb.constraints = RigidbodyConstraints.FreezeAll;  // Bloquea movimiento y rotación total

        // Usar nuevos eventos
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);

        // Desactivar el seguimiento automático del controlador (movimiento y rotación)
        grabInteractable.trackPosition = false;
        grabInteractable.trackRotation = false;
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        transform.localEulerAngles = grabbedRotation;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (args.isCanceled)
            return;

        transform.localEulerAngles = initialRotation;
    }

    void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }
}
