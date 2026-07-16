using UnityEngine;
using UnityEngine.UI;

namespace EscapeRoom.Puzzles.Raetsel3
{
    /// <summary>
    /// Zeigt die Anweisungstexte aus dem Mock (Schritt 28–35) oben im HUD an.
    /// </summary>
    public class Raetsel3InstructionUI : MonoBehaviour
    {
        [SerializeField] private Text instructionText;
        [SerializeField] private Text puzzleHeaderText;

        public void SetInstruction(string message)
        {
            if (instructionText != null)
                instructionText.text = message;
        }

        public void SetHeader(string header)
        {
            if (puzzleHeaderText != null)
                puzzleHeaderText.text = header;
        }
    }

}
