using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
public class GeneradorPapelPH : MonoBehaviour
{
    [Header("Objeto a copiar (ya en la escena, inicialmente invisible)")]
    public GameObject papelPHOriginal;

    [Header("Punto de aparición")]
    public Transform puntoSpawn;

    [Header("Nombres de referencia (para prefabs)")]
    public string nombrePapel = "papel_PH(2)";
    public string nombreSpawn = "SpawnPapel";

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
    }

    private void Start()
    {
        // Buscar objeto original si no está asignado
        if (papelPHOriginal == null && !string.IsNullOrEmpty(nombrePapel))
        {
            GameObject foundPapel = GameObject.Find(nombrePapel);
            if (foundPapel != null)
            {
                papelPHOriginal = foundPapel;
                Debug.Log($"[GeneradorPapelPH] Referencia encontrada: {nombrePapel}");
            }
            else
            {
                Debug.LogWarning($"[GeneradorPapelPH] No se encontró el objeto '{nombrePapel}' en la escena.");
            }
        }

        // Buscar punto de spawn
        if (puntoSpawn == null && !string.IsNullOrEmpty(nombreSpawn))
        {
            GameObject foundSpawn = GameObject.Find(nombreSpawn);
            if (foundSpawn != null)
            {
                puntoSpawn = foundSpawn.transform;
                Debug.Log($"[GeneradorPapelPH] Punto de spawn encontrado: {nombreSpawn}");
            }
            else
            {
                Debug.LogWarning($"[GeneradorPapelPH] No se encontró el punto de spawn '{nombreSpawn}' en la escena.");
            }
        }

        // ✅ Activar automáticamente el objeto original si está desactivado
        if (papelPHOriginal != null && !papelPHOriginal.activeSelf)
        {
            papelPHOriginal.SetActive(true);
            Debug.Log("🔵 Activando automáticamente el papel pH original.");
        }
    }

    private void OnEnable()
    {
        interactable.selectEntered.AddListener(OnSelectEntered);
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnSelectEntered);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        GenerarCopia();
    }

    void GenerarCopia()
    {
        if (papelPHOriginal != null && puntoSpawn != null)
        {
            GameObject nuevaCopia = Instantiate(papelPHOriginal, puntoSpawn.position, puntoSpawn.rotation);
            nuevaCopia.SetActive(true); // por si el original estuviera apagado antes
            Debug.Log("✅ Nuevo papel pH generado correctamente.");
        }
        else
        {
            Debug.LogWarning("⚠️ Falta asignar el objeto original o el punto de aparición en el inspector o la escena.");
        }
    }
}
