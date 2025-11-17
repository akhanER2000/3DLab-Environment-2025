using UnityEngine;

[DisallowMultipleComponent]
public class PinzasController : MonoBehaviour
{
    [Header("Referencias")]
    public Transform tipTransform;
    public TipSensor tipSensor;

    [HideInInspector] public AutoGrabPoint objetoAgarrado;

    public void OnActivate()
    {
        if (objetoAgarrado != null || tipSensor == null || tipTransform == null) return;

        var candidato = tipSensor.GetClosestCandidate(tipTransform.position);
        if (candidato != null)
        {
            candidato.AttachTo(tipTransform, this);
        }
    }

    public void OnDeactivate()
    {
        Soltar();
    }

    public void OnSelectExited()
    {
        Soltar();
    }

    public void Soltar()
    {
        if (objetoAgarrado != null)
        {
            objetoAgarrado.Release();
            objetoAgarrado = null;
        }
    }
}