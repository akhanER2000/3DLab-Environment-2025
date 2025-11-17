using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VasoReceptor : MonoBehaviour
{
    [Header("Referencias")]
    public MeshRenderer rendererCilindroAgua;

    [Header("Colores")]
    public Color colorInicial = Color.cyan;
    public Color colorFinal = Color.yellow;

    [Header("Mensajes esperados")]
    public List<string> mensajesEsperados = new List<string>(); // Mensajes que debe recibir
    private HashSet<string> mensajesRecibidos = new HashSet<string>(); // Mensajes ya recibidos

    [Header("Configuración de Transición")]
    public float tiempoDeCambio = 2.0f; // Tiempo para la transición de color

    [Header("Precipitado")]
    [Tooltip("Arrastra aquí el sistema de partículas del precipitado.")]
    public ParticleSystem particulasPrecipitado;

    private Material materialAgua;

    void Start()
    {
        // Crear una instancia del material
        materialAgua = rendererCilindroAgua.material;
        materialAgua.SetColor("_frontcolor", colorInicial);
        materialAgua.SetColor("_blackcolor", colorInicial);
    }

    public void RecibirMensaje(string mensaje)
    {
        // Registrar el mensaje recibido
        if (mensajesEsperados.Contains(mensaje))
        {
            mensajesRecibidos.Add(mensaje);
            Debug.Log($"Mensaje recibido: {mensaje}");

            // Verificar si se han recibido todos los mensajes
            if (mensajesRecibidos.Count == mensajesEsperados.Count)
            {
                StartCoroutine(CambiarColorSuavemente()); // Usamos la transición suave
            }
        }
    }

    private IEnumerator CambiarColorSuavemente()
    {
        Debug.Log("Todos los mensajes recibidos. Cambiando color suavemente.");

        float tiempoPasado = 0;

        while (tiempoPasado < tiempoDeCambio)
        {
            // Interpolamos los colores
            Color nuevoColor = Color.Lerp(colorInicial, colorFinal, tiempoPasado / tiempoDeCambio);
            materialAgua.SetColor("_frontcolor", nuevoColor);
            materialAgua.SetColor("_blackcolor", nuevoColor);

            tiempoPasado += Time.deltaTime;
            yield return null; // Esperar al siguiente frame
        }

        // Aseguramos que el color final sea exacto al terminar la interpolación
        materialAgua.SetColor("_frontcolor", colorFinal);
        materialAgua.SetColor("_blackcolor", colorFinal);

        Debug.Log("La transición de color ha finalizado.");

        // 🔹 Activar precipitado
        if (particulasPrecipitado != null)
        {
            particulasPrecipitado.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // Reinicia por si estaba corriendo
            particulasPrecipitado.Play();
            Debug.Log("Precipitado activado.");
        }
    }
}
