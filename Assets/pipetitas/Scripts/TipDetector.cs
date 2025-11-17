using UnityEngine;

public class TipDetector : MonoBehaviour
{
    [Tooltip("Referencia al PipetteController (puede estar en el padre).")]
    public PipetteController pipette;

    void Start()
    {
        if (pipette == null)
            pipette = GetComponentInParent<PipetteController>();

        if (pipette == null)
            Debug.LogWarning("TipDetector: no se encontró PipetteController en padres.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (pipette == null) return;

        if (other.CompareTag("FlaskMouth"))
        {
            pipette.canSuck = true;
            Debug.Log("Tip: Entered Flask mouth -> canSuck = true");
        }

        if (other.CompareTag("TestTubeMouth"))
        {
            pipette.tipInTestTube = true;
            // buscar componente TestTubeController en los padres del collider
            var tt = other.GetComponentInParent<TestTubeController>();
            pipette.currentTestTube = tt;
            Debug.Log("Tip: Entered TestTube mouth -> tipInTestTube = true");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (pipette == null) return;

        if (other.CompareTag("FlaskMouth"))
        {
            pipette.canSuck = false;
            Debug.Log("Tip: Exited Flask mouth -> canSuck = false");
        }

        if (other.CompareTag("TestTubeMouth"))
        {
            pipette.tipInTestTube = false;
            pipette.currentTestTube = null;
            Debug.Log("Tip: Exited TestTube mouth -> tipInTestTube = false");
        }
    }
}
