using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Calculator : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI displayText; // arrastra aquí el DisplayText (TMP)
    
    [Header("Opciones de visualización")]
    [Tooltip("Número máximo de decimales a mostrar (evita notación científica y largas colas).")]
    public int maxDecimalDigits = 10;

    // Estado interno
    private string inputBuffer = "0";         // lo que el usuario está tipeando (usa ',' como separador mostrado)
    private double storedValue = 0.0;         // valor guardado cuando se presiona una operación
    private string pendingOperator = null;    // "+", "-", "*", "/"
    private bool enteringNewNumber = true;    // si true, el siguiente dígito reemplaza el display (nuevo número)
    private bool lastPressedEquals = false;   // para comportamiento después de '='

    // Culture helpers: usamos InvariantCulture internamente (punto decimal) y mostramos coma
    private readonly CultureInfo inv = CultureInfo.InvariantCulture;

    void Start()
    {
        if (displayText == null)
        {
            Debug.LogError("[Calculator] Asigna el DisplayText (TextMeshProUGUI) en el Inspector.");
        }
        UpdateDisplay();
    }

    #region Métodos públicos (conectar desde botones)
    // Llamar con el dígito como string: "0".."9"
    public void OnDigitPressed(string digit)
    {
        if (string.IsNullOrEmpty(digit)) return;
        if (lastPressedEquals)
        {
            // si justo presionamos '=', empezar de nuevo al teclear un dígito
            ClearAllButDisplay();
            lastPressedEquals = false;
        }

        if (enteringNewNumber)
        {
            // reemplaza si es inicio de número
            if (digit == "0")
            {
                // evitar múltiples ceros a la izquierda: si buffer ya "0", no agregar más ceros
                if (inputBuffer == "0") { /* nada */ }
                else inputBuffer = digit;
            }
            else
            {
                inputBuffer = digit;
            }
            enteringNewNumber = false;
        }
        else
        {
            // concatenar dígito
            if (inputBuffer == "0")
                inputBuffer = digit; // evita "02"
            else
                inputBuffer += digit;
        }
        UpdateDisplay();
    }

    // Botón coma (muestra ',' pero internamente guardamos con '.')
    public void OnCommaPressed()
    {
        if (lastPressedEquals)
        {
            ClearAllButDisplay();
            lastPressedEquals = false;
        }

        if (enteringNewNumber)
        {
            // empezar "0,"
            inputBuffer = "0,";
            enteringNewNumber = false;
        }
        else
        {
            if (!inputBuffer.Contains(","))
            {
                inputBuffer += ",";
            }
        }
        UpdateDisplay();
    }

    // Operadores: "+", "-", "*", "/"
    public void OnOperatorPressed(string op)
    {
        // si venimos de '=', queremos operar con el resultado mostrado (storedValue ya está listo)
        if (!enteringNewNumber && !lastPressedEquals)
        {
            // si hay un pending operator, ejecutarlo primero
            TryComputePendingOperation();
        }
        else if (lastPressedEquals)
        {
            lastPressedEquals = false;
            // storedValue ya contiene el resultado mostrado
        }
        else
        {
            // no hemos tecleado un nuevo número (ej: usuario presionó repetidamente operadores): solo actualizamos storedValue desde buffer
            double val = ParseBufferToDouble(inputBuffer);
            storedValue = val;
        }

        pendingOperator = op;
        enteringNewNumber = true; // siguiente dígito inicia nuevo número
        UpdateDisplay();
    }

    // Igual
    public void OnEqualsPressed()
    {
        TryComputePendingOperation();
        pendingOperator = null;
        enteringNewNumber = true;
        lastPressedEquals = true;
        UpdateDisplay();
    }

    // Clear (C) — borra todo
    public void OnClearPressed()
    {
        ClearAll();
        UpdateDisplay();
    }
    #endregion

    #region Lógica interna
    private void TryComputePendingOperation()
    {
        double current = ParseBufferToDouble(inputBuffer);

        if (pendingOperator == null)
        {
            // si no hay operador pendiente, almacenamos el valor actual
            storedValue = current;
        }
        else
        {
            double result = storedValue; // por defecto
            bool error = false;

            switch (pendingOperator)
            {
                case "+":
                    result = storedValue + current;
                    break;
                case "-":
                    result = storedValue - current;
                    break;
                case "*":
                    result = storedValue * current;
                    break;
                case "/":
                    if (Mathf.Approximately((float)current, 0f))
                    {
                        // división por cero
                        ShowError("ERR"); // muestra error
                        ClearAll(); // resetea para recuperar
                        error = true;
                    }
                    else result = storedValue / current;
                    break;
            }

            if (!error)
            {
                storedValue = result;
                // ponemos el resultado en el buffer para que se muestre y permita seguir operando sobre él
                inputBuffer = DoubleToBufferString(storedValue);
            }
        }
    }

    // convierte el buffer (p. ej. "12,34") a double (usa '.' internamente)
    private double ParseBufferToDouble(string buf)
    {
        if (string.IsNullOrEmpty(buf)) return 0.0;
        string normalized = buf.Replace(',', '.'); // reemplaza coma por punto
        if (double.TryParse(normalized, NumberStyles.Float, inv, out double v))
            return v;
        return 0.0;
    }

    // convierte un double a string de buffer (usa coma para mostrar)
    private string DoubleToBufferString(double value)
    {
        // formato con hasta maxDecimalDigits decimales y sin ceros finales
        string fmt = "0";
        if (maxDecimalDigits > 0)
            fmt = "0." + new string('#', maxDecimalDigits); // ej: "0.##########"
        string s = value.ToString(fmt, inv);
        // evita "-0" raros
        if (s == "-0" || s == "-0.0") s = "0";
        // mostrar con coma
        s = s.Replace('.', ',');
        return s;
    }

    // actualiza el TMP display con el buffer actual
    private void UpdateDisplay()
    {
        if (displayText == null) return;
        displayText.text = inputBuffer;
    }

    private void ShowError(string text)
    {
        if (displayText != null) displayText.text = text;
    }

    private void ClearAll()
    {
        inputBuffer = "0";
        storedValue = 0.0;
        pendingOperator = null;
        enteringNewNumber = true;
        lastPressedEquals = false;
    }

    // borra todo excepto display si queremos mantener comportamiento de teclas
    private void ClearAllButDisplay()
    {
        inputBuffer = "0";
        storedValue = 0.0;
        pendingOperator = null;
        enteringNewNumber = true;
    }
    #endregion
}
