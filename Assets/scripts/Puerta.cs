using UnityEngine;
using System.Collections.Generic;

public class Puerta : MonoBehaviour
{
    [Header("Configuración de mensajes")]
    public List<string> mensajesEsperados; // Lista de mensajes que deben llegar
    private HashSet<string> mensajesRecibidos = new HashSet<string>();

    [Header("Transformaciones a aplicar")]
    public Vector3 nuevaPosicion;
    public Vector3 nuevaRotacion;
    public Vector3 nuevaEscala;

    [Header("Receptor adicional (Advertencia)")]
    public Advertencia advertenciaReceptor; // Arrastra aquí el panel de advertencia
    public string mensajeParaAdvertencia = "activarAdvertencia"; // Mensaje que se enviará

    private bool accionEjecutada = false;

    public void RecibirMensaje(string mensaje)
    {
        // Guardamos el mensaje recibido si está en la lista esperada
        if (mensajesEsperados.Contains(mensaje))
        {
            mensajesRecibidos.Add(mensaje);
        }

        // Si ya tenemos todos los mensajes esperados, ejecutamos la acción
        if (!accionEjecutada && mensajesRecibidos.Count == mensajesEsperados.Count)
        {
            EjecutarAccion();
            accionEjecutada = true; // Para que no se repita

            // Enviar mensaje al panel de Advertencia
            if (advertenciaReceptor != null)
            {
                advertenciaReceptor.RecibirMensaje(mensajeParaAdvertencia);
            }
        }
    }

    private void EjecutarAccion()
    {
        // Aplicar modificaciones al transform
        transform.localPosition = nuevaPosicion;
        transform.localEulerAngles = nuevaRotacion;
        transform.localScale = nuevaEscala;

        Debug.Log("¡Todos los mensajes recibidos! Acción ejecutada en la puerta.");
    }
}