using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit;

public class RotateOnGrab : MonoBehaviour
{
    public Vector3 initialRotation = new Vector3(-91.641f, 86.807f, -87.37299f);
    public Vector3 grabbedRotation = new Vector3(-69.765f, 89.637f, -90.217f);

    public GameObject aguaParticulas; // Referencia al sistema de partículas

    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        transform.localEulerAngles = initialRotation;

        // Nuevos eventos
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);

        if (aguaParticulas != null)
            aguaParticulas.SetActive(false); // Asegura que comience apagado
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        transform.localEulerAngles = grabbedRotation;

        if (aguaParticulas != null)
            aguaParticulas.SetActive(true); // Activa el agua
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        // Importante: verificar que no se haya cancelado
        if (args.isCanceled)
            return;

        transform.localEulerAngles = initialRotation;

        if (aguaParticulas != null)
            aguaParticulas.SetActive(false); // Desactiva el agua
    }

    void OnDestroy()
    {
        // Siempre remover listeners para evitar leaks
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }
}
