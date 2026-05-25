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
    public string msgHelper        = "Busca pistas en las cajas y combina tus palancas hasta que tu compañero te diga que tu luz está encendida.";
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

        if (assign.CodePuzzleSlot.Value < 0 || assign.PatternPuzzleSlot.Value < 0)
        {
            taskText.text = msgWaiting;
            return;
        }

        // Si todo está resuelto, mostrar victoria
        bool codeSolved    = PuzzleCodeManager.Instance    != null && PuzzleCodeManager.Instance.IsSolved.Value;
        bool patternSolved = PatternPuzzleManager.Instance != null && PatternPuzzleManager.Instance.IsSolved.Value;
        if (codeSolved && patternSolved)
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
        else
            taskText.text = msgHelper;
    }
}
