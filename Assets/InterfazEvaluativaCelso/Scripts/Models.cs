// Models.cs
using System;
using System.Collections.Generic;

[Serializable]
public class Pregunta
{
    public int idPregunta;
    public string enunciado;
    public Dictionary<string, string> opciones;
}

[Serializable]
public class Evaluacion
{
    public List<Pregunta> preguntas;
}

[Serializable]
public class LMSResultadoDetalle
{
    public int preguntaId;
    public string enunciado;
    public string seleccion;
    public bool esCorrecta;
    public int puntosOtorgados;
    public int puntosPregunta;
    public string retroalimentacion;
}

[Serializable]
public class LMSResultadoResponse
{
    public int evaluacionId;
    public string alumnoId;
    public int puntajeObtenido;
    public int puntajeMaximo;
    public float calificacion;
    public List<LMSResultadoDetalle> detalle;
    public string fechaCalificacion;
}
[Serializable]
public class TokenResponse
{
    public string token;
    public string expiracion; // opcional, según lo que devuelva tu LMS
}