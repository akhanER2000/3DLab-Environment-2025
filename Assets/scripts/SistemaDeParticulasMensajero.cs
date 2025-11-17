using UnityEngine;

public class SistemaDeParticulasMensajero : MonoBehaviour
{
    private string mensaje;

    // Método público para asignar el mensaje
    public void SetMensaje(string nuevoMensaje)
    {
        mensaje = nuevoMensaje;
        Debug.Log($"[SistemaDeParticulasMensajero] Mensaje recibido: {mensaje}");
    }

    // Método público para obtener el mensaje (si lo necesitas)
    public string GetMensaje()
    {
        return mensaje;
    }

    private void OnParticleCollision(GameObject other)
    {
        // Ejemplo de envío a un receptor cuando las partículas colisionan
        if (other.CompareTag("Matraz")) // Ajusta el tag según tu objeto
        {
            MatrazRecibidor receptor = other.GetComponent<MatrazRecibidor>();
            if (receptor != null)
            {
                receptor.RecibirMensaje(mensaje);
                Debug.Log($"[SistemaDeParticulasMensajero] Mensaje enviado a matraz: {mensaje}");
            }
        }
    }
}