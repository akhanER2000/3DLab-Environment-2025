using UnityEngine;

public class BurbujaControl : MonoBehaviour
{
    [Header("Referencias")]
    public ParticleSystem burbujas;       // Sistema de partículas
    public WaterControl aguaControl;      // Script del agua (para leer nivel)

    [Header("Configuración de Lifetime")]
    public float minLifetime = 0.0001f;      // Vida mínima
    public float maxLifetime = 8f;      // Vida máxima
    public float lifetimeIncrement = 0.1f; // Incremento al variar con el agua

    private ParticleSystem.MainModule burbujasMain;

    void Start()
    {
        if (burbujas != null)
            burbujasMain = burbujas.main;
    }

    void Update()
    {
        if (aguaControl == null || burbujas == null) return;

        float nivel = aguaControl.GetFillValue();

        // Normalizamos el nivel entre 0 y 1 usando los límites de WaterControl
        float t = Mathf.InverseLerp(aguaControl.fillMin, aguaControl.fillMax, nivel);

        // Ajustamos el tiempo de vida con incremento extra
        float lifetime = Mathf.Lerp(minLifetime, maxLifetime, t) + lifetimeIncrement;

        burbujasMain.startLifetime = lifetime;
    }
}