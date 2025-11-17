using UnityEngine;

public class FiltroInteractivo : MonoBehaviour
{
    [Header("Configuración del Filtro")]
    [Tooltip("Arrastra aquí el objeto Mesh Renderer del papel filtro.")]
    public MeshRenderer papelFiltroRenderer;

    [Tooltip("Define qué tan rápido se tiñe el filtro. Un valor más pequeño es más lento.")]
    public float velocidadDeFiltrado = 0.005f;

    [Header("Configuración de Salida")]
    [Tooltip("Arrastra aquí el sistema de partículas de la salida del embudo.")]
    public ParticleSystem particulasSalida;

    [Tooltip("Tiempo en segundos sin recibir partículas antes de detener la salida.")]
    public float tiempoDeEsperaParaDetener = 0.5f;

    [Header("Configuración de Color de Salida")]
    [Tooltip("Qué tan pálido será el líquido de salida (0=mismo color, 1=sin color).")]
    [Range(0f, 1f)]
    public float factorDeClaridad = 0.6f; // 60% más claro

    [Tooltip("Qué tan transparente será el líquido de salida (0=opaco, 1=totalmente transparente).")]
    [Range(0f, 1f)]
    public float factorDeTransparencia = 0.5f; // 50% transparente

    // Variables privadas
    private Material filterMaterialInstance;
    private float progresoActual = 0f;
    private float tiempoSinColision = 0f;
    private bool colorSalidaDefinido = false;
    private static readonly int FiltrationProgress = Shader.PropertyToID("_Filtration_Progress");

    void Start()
    {
        if (papelFiltroRenderer != null)
        {
            filterMaterialInstance = papelFiltroRenderer.material;
            filterMaterialInstance.SetFloat(FiltrationProgress, 0f);
        }
        else
        {
            Debug.LogError("¡No se ha asignado el Mesh Renderer del papel filtro en el Inspector!", this);
        }

        if (particulasSalida != null)
        {
            particulasSalida.Stop();
        }
    }

    void Update()
    {
        // Si las partículas de salida están activas, contamos el tiempo desde la última colisión.
        if (particulasSalida != null && particulasSalida.isPlaying)
        {
            tiempoSinColision += Time.deltaTime;
            // Si pasa demasiado tiempo, detenemos el flujo de salida.
            if (tiempoSinColision > tiempoDeEsperaParaDetener)
            {
                particulasSalida.Stop();
                colorSalidaDefinido = false; // Reseteamos para el próximo vertido.
            }
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        // --- 1. Lógica del teñido del filtro ---
        if (progresoActual < 1.0f)
        {
            progresoActual += velocidadDeFiltrado;
            filterMaterialInstance.SetFloat(FiltrationProgress, Mathf.Clamp01(progresoActual));
        }

        // --- 2. Lógica para definir el color de salida automáticamente ---
        if (!colorSalidaDefinido && particulasSalida != null)
        {
            ParticleSystem particulasEntrada = other.GetComponent<ParticleSystem>();
            if (particulasEntrada != null)
            {
                Color colorEntrada = particulasEntrada.main.startColor.color;

                // Calculamos una versión más pálida y transparente del color
                Color.RGBToHSV(colorEntrada, out float H, out float S, out float V);
                S *= (1 - factorDeClaridad); // Reducimos la saturación
                Color colorSalida = Color.HSVToRGB(H, S, V);
                colorSalida.a = 1 - factorDeTransparencia; // Ajustamos la transparencia

                // Aplicamos el nuevo color a nuestras partículas de salida
                var mainModule = particulasSalida.main;
                mainModule.startColor = colorSalida;

                colorSalidaDefinido = true;
            }
        }

        // --- 3. Lógica para la emisión de partículas de salida ---
        tiempoSinColision = 0f; // Reseteamos el temporizador porque acabamos de recibir una partícula.
        if (particulasSalida != null && !particulasSalida.isPlaying)
        {
            particulasSalida.Play();
        }
    }
}