using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[DisallowMultipleComponent]
public class AutoGrabPoint : MonoBehaviour
{
    [Header("Ajustes básicos")]
    public bool autoAttachOnTrigger = false;
    public string pinzasTipTag = "Pinzas";

    public enum AttachMode { Reparent, Follow }
    [Header("Modo de adjunte")]
    public AttachMode attachMode = AttachMode.Reparent;

    [Header("Soltado")]
    public bool transferVelocityOnRelease = true;

    // Estado interno
    private Transform vasoRoot;
    private Rigidbody vasoRb;
    private bool agarrado = false;
    private Transform agarradoPor;
    private PinzasController pinzasControllerRef;

    // Guardar estado original del Rigidbody
    private bool originalUseGravity;
    private bool originalIsKinematic;
    private RigidbodyInterpolation originalInterpolation;
    private CollisionDetectionMode originalCollisionMode;
    private RigidbodyConstraints originalConstraints;

    // Follow mode
    private bool followActivo = false;
    private Transform followTarget;
    private Vector3 followPosOffset;
    private Quaternion followRotOffset;

    public bool IsGrabbed => agarrado;

    private void OnTriggerEnter(Collider other)
    {
        if (!autoAttachOnTrigger) return;
        if (agarrado) return;
        if (!other.CompareTag(pinzasTipTag)) return;

        TryAttach_Internal(other.transform, other.GetComponentInParent<PinzasController>());
    }

    private void OnTriggerExit(Collider other)
    {
        if (agarrado && other.transform == agarradoPor && pinzasControllerRef == null)
        {
            Release();
        }
    }

    public bool AttachTo(Transform tip, PinzasController controller)
    {
        if (agarrado || tip == null) return agarrado;
        return TryAttach_Internal(tip, controller);
    }

    public void Release()
    {
        if (!agarrado) return;
    
        // Quitar vínculo o apagar follow
        if (attachMode == AttachMode.Reparent)
        {
            if (vasoRoot != null)
                vasoRoot.SetParent(null, true);
        }
        else
        {
            followActivo = false;
            followTarget = null;
        }
    
        bool snappeado = false;
    
        if (vasoRb != null)
        {
            // Restaurar propiedades originales del Rigidbody
            vasoRb.isKinematic = originalIsKinematic;
            vasoRb.interpolation = originalInterpolation;
            vasoRb.collisionDetectionMode = originalCollisionMode;
            vasoRb.constraints = originalConstraints;
    
            // --- NUEVO: Si no hay pinzas, devolvemos la gravedad ---
            if (pinzasControllerRef == null)
                vasoRb.useGravity = true;
    
            // (Opcional) Transferir velocidad del tip al soltar
            if (transferVelocityOnRelease && agarradoPor != null)
            {
                var tipRb = agarradoPor.GetComponentInParent<Rigidbody>();
                if (tipRb != null)
                {
                    vasoRb.linearVelocity = tipRb.linearVelocity;
                    vasoRb.angularVelocity = tipRb.angularVelocity;
                }
            }
    
            vasoRb.WakeUp();
        }
    
        if (pinzasControllerRef != null && pinzasControllerRef.objetoAgarrado == this)
            pinzasControllerRef.objetoAgarrado = null;
    
        agarrado = false;
        Transform vasoRootCopy = vasoRoot; // Guardamos referencia para la coroutine
        vasoRoot = null;
        vasoRb = null;
        agarradoPor = null;
        pinzasControllerRef = null;
    
        // Iniciamos coroutine para SnapPoint después de restaurar físicas
        if (vasoRootCopy != null)
            StartCoroutine(TrySnapNextFrame(vasoRootCopy));
    }

    private IEnumerator TrySnapNextFrame(Transform tubo)
    {
        // Esperamos un frame para que las físicas se estabilicen
        yield return new WaitForFixedUpdate();

        Collider[] hits = Physics.OverlapSphere(tubo.position, 0.1f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("SnapPoint"))
            {
                var snap = hit.GetComponent<SimpleSnapPoint>();
                if (snap != null)
                {
                    snap.AttemptSnap(tubo.gameObject);
                    break;
                }
            }
        }
    }

    private bool TryAttach_Internal(Transform tip, PinzasController controller)
    {
        vasoRoot = transform.root;
        if (vasoRoot == null) return false;
    
        vasoRb = vasoRoot.GetComponent<Rigidbody>();
        if (vasoRb == null) return false;
    
        // Guardar estado original para restaurar al soltar
        originalUseGravity = vasoRb.useGravity;
        originalIsKinematic = vasoRb.isKinematic;
        originalInterpolation = vasoRb.interpolation;
        originalCollisionMode = vasoRb.collisionDetectionMode;
        originalConstraints = vasoRb.constraints;
    
        // Ajustar físicas según quien agarra
        if (controller != null) // agarrado por pinzas
        {
            vasoRb.useGravity = false;
            vasoRb.isKinematic = true;
            vasoRb.interpolation = RigidbodyInterpolation.None;
        }
        else // agarrado por mano
        {
            vasoRb.useGravity = false;
            vasoRb.isKinematic = false;
            vasoRb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    
        // Asegurar que esté en la misma escena que el tip
        var targetScene = tip.gameObject.scene;
        if (vasoRoot.gameObject.scene != targetScene)
        {
            SceneManager.MoveGameObjectToScene(vasoRoot.gameObject, targetScene);
        }
    
        // Vincular según el modo
        if (attachMode == AttachMode.Reparent)
        {
            vasoRoot.SetParent(tip, true);
        }
        else
        {
            followTarget = tip;
            followPosOffset = Quaternion.Inverse(followTarget.rotation) * (vasoRoot.position - followTarget.position);
            followRotOffset = Quaternion.Inverse(followTarget.rotation) * vasoRoot.rotation;
            followActivo = true;
        }
    
        agarrado = true;
        agarradoPor = tip;
        pinzasControllerRef = controller;
        if (pinzasControllerRef != null)
            pinzasControllerRef.objetoAgarrado = this;
    
        return true;
    }

    private void LateUpdate()
    {
        if (!followActivo || followTarget == null || vasoRoot == null) return;

        var worldRot = followTarget.rotation * followRotOffset;
        var worldPos = followTarget.position + followTarget.rotation * followPosOffset;
        vasoRoot.SetPositionAndRotation(worldPos, worldRot);
    }

    private void OnDisable()
    {
        if (agarrado) Release();
    }
}
