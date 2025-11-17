using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AccionPorTag
{
    public string tag;                      // Tag que debe detectar
    public bool activaParticulas;           // Activa partículas de la cuchara
    public bool desactivaParticulas;        // Desactiva partículas de la cuchara
    public bool avisarAgua;                 // Llamar a ControlAgua.AnadirCompuesto()
    public bool avisarBeaker;               // Llamar a BeakerController.AddSpoonful()
    public string mensaje;                   // Mensaje que se preparará
}

public class ControlCuchara : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Arrastra aquí el Sistema de Partículas que es hijo de la cuchara.")]
    public ParticleSystem particulasCompuesto;

    [Header("Sonidos")]
    public AudioClip sonidoRecoger;
    public AudioClip sonidoDisolver;

    [Header("Acciones por Tag")]
    public List<AccionPorTag> accionesPorTag = new List<AccionPorTag>();

    private bool tieneCompuesto = false;
    private string mensajePreparado; // Mensaje guardado al chocar con un objeto con tag específico

    void Start()
    {
        if (particulasCompuesto != null)
        {
            var rend = particulasCompuesto.GetComponent<Renderer>();
            if (rend != null) rend.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        foreach (var accion in accionesPorTag)
        {
            if (other.CompareTag(accion.tag))
            {
                Debug.Log($"Cuchara entró en contacto con {other.tag}");

                if (accion.activaParticulas && particulasCompuesto != null)
                {
                    particulasCompuesto.GetComponent<Renderer>().enabled = true;
                    tieneCompuesto = true;
                    if (sonidoRecoger != null)
                        AudioSource.PlayClipAtPoint(sonidoRecoger, transform.position);
                }

                if (accion.desactivaParticulas && particulasCompuesto != null)
                {
                    particulasCompuesto.GetComponent<Renderer>().enabled = false;
                    tieneCompuesto = false;
                    if (sonidoDisolver != null)
                        AudioSource.PlayClipAtPoint(sonidoDisolver, transform.position);
                }

                if (accion.avisarAgua)
                {
                    ControlAgua agua = other.GetComponentInParent<ControlAgua>();
                    if (agua != null) agua.AnadirCompuesto();
                }

                if (accion.avisarBeaker)
                {
                    BeakerController beaker = other.GetComponentInParent<BeakerController>();
                    if (beaker != null) beaker.AddSpoonful();
                }

                if (!string.IsNullOrEmpty(accion.mensaje))
                {
                    mensajePreparado = accion.mensaje;
                    Debug.Log($"Cuchara preparó el mensaje: {mensajePreparado}");
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!string.IsNullOrEmpty(mensajePreparado))
        {
            VasoPrecipitado vaso = collision.gameObject.GetComponentInParent<VasoPrecipitado>();
            if (vaso != null)
            {
                vaso.RecibirMensajeDeColision(mensajePreparado);
                mensajePreparado = null;
            }
        }
    }

    // 🔹 Nuevo método público para que otros scripts puedan pedir el mensaje
    public string GetMensajePreparado()
    {
        return mensajePreparado;
    }
}