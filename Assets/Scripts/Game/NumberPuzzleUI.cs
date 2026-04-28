// NumberPuzzleUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NumberPuzzleUI : MonoBehaviour
{
    [Header("Arrastra los 4 textos Digit desde Hierarchy")]
    public TextMeshProUGUI[] digitDisplays;

    [Header("Arrastra los 10 botones numéricos")]
    public Button[] numberButtons;

    [Header("Arrastra los botones de acción")]
    public Button btnBorrar;
    public Button btnConfirmar;
    public Button btnCerrar;

    [Header("Arrastra el texto de feedback")]
    public TextMeshProUGUI feedbackText;

    // Código fijo para probar - luego lo haremos aleatorio
    private int[] solucion = new int[] { 1, 5, 3, 4 };
    private int[] respuesta = new int[4];
    private int indice = 0;

    public void Inicializar()
    {
         indice = 0;
        respuesta = new int[4];

        // Usar código de red si existe, si no usar 1234 de prueba
        if (PuzzleCodeManager.Instance != null)
            solucion = PuzzleCodeManager.Instance.ObtenerCodigoAResolver();
        else
            solucion = new int[] { 1, 2, 3, 4 };

        foreach (var d in digitDisplays)
        {
            d.text = "_";
            d.color = Color.white;
        }
        feedbackText.text = "";

        // Conectar botones numéricos
        for (int i = 0; i < numberButtons.Length; i++)
        {
            int num = i;
            numberButtons[i].onClick.RemoveAllListeners();
            numberButtons[i].onClick.AddListener(() => PresionarNumero(num));
        }

        // Conectar botones de acción
        btnBorrar.onClick.RemoveAllListeners();
        btnBorrar.onClick.AddListener(Borrar);

        btnConfirmar.onClick.RemoveAllListeners();
        btnConfirmar.onClick.AddListener(Confirmar);

        btnCerrar.onClick.RemoveAllListeners();
        btnCerrar.onClick.AddListener(() => PuzzleManager.Instance.CerrarPuzzle());
    }

    void PresionarNumero(int num)
    {
        if (indice >= 4) return;

        respuesta[indice] = num;
        digitDisplays[indice].text = num.ToString();
        indice++;
    }

    void Borrar()
    {
        if (indice <= 0) return;
        indice--;
        digitDisplays[indice].text = "_";
        respuesta[indice] = 0;
    }

    void Confirmar()
    {
        if (indice < 4)
        {
            feedbackText.text = "Ingresa los 4 números";
            feedbackText.color = Color.yellow;
            return;
        }

        bool correcto = true;
        for (int i = 0; i < 4; i++)
        {
            if (respuesta[i] != solucion[i])
            {
                correcto = false;
                break;
            }
        }

        if (correcto)
        {
            // Poner dígitos en verde
            foreach (var d in digitDisplays)
                d.color = Color.green;

            feedbackText.text = "¡Correcto!";
            feedbackText.color = Color.green;
            Invoke(nameof(Completar), 1f);
        }
        else
        {
            // Poner dígitos en rojo
            foreach (var d in digitDisplays)
                d.color = Color.red;

            feedbackText.text = "Código incorrecto";
            feedbackText.color = Color.red;
            Invoke(nameof(Resetear), 0.8f);
        }
    }

    void Resetear()
    {
        indice = 0;
        foreach (var d in digitDisplays)
        {
            d.text = "_";
            d.color = Color.white;
        }
        feedbackText.text = "";
    }

    void Completar()
    {
        PuzzleManager.Instance.PuzzleCompletado();
    }
}