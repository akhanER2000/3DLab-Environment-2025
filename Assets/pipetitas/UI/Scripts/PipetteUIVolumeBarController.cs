using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PipetteUIVolumeBarController : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup canvasGroup;          // para Show/Hide
    public Image barFill;                    // Image.Type = Filled (Vertical, Origin=Bottom)
    public RectTransform ticksContainer;     // contenedor donde se instancian las ticks
    public GameObject tickPrefab;            // prefab UI Image
    public TMP_Text instructionsText;        // texto en el lado derecho
    public TMP_Text volumeTextBelow;         // contador numérico debajo de instrucciones
    public Camera followCamera;              // opcional: si null usa Camera.main

    [Header("Settings")]
    public float maxVolume = 10f;            // debe coincidir con PipetteController.maxVolume
    public int numTicks = 10;                // cuántas graduaciones (si maxVolume=10 y quieres 10 ticks => 1 mL por tick)
    public float fillSmoothTime = 0.06f;     // suavizado visual del fill
    public Color tickNormalColor = new Color(1f, 1f, 1f, 0.4f);
    public Color tickHighlightColor = Color.white;
    public float tickHighlightScale = 1.2f;
    public bool lockBillboardY = true;       // si true solo rota en yaw para mantener vertical

    // internals
    float _displayFill = 0f;
    float _fillVelocity = 0f;
    bool _visible = false;

    void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (followCamera == null && Camera.main != null) followCamera = Camera.main;
    }

    void Start()
    {
        if (ticksContainer != null && tickPrefab != null)
            GenerateTicks();

        // Init visuals
        SetVolumeVisual(0f, instant: true);
        Hide(); // por defecto oculto; se mostrará al agarrar
    }

    void Update()
    {
        if (barFill != null)
        {
            barFill.fillAmount = Mathf.SmoothDamp(barFill.fillAmount, _displayFill, ref _fillVelocity, fillSmoothTime);
        }
    }

    void LateUpdate()
    {
        if (_visible && followCamera != null)
            BillboardToCamera();
    }

    void BillboardToCamera()
    {
        Vector3 camPos = followCamera.transform.position;
        Vector3 dir = transform.position - camPos;
        if (lockBillboardY) dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.LookRotation(dir.normalized);
    }

    #region Public API
    public void Show()
    {
        _visible = true;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        _visible = false;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        // opcional: mantener activo pero invisible. Si quieres desactivar el GO descomenta:
        // gameObject.SetActive(false);
    }

    /// <summary>
    /// Actualiza la UI con el volumen actual en mL.
    /// </summary>
    public void SetVolumeVisual(float currentVolume, bool instant = false)
    {
        float targetFill = Mathf.Clamp01(currentVolume / Mathf.Max(0.0001f, maxVolume));
        if (instant && barFill != null)
            barFill.fillAmount = targetFill;

        _displayFill = targetFill;

        if (volumeTextBelow != null)
            volumeTextBelow.text = $"{currentVolume:0} mL";

        HighlightNearestTick(currentVolume);
    }

    public void SetInstructions(string text)
    {
        if (instructionsText != null)
            instructionsText.text = text;
    }
    #endregion

    #region Ticks generation & highlight
    void GenerateTicks()
    {
        // remove existing
        for (int i = ticksContainer.childCount - 1; i >= 0; i--)
            DestroyImmediate(ticksContainer.GetChild(i).gameObject);

        float height = ticksContainer.rect.height;

        // generamos i = 0..numTicks (inclusivo) => numTicks + 1 marcas
        for (int i = 0; i <= numTicks; i++)
        {
            float t = (float)i / (float)numTicks; // 0..1
            GameObject tk = Instantiate(tickPrefab, ticksContainer);
            RectTransform rt = tk.GetComponent<RectTransform>();
            // anchors para que ocupe el ancho del contenedor
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            float y = t * height;
            rt.anchoredPosition = new Vector2(0f, y);
            // opcional: agrandar ticks mayores (cada X)
            Image img = tk.GetComponent<Image>();
            if (img != null)
            {
                img.color = tickNormalColor;
            }
        }
    }

    void HighlightNearestTick(float currentVolume)
    {
        if (ticksContainer == null) return;
        float normalized = Mathf.Clamp01(currentVolume / Mathf.Max(0.0001f, maxVolume));
        int nearestIndex = Mathf.RoundToInt(normalized * numTicks);

        for (int i = 0; i < ticksContainer.childCount; i++)
        {
            Transform child = ticksContainer.GetChild(i);
            Image img = child.GetComponent<Image>();
            if (img == null) continue;

            if (i == nearestIndex)
            {
                img.color = tickHighlightColor;
                child.localScale = Vector3.one * tickHighlightScale;
            }
            else
            {
                img.color = tickNormalColor;
                child.localScale = Vector3.one;
            }
        }
    }
    #endregion
}
