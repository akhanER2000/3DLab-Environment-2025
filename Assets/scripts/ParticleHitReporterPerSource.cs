using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleHitReporterPerSource : MonoBehaviour
{
    [Header("Referencias opcionales (fallback)")]
    [Tooltip("Puedes dejar vacío. Si el autodetect falla, se usará alguno de estos matraces (el que sea padre del tip, o el más cercano).")]
    public List<VolumeFlaskPerSourceTMP> flasks = new List<VolumeFlaskPerSourceTMP>();

    [Header("¿Este emisor es el vaso?")]
    public bool esVaso = false;

    [Header("Filtro por capa del tip (opcional, recomendado)")]
    public LayerMask flaskTipLayers;

    [Header("Autodetectar el matraz desde el tip golpeado")]
    public bool autoDetectFromTip = true;

    private void Reset()
    {
        flasks.Clear();
        flasks.AddRange(FindObjectsOfType<VolumeFlaskPerSourceTMP>());
    }

    private void OnParticleCollision(GameObject other)
    {
        // 1) Filtrar por capa (si configuraste flaskTipLayers)
        if (flaskTipLayers.value != 0)
        {
            int otherMask = 1 << other.layer;
            if ((flaskTipLayers.value & otherMask) == 0)
                return;
        }

        VolumeFlaskPerSourceTMP target = null;

        // 2) Autodetección directa por jerarquía (tip -> padres)
        if (autoDetectFromTip && other != null)
            target = other.GetComponentInParent<VolumeFlaskPerSourceTMP>();

        // 3) Fallback: usar lista de referencias
        if (target == null && flasks != null && flasks.Count > 0)
        {
            // 3a) Preferir el matraz que sea padre del 'other'
            foreach (var f in flasks)
            {
                if (f == null) continue;
                if (other.transform.IsChildOf(f.transform))
                {
                    target = f;
                    break;
                }
            }
            // 3b) O elegir el más cercano al tip golpeado
            if (target == null)
            {
                float bestDist = float.MaxValue;
                Vector3 p = other.transform.position;
                foreach (var f in flasks)
                {
                    if (f == null) continue;
                    float d = (f.transform.position - p).sqrMagnitude;
                    if (d < bestDist)
                    {
                        bestDist = d;
                        target = f;
                    }
                }
            }
        }

        if (target != null)
            target.AddFromSource(esVaso);
        // else: si quieres, Debug.LogWarning para depurar
    }
}