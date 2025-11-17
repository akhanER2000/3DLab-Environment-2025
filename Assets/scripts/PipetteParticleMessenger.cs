using UnityEngine;

public class PipetteParticleMessenger : MonoBehaviour
{
    [HideInInspector]
    public string mensajeActual = "";

    // Este método es llamado por PipetteReceiver
    public void SetMensaje(string mensaje)
    {
        mensajeActual = mensaje;
        Debug.Log($"[PipetteParticleMessenger] Mensaje guardado: {mensaje}");
    }

    void OnParticleCollision(GameObject other)
    {
        if (string.IsNullOrEmpty(mensajeActual)) return;

        // Revisar si el objeto colisionado tiene un VasoReceptor
        var receptor = other.GetComponent<VasoReceptor>();
        if (receptor != null)
        {
            receptor.RecibirMensaje(mensajeActual);
            Debug.Log($"[PipetteParticleMessenger] Mensaje '{mensajeActual}' enviado al {other.name}");
        }
    }
}