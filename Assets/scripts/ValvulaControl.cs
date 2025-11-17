using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LlaveDePaso : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;
    private bool isOpen = false;

    [Header("Offset de apertura en grados")]
    public Vector3 openOffset = new Vector3(0f, 0f, 90f);

    private Quaternion closedQuat;
    private Quaternion openQuat;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();

        // Guardamos la rotación actual como "cerrada"
        closedQuat = transform.rotation;
        openQuat = closedQuat * Quaternion.Euler(openOffset);

        interactable.selectEntered.AddListener(OnClicked);
    }

    private void OnClicked(SelectEnterEventArgs args)
    {
        isOpen = !isOpen;

        if (isOpen)
            transform.rotation = openQuat;   // abrir
        else
            transform.rotation = closedQuat; // cerrar
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}
