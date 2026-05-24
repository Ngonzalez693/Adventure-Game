using TMPro;
using Unity.Netcode;
using UnityEngine;

// Muestra la tarea actual del jugador local según la asignación del puzzle.
// Pon este script en el Canvas (o donde quieras) y arrastra el TextMeshPro
// del HUD.
public class TaskUI : MonoBehaviour
{
    [Header("Referencia UI")]
    public TextMeshProUGUI taskText;

    [Header("Mensajes de tarea")]
    public string taskMessageSolver = "Tarea: ingresa el código de 4 dígitos en la consola.";
    public string taskMessageHelper = "Tarea: busca pistas en las cajas y comparte los números coloreados con tu equipo.";
    public string taskMessageNoAssignment = "Esperando rompecabezas...";
    public string taskMessageSolved = "¡Rompecabezas resuelto!";

    private void Update()
    {
        if (taskText == null) return;

        var mgr     = PuzzleAssignmentManager.Instance;
        var spawner = NetworkPlayerSpawner.Instance;
        var codeMgr = PuzzleCodeManager.Instance;

        if (mgr == null || spawner == null || NetworkManager.Singleton == null)
        {
            taskText.text = taskMessageNoAssignment;
            return;
        }

        // Si el puzzle ya fue resuelto, mostrar mensaje de victoria
        if (codeMgr != null && codeMgr.IsSolved.Value)
        {
            taskText.text = taskMessageSolved;
            return;
        }

        // La asignación arranca con -1; mientras no se inicialice mostramos espera
        if (mgr.CodePuzzleSlot.Value < 0)
        {
            taskText.text = taskMessageNoAssignment;
            return;
        }

        // Determinar si soy el solucionador o un ayudante
        ulong myId  = NetworkManager.Singleton.LocalClientId;
        int mySlot  = spawner.GetSlotForClient(myId);

        taskText.text = mySlot == mgr.CodePuzzleSlot.Value
            ? taskMessageSolver
            : taskMessageHelper;
    }
}
