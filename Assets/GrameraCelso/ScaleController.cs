using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScaleController : MonoBehaviour
{
    [Header("UI")]
    public Canvas worldSpaceUI;       // Canvas que mostrará el peso
    public TextMeshProUGUI weightText;           // Texto del peso en gramos

    [Header("Detección")]
    public Collider plateCollider;    // Collider del plato (marcar como IsTrigger)

    private HashSet<Rigidbody> currentObjects = new HashSet<Rigidbody>();
    private float tareOffset = 0f;

    private void Start()
    {
        if (worldSpaceUI != null)
            worldSpaceUI.enabled = false; // al inicio, oculto
            Debug.Log("Oculto inicio");
    }

    private void Update()
    {
        if (currentObjects.Count > 0)
        {
            float totalMass = 0f;
            foreach (var rb in currentObjects)
            {
                if (rb != null) totalMass += rb.mass;
            }

            float grams = (totalMass * 1000f) - tareOffset;
            if (weightText != null)
                weightText.text = $"{grams:F2} g";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody != null)
        {
            currentObjects.Add(other.attachedRigidbody);
            ShowUI();
            Debug.Log("Mostrando");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody != null)
        {
            currentObjects.Remove(other.attachedRigidbody);

            // si ya no queda nada encima → ocultar UI
            if (currentObjects.Count == 0)
                HideUI();
                Debug.Log("Ocultand");
        }
    }

    public void Tare()
    {
        float totalMass = 0f;
        foreach (var rb in currentObjects)
        {
            if (rb != null) totalMass += rb.mass;
        }

        tareOffset = totalMass * 1000f;
        Debug.Log($"Tara aplicada en {tareOffset:F2} g");
    }

    private void ShowUI()
    {
        if (worldSpaceUI != null)
            worldSpaceUI.enabled = true;
    }

    private void HideUI()
    {
        if (worldSpaceUI != null)
            worldSpaceUI.enabled = false;
    }
}