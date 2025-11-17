using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MensajeColor
{
    public string mensaje;
    public Color color;
}

[System.Serializable]
public class CombinacionMensajesColor
{
    public List<string> mensajes = new List<string>();
    public Color color;
}

public class VasoPrecipitado : MonoBehaviour
{
    [Header("Referencias")]
    public MeshRenderer rendererCilindroAgua;
    public ParticleSystem sistemaParticulas; // Sistema de partículas del vaso

    [Header("Colores por mensaje único")]
    public List<MensajeColor> coloresPorMensaje = new List<MensajeColor>();

    [Header("Colores por combinación de mensajes")]
    public List<CombinacionMensajesColor> coloresPorCombinacion = new List<CombinacionMensajesColor>();

    [Header("Configuración del trigger")]
    public string tagCuchara = "Cuchara"; // Tag que debe tener la cuchara para enviar mensaje

    private HashSet<string> mensajesRecibidos = new HashSet<string>();
    private Material materialAgua;

    void Start()
    {
        materialAgua = rendererCilindroAgua.material;
    }

    // Este método se llamará desde el trigger del vaso
    public void RecibirMensajeDeColision(string mensaje)
    {
        if (string.IsNullOrEmpty(mensaje)) return;

        mensajesRecibidos.Add(mensaje);
        Debug.Log($"{name} recibió el mensaje: {mensaje}");

        // Guardar el mensaje en el sistema de partículas pero NO activarlo aún
        if (sistemaParticulas != null)
        {
            var messenger = sistemaParticulas.GetComponent<SistemaDeParticulasMensajero>();
            if (messenger != null)
            {
                messenger.SetMensaje(mensaje);
                Debug.Log($"Mensaje '{mensaje}' guardado en el sistema de partículas del vaso, listo para usarse.");
            }
        }

        // Cambiar color según lógica
        RevisarMensajes(mensaje);
    }

    private void RevisarMensajes(string mensaje)
    {
        // 1. Revisar combinaciones
        foreach (var combinacion in coloresPorCombinacion)
        {
            bool todosPresentes = true;
            foreach (var msg in combinacion.mensajes)
            {
                if (!mensajesRecibidos.Contains(msg))
                {
                    todosPresentes = false;
                    break;
                }
            }

            if (todosPresentes)
            {
                CambiarColor(combinacion.color);
                return;
            }
        }

        // 2. Revisar mensajes individuales
        foreach (var entry in coloresPorMensaje)
        {
            if (entry.mensaje == mensaje)
            {
                CambiarColor(entry.color);
                return;
            }
        }
    }

    private void CambiarColor(Color nuevoColor) 
    { 
        materialAgua.SetColor("_frontcolor", nuevoColor); 
        materialAgua.SetColor("_blackcolor", nuevoColor); 
        Debug.Log($"{name} cambió de color a {nuevoColor}"); 
    }

    public void ActivarParticulas()
    {
        if (sistemaParticulas != null)
        {
            sistemaParticulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            sistemaParticulas.Play();
            Debug.Log("Sistema de partículas del vaso precipitado activado manualmente.");
        }
    }

    // 🔹 Nuevo: trigger para detectar la cuchara
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagCuchara))
        {
            // Intentamos obtener el mensaje preparado de la cuchara
            var cuchara = other.GetComponent<ControlCuchara>();
            if (cuchara != null)
            {
                string mensaje = cuchara.GetMensajePreparado();
                if (!string.IsNullOrEmpty(mensaje))
                {
                    RecibirMensajeDeColision(mensaje);
                }
            }
        }
    }
}