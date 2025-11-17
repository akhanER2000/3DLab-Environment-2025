//frab+
//este se le pone al ph metro

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PhMeterActivation : MonoBehaviour
{
    public GameObject panelPhMetro;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;
    private bool isActive = false; // Estado para alternar visibilidad

    private void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
    }

    private void OnEnable()
    {
        interactable.selectEntered.AddListener(OnSelectEntered);
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnSelectEntered);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (panelPhMetro != null)
        {
            isActive = !isActive;
            panelPhMetro.SetActive(isActive);
        }
    }
}
