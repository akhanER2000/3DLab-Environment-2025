using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ExtintorController : MonoBehaviour
{
    [Header("Input Actions (arrastrar desde tu mapa de Input)")]
    public InputActionReference removePinAction;   // Botón para quitar seguro (como Suction)
    public InputActionReference sprayAction;       // Botón para activar spray (como Release)

    [Header("Referencias")]
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    public GameObject seguro;                 // El seguro físico que desaparece
    public Transform nozzleTip;               // Punta del extintor donde sale el spray
    public GameObject sprayPrefab;            // Prefab de partículas del extintor

    [Header("Partículas")]
    public float sprayDurationWhileHeld = 0.1f;    // Qué tan seguido renueva emisión
    public int emitImmediateCount = 30;            // Partículas inmediatas

    private bool isGrabbed = false;
    private bool seguroQuitado = false;

    private GameObject sprayInstance;
    private ParticleSystem[] spraySystems;
    private Coroutine stopRoutine;

    void Awake()
    {
        if (!grabInteractable)
            grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    void OnEnable()
    {
        // Eventos de Input
        if (removePinAction != null)
        {
            removePinAction.action.performed += OnRemovePinPerformed;
            removePinAction.action.Enable();
        }

        if (sprayAction != null)
        {
            sprayAction.action.performed += OnSprayPressed;
            sprayAction.action.canceled  += OnSprayReleased;
            sprayAction.action.Enable();
        }

        // Eventos de agarre
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    void OnDisable()
    {
        if (removePinAction != null)
        {
            removePinAction.action.performed -= OnRemovePinPerformed;
            removePinAction.action.Disable();
        }

        if (sprayAction != null)
        {
            sprayAction.action.performed -= OnSprayPressed;
            sprayAction.action.canceled  -= OnSprayReleased;
            sprayAction.action.Disable();
        }

        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    //──────────────────────────────────────────────
    // GRAB EVENTS
    //──────────────────────────────────────────────
    void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        StopAndScheduleDestroyParticles(true);
    }

    //──────────────────────────────────────────────
    // INPUT: QUITAR SEGURO
    //──────────────────────────────────────────────
    void OnRemovePinPerformed(InputAction.CallbackContext ctx)
    {
        if (!isGrabbed) return;
        if (seguroQuitado) return;

        seguroQuitado = true;
        seguro.SetActive(false);

        Debug.Log("🔓 Extintor: Seguro retirado");
    }

    //──────────────────────────────────────────────
    // INPUT: ACTIVAR SPRAY
    //──────────────────────────────────────────────
    void OnSprayPressed(InputAction.CallbackContext ctx)
    {
        if (!isGrabbed) return;
        if (!seguroQuitado) return;

        PlaySprayParticles();

        // emitir inmediatamente para que sea visible
        if (spraySystems != null)
        {
            foreach (var ps in spraySystems)
                ps.Emit(emitImmediateCount);
        }

        Debug.Log("🧯 Extintor: Spray ACTIVADO");
    }

    void OnSprayReleased(InputAction.CallbackContext ctx)
    {
        Debug.Log("🧯 Extintor: Spray DESACTIVADO");
        StopAndScheduleDestroyParticles(false);
    }

    //──────────────────────────────────────────────
    // PARTICLE MANAGEMENT (igual que tu pipeta)
    //──────────────────────────────────────────────
    void PlaySprayParticles()
    {
        if (sprayPrefab == null || nozzleTip == null)
            return;

        if (sprayInstance == null)
        {
            sprayInstance = Instantiate(sprayPrefab, nozzleTip.position, nozzleTip.rotation, nozzleTip);
            spraySystems = sprayInstance.GetComponentsInChildren<ParticleSystem>();

            if (spraySystems != null)
            {
                foreach (var ps in spraySystems)
                {
                    var main = ps.main;
                    main.playOnAwake = false;
                    main.simulationSpace = ParticleSystemSimulationSpace.World;
                    ps.Play();
                }
            }
        }
        else
        {
            sprayInstance.transform.SetParent(nozzleTip, false);

            foreach (var ps in spraySystems)
            {
                if (!ps.isPlaying)
                    ps.Play();
            }
        }
    }

    void StopAndScheduleDestroyParticles(bool immediateClear)
    {
        if (sprayInstance == null)
            return;

        if (spraySystems == null || spraySystems.Length == 0)
            spraySystems = sprayInstance.GetComponentsInChildren<ParticleSystem>();

        float maxLifetime = 0f;

        foreach (var ps in spraySystems)
        {
            if (immediateClear)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            else
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            float life = GetStartLifetime(ps);
            if (life > maxLifetime) maxLifetime = life;
        }

        float destroyDelay = immediateClear ? 0.05f : maxLifetime + 0.15f;

        Destroy(sprayInstance, destroyDelay);
        sprayInstance = null;
        spraySystems = null;
    }

    float GetStartLifetime(ParticleSystem ps)
    {
        var main = ps.main;

        if (main.startLifetime.mode == ParticleSystemCurveMode.Constant)
            return main.startLifetime.constant;

        if (main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants)
            return main.startLifetime.constantMax;

        return main.startLifetime.constantMax;
    }
}
