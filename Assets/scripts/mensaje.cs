using UnityEngine;

public class SistemaDeParticulas : MonoBehaviour
{
    public string mensaje; // Mensaje único para este sistema de partículas
    public GameObject vaso; // Referencia al vaso que recibirá el mensaje

    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Vaso"))
        {
            // Enviar el mensaje al vaso
            VasoReceptor receptor = vaso.GetComponent<VasoReceptor>();
            if (receptor != null)
            {
                receptor.RecibirMensaje(mensaje);
            }
        }
    }
}