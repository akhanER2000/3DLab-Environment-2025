using System.Collections;
using UnityEngine;

public class DetectarParticulas : MonoBehaviour
{
    [Header("Configuración de llenado")]
    public string tagParticulas = "AcidoMagnesio"; 
    public float incrementoNivel = 1f;  
    public float nivelMaximo = 100f;    

    [Header("Reacción al fuego")]
    public string tagFuego = "Fuego";  
    public Light flashLight;
    public AudioSource explosionSfx;
    public ParticleSystem waterDrops;

    [Header("Timings de la reacción")]
    public float flashDuration = 0.1f;
    public float waterDelay = 1.0f;

    private float nivelActual = 0f;     
    private bool vasoLleno = false;     
    private bool reaccionEjecutada = false; // Para evitar múltiples ejecuciones

    void Update()
    {
        // Chequea si el tubo está boca arriba para vaciar el gas
        if (vasoLleno && EstaBocaArriba())
        {
            VaciarGas();
        }
    }

    void OnParticleCollision(GameObject other)
    {
        // 1️⃣ Llenado de gas
        if (other.CompareTag(tagParticulas) && !vasoLleno)
        {
            nivelActual += incrementoNivel;
            Debug.Log("Nivel actual del vaso: " + nivelActual);

            if (nivelActual >= nivelMaximo)
            {
                vasoLleno = true;
                Debug.Log("✅ Tubo lleno de gas");
            }
        }

        // 2️⃣ Reacción con fuego
        if (other.CompareTag(tagFuego) && vasoLleno && !reaccionEjecutada && EstaBocaAbajo())
        {
            reaccionEjecutada = true;
            StartCoroutine(ReactionSequence());
        }
    }

    IEnumerator ReactionSequence()
    {
    
        if (flashLight != null)
            flashLight.gameObject.SetActive(true);
    
        if (explosionSfx != null)
            explosionSfx.Play();
    
        if (flashLight != null)
            yield return new WaitForSeconds(flashDuration);
        
        if (flashLight != null)
            flashLight.gameObject.SetActive(false);
    
        yield return new WaitForSeconds(waterDelay);
    
        if (waterDrops != null)
            waterDrops.Play();
    }


    // Detecta si el tubo está boca abajo (aproximadamente)
    private bool EstaBocaAbajo()
    {
        float angulo = Vector3.Angle(transform.up, Vector3.down);
        return angulo < 30f; // margen de 30°
    }

    // Detecta si el tubo está boca arriba (aproximadamente)
    private bool EstaBocaArriba()
    {
        float angulo = Vector3.Angle(transform.up, Vector3.up);
        return angulo < 30f; // margen de 30°
    }

    private void VaciarGas()
    {
        Debug.Log("⚠️ Tubo girado, gas perdido");
        vasoLleno = false;
        nivelActual = 0f;
        reaccionEjecutada = false; // Permite que pueda reaccionar otra vez si se llena
    }

    public bool EstaLleno()
    {
        return vasoLleno;
    }
}
