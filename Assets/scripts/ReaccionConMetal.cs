using UnityEngine;

public class ReaccionConMetal : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject zincDentro;      // El hijo invisible (zinc dentro del vaso)
    public ParticleSystem burbujas;    // Partículas de reacción
    public WaterControl aguaControl;   // Referencia al script del líquido

    [Header("Configuración")]
    public string objetoReaccionTag = "Zinc"; // Tag del objeto que debe entrar al vaso

    private bool reaccionActiva = false;
    private bool zincPresente = false;

    void Start()
    {
        // Asegurar que el zinc dentro empieza invisible
        if (zincDentro != null && zincDentro.activeSelf)
            zincDentro.SetActive(false);

        // Asegurar que las burbujas empiezan apagadas
        if (burbujas != null && burbujas.gameObject.activeSelf)
            burbujas.gameObject.SetActive(false);
    }

    void Update()
    {
        // Revisar constantemente si ya hay zinc y agua suficiente
        if (zincPresente && !reaccionActiva && aguaControl != null)
        {
            if (aguaControl.GetFillValue() > aguaControl.fillMin)
            {
                ActivarReaccion();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Si el objeto que entra tiene el tag correcto
        if (other.CompareTag(objetoReaccionTag) && !zincPresente)
        {
            zincPresente = true;

            // 1) Hacer visible el zinc dentro del tubo
            if (zincDentro != null)
                zincDentro.SetActive(true);

            // 2) 🔻 Desaparecer el objeto que tocó el tubo (la lenteja en la punta)
            other.gameObject.SetActive(false);

            // 3) Revisar inmediatamente si ya hay agua suficiente
            if (aguaControl != null && aguaControl.GetFillValue() > aguaControl.fillMin)
            {
                ActivarReaccion();
            }
        }
    }

    void ActivarReaccion()
    {
        reaccionActiva = true;

        if (burbujas != null)
        {
            burbujas.gameObject.SetActive(true);
            burbujas.Play();
        }
    }
}