using UnityEngine;

public class ControlPinzasDetector : MonoBehaviour
{
    [Header("Referencia al hijo dentro de las pinzas")]
    public GameObject hijoEnPinzas; // El objeto hijo que representa lo recogido

    [Header("Configuración")]
    public string tagObjeto = "CapsulaEntorno"; // Tag del objeto que puede ser recogido

    void Start()
    {
        // Asegurar que el hijo dentro de las pinzas empiece invisible
        if (hijoEnPinzas != null)
            hijoEnPinzas.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Solo reaccionar si colisiona con el tag correcto
        if (other.CompareTag(tagObjeto))
        {
            Debug.Log("Pinzas: recogiendo objeto con tag " + tagObjeto);

            // Ocultar el objeto del entorno
            other.gameObject.SetActive(false);

            // Activar el hijo en las pinzas
            if (hijoEnPinzas != null)
                hijoEnPinzas.SetActive(true);
        }
    }
}