using UnityEngine;

public class VasoRomper : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject vasoRotoPrefab;   // Prefab del vaso roto
    public float fuerzaMinima = 3f;     // Fuerza mínima para romperse

    private bool roto = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (roto) return;

        // Medir la fuerza del impacto
        float fuerzaImpacto = collision.relativeVelocity.magnitude;

        if (fuerzaImpacto >= fuerzaMinima)
        {
            Romper(collision);
        }
    }

    void Romper(Collision collision)
    {
        roto = true;

        // Posición exacta del contacto
        ContactPoint contacto = collision.contacts[0];
        Vector3 posicion = contacto.point;
        Quaternion rotacion = transform.rotation;

        // Ocultar vaso intacto
        gameObject.SetActive(false);

        // Instanciar el vaso roto
        if (vasoRotoPrefab != null)
        {
            GameObject rotoObj = Instantiate(vasoRotoPrefab, posicion, rotacion);
            rotoObj.SetActive(true);
        }
    }
}