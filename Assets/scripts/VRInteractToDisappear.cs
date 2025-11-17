using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

public class XRInteractToDisappear : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    public AudioClip grabSound;
    public RawImage rawImage;

    [Header("Mensajes y Receptores")]
    public string mensaje; // Mensaje configurable en el inspector
    public List<MonoBehaviour> receptores; // Lista de receptores (Puerta, Advertencia, etc.)

    private bool isVisible = true;
    private Renderer[] renderers;
    private Collider[] colliders;

    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrabbed);

        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();

        SetObjectVisibility(true);
        SetRawImageVisibility(false);
    }

    void Update()
    {
        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            ToggleBoth();
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        ToggleBoth();
    }

    private void ToggleBoth()
    {
        if (grabSound != null)
        {
            AudioSource.PlayClipAtPoint(grabSound, transform.position);
        }

        isVisible = !isVisible;

        SetObjectVisibility(isVisible);
        SetRawImageVisibility(!isVisible);

        // Enviar mensaje a todos los receptores
        foreach (var r in receptores)
        {
            if (r == null) continue;

            // Verifica si tiene el método RecibirMensaje
            var tipoPuerta = r as Puerta;
            if (tipoPuerta != null)
            {
                tipoPuerta.RecibirMensaje(mensaje);
                continue;
            }

            var tipoAdvertencia = r as Advertencia;
            if (tipoAdvertencia != null)
            {
                tipoAdvertencia.RecibirMensaje(mensaje);
                continue;
            }

            // Si hay otros receptores, puedes agregarlos aquí
        }
    }

    private void SetObjectVisibility(bool visible)
    {
        foreach (var rend in renderers)
            rend.enabled = visible;

        foreach (var col in colliders)
            col.enabled = visible;
    }

    private void SetRawImageVisibility(bool visible)
    {
        if (rawImage == null) return;

        Color color = rawImage.color;
        color.a = visible ? 1f : 0f;
        rawImage.color = color;
    }
}