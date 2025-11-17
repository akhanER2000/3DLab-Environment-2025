using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ControlLuzInterruptor : MonoBehaviour
{
    // Variable pública para arrastrar nuestra luz desde el editor.
    // La hacemos pública para que aparezca en el Inspector de Unity.
    public Light luzAControlar;

    // Variable privada para llevar el registro de si la luz está encendida o apagada.
    private bool estaEncendida = false;

    void Start()
    {
        // Es una buena práctica asegurarse de que la luz esté apagada al empezar.
        // Primero, comprobamos que el usuario no se haya olvidado de asignar la luz.
        if (luzAControlar != null)
        {
            // Apagamos la luz al inicio, asegurando el estado inicial.
            luzAControlar.enabled = false;
        }
        else
        {
            // Si no se asignó ninguna luz, mostramos un error en la consola para facilitar la depuración.
            Debug.LogError("¡ERROR! No se ha asignado una luz al interruptor '" + this.gameObject.name + "'.");
        }
    }

    // Esta función pública la llamaremos desde el componente de interacción VR.
    public void AccionarInterruptor()
    {
        // Si no hay luz asignada, no hacemos nada para evitar errores.
        if (luzAControlar == null) return;

        // Invertimos el estado. Si estaba encendida, se apaga. Si estaba apagada, se enciende.
        estaEncendida = !estaEncendida;

        // Aplicamos el nuevo estado al componente 'Light' de nuestra luz.
        // Activar o desactivar el componente es lo que enciende o apaga la luz.
        luzAControlar.enabled = estaEncendida;

        // Mensajes de consola para saber qué está pasando (opcional pero útil).
        if (estaEncendida)
        {
            Debug.Log("Luz ENCENDIDA.");
        }
        else
        {
            Debug.Log("Luz APAGADA.");
        }
    }
}