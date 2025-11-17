using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Renderer))]
public class Whiteboard : MonoBehaviour
{
    public enum Axis { X, Y, Z }

    [Header("Canvas")]
    public int textureWidth = 1024;
    public int textureHeight = 1024;
    public string texturePropertyName = "_BaseMap";
    public Color clearColor = Color.white;
    public bool useMipMaps = false;

    [Header("Mapeo UV")]
    public Axis uAxis = Axis.X;
    public Axis vAxis = Axis.Y;
    public bool flipU = false;
    public bool flipV = false;

    [Header("Debug")]
    public Texture2D canvasTexture;

    private Renderer rend;
    private MeshFilter meshFilter;
    
    // --- VARIABLES CLAVE PARA LA OPTIMIZACIÓN ---
    private Color32[] pixelData; // Nuestra copia de los píxeles en la CPU
    private Color32[] clearArray; // Un array pre-calculado con el color de fondo
    private bool needsApply = false;
    // ---

    void Awake()
    {
        rend = GetComponent<Renderer>();
        meshFilter = GetComponent<MeshFilter>();

        if (rend.sharedMaterial != null)
        {
            rend.material = new Material(rend.sharedMaterial);
        }

        InitTexture();
    }

    public void InitTexture()
    {
        if (canvasTexture != null)
            Destroy(canvasTexture);

        canvasTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, useMipMaps);
        
        clearArray = new Color32[textureWidth * textureHeight];
        for (int i = 0; i < clearArray.Length; i++) clearArray[i] = clearColor;

        canvasTexture.SetPixels32(clearArray);
        canvasTexture.Apply(false, false);
        pixelData = canvasTexture.GetPixels32(); // ¡La copia se hace UNA SOLA VEZ aquí!

        rend.material.SetTexture(texturePropertyName, canvasTexture);
    }

    public void Clear()
    {
        if (canvasTexture == null || pixelData == null || clearArray == null) return;
        System.Buffer.BlockCopy(clearArray, 0, pixelData, 0, clearArray.Length * 4);
        needsApply = true;
    }
    
    public void PaintUV(Vector2 uv, Color color, int brushRadius)
    {
        if (canvasTexture == null) return;

        int cx = Mathf.RoundToInt(uv.x * (textureWidth - 1));
        int cy = Mathf.RoundToInt(uv.y * (textureHeight - 1));

        int r = Mathf.Max(1, brushRadius);
        int r2 = r * r;

        // Modificamos directamente nuestro array 'pixelData' en la CPU (esto es ultra rápido)
        for (int y = -r; y <= r; y++)
        {
            if (cy + y < 0 || cy + y >= textureHeight) continue;
            
            for (int x = -r; x <= r; x++)
            {
                if (cx + x < 0 || cx + x >= textureWidth) continue;

                if (x * x + y * y <= r2)
                {
                    pixelData[(cy + y) * textureWidth + (cx + x)] = color;
                }
            }
        }

        needsApply = true;
    }

    public void PaintLine(Vector2 fromUV, Vector2 toUV, Color color, int brushRadius)
    {
        if (canvasTexture == null) return;
        
        int x0 = Mathf.RoundToInt(fromUV.x * (textureWidth - 1));
        int y0 = Mathf.RoundToInt(fromUV.y * (textureHeight - 1));
        int x1 = Mathf.RoundToInt(toUV.x * (textureWidth - 1));
        int y1 = Mathf.RoundToInt(toUV.y * (textureHeight - 1));

        float dx = x1 - x0;
        float dy = y1 - y0;
        float dist = Mathf.Sqrt(dx * dx + dy * dy);
        int steps = Mathf.CeilToInt(dist / brushRadius) + 1;
        steps = Mathf.Max(1, steps);

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 uv = Vector2.Lerp(fromUV, toUV, t);
            PaintUV(uv, color, brushRadius);
        }
    }

    void LateUpdate()
    {
        if (needsApply && canvasTexture != null)
        {
            // Enviamos nuestro array modificado a la GPU una sola vez por fotograma
            canvasTexture.SetPixels32(pixelData);
            canvasTexture.Apply(false, false);
            needsApply = false;
        }
    }
    
    public bool TryWorldToUV(Vector3 worldPoint, out Vector2 uv)
    {
        uv = Vector2.zero;
        if (meshFilter == null || meshFilter.sharedMesh == null) return false;

        var local = transform.InverseTransformPoint(worldPoint);
        var bounds = meshFilter.sharedMesh.bounds;

        float GetAxis(Axis a, Vector3 v)
        {
            switch (a) { case Axis.X: return v.x; case Axis.Y: return v.y; case Axis.Z: return v.z; }
            return v.x;
        }

        float u = Mathf.InverseLerp(bounds.min[(int)uAxis], bounds.max[(int)uAxis], GetAxis(uAxis, local));
        float v = Mathf.InverseLerp(bounds.min[(int)vAxis], bounds.max[(int)vAxis], GetAxis(vAxis, local));

        if (flipU) u = 1f - u;
        if (flipV) v = 1f - v;

        uv = new Vector2(u, v);
        return true;
    }
}