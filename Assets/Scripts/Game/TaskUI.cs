using TMPro;
using Unity.Netcode;
using UnityEngine;

// Muestra la tarea del jugador local según los puzzles que tiene asignados.
public class TaskUI : MonoBehaviour
{
    [Header("Referencia UI")]
    public TextMeshProUGUI taskText;

    [Header("Mensajes de tarea")]
    public string msgCodeSolver    = "Ingresa el código en la consola (pide los números a tu equipo).";
    public string msgPatternSolver = "Mira las 3 luces y avísale a cada compañero si su luz está encendida o apagada.";
    public string msgMemorySolver  = "Pulsa los 3 botones de colores siguiendo las secuencias 1, 2 y 3 que te dicten.";
    public string msgHelper        = "Busca pistas, mueve palancas y revisa tu consola de secuencia cuando te lo pidan.";
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
            || assign.MemoryPuzzleSlot.Value < 0)
        {
            taskText.text = msgWaiting;
            return;
        }

        // Si todo está resuelto, mostrar victoria
        bool codeSolved    = PuzzleCodeManager.Instance    != null && PuzzleCodeManager.Instance.IsSolved.Value;
        bool patternSolved = PatternPuzzleManager.Instance != null && PatternPuzzleManager.Instance.IsSolved.Value;
        bool memorySolved  = MemoryPuzzleManager.Instance  != null && MemoryPuzzleManager.Instance.IsSolved.Value;
        if (codeSolved && patternSolved && memorySolved)
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
        else
            taskText.text = msgHelper;
    }
}
