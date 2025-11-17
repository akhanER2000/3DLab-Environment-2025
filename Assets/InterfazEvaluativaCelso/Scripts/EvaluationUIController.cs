using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Necesario para OrderBy si lo usaras
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class EvaluationUIController : MonoBehaviour
{
    #region --- Referencias UI ---
    [Header("Referencias UI")]
    public TextMeshProUGUI preguntaTexto;
    public List<Button> botonesOpciones;
    public TextMeshProUGUI botonSiguienteTexto;
    public GameObject panelEvaluacion;

    [Header("Colores de botones")]
    public Color colorNormal = new Color32(117, 28, 219, 255);
    public Color colorSeleccionado = new Color32(78, 0, 65, 255);
    #endregion

    #region --- Configuración LMS ---
    [Header("Configuración LMS")]
    public string lmsBaseUrl = "http://135.148.148.88:5253";
    public string email = "3dlab@laboratorio.edu";
    public string password = "LMS3dLab";
    public int idEvaluacion = 36;
    public string idAlumno = "";
    public string sesionId = "";
    public int preguntasPorSesionLaboratorio = 5; // Cantidad de preguntas a enviar
    private bool mostrandoResumen = false;
    #endregion

    private string tokenAcceso = "";
    private Evaluacion evaluacion;
    private int indicePreguntaActual = 0;
    private Dictionary<int, string> respuestasUsuario = new Dictionary<int, string>();

    #region --- Unity Methods ---
    void Start()
    {
        StartCoroutine(FlujoEvaluacionCompleta());
    }
    #endregion

    #region --- Flujo Principal ---
    IEnumerator FlujoEvaluacionCompleta()
    {
        // Obtener token
        bool tokenObtenido = false;
        yield return StartCoroutine(ObtenerToken(success => tokenObtenido = success));
        if (!tokenObtenido || string.IsNullOrEmpty(tokenAcceso)) yield break;

        // Extraer ID del alumno desde token
        idAlumno = ExtraerAlumnoIdDesdeToken(tokenAcceso);
        Debug.Log($"✅ AlumnoId extraído del token: {idAlumno}");

        // Obtener sesión y preguntas seleccionadas
        yield return StartCoroutine(ObtenerSesionEvaluacion());
        if (string.IsNullOrEmpty(sesionId) || evaluacion == null || evaluacion.preguntas == null || evaluacion.preguntas.Count == 0)
            yield break;

        // Mostrar primera pregunta
        MostrarPreguntaActual();
    }
    #endregion

    #region --- Token y AlumnoId ---
    string ExtraerAlumnoIdDesdeToken(string token)
    {
        string alumnoIdExtraido = "";
        string[] partes = token.Split('.');
        if (partes.Length == 3)
        {
            string payload = partes[1].Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            try
            {
                byte[] data = Convert.FromBase64String(payload);
                string json = System.Text.Encoding.UTF8.GetString(data);
                var payloadDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (payloadDict != null && payloadDict.ContainsKey("sub"))
                    alumnoIdExtraido = payloadDict["sub"].ToString();
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Error al decodificar token: {ex.Message}");
            }
        }
        return alumnoIdExtraido;
    }

    IEnumerator ObtenerToken(Action<bool> callback)
    {
        string url = $"{lmsBaseUrl}/api/v2/3dlab/token";
        var loginData = new { email, password, nombre = "Integración 3DLAB", rut = "99999999-9", rol = "Alumno" };
        string jsonBody = JsonConvert.SerializeObject(loginData);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonBody));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("X-3DLAB-Key", "3DLAB-SECRET-KEY-CHANGE-IN-PRODUCTION");
            request.certificateHandler = new BypassCertificate();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(request.downloadHandler.text);
                if (tokenResponse != null)
                {
                    tokenAcceso = tokenResponse.token;
                    callback(true);
                    yield break;
                }
            }
        }

        callback(false);
    }
    #endregion

    #region --- Sesión y Preguntas ---
    IEnumerator ObtenerSesionEvaluacion()
    {
        string url = $"{lmsBaseUrl}/api/v2/3dlab/evaluaciones/{idEvaluacion}/seleccion?alumnoId={idAlumno}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("X-3DLAB-Key", "3DLAB-SECRET-KEY-CHANGE-IN-PRODUCTION");
            request.SetRequestHeader("Content-Type", "application/json");
            request.certificateHandler = new BypassCertificate();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var respuesta = JsonConvert.DeserializeObject<SesionEvaluacionResponse>(request.downloadHandler.text);
                if (respuesta != null)
                {
                    sesionId = respuesta.sesionId;

                    // 🔹 Guardar preguntas directamente desde la sesión
                    evaluacion = new Evaluacion();
                    evaluacion.preguntas = respuesta.preguntas;
                }
            }
        }
    }
    #endregion

    #region --- UI Preguntas ---
    void MostrarPreguntaActual()
    {
        if (evaluacion == null || evaluacion.preguntas == null || evaluacion.preguntas.Count == 0) return;

        Pregunta p = evaluacion.preguntas[indicePreguntaActual];
        preguntaTexto.text = p.enunciado;

        List<string> claves = new List<string> { "a", "b", "c", "d" };

        for (int i = 0; i < botonesOpciones.Count; i++)
        {
            if (i < claves.Count && p.opciones.ContainsKey(claves[i]))
            {
                string clave = claves[i];
                string valor = p.opciones[clave];

                botonesOpciones[i].gameObject.SetActive(true);
                botonesOpciones[i].GetComponentInChildren<TextMeshProUGUI>().text = valor;
                botonesOpciones[i].onClick.RemoveAllListeners();

                int preguntaID = p.idPregunta;
                botonesOpciones[i].onClick.AddListener(() => SeleccionarOpcion(preguntaID, clave));

                botonesOpciones[i].GetComponent<Image>().color =
                    (respuestasUsuario.ContainsKey(preguntaID) && respuestasUsuario[preguntaID] == clave) ? colorSeleccionado : colorNormal;
            }
            else botonesOpciones[i].gameObject.SetActive(false);
        }

        botonSiguienteTexto.text = (indicePreguntaActual == evaluacion.preguntas.Count - 1) ? "Finalizar" : "Siguiente";
    }

    void SeleccionarOpcion(int idPregunta, string opcionSeleccionada)
    {
        respuestasUsuario[idPregunta] = opcionSeleccionada;

        List<string> claves = new List<string> { "a", "b", "c", "d" };
        for (int i = 0; i < botonesOpciones.Count; i++)
        {
            string clave = claves[i];
            if (botonesOpciones[i].gameObject.activeSelf)
                botonesOpciones[i].GetComponent<Image>().color = (clave == opcionSeleccionada) ? colorSeleccionado : colorNormal;
        }
    }

    public void BotonSiguiente()
    {
        if (mostrandoResumen) return; // Si estamos mostrando resumen, no hacemos nada

        if (indicePreguntaActual < evaluacion.preguntas.Count - 1)
        {
            indicePreguntaActual++;
            MostrarPreguntaActual();
        }
        else
        {
            FinalizarEvaluacion(); // Solo se ejecuta una vez
        }
    }

    void FinalizarEvaluacion()
    {
        StartCoroutine(EnviarRespuestasAlLMS()); // Se envían los resultados SOLO una vez
    }
    #endregion

    #region --- Envío Respuestas ---
    IEnumerator EnviarRespuestasAlLMS()
    {
        var cuerpo = new
        {
            sesionId,
            evaluacionId = idEvaluacion,
            alumnoId = idAlumno,
            respuestas = new List<object>()
        };

        foreach (var p in evaluacion.preguntas)
        {
            string seleccion = respuestasUsuario.ContainsKey(p.idPregunta)
                ? respuestasUsuario[p.idPregunta]
                : (p.opciones.ContainsKey("a") ? "a" : p.opciones.Keys.First()); // Usa "a" o la primera opción disponible
        
            ((List<object>)cuerpo.respuestas).Add(new { preguntaId = p.idPregunta, seleccion });
        }

        string jsonBody = JsonConvert.SerializeObject(cuerpo, Formatting.Indented);
        string url = $"{lmsBaseUrl}/api/v2/3dlab/resultados";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonBody));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("X-3DLAB-Key", "3DLAB-SECRET-KEY-CHANGE-IN-PRODUCTION");
            request.certificateHandler = new BypassCertificate();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var resultado = JsonConvert.DeserializeObject<LMSResultadoResponse>(request.downloadHandler.text);
                if (resultado != null)
                {
                    MostrarResumenResultados(resultado);
                }
            }
            else
            {
                Debug.LogError($"❌ Error al enviar respuestas: {request.error}\n{request.downloadHandler.text}");
            }
        }
    }
    #endregion
    void MostrarResumenResultados(LMSResultadoResponse resultado)
    {
        mostrandoResumen = true; // Activamos el flag
        preguntaTexto.text = $"✅ Puntaje: {resultado.puntajeObtenido}/{resultado.puntajeMaximo}\n✅ Calificación: {resultado.calificacion:F2}";
    
        foreach (var btn in botonesOpciones)
            btn.gameObject.SetActive(false);
    
        botonSiguienteTexto.text = "Cerrar";
    
        // Botón cerrar solo cierra la interfaz
        botonSiguienteTexto.GetComponentInParent<Button>().onClick.RemoveAllListeners();
        botonSiguienteTexto.GetComponentInParent<Button>().onClick.AddListener(() =>
        {
            panelEvaluacion.SetActive(false);
            mostrandoResumen = false; // opcional: reinicia el flag si vuelves a usar la UI
        });
    }


    #region --- Helpers ---
    class BypassCertificate : CertificateHandler { protected override bool ValidateCertificate(byte[] certificateData) => true; }

    [Serializable]
    public class SesionEvaluacionResponse
    {
        public string sesionId;
        public int evaluacionId;
        public string alumnoId;
        public List<Pregunta> preguntas; // 🔹 Ahora incluye la lista de preguntas de la sesión
    }
    #endregion
}

