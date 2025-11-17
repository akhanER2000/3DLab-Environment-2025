using UnityEngine;

public class Mechero : MonoBehaviour
{
    [Header("Llaves de paso")]
    public LlaveDePaso valve1;  // Se intentará buscar automáticamente
    public LlaveDePaso valve2;

    [Header("Particle System del mechero")]
    public ParticleSystem flame;

    [Header("Tag de partículas del encendedor")]
    public string fireTag = "Fuego";

    private bool isFlameOn = false;

    void Start()
    {
        // Buscar automáticamente valve1 si no está asignada en el prefab
        if (valve1 == null)
        {
            GameObject valveObj = GameObject.Find("GasValveAssembly-2/Valve_Handle");
            if (valveObj != null)
            {
                valve1 = valveObj.GetComponent<LlaveDePaso>();
                if (valve1 != null)
                    Debug.Log("Valve1 encontrada y asignada automáticamente.");
                else
                    Debug.LogWarning("Valve_Handle encontrada pero no tiene componente LlaveDePaso.");
            }
            else
            {
                Debug.LogWarning("No se encontró 'GasValveAssembly-2/Valve_Handle' en la escena.");
            }
        }

        // Buscar automáticamente valve2 si fuera necesario
        if (valve2 == null)
        {
            // Ejemplo: buscar otra válvula si se llama diferente
            GameObject valveObj2 = GameObject.Find("GasValveAssembly-3/Valve_Handle");
            if (valveObj2 != null)
                valve2 = valveObj2.GetComponent<LlaveDePaso>();
        }
    }

    void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag(fireTag))
        {
            Debug.Log("Partícula tocó el mechero");
            Debug.Log("Valve1 isOpen: " + (valve1 != null ? valve1.IsOpen().ToString() : "NULL"));
            Debug.Log("Valve2 isOpen: " + (valve2 != null ? valve2.IsOpen().ToString() : "NULL"));

            if (valve1 != null && valve2 != null && valve1.IsOpen() && valve2.IsOpen())
            {
                TurnOnFlame();
                Debug.Log("🔥 Mechero encendido!");
            }
        }
    }

    void Update()
    {
        if (isFlameOn && ((!valve1?.IsOpen() ?? false) || (!valve2?.IsOpen() ?? false)))
        {
            TurnOffFlame();
        }
    }

    void TurnOnFlame()
    {
        if (!isFlameOn && flame != null)
        {
            flame.gameObject.SetActive(true);
            flame.Play();
            isFlameOn = true;
        }
    }

    void TurnOffFlame()
    {
        if (isFlameOn && flame != null)
        {
            flame.Stop();
            isFlameOn = false;
        }
    }
}
