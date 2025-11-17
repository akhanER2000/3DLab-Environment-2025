using UnityEngine;
using System.Collections.Generic;

public class LightingGridGenerator : MonoBehaviour
{
    [Header("Grid Configuration")]
    [SerializeField] private GameObject lightPrefab;
    [SerializeField] private Vector2 roomSize = new Vector2(10, 10);
    [SerializeField] private float ceilingHeight = 3f;
    [SerializeField] private Vector2 lightSpacing = new Vector2(2.5f, 2.5f);
    [SerializeField] private Vector2 offset = new Vector2(1f, 1f);

    [Header("Generation")]
    [SerializeField] private bool autoGenerate = false;
    private List<GameObject> generatedLights = new List<GameObject>();

    [ContextMenu("Generate Lighting Grid")]
    public void GenerateLightingGrid()
    {
        ClearExistingLights();

        int lightsX = Mathf.FloorToInt(roomSize.x / lightSpacing.x);
        int lightsZ = Mathf.FloorToInt(roomSize.y / lightSpacing.y);

        Vector3 startPos = new Vector3(
            -roomSize.x / 2f + offset.x,
            ceilingHeight,
            -roomSize.y / 2f + offset.y
        );

        for (int x = 0; x < lightsX; x++)
        {
            for (int z = 0; z < lightsZ; z++)
            {
                Vector3 position = startPos + new Vector3(
                    x * lightSpacing.x,
                    0,
                    z * lightSpacing.y
                );

                GameObject light = Instantiate(lightPrefab, position,
                    Quaternion.identity, transform);
                light.name = $"LabLight_{x}_{z}";
                generatedLights.Add(light);
            }
        }

        Debug.Log($"Generated {generatedLights.Count} lights");
    }

    [ContextMenu("Clear Lighting Grid")]
    public void ClearExistingLights()
    {
        for (int i = generatedLights.Count - 1; i >= 0; i--)
        {
            if (generatedLights[i] != null)
            {
                if (Application.isPlaying)
                    Destroy(generatedLights[i]);
                else
                    DestroyImmediate(generatedLights[i]);
            }
        }
        generatedLights.Clear();

        // Limpiar hijos huérfanos
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(transform.GetChild(i).gameObject);
            else
                DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            new Vector3(0, ceilingHeight, 0),
            new Vector3(roomSize.x, 0.1f, roomSize.y)
        );

        // Mostrar grid
        Gizmos.color = Color.green;
        int lightsX = Mathf.FloorToInt(roomSize.x / lightSpacing.x);
        int lightsZ = Mathf.FloorToInt(roomSize.y / lightSpacing.y);

        Vector3 startPos = new Vector3(
            -roomSize.x / 2f + offset.x,
            ceilingHeight,
            -roomSize.y / 2f + offset.y
        );

        for (int x = 0; x < lightsX; x++)
        {
            for (int z = 0; z < lightsZ; z++)
            {
                Vector3 position = startPos + new Vector3(
                    x * lightSpacing.x,
                    0,
                    z * lightSpacing.y
                );
                Gizmos.DrawWireSphere(position, 0.2f);
            }
        }
    }
}