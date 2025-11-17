using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro; // Añadido para poder usar el texto de debug

[DisallowMultipleComponent]
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor))]
public class LaserWhiteboardEraser : MonoBehaviour
{
    [Header("Filtros")]
    public LayerMask whiteboardLayer;

    [Header("Pincel")]
    public Color brushColor = Color.black;
    [Range(1, 64)] public int brushRadius = 6;

    [Header("Entrada")]
    public bool requireActivatePress = false;

    [Header("Debug (Opcional)")]
    public TextMeshProUGUI debugText; // Arrastra tu objeto de texto aquí

    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor ray;
    private Vector2? lastUV;

    void Awake()
    {
        ray = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();
    }

    void Update()
    {
        // --- CÓDIGO DE DEBUG AÑADIDO ---
        bool isActivatePressed = false;
        var ctrl = ray.GetComponentInParent<XRBaseController>(); // Usar GetComponentInParent es más seguro
        if (ctrl != null)
        {
            isActivatePressed = ctrl.activateInteractionState.active;
        }

        if (debugText != null)
        {
            debugText.text = $"Gatillo presionado: {isActivatePressed}\n";
        }
        // --- FIN CÓDIGO DE DEBUG ---

        if (requireActivatePress && !isActivatePressed)
        {
            lastUV = null;
            if (debugText != null) debugText.text += "Estado: Input NO activo.";
            return;
        }

        if (ray.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            if (debugText != null) debugText.text += $"Impacto: {hit.collider.name}\n";

            if (((1 << hit.collider.gameObject.layer) & whiteboardLayer.value) == 0)
            {
                lastUV = null;
                if (debugText != null) debugText.text += "Error: Layer incorrecta.";
                return;
            }

            var board = hit.collider.GetComponentInParent<Whiteboard>();
            if (board == null)
            {
                lastUV = null;
                if (debugText != null) debugText.text += "Error: No se encontró script 'Whiteboard'.";
                return;
            }

            Vector2 uv;
            bool haveUV = false;

            if (hit.collider is MeshCollider)
            {
                uv = hit.textureCoord;
                haveUV = true;
            }
            else
            {
                haveUV = board.TryWorldToUV(hit.point, out uv);
            }

            if (!haveUV)
            {
                lastUV = null;
                if (debugText != null) debugText.text += "Error: No se pudo obtener UV.";
                return;
            }

            if (debugText != null) debugText.text += $"Estado: Pintando en UV {uv}";

            uv = new Vector2(Mathf.Clamp01(uv.x), Mathf.Clamp01(uv.y));

            if (lastUV.HasValue)
            {
                // En lugar de brushColor, usamos el color de fondo de la propia pizarra.
                board.PaintLine(lastUV.Value, uv, board.clearColor, brushRadius);
            }
            else
            {
                board.PaintUV(uv, board.clearColor, brushRadius);
            }

            lastUV = uv;
        }
        else
        {
            lastUV = null;
            if (debugText != null) debugText.text += "Estado: El láser no impacta nada.";
        }
    }
}