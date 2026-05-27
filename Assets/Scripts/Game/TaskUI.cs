using TMPro;
using Unity.Netcode;
using UnityEngine;

// HUD del objetivo: muestra el nombre de la tarea + 4 pasos a seguir.
// El contenido depende del puzzle que el jugador local debe resolver.
public class TaskUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [Tooltip("Texto del nombre de la tarea (ej: 'Ingresa el código correcto').")]
    public TextMeshProUGUI titleText;

    [Tooltip("Los 4 textos de pasos. Tamaño esperado: 4.")]
    public TextMeshProUGUI[] stepTexts;

    [System.Serializable]
    public class TaskInfo
    {
        [TextArea(1, 2)] public string title;
        [TextArea(1, 2)] public string[] steps = new string[4];
    }

    [Header("Tareas por rol")]
    public TaskInfo codeTask = new()
    {
        title = "Ingresa el código correcto",
        steps = new[]
        {
            "Busca la terminal para digitar el código",
            "Busca el orden del código (colores)",
            "Entre todos busquen los dígitos",
            "Escribe el código en la terminal"
        }
    };

    public TaskInfo patternTask = new()
    {
        title = "Prende todas las luces a verde",
        steps = new[]
        {
            "Busca la interfaz de las luces",
            "Cambia las luces en verde indicando a los demás",
            "Indica a los jugadores cuando su patrón esté correcto",
            "Guía a todos los jugadores para completar"
        }
    };

    public TaskInfo memoryTask = new()
    {
        title = "Ejecuta la secuencia de botones correcta",
        steps = new[]
        {
            "Busca las vitrinas de botones de colores",
            "Dile a los jugadores que te digan la posición del desafío",
            "Sigue sus instrucciones con los patrones",
            "Presiona los botones con el patrón completo"
        }
    };

    public TaskInfo pressureTask = new()
    {
        title = "Estabiliza la presión",
        steps = new[]
        {
            "Busca la pantalla de presión",
            "Identifica el rango ideal de presión",
            "Dile a los jugadores interactuar con las válvulas",
            "Estabiliza la presión"
        }
    };

    [Header("Estados especiales")]
    public TaskInfo waitingTask = new()
    {
        title = "Esperando rompecabezas...",
        steps = new[] { "", "", "", "" }
    };

    public TaskInfo allSolvedTask = new()
    {
        title = "¡Todos los rompecabezas resueltos!",
        steps = new[] { "Buen trabajo", "Buen trabajo", "Buen trabajo", "Buen trabajo" }
    };

    public TaskInfo helperFallbackTask = new()
    {
        title = "Apoya a tu equipo",
        steps = new[]
        {
            "Busca pistas en las cajas",
            "Manipula los objetos de tu sala",
            "Comparte lo que encuentres por voz",
            "Coordina con los demás"
        }
    };

    private void Update()
    {
        TaskInfo info = ResolveTask();
        ApplyTask(info);
    }

    private TaskInfo ResolveTask()
    {
        var assign  = PuzzleAssignmentManager.Instance;
        var spawner = NetworkPlayerSpawner.Instance;

        if (assign == null || spawner == null || NetworkManager.Singleton == null)
            return waitingTask;

        if (assign.CodePuzzleSlot.Value     < 0 ||
            assign.PatternPuzzleSlot.Value  < 0 ||
            assign.MemoryPuzzleSlot.Value   < 0 ||
            assign.PressurePuzzleSlot.Value < 0)
            return waitingTask;

        // ¿Todo resuelto?
        bool codeSolved     = PuzzleCodeManager.Instance     != null && PuzzleCodeManager.Instance.IsSolved.Value;
        bool patternSolved  = PatternPuzzleManager.Instance  != null && PatternPuzzleManager.Instance.IsSolved.Value;
        bool memorySolved   = MemoryPuzzleManager.Instance   != null && MemoryPuzzleManager.Instance.IsSolved.Value;
        bool pressureSolved = PressurePuzzleManager.Instance != null && PressurePuzzleManager.Instance.IsSolved.Value;
        if (codeSolved && patternSolved && memorySolved && pressureSolved)
            return allSolvedTask;

        ulong myId  = NetworkManager.Singleton.LocalClientId;
        int   mySlot = spawner.GetSlotForClient(myId);

        if (mySlot == assign.CodePuzzleSlot.Value)     return codeTask;
        if (mySlot == assign.PatternPuzzleSlot.Value)  return patternTask;
        if (mySlot == assign.MemoryPuzzleSlot.Value)   return memoryTask;
        if (mySlot == assign.PressurePuzzleSlot.Value) return pressureTask;
        return helperFallbackTask;
    }

    private void ApplyTask(TaskInfo info)
    {
        if (info == null) return;

        if (titleText != null) titleText.text = info.title;

        if (stepTexts == null) return;
        for (int i = 0; i < stepTexts.Length; i++)
        {
            if (stepTexts[i] == null) continue;
            stepTexts[i].text = (info.steps != null && i < info.steps.Length)
                ? info.steps[i] : "";
        }
    }
}
