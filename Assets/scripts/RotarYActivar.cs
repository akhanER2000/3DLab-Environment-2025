using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
public class RotarYActivarParticulas : MonoBehaviour
{
    [Header("Configuración de rotación")]
    public Transform objetoARotar;
    public float anguloY = 90f;
    public float velocidadRotacion = 90f;

    [Header("Sistemas de partículas")]
    public ParticleSystem particulas1;
    public ParticleSystem particulas2;

    private bool rotar = false;
    private bool girado = false;
    private Quaternion rotacionInicial;
    private Quaternion rotacionFinal;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    void Start()
    {
        if (objetoARotar != null)
        {
            rotacionInicial = objetoARotar.rotation;
            rotacionFinal = Quaternion.Euler(0, anguloY, 0) * rotacionInicial;
        }

        if (particulas1 != null)
            particulas1.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (particulas2 != null)
            particulas2.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Configurar el interactable de XR
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        interactable.activated.AddListener(ctx => ActivarAccion());
    }

    void Update()
    {
        if (Keyboard.current.nKey.wasPressedThisFrame)
            ActivarAccion();

        if (rotar && objetoARotar != null)
        {
            Quaternion destino = girado ? rotacionFinal : rotacionInicial;
            objetoARotar.rotation = Quaternion.RotateTowards(objetoARotar.rotation, destino, velocidadRotacion * Time.deltaTime);

            if (Quaternion.Angle(objetoARotar.rotation, destino) < 0.1f)
                rotar = false;
        }
    }

    public void ActivarAccion()
    {
        rotar = true;

        if (!girado)
        {
            if (particulas1 != null) particulas1.Play();
            if (particulas2 != null) particulas2.Play();
        }
        else
        {
            if (particulas1 != null) particulas1.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (particulas2 != null) particulas2.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        girado = !girado;
    }
}