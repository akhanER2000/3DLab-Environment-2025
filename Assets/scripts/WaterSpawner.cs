using UnityEngine;

public class WaterSpawner : MonoBehaviour
{
    public GameObject waterPrefab; // Prefab de la gota
    public float spawnRate = 0.1f; // Intervalo entre "oleadas"
    public int dropletsPerSpawn = 3; // Número de gotas por oleada
    public float spawnAreaRadius = 0.01f; // Para que no todas salgan del mismo punto exacto

    private bool isSpawning = false;

    private void Start()
    {
        // Desactivado al inicio
    }

    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            InvokeRepeating(nameof(SpawnDroplets), 0f, spawnRate);
        }
    }

    public void StopSpawning()
    {
        if (isSpawning)
        {
            isSpawning = false;
            CancelInvoke(nameof(SpawnDroplets));
        }
    }

    private void SpawnDroplets()
    {
        for (int i = 0; i < dropletsPerSpawn; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * spawnAreaRadius;
            Instantiate(waterPrefab, transform.position + randomOffset, Quaternion.identity);
        }
    }
}
