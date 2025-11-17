using UnityEngine;

public class PipetteTrigger : MonoBehaviour
{
    private PipetteReceiver pipetteReceiver;
    public string matrazTag = "MatrazTrigger"; // Tag del box trigger del matraz

    void Start()
    {
        // Buscamos el componente PipetteReceiver en el padre
        pipetteReceiver = GetComponentInParent<PipetteReceiver>();
        if (pipetteReceiver == null)
            Debug.LogError($"{name}: No se encontró PipetteReceiver en el padre.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(matrazTag))
        {
            // Tomamos el MatrazRecibidor del padre del trigger del matraz
            MatrazRecibidor matraz = other.GetComponentInParent<MatrazRecibidor>();
            if (matraz != null)
            {
                string mensaje = matraz.GetMensajeActual();
                pipetteReceiver.RecibirMensajeDesdeMatraz(mensaje);
                Debug.Log($"[PipetteTrigger] La pipette recibió el mensaje '{mensaje}' del matraz ({other.name}).");
            }
        }
    }
}