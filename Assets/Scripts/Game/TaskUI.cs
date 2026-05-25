using TMPro;
using Unity.Netcode;
using UnityEngine;

// Muestra la tarea del jugador local según los puzzles que tiene asignados.
public class TaskUI : MonoBehaviour
{
    [Header("Referencia UI")]
    public TextMeshProUGUI taskText;

    [Header("Mensajes de tarea")]
    public string msgCodeSolver     = "Ingresa el código en la consola (pide los números a tu equipo).";
    public string msgPatternSolver  = "Mira las 3 luces y avísale a cada compañero si su luz está encendida o apagada.";
    public string msgMemorySolver   = "Pulsa los 3 botones de colores siguiendo las secuencias 1, 2 y 3 que te dicten.";
    public string msgPressureSolver = "Estabiliza la presión en el rango objetivo. Dile a tus compañeros qué válvulas abrir o cerrar.";
    public string msgHelper         = "Busca pistas y manipula los aparatos de tu sala (palancas, válvulas, consolas) cuando te lo pidan.";
    public string msgWaiting       = "Esperando rompecabezas...";
    public string msgAllSolved     = "¡Todos los rompecabezas resueltos!";

    private void Update()
    {
        if (taskText == null) return;

        var assign  = PuzzleAssignmentManager.Instance;
        var spawner = NetworkPlayerSpawner.Instance;

        if (assign == null || spawner == null || NetworkManager.Singleton == null)
        {
            taskText.text = msgWaiting;
            return;
        }

        if (assign.CodePuzzleSlot.Value < 0 || assign.PatternPuzzleSlot.Value < 0
            || assign.MemoryPuzzleSlot.Value < 0 || assign.PressurePuzzleSlot.Value < 0)
        {
            taskText.text = msgWaiting;
            return;
        }

        // Si todo está resuelto, mostrar victoria
        bool codeSolved     = PuzzleCodeManager.Instance     != null && PuzzleCodeManager.Instance.IsSolved.Value;
        bool patternSolved  = PatternPuzzleManager.Instance  != null && PatternPuzzleManager.Instance.IsSolved.Value;
        bool memorySolved   = MemoryPuzzleManager.Instance   != null && MemoryPuzzleManager.Instance.IsSolved.Value;
        bool pressureSolved = PressurePuzzleManager.Instance != null && PressurePuzzleManager.Instance.IsSolved.Value;
        if (codeSolved && patternSolved && memorySolved && pressureSolved)
        {
            taskText.text = msgAllSolved;
            return;
        }

        ulong myId = NetworkManager.Singleton.LocalClientId;
        int mySlot = spawner.GetSlotForClient(myId);

        if (mySlot == assign.CodePuzzleSlot.Value)
            taskText.text = msgCodeSolver;
        else if (mySlot == assign.PatternPuzzleSlot.Value)
            taskText.text = msgPatternSolver;
        else if (mySlot == assign.MemoryPuzzleSlot.Value)
            taskText.text = msgMemorySolver;
        else if (mySlot == assign.PressurePuzzleSlot.Value)
            taskText.text = msgPressureSolver;
        else
            taskText.text = msgHelper;
    }
}
