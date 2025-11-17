using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class AerosolSprayController_XR_Activate : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Transform de la boquilla (Z+ = dirección).")]
    public Transform nozzle;
    [Tooltip("ParticleSystem del spray (hijo de Nozzle o de la botella).")]
    public ParticleSystem sprayPS;
    [Tooltip("Audio (loop) del spray, opcional.")]
    public AudioSource sprayAudio;

    [Header("Ajustes del spray")]
    [Tooltip("Partículas/seg mientras se mantiene Activate presionado.")]
    public float sprayRateOverTime = 300f;
    [Tooltip("Suavizado al arrancar/detener (seg).")]
    public float emissionSmoothTime = 0.05f;
    [Tooltip("Multiplicador de velocidad mientras rocía.")]
    public float speedMultiplierWhileSpraying = 1.0f;

    private XRGrabInteractable _grab;
    private IXRSelectInteractor _currentInteractor;
    private ParticleSystem.EmissionModule _emission;
    private ParticleSystem.MainModule _main;
    private ParticleSystem.ShapeModule _shape;

    private float _targetRate = 0f, _currentRate = 0f, _rateVel = 0f;
    private bool _spraying = false;

    private void Awake()
    {
        _grab = GetComponent<XRGrabInteractable>();
        _grab.selectEntered.AddListener(OnSelectEntered);
        _grab.selectExited.AddListener(OnSelectExited);
        _grab.activated.AddListener(OnActivated);     // ← botón de usar (Trigger por defecto)
        _grab.deactivated.AddListener(OnDeactivated); // ← al soltar el botón

        if (sprayPS != null)
        {
            _emission = sprayPS.emission;
            _main = sprayPS.main;
            _shape = sprayPS.shape;

            // Estado inicial
            _emission.rateOverTime = 0f;
            _main.simulationSpace = ParticleSystemSimulationSpace.World;

            // Valores por defecto para chorro más “apretado”
            if (_main.startSize.mode == ParticleSystemCurveMode.Constant)
                _main.startSize = Mathf.Clamp(_main.startSize.constant, 0.005f, 0.012f);
            if (_main.startLifetime.mode == ParticleSystemCurveMode.Constant)
                _main.startLifetime = Mathf.Clamp(_main.startLifetime.constant, 0.25f, 0.7f);
            if (_main.startSpeed.mode == ParticleSystemCurveMode.Constant)
                _main.startSpeed = Mathf.Clamp(_main.startSpeed.constant, 4f, 10f);

            _shape.enabled = true;
            _shape.shapeType = ParticleSystemShapeType.Cone;
            _shape.angle = 3.5f;     // más chico = más concentrado
            _shape.radius = 0.0015f; // boca pequeña
            _shape.alignToDirection = true;
        }

        if (sprayAudio != null)
        {
            sprayAudio.loop = true;
            sprayAudio.playOnAwake = false;
        }
    }

    private void OnDestroy()
    {
        if (_grab != null)
        {
            _grab.selectEntered.RemoveListener(OnSelectEntered);
            _grab.selectExited.RemoveListener(OnSelectExited);
            _grab.activated.RemoveListener(OnActivated);
            _grab.deactivated.RemoveListener(OnDeactivated);
        }
    }

    private void Update()
    {
        // Suavizado de la emisión
        if (sprayPS != null)
        {
            _currentRate = Mathf.SmoothDamp(_currentRate, _targetRate, ref _rateVel, emissionSmoothTime);
            _emission.rateOverTime = _currentRate;

            // Alinear PS a la boquilla
            if (nozzle != null)
            {
                sprayPS.transform.position = nozzle.position;
                sprayPS.transform.rotation = nozzle.rotation;
            }
        }

        // Audio on/off
        if (sprayAudio != null)
        {
            bool shouldPlay = _targetRate > 0.01f && _currentInteractor != null;
            if (shouldPlay && !sprayAudio.isPlaying) sprayAudio.Play();
            if (!shouldPlay && sprayAudio.isPlaying) sprayAudio.Stop();
        }
    }

    // ——— Eventos XR ———
    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        _currentInteractor = args.interactorObject; // ahora está agarrado
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        _currentInteractor = null;
        SetSpray(false); // seguridad al soltar
    }

    private void OnActivated(ActivateEventArgs args)
    {
        if (_currentInteractor == null) return;
        SetSpray(true);
    }

    private void OnDeactivated(DeactivateEventArgs args)
    {
        SetSpray(false);
    }

    private void SetSpray(bool on)
    {
        _spraying = on && _currentInteractor != null;
        _targetRate = _spraying ? sprayRateOverTime : 0f;

        if (sprayPS != null)
        {
            var m = sprayPS.main;
            m.startSpeedMultiplier = _spraying ? speedMultiplierWhileSpraying : 1.0f;
        }
    }
}