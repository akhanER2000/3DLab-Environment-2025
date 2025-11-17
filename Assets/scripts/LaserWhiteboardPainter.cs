using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

[DisallowMultipleComponent]
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor))]
public class LaserWhiteboardPainter : MonoBehaviour
{
    [Header("Filtros")]
    public LayerMask whiteboardLayer;

    [Header("Pincel")]
    public Color brushColor = Color.black;
    [Range(1, 64)] public int brushRadius = 6;

    [Header("Amortiguador (Smoothing)")]
    [Range(1f, 60f)] public float smoothingFactor = 20f;

    [Header("Entrada")]
    public bool requireActivatePress = false;

    [Header("Debug (Opcional)")]
    public TextMeshProUGUI debugText;

    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor ray;
    private Vector2? lastUV;
    
    private Vector2 smoothedUV;
    private bool isDrawing = false;

    void Awake()
    {
        ray = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();
    }

    void Update()
    {
        bool isActivatePressed = false;
        var ctrl = ray.GetComponentInParent<XRBaseController>();
        if (ctrl != null)
        {
            isActivatePressed = ctrl.activateInteractionState.active;
        }

        if (debugText != null)
        {
            debugText.text = $"Gatillo presionado: {isActivatePressed}\n";
        }

        if (requireActivatePress && !isActivatePressed)
        {
            lastUV = null;
            isDrawing = false;
            if (debugText != null) debugText.text += "Estado: Input NO activo.";
            return;
        }

        if (ray.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            if (debugText != null) debugText.text += $"Impacto: {hit.collider.name}\n";

            if (((1 << hit.collider.gameObject.layer) & whiteboardLayer.value) == 0)
            {
                lastUV = null; isDrawing = false;
                if (debugText != null) debugText.text += "Error: Layer incorrecta.";
                return;
            }

            var board = hit.collider.GetComponentInParent<Whiteboard>();
            if (board == null)
            {
                lastUV = null; isDrawing = false;
                if (debugText != null) debugText.text += "Error: No se encontró script 'Whiteboard'.";
                return;
            }

            Vector2 currentUV;
            bool haveUV;

            // --- ESTA ES LA PARTE CORREGIDA ---
            // Volvemos a usar el método original y más preciso para obtener las coordenadas.
            // hit.textureCoord es la forma más fiable si usas un MeshCollider (como en un Plane).
            if (hit.collider is MeshCollider)
            {
                currentUV = hit.textureCoord;
                haveUV = true;
            }
            else
            {
                // El método manual se usa como fallback para otros tipos de colliders.
                haveUV = board.TryWorldToUV(hit.point, out currentUV);
            }
            // --- FIN DE LA CORRECCIÓN ---


            if (!haveUV)
            {
                lastUV = null; isDrawing = false;
                if (debugText != null) debugText.text += "Error: No se pudo obtener UV.";
                return;
            }
            
            if (!isDrawing)
            {
                smoothedUV = currentUV;
                isDrawing = true;
            }
            else
            {
                smoothedUV = Vector2.Lerp(smoothedUV, currentUV, Time.deltaTime * smoothingFactor);
            }

            if (debugText != null) debugText.text += $"Estado: Pintando en UV {smoothedUV}";

            if (lastUV.HasValue)
            {
                board.PaintLine(lastUV.Value, smoothedUV, brushColor, brushRadius);
            }
            else
            {
                board.PaintUV(smoothedUV, brushColor, brushRadius);
            }

            lastUV = smoothedUV;
        }
        else
        {
            lastUV = null;
            isDrawing = false;
            if (debugText != null) debugText.text += "Estado: El láser no impacta nada.";
        }
    }
}