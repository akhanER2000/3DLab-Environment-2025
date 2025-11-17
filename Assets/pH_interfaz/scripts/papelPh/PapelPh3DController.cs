//frab+
using UnityEngine;
using System.Collections;

public class PapelPh3DController : MonoBehaviour
{
    [Header("Paneles 3D del papel pH")]
    [Tooltip("Asigna aquí los 4 paneles del modelo 3D del papel pH (Renderers de los objetos hijos)")]
    public Renderer[] panelRenderers;

    private bool hasMeasured = false;

    /// <summary>
    /// Cambia los colores de los paneles 3D de forma suave.
    /// </summary>
    public void SetColors(Color[] newColors)
    {
        if (hasMeasured) return;

        StartCoroutine(TransitionColors(newColors, 2f)); // transición de 2 segundos
        hasMeasured = true;
    }

    /// <summary>
    /// Transición suave entre los colores actuales y los nuevos.
    /// </summary>
    private IEnumerator TransitionColors(Color[] targetColors, float duration)
    {
        if (panelRenderers == null || panelRenderers.Length == 0)
            yield break;

        Color[] initialColors = new Color[panelRenderers.Length];
        for (int i = 0; i < panelRenderers.Length; i++)
        {
            initialColors[i] = panelRenderers[i].material.color;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);

            for (int i = 0; i < panelRenderers.Length && i < targetColors.Length; i++)
            {
                panelRenderers[i].material.color = Color.Lerp(initialColors[i], targetColors[i], t);
            }

            yield return null;
        }

        for (int i = 0; i < panelRenderers.Length && i < targetColors.Length; i++)
        {
            panelRenderers[i].material.color = targetColors[i];
        }
    }

    /// <summary>
    /// Permite resetear el estado de medición para volver a usar el papel.
    /// </summary>
    public void ResetMeasurement()
    {
        hasMeasured = false;
    }
}
