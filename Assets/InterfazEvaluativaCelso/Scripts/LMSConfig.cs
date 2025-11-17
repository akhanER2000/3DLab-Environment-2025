public static class LMSConfig
{
    public const string LMS_BASE_URL = "http://135.148.148.88:5253";

    // Endpoints del LMS 3DLab
    public const string TOKEN = "/api/v2/3dlab/token";
    public const string PREGUNTAS = "/api/v2/3dlab/preguntas?evaluacionId={0}";
    public const string SELECCION = "/api/v2/3dlab/evaluaciones/{0}/seleccion?alumnoId={1}";
    public const string RESULTADOS = "/api/v2/3dlab/resultados";

    // Configuración de prueba
    public const int ALUMNO_ID = 1001;
    public const int EVALUACION_ID = 1;

    // Credenciales si las pide el token
    public const string CLIENT_ID = "3dlab";
    public const string CLIENT_SECRET = "12345";
}
