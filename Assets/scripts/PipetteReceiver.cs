using UnityEngine;

public class PipetteReceiver : MonoBehaviour
{
    private string mensajeGuardado;
    private PipetteParticleMessenger messenger;

    void Start()
    {
        // Buscar el PipetteParticleMessenger en los hijos (sistema de partículas)
        messenger = GetComponentInChildren<PipetteParticleMessenger>();
        if (messenger == null)
            Debug.LogError("[PipetteReceiver] No se encontró PipetteParticleMessenger en los hijos.");
    }

    // Llamado por el trigger cuando la punta de la pipeta entra al matraz
    public void RecibirMensajeDesdeMatraz(string mensaje)
    {
        mensajeGuardado = mensaje;
        Debug.Log($"[PipetteReceiver] Pipette guardó el mensaje del matraz: {mensaje}");

        // Enviar el mensaje al messenger para que las partículas lo transmitan
        if (messenger != null)
        {
            messenger.SetMensaje(mensaje);
        }
    }
}