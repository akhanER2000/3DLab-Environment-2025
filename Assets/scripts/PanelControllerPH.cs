using UnityEngine;
using UnityEngine.InputSystem; // Aseg�rate de que esta l�nea est� presente

public class PanelController : MonoBehaviour
{
    private Animator panelAnimator;
    private bool isPanelExpanded = false;

    // A�ADIDO: Un campo p�blico para asignar la acci�n de VR desde el Inspector.
    [Tooltip("La acci�n del control de VR que activar� el panel.")]
    public InputActionProperty toggleAction;

    void Start()
    {
        panelAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        // MODIFICADO: Ahora revisa la tecla 'P' O la acci�n del control de VR.
        if (Keyboard.current.pKey.wasPressedThisFrame || (toggleAction != null && toggleAction.action.WasPressedThisFrame()))
        {
            TogglePanel();
        }
    }

    public void TogglePanel()
    {
        isPanelExpanded = !isPanelExpanded;

        if (isPanelExpanded)
        {
            panelAnimator.SetTrigger("Expand");
        }
        else
        {
            panelAnimator.SetTrigger("Minimize");
        }
    }
}