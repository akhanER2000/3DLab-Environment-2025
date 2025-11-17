using UnityEngine;

public class TrailHandler : MonoBehaviour
{
    private Vector3 objectDimensions;         // Dimensiones del objeto
    private bool isObjectColliding = false;   // Estado de colisión
    private LineRenderer activeTrail;         // El LineRenderer actual para la colisión
    private Material objectTrailMaterial;     // Material para el rastro
    private Rigidbody objectRigidbody;        // Referencia al Rigidbody

    public bool isObjectGrabbed = false;      // Estado de agarre por el VR
    public float objectFollowSpeed = 10f;     // Velocidad de seguimiento al ser agarrado

    private Vector3 targetPosition;           // Posición objetivo para el objeto

    void Start()
    {
        // Obtener las dimensiones del objeto desde el BoxCollider
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        objectRigidbody = GetComponent<Rigidbody>();

        if (boxCollider != null)
        {
            objectDimensions = boxCollider.size;
        }
        else
        {
            Debug.LogError("El objeto no tiene un BoxCollider.");
            return;
        }

        // Crear el material para los rastros
        objectTrailMaterial = new Material(Shader.Find("Sprites/Default"));
        objectTrailMaterial.color = Color.red;

        if (objectRigidbody == null)
        {
            Debug.LogError("El objeto no tiene un Rigidbody.");
        }
    }

    void Update()
    {
        if (isObjectGrabbed)
        {
            Vector3 direction = targetPosition - transform.position;
            objectRigidbody.linearVelocity = direction * objectFollowSpeed;
            return;
        }

        if (isObjectColliding && activeTrail != null)
        {
            Vector3 trailPosition = transform.position + new Vector3(0, -objectDimensions.y / 2, 0);

            int nextIndex = activeTrail.positionCount;
            activeTrail.positionCount = nextIndex + 1;
            activeTrail.SetPosition(nextIndex, trailPosition);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isObjectGrabbed) return;

        isObjectColliding = true;

        activeTrail = new GameObject("Trail").AddComponent<LineRenderer>();
        activeTrail.material = objectTrailMaterial;
        activeTrail.startColor = Color.red;
        activeTrail.endColor = Color.red;

        activeTrail.startWidth = 0.05f;
        activeTrail.endWidth = 0.05f;

        activeTrail.positionCount = 0;
    }

    void OnCollisionStay(Collision collision)
    {
        if (isObjectGrabbed) return;
        isObjectColliding = true;
    }

    void OnCollisionExit(Collision collision)
    {
        if (isObjectGrabbed) return;
        isObjectColliding = false;
        activeTrail = null;
    }

    public void GrabObject(Vector3 grabPosition)
    {
        isObjectGrabbed = true;
        targetPosition = grabPosition;

        objectRigidbody.isKinematic = false;
        objectRigidbody.useGravity = false;
    }

    public void ReleaseObject()
    {
        isObjectGrabbed = false;
        objectRigidbody.linearVelocity = Vector3.zero;
        objectRigidbody.useGravity = true;
    }

    public void UpdateGrabPosition(Vector3 newGrabPosition)
    {
        targetPosition = newGrabPosition;
    }
}