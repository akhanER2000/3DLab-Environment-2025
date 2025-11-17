using UnityEngine;

public class ActivarReaccion : MonoBehaviour
{
    [Header("Condiciones")]
    public string tagParticulaColision = "ParticulaA";  // Tag de las partículas que deben chocar con el vaso
    public ParticleSystem particulasCondicion;          // Partículas que deben estar activadas

    [Header("Reacción")]
    public ParticleSystem particulasReaccion;           // Sistema de partículas a activar (reacción)

    private bool colisionDetectada = false;

    private void OnParticleCollision(GameObject other)
    {
        // Verifica si la partícula que chocó tiene el tag correcto
        if (other.CompareTag(tagParticulaColision))
        {
            colisionDetectada = true;
            VerificarActivacion();
        }
    }

    private void VerificarActivacion()
    {
        // Condición 1: Hubo colisión
        // Condición 2: El otro sistema de partículas está activo
        if (colisionDetectada && particulasCondicion.isPlaying)
        {
            if (!particulasReaccion.isPlaying)
            {
                particulasReaccion.Play();  // Activa la reacción
                Debug.Log("¡Reacción activada!");
            }
        }
    }

    // Reset por si quieres que se pueda volver a usar
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(tagParticulaColision))
        {
            colisionDetectada = false;
        }
    }
}