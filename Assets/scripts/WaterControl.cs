using System.Collections;
using UnityEngine;

public class ControlAgua : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Arrastra aquí el objeto 'cilindro' que representa el agua.")]
    public MeshRenderer rendererCilindroAgua;

    [Header("Configuración de Color")]
    public Color colorInicial = Color.cyan;
    public Color colorFinal = new Color(1, 1, 0, 0.5f); // Amarillo semitransparente

    [Header("Configuración de Reacción")]
    public float tiempoDeCambio = 2.0f;
    public AudioClip sonidoReaccion;

    [Header("Precipitado")]
    [Tooltip("Arrastra aquí el sistema de partículas del precipitado.")]
    public ParticleSystem particulasPrecipitado;

    private Material materialAgua;
    private bool reaccionOcurrida = false;

    void Start()
    {
        // Creamos una instancia del material para evitar modificar el material compartido.
        materialAgua = rendererCilindroAgua.material;
        materialAgua.SetColor("_frontcolor", colorInicial); // Asignamos el color inicial al frente.
    }

    public void AnadirCompuesto()
    {
        if (reaccionOcurrida) return;

        reaccionOcurrida = true;

        if (sonidoReaccion != null)
            AudioSource.PlayClipAtPoint(sonidoReaccion, transform.position);

        StartCoroutine(CambiarColorSuavemente());
    }

    private IEnumerator CambiarColorSuavemente()
    {
        float tiempoPasado = 0;

        while (tiempoPasado < tiempoDeCambio)
        {
            // Interpolamos los colores para las propiedades específicas del shader.
            Color nuevoColor = Color.Lerp(colorInicial, colorFinal, tiempoPasado / tiempoDeCambio);
            materialAgua.SetColor("_frontcolor", nuevoColor);
            materialAgua.SetColor("_blackcolor", nuevoColor); // Asumimos que ambas partes del cilindro cambian.

            tiempoPasado += Time.deltaTime;
            yield return null;
        }

        // Aseguramos que el color final sea exacto.
        materialAgua.SetColor("_frontcolor", colorFinal);
        materialAgua.SetColor("_blackcolor", colorFinal);

        Debug.Log("La reacción del agua ha finalizado.");

        // 🔹 Activar precipitado
        if (particulasPrecipitado != null)
        {
            particulasPrecipitado.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // Reinicia por si estaba corriendo
            particulasPrecipitado.Play();
            Debug.Log("Precipitado activado.");
        }
    }
}
