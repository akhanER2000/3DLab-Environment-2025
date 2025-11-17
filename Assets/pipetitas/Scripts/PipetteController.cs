using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// PipetteController completo:
/// - Succión (B) incrementa currentVolume (no VFX)
/// - Liberación (A) muestra VFX (chorro) y luego lo detiene y destruye limpiamente
/// - Forza rotation Y=180 en la instancia si alignToTipDown = true
/// - Forza simulationSpace = World en los ParticleSystems instanciados
/// - Emite un número inmediato de partículas para garantizar visibilidad (emitImmediateCount)
/// </summary>
public class PipetteController : MonoBehaviour
{
    [Header("Volumen")]
    public float currentVolume = 0f;
    public float maxVolume = 10f;
    public float increment = 2f; // mL por click

    [Header("Input (arrastrar desde PipetteActions)")]
    public InputActionReference suctionAction; // Pipette/Suction
    public InputActionReference releaseAction; // Pipette/Release

    [Header("Referencias (Inspector)")]
    public XRGrabInteractable grabInteractable;   // XRGrabInteractable del objeto pipeta
    public Transform tipTransform;                // transform de la punta (donde se posiciona el VFX)
    public GameObject spillPrefab;                // prefab opcional de derrame (ParticleSystem)
    public PipetteUIVolumeBarController uiController; // UI (barra + texto)

    [Header("Particle VFX (chorro al liberar)")]
    [Tooltip("Prefab con ParticleSystem(s). PlayOnAwake = false recomendado.")]
    public GameObject suctionParticlePrefab; // prefab del chorro/particulas que se visualizará al liberar

    [Header("Particle VFX timing")]
    [Tooltip("Tiempo (s) que dejamos emitir la instancia del chorro antes de detenerla")]
    public float emitDuration = 0.25f;
    [Tooltip("Si el sistema está configurado con rate=0, forzar este número de partículas al mostrar")]
    public int emitImmediateCount = 20;

    [HideInInspector] public bool canSuck = false;
    [HideInInspector] public bool tipInTestTube = false;
    [HideInInspector] public TestTubeController currentTestTube;

    bool isGrabbed = false;

    private GameObject suctionParticlesInstance = null;
    private ParticleSystem[] suctionParticleSystems = null;
    private Coroutine _stopCoroutine = null;

    void Awake()
    {
        if (grabInteractable == null)
            grabInteractable = GetComponent<XRGrabInteractable>();
    }

    void OnEnable()
    {
        if (suctionAction != null && suctionAction.action != null)
        {
            suctionAction.action.performed += OnSuctionPerformed;
            suctionAction.action.Enable();
        }

        if (releaseAction != null && releaseAction.action != null)
        {
            releaseAction.action.performed += OnReleasePerformed;
            releaseAction.action.Enable();
        }

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnReleaseGrab);
        }
    }

    void OnDisable()
    {
        if (suctionAction != null && suctionAction.action != null)
        {
            suctionAction.action.performed -= OnSuctionPerformed;
            suctionAction.action.Disable();
        }
        if (releaseAction != null && releaseAction.action != null)
        {
            releaseAction.action.performed -= OnReleasePerformed;
            releaseAction.action.Disable();
        }

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
            grabInteractable.selectExited.RemoveListener(OnReleaseGrab);
        }
    }

    #region Grab handlers
    void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        if (uiController != null)
        {
            uiController.Show();
            uiController.SetVolumeVisual(currentVolume, true);
        }
    }

    void OnReleaseGrab(SelectExitEventArgs args)
    {
        if (args.isCanceled)
            return;

        isGrabbed = false;
        if (uiController != null) uiController.Hide();
        StopAndScheduleDestroySuctionParticles(immediateClear: true);
    }
    #endregion

    #region Input handlers
    void OnSuctionPerformed(InputAction.CallbackContext ctx)
    {
        if (!isGrabbed) return;
        if (!canSuck) return;

        float previous = currentVolume;
        currentVolume = Mathf.Min(maxVolume, currentVolume + increment);

        if (currentVolume != previous)
        {
            if (uiController != null) uiController.SetVolumeVisual(currentVolume);
            Debug.Log($"Pipette: succión -> {currentVolume} mL");
        }
    }

    void OnReleasePerformed(InputAction.CallbackContext ctx)
    {
        if (!isGrabbed) return;
        if (currentVolume <= 0f)
        {
            StopAndScheduleDestroySuctionParticles(immediateClear: true);
            return;
        }

        PlaySuctionParticles(alignToTipDown: true);

        if (suctionParticleSystems != null)
        {
            foreach (var ps in suctionParticleSystems)
            {
                if (ps == null) continue;
                if (emitImmediateCount > 0)
                    ps.Emit(emitImmediateCount);
            }
        }

        if (_stopCoroutine != null)
        {
            StopCoroutine(_stopCoroutine);
            _stopCoroutine = null;
        }
        _stopCoroutine = StartCoroutine(StopParticlesAfterDelayCoroutine(emitDuration, immediateClear: false));

        if (tipInTestTube && currentTestTube != null)
        {
            currentTestTube.AddVolume(currentVolume);
            Debug.Log($"Pipette: liberado {currentVolume} mL en TestTube.");
            currentVolume = 0f;
            if (uiController != null) uiController.SetVolumeVisual(currentVolume);
        }
        else
        {
            if (spillPrefab != null && tipTransform != null)
            {
                GameObject spill = Instantiate(spillPrefab, tipTransform.position, Quaternion.identity);
                var ps = spill.GetComponentsInChildren<ParticleSystem>();
                float maxLife = 0f;
                foreach (var p in ps)
                {
                    float life = GetParticleMaxLifetime(p);
                    if (life > maxLife) maxLife = life;
                }
                Destroy(spill, Mathf.Max(0.1f, maxLife + 0.15f));
            }

            Debug.Log($"Pipette: derramado {currentVolume} mL (no en tubo).");
            currentVolume = 0f;
            if (uiController != null) uiController.SetVolumeVisual(currentVolume);
        }
    }
    #endregion

    #region Particle management
    void PlaySuctionParticles(bool alignToTipDown = false)
    {
        if (suctionParticlePrefab == null || tipTransform == null) return;

        if (suctionParticlesInstance == null)
        {
            suctionParticlesInstance = Instantiate(suctionParticlePrefab, tipTransform.position, tipTransform.rotation, tipTransform);
            suctionParticleSystems = suctionParticlesInstance.GetComponentsInChildren<ParticleSystem>();

            if (suctionParticleSystems != null && suctionParticleSystems.Length > 0)
            {
                foreach (var ps in suctionParticleSystems)
                {
                    if (ps == null) continue;
                    var main = ps.main;
                    main.playOnAwake = false;
                    main.simulationSpace = ParticleSystemSimulationSpace.World;
                    ps.Play();
                }
            }
            else
            {
                Debug.LogWarning("PipetteController: suctionParticlePrefab no contiene ParticleSystem.");
            }
        }
        else
        {
            suctionParticlesInstance.transform.SetParent(tipTransform, false);
            suctionParticlesInstance.transform.localPosition = Vector3.zero;
            suctionParticlesInstance.transform.localRotation = Quaternion.identity;

            if (suctionParticleSystems != null)
            {
                foreach (var ps in suctionParticleSystems)
                {
                    if (ps == null) continue;
                    var main = ps.main;
                    main.simulationSpace = ParticleSystemSimulationSpace.World;
                    if (!ps.isPlaying) ps.Play();
                }
            }
        }

        if (suctionParticlesInstance != null)
        {
            if (alignToTipDown)
            {
                suctionParticlesInstance.transform.rotation = tipTransform.rotation * Quaternion.Euler(0f, 180f, 0f);
            }
            else
            {
                suctionParticlesInstance.transform.rotation = tipTransform.rotation;
            }
        }
    }

    void StopAndScheduleDestroySuctionParticles(bool immediateClear = false)
    {
        if (suctionParticlesInstance == null) return;

        if (suctionParticleSystems == null || suctionParticleSystems.Length == 0)
            suctionParticleSystems = suctionParticlesInstance.GetComponentsInChildren<ParticleSystem>();

        float maxLifetime = 0f;

        foreach (var ps in suctionParticleSystems)
        {
            if (ps == null) continue;
            if (immediateClear)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            else
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            float life = GetParticleMaxLifetime(ps);
            if (life > maxLifetime) maxLifetime = life;
        }

        float destroyDelay = immediateClear ? 0.05f : Mathf.Max(0.05f, maxLifetime + 0.15f);
        Destroy(suctionParticlesInstance, destroyDelay);
        suctionParticlesInstance = null;
        suctionParticleSystems = null;
    }

    private IEnumerator StopParticlesAfterDelayCoroutine(float delay, bool immediateClear)
    {
        yield return new WaitForSeconds(delay);
        StopAndScheduleDestroySuctionParticles(immediateClear);
        _stopCoroutine = null;
    }

    float GetParticleMaxLifetime(ParticleSystem ps)
    {
        if (ps == null) return 0f;
        var main = ps.main;

        if (main.startLifetime.mode == ParticleSystemCurveMode.Constant)
            return main.startLifetime.constant;
        if (main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants)
            return main.startLifetime.constantMax;
        return main.startLifetime.constantMax;
    }
    #endregion
}
