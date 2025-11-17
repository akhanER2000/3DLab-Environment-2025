using UnityEngine;

public class TestTubeController : MonoBehaviour
{
    [Header("Volumen")]
    public float currentVolume = 0f;
    public float maxVolume = 20f;

    [Header("Visual (opcional)")]
    public Transform fillVisual; // child que se escala en Y para mostrar nivel (localScale.y)

    public void AddVolume(float amount)
    {
        currentVolume = Mathf.Clamp(currentVolume + amount, 0f, maxVolume);
        UpdateVisual();
        Debug.Log($"TestTube: se añadieron {amount} mL. Volumen actual: {currentVolume} mL");
    }

    void UpdateVisual()
    {
        if (fillVisual == null) return;
        float t = Mathf.Clamp01(maxVolume > 0f ? currentVolume / maxVolume : 0f);
        Vector3 s = fillVisual.localScale;
        s.y = Mathf.Lerp(0.001f, 1f, t); // evitar escala 0
        fillVisual.localScale = s;
        // Ajusta posición si tu visual debe "subir" a medida que crece.
    }
}
