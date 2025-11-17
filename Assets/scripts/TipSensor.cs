using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class TipSensor : MonoBehaviour
{
    [Header("Filtro opcional (mejor rendimiento)")]
    [Tooltip("Solo considerará colliders en estas capas como candidatos")]
    public LayerMask grabPointsMask = ~0;

    [Tooltip("Si se define, solo considerará colliders con este Tag (deja vacío para no filtrar)")]
    public string puntoTagOpcional = "";

    private readonly List<AutoGrabPoint> candidatos = new List<AutoGrabPoint>();

    /// <summary> Devuelve el AutoGrabPoint más cercano actualmente dentro del trigger. </summary>
    public AutoGrabPoint GetClosestCandidate(Vector3 reference)
    {
        AutoGrabPoint mejor = null;
        float mejorDist2 = float.MaxValue;

        // Iteramos la lista (normalmente muy pequeña)
        for (int i = candidatos.Count - 1; i >= 0; i--)
        {
            var agp = candidatos[i];
            if (agp == null || agp.IsGrabbed)
            {
                candidatos.RemoveAt(i);
                continue;
            }

            float d2 = (agp.transform.position - reference).sqrMagnitude;
            if (d2 < mejorDist2) { mejorDist2 = d2; mejor = agp; }
        }
        return mejor;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!LayerMatch(other.gameObject.layer)) return;
        if (!TagMatch(other)) return;

        var agp = other.GetComponentInParent<AutoGrabPoint>();
        if (agp != null && !agp.IsGrabbed && !candidatos.Contains(agp))
            candidatos.Add(agp);
    }

    private void OnTriggerExit(Collider other)
    {
        var agp = other.GetComponentInParent<AutoGrabPoint>();
        if (agp != null)
            candidatos.Remove(agp);
    }

    private bool LayerMatch(int layer)
    {
        // Si el mask es Everything, pasa todo
        return (grabPointsMask.value & (1 << layer)) != 0;
    }

    private bool TagMatch(Collider other)
    {
        if (string.IsNullOrEmpty(puntoTagOpcional)) return true;
        return other.CompareTag(puntoTagOpcional);
    }
}