using UnityEngine;

public class Charco : MonoBehaviour
{
    public float crecimientoVelocidad = 0.2f;
    public float escalaMax = 1.5f;

    void Update()
    {
        if (transform.localScale.x < escalaMax)
        {
            float delta = crecimientoVelocidad * Time.deltaTime;
            transform.localScale += new Vector3(delta, 0, delta);
        }
    }
}