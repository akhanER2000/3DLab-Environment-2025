//frab+
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PhPaperInteraction : MonoBehaviour
{
    [Header("Referencia UI")]
    public PhPaperUIController uiController;

    [Header("Referencia modelo 3D del papel pH")]
    public PapelPh3DController papel3DController;
    
    [Header("Colores por sustancia (configurables en el inspector)")]
    public Color[] aguaColors = { Color.blue, Color.cyan, Color.green, Color.yellow };
    public Color[] acidoColors = { Color.red, Color.magenta, Color.yellow, Color.green };
    public Color[] baseColors = { Color.green, Color.yellow, Color.cyan, Color.blue };

    private bool hasMeasured = false;
    private XRGrabInteractable grab;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();

        // Asegurar que la UI esté oculta al inicio
        if (uiController != null)
        {
            uiController.HideUI();
        }
    }

    private void OnEnable()
    {
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    private void OnDisable()
    {
        grab.selectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (uiController != null)
        {
            uiController.ShowUI();
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (uiController != null)
        {
            uiController.HideUI();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasMeasured) return;

        if (other.CompareTag("pHAgua"))
        {
            ApplyMeasurement(aguaColors);
        }
        else if (other.CompareTag("pHAcido"))
        {
            ApplyMeasurement(acidoColors);
        }
        else if (other.CompareTag("pHBase"))
        {
            ApplyMeasurement(baseColors);
        }
    }

    /// <summary>
    /// Aplica el conjunto de colores a la UI y al modelo 3D.
    /// </summary>
    private void ApplyMeasurement(Color[] colors)
    {
        if (uiController != null)
            uiController.SetColors(colors);

        if (papel3DController != null)
            papel3DController.SetColors(colors);

        hasMeasured = true;
    }

    /// <summary>
    /// Permite reiniciar el estado del papel para una nueva medición.
    /// </summary>
    public void ResetMeasurement()
    {
        hasMeasured = false;
        if (papel3DController != null)
            papel3DController.ResetMeasurement();
    }
}
