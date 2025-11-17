using UnityEngine;

public class PipetteAspirateToFlask : MonoBehaviour
{
    [Header("Referencias")]
    public MonoBehaviour pipetteControllerMono; // arrastra aquí tu PipetteController (como MonoBehaviour)
    public Transform pipetteTip;                // punta física (un Empty en la boquilla)
    public LayerMask flaskTipLayers;            // capa de los colliders de la punta del matraz (ej. FlaskTip)

    [Header("Detección")]
    public float detectRadius = 0.02f;          // radio para buscar la boca del matraz

    // cache de reflejo al PipetteController
    private System.Reflection.FieldInfo _currentVolumeField;
    private float _lastVolume = 0f;

    private void Awake()
    {
        // intentar cachear el campo 'currentVolume' del PipetteController sin acoplar dependencias
        if (pipetteControllerMono != null)
        {
            var t = pipetteControllerMono.GetType();
            _currentVolumeField = t.GetField("currentVolume");
            if (_currentVolumeField == null)
            {
                Debug.LogWarning($"[PipetteAspirateToFlask] No encontré 'currentVolume' en {t.Name}. Asigna el componente correcto.");
            }
        }
    }

    private void Reset()
    {
        if (pipetteTip == null)
        {
            var tip = transform.Find("Tip");
            if (tip != null) pipetteTip = tip;
        }
    }

    private void Update()
    {
        if (pipetteControllerMono == null || _currentVolumeField == null || pipetteTip == null) return;

        // leer volumen actual de la pipeta (asumido en mL)
        float current = 0f;
        object boxed = _currentVolumeField.GetValue(pipetteControllerMono);
        if (boxed is float f) current = Mathf.Max(0f, f);

        float delta = current - _lastVolume;

        // Aspiración => volumen de pipeta sube => restar del matraz
        if (delta > 0.001f)
        {
            VolumeFlaskPerSourceTMP targetFlask = FindNearestFlaskAtTip();
            if (targetFlask != null)
            {
                targetFlask.ConsumeMlFloat(delta); // redondea a entero dentro del método
            }
        }

        _lastVolume = current;
    }

    private VolumeFlaskPerSourceTMP FindNearestFlaskAtTip()
    {
        Collider[] hits = Physics.OverlapSphere(
            pipetteTip.position,
            detectRadius,
            flaskTipLayers,
            QueryTriggerInteraction.Ignore
        );

        VolumeFlaskPerSourceTMP bestFlask = null;
        float best = float.MaxValue;

        foreach (var h in hits)
        {
            var f = h.GetComponentInParent<VolumeFlaskPerSourceTMP>();
            if (f == null) continue;

            float d = (h.ClosestPoint(pipetteTip.position) - pipetteTip.position).sqrMagnitude;
            if (d < best)
            {
                best = d;
                bestFlask = f;
            }
        }
        return bestFlask;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (pipetteTip == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(pipetteTip.position, detectRadius);
    }
#endif
}