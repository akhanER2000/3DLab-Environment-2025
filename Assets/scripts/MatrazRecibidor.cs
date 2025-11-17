using UnityEngine;

public class MatrazRecibidor : MonoBehaviour
{
    private string mensajeActual;

    // Método público que recibe el mensaje desde las partículas
    public void RecibirMensaje(string mensaje)
    {
        mensajeActual = mensaje;
        Debug.Log($"[MatrazRecibidor] Matraz recibió mensaje: {mensajeActual}");
    }

    // Método público para que la pipeta pueda tomar el mensaje
    public string GetMensajeActual()
    {
        return mensajeActual;
    }
}
