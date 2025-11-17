using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem; // Para usar Keyboard.current

[RequireComponent(typeof(RectTransform))]
public class Advertencia : MonoBehaviour
{
    [Header("Configuración de mensajes")]
    public List<string> mensajesEsperados;
    private HashSet<string> mensajesRecibidos = new HashSet<string>();

    [Header("Movimiento del Panel")]
    public Vector2 nuevaPosicion; // Posición destino (X,Y)

    private bool accionEjecutada = false;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // Presionando la tecla L se activa la acción
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            EjecutarAccion();
        }
    }

    public void RecibirMensaje(string mensaje)
    {
        if (mensajesEsperados.Contains(mensaje))
        {
            mensajesRecibidos.Add(mensaje);
        }

        if (!accionEjecutada && mensajesRecibidos.Count == mensajesEsperados.Count)
        {
            EjecutarAccion();
            accionEjecutada = true;
        }
    }

    private void EjecutarAccion()
    {
        rectTransform.anchoredPosition = nuevaPosicion;
        Debug.Log($"Advertencia activada: Panel movido a {nuevaPosicion}");
    }
}