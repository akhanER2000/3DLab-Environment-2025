using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PhPaperUIController : MonoBehaviour
{
    [Header("Paneles de color")]
    public Image[] colorPanels; // Asigna los 4 paneles en el inspector

    [Header("UI Root (solo la interfaz, NO el objeto físico)")]
    public GameObject uiRoot;

    private bool hasMeasured = false;

    public void SetColors(Color[] newColors)
    {
        if (hasMeasured) return;

        StartCoroutine(TransitionColors(newColors, 2f));
        hasMeasured = true;
    }

    private IEnumerator TransitionColors(Color[] targetColors, float duration)
    {
        Color[] initialColors = new Color[colorPanels.Length];
        for (int i = 0; i < colorPanels.Length; i++)
            initialColors[i] = colorPanels[i].color;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float tSmooth = Mathf.SmoothStep(0, 1, elapsed / duration);

            for (int i = 0; i < colorPanels.Length && i < targetColors.Length; i++)
                colorPanels[i].color = Color.Lerp(initialColors[i], targetColors[i], tSmooth);

            yield return null;
        }

        for (int i = 0; i < colorPanels.Length && i < targetColors.Length; i++)
            colorPanels[i].color = targetColors[i];
    }

    public void ShowUI()
    {
        uiRoot.SetActive(true);
    }

    public void HideUI()
    {
        uiRoot.SetActive(false);
    }
}
