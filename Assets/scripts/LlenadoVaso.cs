using UnityEngine;

public class LlenadoVaso : MonoBehaviour
{
    public GameObject aguaVisual;
    public float incrementoAltura = 0.01f;
    public float maxAltura = 0.2f;

    private Vector3 escalaInicial;

    void Start()
    {
        if (aguaVisual != null)
        {
            escalaInicial = aguaVisual.transform.localScale;
        }
    }

    void OnParticleCollision(GameObject other)
    {
        if (aguaVisual != null)
        {
            Vector3 escalaActual = aguaVisual.transform.localScale;

            if (escalaActual.y < maxAltura)
            {
                escalaActual.y += incrementoAltura;
                aguaVisual.transform.localScale = escalaActual;
            }
        }
    }
}
