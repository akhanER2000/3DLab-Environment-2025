using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class GasValve : MonoBehaviour
{
    [Header("Valve Configuration")]
    [SerializeField] private Transform valveHandle;
    [SerializeField] private Transform gasOutlet;
    [SerializeField] private float maxRotation = 90f;
    [SerializeField] private float rotationSpeed = 45f;

    [Header("State")]
    [Range(0, 1)]
    [SerializeField] private float openPercentage = 0f;
    [SerializeField] private bool isValveOpen = false;
    [SerializeField] private bool isInteractable = true;

    [Header("Gas Flow")]
    [SerializeField] private float maxGasFlow = 100f;
    [SerializeField] private AnimationCurve flowCurve = AnimationCurve.Linear(0, 0, 1, 1);
    private float currentGasFlow = 0f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip valveTurnSound;
    [SerializeField] private AudioClip gasFlowSound;

    [Header("Visual Feedback")]
    [SerializeField] private Renderer valveHandleRenderer;
    [SerializeField] private Color closedColor = Color.red;
    [SerializeField] private Color openColor = Color.green;

    [Header("Events")]
    public UnityEvent<float> OnValveRotated;
    public UnityEvent OnValveOpened;
    public UnityEvent OnValveClosed;
    public UnityEvent<float> OnGasFlowChanged;

    private float currentRotation = 0f;
    private bool isBeingInteracted = false;
    private Material valveHandleMaterial;

    void Start()
    {
        InitializeValve();
    }

    void InitializeValve()
    {
        SetValveOpenness(openPercentage);

        if (gasOutlet == null)
        {
            Transform outlet = transform.Find("GasOutlet");
            if (outlet != null) gasOutlet = outlet;
        }

        if (valveHandleRenderer != null)
        {
            valveHandleMaterial = valveHandleRenderer.material;
        }
    }

    public void StartInteraction()
    {
        if (!isInteractable) return;
        isBeingInteracted = true;

        if (audioSource && valveTurnSound)
        {
            audioSource.PlayOneShot(valveTurnSound);
        }
    }

    public void EndInteraction()
    {
        isBeingInteracted = false;
    }

    public void RotateValve(float inputDelta)
    {
        if (!isInteractable || !isBeingInteracted) return;

        float rotationDelta = inputDelta * rotationSpeed * Time.deltaTime;
        currentRotation = Mathf.Clamp(currentRotation + rotationDelta, 0, maxRotation);

        openPercentage = currentRotation / maxRotation;

        if (valveHandle)
        {
            valveHandle.localRotation = Quaternion.Euler(0, currentRotation, 0);
        }

        UpdateGasFlow();
        CheckValveState();
        OnValveRotated?.Invoke(openPercentage);
    }

    public void SetValveOpenness(float percentage)
    {
        openPercentage = Mathf.Clamp01(percentage);
        currentRotation = openPercentage * maxRotation;

        if (valveHandle)
        {
            valveHandle.localRotation = Quaternion.Euler(0, currentRotation, 0);
        }

        UpdateGasFlow();
        CheckValveState();
    }

    void UpdateGasFlow()
    {
        float normalizedFlow = flowCurve.Evaluate(openPercentage);
        currentGasFlow = normalizedFlow * maxGasFlow;

        if (audioSource && gasFlowSound)
        {
            if (currentGasFlow > 0)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.clip = gasFlowSound;
                    audioSource.loop = true;
                    audioSource.Play();
                }
                audioSource.volume = normalizedFlow * 0.5f;
            }
            else
            {
                audioSource.Stop();
            }
        }

        if (valveHandleMaterial != null)
        {
            Color targetColor = Color.Lerp(closedColor, openColor, openPercentage);
            valveHandleMaterial.SetColor("_BaseColor", targetColor);
            valveHandleMaterial.SetColor("_Color", targetColor);
        }

        OnGasFlowChanged?.Invoke(currentGasFlow);
    }

    void CheckValveState()
    {
        bool wasOpen = isValveOpen;
        isValveOpen = openPercentage > 0.05f;

        if (isValveOpen && !wasOpen)
        {
            OnValveOpened?.Invoke();
            Debug.Log("Válvula abierta");
        }
        else if (!isValveOpen && wasOpen)
        {
            OnValveClosed?.Invoke();
            Debug.Log("Válvula cerrada");
        }
    }

    public float GetGasFlow() => currentGasFlow;
    public bool IsOpen() => isValveOpen;
    public float GetOpenPercentage() => openPercentage;
}