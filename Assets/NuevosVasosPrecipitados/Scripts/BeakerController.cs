using System.Collections.Generic;
using UnityEngine;

public class BeakerController : MonoBehaviour
{
    [Header("Visual (powder)")]
    public GameObject powderLayerPrefab;   // prefab: cilindro muy plano
    public Transform powderRoot;           // hijo ubicado en el fondo interior del vaso
    public float layerThickness = 0.01f;   // grosor (m) de cada "capa" por cucharada
    public float layerRadius = 0.03f;      // radio en metros (ajusta seg�n tu vaso)
    public int maxLayers = 50;

    [Header("Masa y conteo")]
    public float gramsPerSpoon = 5f;       // 5 gramos por cucharada (tu requisito)
    public int particlesPerSpoon = 20;     // si usas conteo de part�culas (fallback)
    public Rigidbody attachedRigidbody;    // opcional: asignar Rigidbody del vaso

    // estado interno
    private List<GameObject> layers = new List<GameObject>();
    private int currentParticleCount = 0;
    public float contentMassGrams = 0f;    // masa acumulada en gramos
    private float initialMassKg = 0f;

    private void Start()
    {
        if (powderRoot == null)
            Debug.LogWarning($"[{name}] powderRoot no asignado.");

        if (attachedRigidbody == null)
            attachedRigidbody = GetComponent<Rigidbody>();

        if (attachedRigidbody != null)
            initialMassKg = attachedRigidbody.mass;
    }

    // M�todo p�blico que agrega 1 cucharada (llamar desde la cuchara o desde el detector)
    public void AddSpoonful()
    {
        // 1) actualizar masa en gramos
        contentMassGrams += gramsPerSpoon; // ej: si antes eran 0 -> ahora 5

        // 2) crear visual (una "capa" plana)
        CreatePowderLayer();

        // 3) actualizar masa f�sica (Rigidbody) en kg si corresponde
        UpdateRigidbodyMass();

        Debug.Log($"[{name}] A�adida 1 cucharada: +{gramsPerSpoon} g -> total {contentMassGrams} g");
    }

    private void CreatePowderLayer()
    {
        if (powderLayerPrefab == null || powderRoot == null) return;
        if (layers.Count >= maxLayers) { Debug.Log($"[{name}] Max capas alcanzado."); return; }

        GameObject layer = Instantiate(powderLayerPrefab, powderRoot);
        // Ajustes de escala/posicion considerando el prefab cylinder de Unity (height por defecto = 2)
        float desiredRadius = layerRadius;        // metros
        float desiredThickness = layerThickness;  // metros
        // Para Unity Primitive Cylinder: default height = 2 unidades, diameter = 1.
        // Por eso:
        float scaleX = desiredRadius * 2f;                // di�metro
        float scaleZ = desiredRadius * 2f;
        float scaleY = desiredThickness / 2f;             // porque la primitive tiene height = 2
        layer.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);

        // colocar la capa de modo que su base quede en:
        float yPos = layers.Count * desiredThickness + (desiredThickness / 2f);
        layer.transform.localPosition = new Vector3(0f, yPos, 0f);
        layer.transform.localRotation = Quaternion.identity;

        layers.Add(layer);
        Debug.Log("Se ha creado una capa");
    }

    private void UpdateRigidbodyMass()
    {
        if (attachedRigidbody == null) return;
        // conversi�n paso a paso:
        // contentMassGrams (g) -> contentMassKg = contentMassGrams / 1000
        float addedKg = contentMassGrams / 1000f;
        attachedRigidbody.mass = initialMassKg + addedKg;
        // ej: initialMassKg = 0.2 kg, contentMassGrams = 10 g -> addedKg = 10 / 1000 = 0.01 -> mass = 0.21 kg
    }



    // Fallback 2: si la cuchara/water usa ParticleSystem con colisiones y Send Collision Messages
    // OnParticleCollision recibe el GameObject del ParticleSystem que colision� contra este objeto.
    private void OnParticleCollision(GameObject other)
    {
        string nameLower = other.name.ToLower();

        // detectar agua (el nombre o tag del PS debe indicar que es water)
        if (other.CompareTag("Water") || nameLower.Contains("water"))
        {
            // cuando detectamos agua, eliminamos las capas visuales del reactivo
            RemovePowderVisual();
        }

        // detectar part�culas de cuchara (si el PS de la cuchara tiene tag o nombre)
        if (other.CompareTag("SpoonParticleSystem") || nameLower.Contains("spoon"))
        {
            currentParticleCount++;
            if (currentParticleCount >= particlesPerSpoon)
            {
                currentParticleCount = 0;
                AddSpoonful();
            }
        }
    }

    // elimina todas las capas visuales (el reactivo "desaparece" visualmente)
    public void RemovePowderVisual()
    {
        foreach (var g in layers) if (g != null) Destroy(g);
        layers.Clear();
        //Debug.Log($"[{name}] reactivo visual removido por detecci�n de agua.");
        // NOTA: la masa en gramos (contentMassGrams) se mantiene (el reactivo sigue existiendo, ahora disuelto).
    }
}
