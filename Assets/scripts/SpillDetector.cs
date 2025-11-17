using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SpillDetector : MonoBehaviour
{
    [Header("Sistema de partículas")]
    public ParticleSystem particleSystem;

    [Header("Prefab de charco")]
    public GameObject puddlePrefab;

    [Header("Escalado")]
    public float growAmount = 0.1f; // cuánto crece por trigger

    [Header("Control de frecuencia")]
    public int particlesPerStep = 25; // cada cuántas partículas se instancia o crece el charco

    private Dictionary<Transform, GameObject> activePuddles = new Dictionary<Transform, GameObject>();
    private Dictionary<Transform, int> particleCounters = new Dictionary<Transform, int>(); // contador por superficie
    private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    private void OnParticleCollision(GameObject other)
    {
        if (!other.TryGetComponent<SurfaceReceiver>(out var receiver)) return;

        int numEvents = particleSystem.GetCollisionEvents(other, collisionEvents);

        if (!particleCounters.ContainsKey(receiver.transform))
            particleCounters[receiver.transform] = 0;

        for (int i = 0; i < numEvents; i++)
        {
            particleCounters[receiver.transform]++;

            if (particleCounters[receiver.transform] >= particlesPerStep)
            {
                // Obtener color de la partícula
                Color particleColor = particleSystem.main.startColor.color;

                SpawnOrGrowPuddle(collisionEvents[i].intersection, receiver.transform, particleColor);

                particleCounters[receiver.transform] = 0; // reiniciar contador
            }
        }
    }

    private void SpawnOrGrowPuddle(Vector3 position, Transform surface, Color particleColor)
    {
        GameObject puddle;

        Vector3 spawnPos = position + new Vector3(0, 0.01f, 0); // elevar charco 1 cm

        if (activePuddles.ContainsKey(surface))
        {
            puddle = activePuddles[surface];
            Vector3 newScale = puddle.transform.localScale;
            newScale.x += growAmount;
            newScale.y += growAmount; // crecer en X e Y
            puddle.transform.localScale = newScale;
        }
        else
        {
            puddle = Instantiate(puddlePrefab, spawnPos, Quaternion.Euler(90, 0, 0), surface);
            puddle.transform.localScale = Vector3.one;

            if (puddle.TryGetComponent<MeshRenderer>(out var rend))
            {
                rend.material = new Material(rend.material);
                rend.material.color = particleColor;
            }

            activePuddles.Add(surface, puddle);
        }
    }
}
