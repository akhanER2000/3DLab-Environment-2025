//frab+
//este se le pone al electrodo

using System.Collections; // ¡IMPORTANTE! Añadir esto para la Corrutina
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ElectrodeInteraction : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    [Header("Referencias de UI")]
    [Tooltip("Arrastra aquí el objeto Canvas que tiene el script PhMetroUIController")]
    public PhMeterUIController uiController; // Tu cambio de referencia pública (¡está bien!)

    // --- INICIO DE MODIFICACIONES (AÑADIR ESTO) ---
    [Header("Configuración de Medición")]
    [Tooltip("Tiempo (segundos) que la lectura permanece visible antes de ensuciarse.")]
    public float tiempoDeLectura = 3.0f; // 3 segundos por defecto

    private bool isMeasuring = false; // Para evitar mediciones múltiples
    // --- FIN DE MODIFICACIONES ---

    // (El resto de tus variables...)
    public enum EstadoElectrodo { Limpio, Sucio }
    [Header("Estado del Electrodo")]
    public EstadoElectrodo estadoActual = EstadoElectrodo.Sucio;
    [Header("Tags de Interacción")]
    public string tagAguaDestilada = "AguaDestilada";
    [Header("Valores de pH y Temperatura")]
    public float pH_Agua = 7.0f;
    public float pH_Acido = 2.0f;
    public float pH_Base = 12.0f;
    public float temperatura = 25.0f;

    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (uiController == null)
        {
            Debug.LogError("¡Error en " + gameObject.name + "! La referencia 'uiController' no está asignada en el Inspector.");
        }
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (uiController != null)
        {
            uiController.RemoveProtective(estadoActual);
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (uiController != null)
        {
            uiController.ResetProtective();
            estadoActual = EstadoElectrodo.Sucio;
        }

        // --- AÑADIDO ---
        // Si el usuario suelta el electrodo, cancelamos cualquier medición
        if (isMeasuring)
        {
            StopAllCoroutines();
            isMeasuring = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (uiController == null) return;

        // --- Lógica de Lavado ---
        if (other.CompareTag(tagAguaDestilada))
        {
            // --- AÑADIDO ---
            // Si estábamos midiendo, cancelar
            if (isMeasuring)
            {
                StopAllCoroutines();
                isMeasuring = false;
            }

            estadoActual = EstadoElectrodo.Limpio;
            if (uiController.IsProtectiveRemoved())
            {
                uiController.MostrarMensajeLimpio();
            }
            return;
        }

        // --- Lógica de Medición ---
        if (!uiController.IsProtectiveRemoved())
            return;

        if (estadoActual == EstadoElectrodo.Sucio)
        {
            uiController.MostrarMensajeLavar();
            return;
        }

        // --- AÑADIDO ---
        // Si ya estamos midiendo (esperando el tiempo), no hacer nada
        if (isMeasuring)
        {
            return;
        }

        // --- Proceso de Medición (Solo si está Limpio) ---
        float phValue = 0f;

        if (other.CompareTag("pHAgua"))
        {
            phValue = pH_Agua;
        }
        else if (other.CompareTag("pHAcido"))
        {
            phValue = pH_Acido;
        }
        else if (other.CompareTag("pHBase"))
        {
            phValue = pH_Base;
        }
        else
        {
            return;
        }

        phValue += Random.Range(-0.25f, 0.25f);
        phValue = Mathf.Round(phValue * 100f) / 100f;
        float temperaturaValue = temperatura + Random.Range(-0.5f, 0.5f);
        temperaturaValue = Mathf.Round(temperaturaValue * 100f) / 100f;

        // --- CAMBIADO ---
        // Ya no se ensucia al instante. Ahora llamamos a la Corrutina:
        StartCoroutine(ProcesoDeMedicion(phValue, temperaturaValue));
    }

    // --- ¡NUEVA FUNCIÓN COMPLETA AÑADIDA AL FINAL! ---
    private IEnumerator ProcesoDeMedicion(float ph, float temp)
    {
        // 1. Marcar como "midiendo" para bloquear otras mediciones
        isMeasuring = true;

        // 2. Mostrar la lectura en la UI (Esto es lo que el usuario ve)
        uiController.UpdateReadings(ph, temp);

        // 3. Esperar el tiempo que definiste en el Inspector
        yield return new WaitForSeconds(tiempoDeLectura);

        // 4. (FIN DEL TIEMPO) Ahora, ensuciar el electrodo
        estadoActual = EstadoElectrodo.Sucio;

        // 5. Mostrar el mensaje "Lavar de nuevo"
        uiController.MostrarMensajePostLectura();

        // 6. Marcar como "terminado de medir" y listo para otra
        isMeasuring = false;
    }
}