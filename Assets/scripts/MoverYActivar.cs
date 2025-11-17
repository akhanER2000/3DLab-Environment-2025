using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
public class MoverYActivarParticulas : MonoBehaviour
{
    [Header("Configuración de movimiento")]
    public Transform objetoAMover;
    public float desplazamientoY = -2f;
    public float velocidad = 5f;

    [Header("Partículas")]
    public ParticleSystem sistemaParticulas;

    private bool mover = false;
    private bool abajo = false;
    private Vector3 posicionInicial;
    private Vector3 posicionAbajo;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    void Start()
    {
        if (objetoAMover != null)
        {
            posicionInicial = objetoAMover.position;
            posicionAbajo = posicionInicial + new Vector3(0, desplazamientoY, 0);
        }

        if (sistemaParticulas != null)
            sistemaParticulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Configurar el interactable de XR
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        interactable.activated.AddListener(ctx => ActivarAccion());
    }

    void Update()
    {
        if (Keyboard.current.mKey.wasPressedThisFrame)
            ActivarAccion();

        if (mover && objetoAMover != null)
        {
            Vector3 destino = abajo ? posicionAbajo : posicionInicial;
            objetoAMover.position = Vector3.MoveTowards(objetoAMover.position, destino, velocidad * Time.deltaTime);

            if (Vector3.Distance(objetoAMover.position, destino) < 0.01f)
                mover = false;
        }
    }

    public void ActivarAccion()
    {
        mover = true;

        if (sistemaParticulas != null)
        {
            if (!abajo) sistemaParticulas.Play();
            else sistemaParticulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        abajo = !abajo;
    }
}