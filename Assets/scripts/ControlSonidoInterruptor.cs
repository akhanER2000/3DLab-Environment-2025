using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// Ya no es necesario que el propio interruptor tenga un AudioSource.
// Así que eliminamos la línea [RequireComponent(typeof(AudioSource))]

public class ControlSonidoInterruptor : MonoBehaviour
{
    // Hacemos esta variable pública para poder arrastrar nuestro 
    // emisor de sonido desde el editor de Unity.
    public AudioSource fuenteDeSonidoExterna;
    
    // El booleano para saber si está "encendido" o "apagado".
    private bool estaActivado = false;

    void Start()
    {
        // Al iniciar, nos aseguramos de que el sonido esté apagado
        // y de que no nos hayamos olvidado de conectar la fuente de sonido.
        if (fuenteDeSonidoExterna != null)
        {
            fuenteDeSonidoExterna.Stop();
        }
        else
        {
            Debug.LogError("¡ERROR! No se ha asignado una fuente de sonido al interruptor '" + this.gameObject.name + "'.");
        }
    }

    // Esta función pública la llamamos desde el componente de interacción VR.
    public void TocarInterruptor()
    {
        // Si no hay fuente de sonido asignada, no hacemos nada para evitar errores.
        if (fuenteDeSonidoExterna == null) return;

        // Invertimos el estado actual.
        estaActivado = !estaActivado;

        if (estaActivado)
        {
            // Si el estado ahora es ACTIVADO, le ordenamos a la fuente externa que suene.
            if (!fuenteDeSonidoExterna.isPlaying)
            {
                fuenteDeSonidoExterna.Play();
                Debug.Log("Sonido externo INICIADO.");
            }
        }
        else
        {
            // Si el estado ahora es DESACTIVADO, le ordenamos que se detenga.
            if (fuenteDeSonidoExterna.isPlaying)
            {
                fuenteDeSonidoExterna.Stop();
                Debug.Log("Sonido externo DETENIDO.");
            }
        }
    }
}