using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CableVisual : MonoBehaviour
{
    [Header("Extremos del cable")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Identificadores (para prefabs)")]
    public string startObjectName; // Ej: "Enchufe_Connector"
    public string endObjectName;   // Ej: "PHMetro_Input"

    [Header("Curvatura visual")]
    [Range(0f, 1f)] public float sagAmount = 0.2f;
    public int segments = 20;

    private LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = segments;
        line.useWorldSpace = true;
    }

    void Start()
    {
        // Si está vacío, buscar por nombre (útil para prefabs)
        if (startPoint == null && !string.IsNullOrEmpty(startObjectName))
        {
            GameObject found = GameObject.Find(startObjectName);
            if (found) startPoint = found.transform;
        }

        if (endPoint == null && !string.IsNullOrEmpty(endObjectName))
        {
            GameObject found = GameObject.Find(endObjectName);
            if (found) endPoint = found.transform;
        }
    }

    void LateUpdate()
    {
        if (startPoint == null || endPoint == null) return;

        Vector3 p0 = startPoint.position;
        Vector3 p1 = endPoint.position;
        Vector3 mid = (p0 + p1) / 2;
        mid.y -= sagAmount * Vector3.Distance(p0, p1);

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            Vector3 a = Vector3.Lerp(p0, mid, t);
            Vector3 b = Vector3.Lerp(mid, p1, t);
            Vector3 point = Vector3.Lerp(a, b, t);
            line.SetPosition(i, point);
        }
    }
}
