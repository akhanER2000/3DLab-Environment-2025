using UnityEngine;

public class WaterControl : MonoBehaviour
{
    [Header("Referencias")]
    public Renderer cylinderRenderer;
    public ParticleSystem waterParticles;

    [Header("Configuración de relleno")]
    public float fillMin = -4.78f;
    public float fillMax = 4.58f;
    public float fillIncrement = 0.01f;
    public float decreaseRate = 0.05f;
    private Material cylinderMaterial;
    private float fillValue;
    private bool isPouring = false;

    [Header("Filtros")]
    public string[] ignoreTags;

    // 👇 NUEVA VARIABLE AJUSTABLE EN INSPECTOR
    [Header("Configuración de Vertido")]
    [SerializeField]
    [Range(0, 90)] // Slider de 0 a 90 grados en el Inspector
    [Tooltip("Ángulo (grados desde la vertical) a partir del cual comienza el vertido.")]
    private float anguloUmbralVertido = 60.0f; // Puedes cambiar el valor por defecto aquí

    void Start()
    {
        if (cylinderRenderer != null)
        {
            cylinderMaterial = cylinderRenderer.material; // No es necesario instanciar si no cambias propiedades únicas por objeto en Start
            fillValue = cylinderMaterial.GetFloat("_fill");
        }
        else
        {
            Debug.LogError("Renderer del cilindro no asignado en " + gameObject.name);
        }

        // Asegurarse de que las partículas estén detenidas al inicio
        if (waterParticles != null) waterParticles.Stop();
    }

    void Update()
    {
        if (cylinderMaterial == null) return;

        // --- LÓGICA DE VERTIDO CORREGIDA Y AJUSTABLE ---
        // Calcula cuánto está inclinado el objeto respecto a la vertical (0 = vertical, 90 = horizontal)
        float anguloInclinacionActual = Vector3.Angle(transform.up, Vector3.up);

        // Comprueba si la inclinación supera el umbral que definiste en el Inspector
        bool deberiaVerter = anguloInclinacionActual > anguloUmbralVertido;

        // Inicia o detiene el vertido según corresponda
        if (deberiaVerter && !isPouring)
        {
            StartPouring();
        }
        else if (!deberiaVerter && isPouring)
        {
            StopPouring();
        }
        // --- FIN LÓGICA DE VERTIDO ---


        if (isPouring && fillValue > fillMin)
        {
            DecreaseFill(decreaseRate * Time.deltaTime);
        }
    }

    void OnParticleCollision(GameObject other)
    {
        foreach (string tag in ignoreTags)
        {
            if (other.CompareTag(tag))
                return;
        }

        if (cylinderMaterial != null)
        {
            fillValue = Mathf.Clamp(fillValue + fillIncrement, fillMin, fillMax);
            cylinderMaterial.SetFloat("_fill", fillValue);

            ParticleSystem particulasEntrada = other.GetComponent<ParticleSystem>();
            if (particulasEntrada != null)
            {
                Color colorParticula = particulasEntrada.main.startColor.color;
                if (cylinderMaterial.HasProperty("_frontcolor"))
                {
                    cylinderMaterial.SetColor("_frontcolor", colorParticula);
                }
                // Podrías necesitar actualizar también _blackcolor si usas ambos
                if (cylinderMaterial.HasProperty("_blackcolor"))
                {
                    cylinderMaterial.SetColor("_blackcolor", colorParticula);
                }
            }
        }
    }

    void StartPouring()
    {
        isPouring = true;
        if (waterParticles != null) waterParticles.Play();
        Debug.Log($"Iniciando vertido - Ángulo: {Vector3.Angle(transform.up, Vector3.up)} > {anguloUmbralVertido}");
    }

    void StopPouring()
    {
        isPouring = false;
        if (waterParticles != null) waterParticles.Stop();
        Debug.Log($"Deteniendo vertido - Ángulo: {Vector3.Angle(transform.up, Vector3.up)} <= {anguloUmbralVertido}");
    }

    public float GetFillValue()
    {
        return fillValue;
    }

    public void DecreaseFill(float amount)
    {
        fillValue = Mathf.Clamp(fillValue - amount, fillMin, fillMax);
        if (cylinderMaterial != null) // Añadir check por si acaso
        {
            cylinderMaterial.SetFloat("_fill", fillValue);
        }
    }
}