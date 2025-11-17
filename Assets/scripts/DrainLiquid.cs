using UnityEngine;

public class DrainLiquid : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Verifica si el objeto tiene el componente LiquidIdentifier
        LiquidIdentifier liquid = other.GetComponent<LiquidIdentifier>();
        if (liquid != null)
        {
            Destroy(other.gameObject);  // Elimina la gota
        }
    }
}
