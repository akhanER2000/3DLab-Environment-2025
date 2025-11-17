//frab+
//esta se le pone al canvas de la interfaz

using UnityEngine;
using TMPro;

public class PhMeterUIController : MonoBehaviour
{
    [Header("Referencias UI")]
    public TMP_Text texto_pH;
    public TMP_Text texto_temp;
    public TMP_Text texto_mjs;

    private bool protectiveRemoved = false;

    private void Start()
    {
        texto_pH.text = "--";
        texto_temp.text = "-- °C";
        texto_mjs.text = "Remove protective";
    }

    // --- INICIO DE MODIFICACIONES ---

    // 1. Modificamos 'RemoveProtective' para que acepte el estado del electrodo
    public void RemoveProtective(ElectrodeInteraction.EstadoElectrodo estado)
    {
        protectiveRemoved = true;

        // Muestra un mensaje diferente basado en el estado
        if (estado == ElectrodeInteraction.EstadoElectrodo.Sucio)
        {
            MostrarMensajeLavar();
        }
        else // (estado == Limpio)
        {
            MostrarMensajeLimpio();
        }
    }

    public void ResetProtective()
    {
        protectiveRemoved = false;
        texto_pH.text = "--";
        texto_temp.text = "-- °C";
        texto_mjs.text = "Remove protective";
    }

    // Nuevo método para acceder al estado desde otro script
    public bool IsProtectiveRemoved()
    {
        return protectiveRemoved;
    }

    // 2. Modificamos 'UpdateReadings'
    public void UpdateReadings(float ph, float temp)
    {
        texto_pH.text = ph.ToString("F2");
        texto_temp.text = temp.ToString("F1") + " °C";

        // El mensaje "Cal due" se elimina.
        // Ahora el mensaje se controla por 'MostrarMensajePostLectura'
        // llamado desde InteraccionElectrodo.cs
    }

    // 3. AÑADIMOS NUEVOS MÉTODOS DE MENSAJES
    // Estos métodos son llamados por InteraccionElectrodo.cs

    /// <summary>
    /// Muestra el mensaje para lavar el electrodo (cuando está sucio)
    /// ¡Esta es la lógica de "Bloqueo de Interfaz"!
    /// </summary>
    public void MostrarMensajeLavar()
    {
        texto_pH.text = "--"; // Bloquea la lectura de pH
        texto_temp.text = "-- °C"; // Bloquea la lectura de Temp
        texto_mjs.text = "Lavar electrodo"; // Mensaje clave de la HU
    }

    /// <summary>
    /// Muestra el mensaje de que está listo para medir (cuando está limpio)
    /// </summary>
    public void MostrarMensajeLimpio()
    {
        // No reseteamos los valores de pH/Temp aquí,
        // por si el usuario lo limpió y solo lo está moviendo.
        texto_mjs.text = "Listo para medir";
    }

    /// <summary>
    /// Muestra el mensaje justo después de una lectura exitosa
    /// </summary>
    public void MostrarMensajePostLectura()
    {
        // Este mensaje le indica que para la *siguiente* medición, debe lavar.
        texto_mjs.text = "Lavar de nuevo";
    }

    // --- FIN DE MODIFICACIONES ---
}