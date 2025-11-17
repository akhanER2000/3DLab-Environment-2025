using UnityEngine;
using System.Collections.Generic;

public class GestorExperimento : MonoBehaviour
{
    [Header("Referencias a los Botones")]
    public GameObject botonIniciar;
    public GameObject botonTerminar;

    [Header("Prefabs Iniciales del Experimento")]
    public List<GameObject> prefabsInicialesDelExperimento = new List<GameObject>();

    [Header("Prefabs de Repuesto (También se instancian al inicio)")] // Texto de ayuda modificado
    public List<GameObject> prefabsDeRepuesto = new List<GameObject>();

    [Header("Referencias externas")]
    public GameObject evaluationPanel;

    [Header("Otros Experimentos en la Escena")]
    public GestorExperimento[] otrosControladores;

    // --- CAMBIO 1: Renombrar lista para claridad ---
    private List<GameObject> objetosInstanciadosActivos = new List<GameObject>(); // Para gestionar los objetos creados

    // --- FIN CAMBIO 1 ---

    [Header("Trofeo de este experimento")]
    public GameObject trofeoAsociado;

    [Header("Audio de desbloqueo")]
    public AudioSource audioSource;
    public AudioClip sonidoDesbloqueo;
    public AudioClip sonidoVictoria;

    void Awake()
    {
        if (trofeoAsociado) trofeoAsociado.SetActive(false);
    }

    void Start()
    {
        EstadoInactivo();
        if (trofeoAsociado && trofeoAsociado.activeSelf)
            trofeoAsociado.SetActive(false);
    }

    public void IniciarExperimento()
    {
        botonIniciar.SetActive(false);
        botonTerminar.SetActive(true);

        if (evaluationPanel != null)
        {
            evaluationPanel.SetActive(true);
            Transform player = Camera.main.transform;
            evaluationPanel.transform.position = player.position + player.forward * 1.5f;
            evaluationPanel.transform.LookAt(player);
        }

        foreach (GestorExperimento otroControlador in otrosControladores)
        {
            if (otroControlador != null)
            {
                otroControlador.OcultarAmbosBotones();
            }
        }

        // --- CAMBIO 2: Instanciar prefabs de AMBAS listas ---
        objetosInstanciadosActivos.Clear(); // Limpiar lista antes de instanciar

        // Instanciar Prefabs Iniciales
        foreach (GameObject prefabInicial in prefabsInicialesDelExperimento)
        {
            if (prefabInicial != null)
            {
                GameObject nuevoObjeto = Instantiate(
                    prefabInicial,
                    prefabInicial.transform.position, // Considera usar posiciones específicas si es necesario
                    prefabInicial.transform.rotation
                );
                objetosInstanciadosActivos.Add(nuevoObjeto);
                nuevoObjeto.SetActive(true);
            }
        }

        // Instanciar Prefabs de Repuesto
        foreach (GameObject prefabRepuesto in prefabsDeRepuesto)
        {
            if (prefabRepuesto != null)
            {
                GameObject nuevoObjetoRepuesto = Instantiate(
                    prefabRepuesto,
                    prefabRepuesto.transform.position, // ¡IMPORTANTE! ¿Dónde deben aparecer?
                    prefabRepuesto.transform.rotation
                );
                objetosInstanciadosActivos.Add(nuevoObjetoRepuesto);
                nuevoObjetoRepuesto.SetActive(true);
            }
        }
        // --- FIN CAMBIO 2 ---
    }

    public void TerminarExperimento()
    {
        // --- CAMBIO 3: Usar la lista renombrada para destruir ---
        foreach (GameObject obj in objetosInstanciadosActivos)
        {
            if (obj != null) Destroy(obj);
        }
        objetosInstanciadosActivos.Clear();
        // --- FIN CAMBIO 3 ---

        DesbloquearTrofeo();
        EstadoInactivo();

        foreach (GestorExperimento otroControlador in otrosControladores)
        {
            if (otroControlador != null)
            {
                otroControlador.EstadoInactivo();
            }
        }

        if (evaluationPanel != null)
        {
            evaluationPanel.SetActive(false);
        }
    }

    public void EstadoInactivo()
    {
        botonIniciar.SetActive(true);
        botonTerminar.SetActive(false);
        if (evaluationPanel != null) evaluationPanel.SetActive(false);
    }

    public void OcultarAmbosBotones()
    {
        botonIniciar.SetActive(false);
        botonTerminar.SetActive(false);
    }

    private void DesbloquearTrofeo()
    {
        if (trofeoAsociado)
        {
            trofeoAsociado.SetActive(true);
        }
        if (audioSource && sonidoVictoria)
        {
            audioSource.PlayOneShot(sonidoVictoria);
        }
    }

    // La función ObtenerPrefabDeRepuesto ya no es necesaria si todo se carga al inicio,
    // pero puedes dejarla por si cambian los requisitos de nuevo.
    public GameObject ObtenerPrefabDeRepuesto(string nombreObjeto)
    {
        foreach (GameObject prefabRepuesto in prefabsDeRepuesto)
        {
            if (prefabRepuesto != null && prefabRepuesto.name == nombreObjeto)
            {
                return prefabRepuesto;
            }
        }
        Debug.LogWarning($"No se encontró un prefab de repuesto llamado: {nombreObjeto}");
        return null;
    }
}