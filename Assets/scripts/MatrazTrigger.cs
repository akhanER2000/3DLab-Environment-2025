using UnityEngine;
using System.Collections.Generic;

public class MatrazTrigger : MonoBehaviour
{
    private MatrazRecibidor matrazPadre;

    void Start()
    {
        matrazPadre = GetComponentInParent<MatrazRecibidor>();
        if (matrazPadre == null)
            Debug.LogError($"{name}: No se encontró MatrazRecibidor en el padre.");
    }

    // Se llama cuando partículas chocan con este collider
    void OnParticleCollision(GameObject other)
    {
        // Verificamos si el emisor de partículas tiene un SistemaDeParticulasMensajero
        SistemaDeParticulasMensajero messenger = other.GetComponent<SistemaDeParticulasMensajero>();
        if (messenger != null)
        {
            string mensaje = messenger.GetMensaje();
            if (!string.IsNullOrEmpty(mensaje))
            {
                matrazPadre.RecibirMensaje(mensaje);
                Debug.Log($"[MatrazTrigger] El matraz recibió el mensaje '{mensaje}' desde partículas ({other.name}).");
            }
        }
    }
}
