//frab+
// Este script se le añade al objeto de la Piseta
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; // Necesario para XR

public class PisetaControl : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Asigna aquí el Particle System que simula el chorro de agua.")]
    public ParticleSystem waterParticles;

    // Esta variable es para asegurar que el Tag de las partículas coincide
    // con el que espera el script del electrodo (InteraccionElectrodo.cs)
    [Header("Configuración")]
    [Tooltip("Asegúrate que el GameObject de las partículas tenga este Tag.")]
    public string tagAguaDestilada = "AguaDestilada";

    private void Start()
    {
        // Asegurarse de que las partículas estén detenidas al inicio
        if (waterParticles != null)
        {
            waterParticles.Stop();
        }
        else
        {
            Debug.LogError("No hay 'waterParticles' asignado en la PisetaControl.");
        }
    }

    /// <summary>
    /// MÉTODO PÚBLICO para ser llamado por eventos de XR
    /// Inicia el chorro de agua destilada.
    /// </summary>
    public void EmpezarChorro()
    {
        if (waterParticles != null && !waterParticles.isPlaying)
        {
            waterParticles.Play();
        }
    }

    /// <summary>
    /// MÉTODO PÚBLICO para ser llamado por eventos de XR
    /// Detiene el chorro de agua destilada.
    /// </summary>
    public void DetenerChorro()
    {
        if (waterParticles != null && waterParticles.isPlaying)
        {
            waterParticles.Stop();
        }
    }
}