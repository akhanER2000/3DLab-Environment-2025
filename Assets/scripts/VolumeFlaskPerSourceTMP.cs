using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class VolumeFlaskPerSourceTMP : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text tmpText;

    [Tooltip("Asigna el Canvas (World Space) del matraz para ocultarlo/mostrarlo completo. Si lo dejas vacío, se auto-detecta desde el TMP.")]
    public GameObject canvasRoot;

    [Header("Metas (mL)")]
    public int targetVasoML = 20;
    public int targetPisetaML = 230;

    [Header("Incremento")]
    public int stepML = 5;
    public float tickIntervalSeconds = 0.10f;

    [Header("Eventos (opcional)")]
    public UnityEvent onFull; // se dispara al completar el total

    // Estado interno
    private int currentVasoML = 0;
    private int currentPisetaML = 0;
    private float _lastTickVaso = -999f;
    private float _lastTickPiseta = -999f;

    private int TargetTotal => targetVasoML + targetPisetaML;
    private int CurrentTotal => currentVasoML + currentPisetaML;

    private bool _uiVisible = true;

    private void Awake()
    {
        if (canvasRoot == null && tmpText != null)
        {
            var canvas = tmpText.GetComponentInParent<Canvas>(true);
            canvasRoot = canvas != null ? canvas.gameObject : tmpText.gameObject;
        }
    }

    private void Start()
    {
        UpdateUI();
        SetUIVisible(CurrentTotal > 0); // oculto al inicio si está en 0
    }

    /// <summary>
    /// Llamado por los emisores. fromVaso=true si es el vaso; false si es la piseta.
    /// </summary>
    public void AddFromSource(bool fromVaso)
    {
        if (CurrentTotal >= TargetTotal) return;

        float now = Time.time;

        if (fromVaso)
        {
            if (currentVasoML >= targetVasoML) return;
            if (now - _lastTickVaso >= tickIntervalSeconds)
            {
                int add = Mathf.Min(stepML, targetVasoML - currentVasoML);
                currentVasoML += add;
                _lastTickVaso = now;
                UpdateUI();
                CheckFull();
            }
        }
        else
        {
            if (currentPisetaML >= targetPisetaML) return;
            if (now - _lastTickPiseta >= tickIntervalSeconds)
            {
                int add = Mathf.Min(stepML, targetPisetaML - currentPisetaML);
                currentPisetaML += add;
                _lastTickPiseta = now;
                UpdateUI();
                CheckFull();
            }
        }
    }

    /// <summary>
    /// Resta 'ml' del matraz (en enteros). Devuelve lo efectivamente restado.
    /// Descuenta primero de Piseta y luego de Vaso (puedes invertir el orden si quieres).
    /// </summary>
    public int ConsumeMl(int ml)
    {
        int toRemove = Mathf.Max(0, ml);
        if (toRemove == 0) return 0;

        int removed = 0;

        int fromPiseta = Mathf.Min(toRemove, currentPisetaML);
        currentPisetaML -= fromPiseta;
        removed += fromPiseta;
        toRemove -= fromPiseta;

        if (toRemove > 0)
        {
            int fromVaso = Mathf.Min(toRemove, currentVasoML);
            currentVasoML -= fromVaso;
            removed += fromVaso;
            toRemove -= fromVaso;
        }

        if (removed > 0)
            UpdateUI(); // también gestiona visibilidad del Canvas

        return removed;
    }

    /// <summary>Versión flotante (redondea a entero mL).</summary>
    public int ConsumeMlFloat(float ml)
    {
        int intMl = Mathf.RoundToInt(ml);
        return ConsumeMl(intMl);
    }

    private void UpdateUI()
    {
        if (tmpText != null)
        {
            tmpText.text = $"Volumen: {CurrentTotal} / {TargetTotal} mL";
            tmpText.color = new Color(0.388f, 0.160f, 0.619f); // #63299E
        }
        SetUIVisible(CurrentTotal > 0);
    }

    private void SetUIVisible(bool visible)
    {
        if (_uiVisible == visible) return;
        _uiVisible = visible;

        if (canvasRoot != null) canvasRoot.SetActive(visible);
        else if (tmpText != null) tmpText.gameObject.SetActive(visible);
    }

    private void CheckFull()
    {
        if (CurrentTotal >= TargetTotal)
            onFull?.Invoke();
    }

    // Reset manual (si quieres reiniciar práctica)
    public void ResetVolumes()
    {
        currentVasoML = 0;
        currentPisetaML = 0;
        _lastTickVaso = _lastTickPiseta = -999f;
        UpdateUI(); // ocultará el Canvas por estar en 0
    }

    public int GetCurrentTotal() => CurrentTotal;
}