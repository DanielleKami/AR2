using System;
using UnityEngine;
using EscapeRoom.Core;

namespace EscapeRoom.Puzzles.Raetsel3
{
    /// <summary>
    /// Zustandsmaschine für Rätsel 3 (Bild &amp; Buch). Schaltet nach dem Geraderücken des
    /// Bildes das Buch frei; nach dem Öffnen des Buchs gilt das Rätsel als gelöst und die
    /// Ziffer 2 wird an den <see cref="CodeManager"/> übergeben.
    /// </summary>
    public class Raetsel3Controller : MonoBehaviour, IPuzzle
    {
        [Header("Konfiguration")]
        [SerializeField] private int puzzleId = 3;
        [SerializeField] private int rewardDigit = 2;
        [SerializeField] private bool activateOnStart = true;

        [Header("Referenzen")]
        [SerializeField] private PictureStraighten picture;
        [SerializeField] private PictureFall pictureFall;
        [SerializeField] private BookInteraction book;

        public int PuzzleId => puzzleId;
        public bool IsSolved { get; private set; }
        public event Action<int, int> Solved;

        private bool _activated;

        private void Start()
        {
            if (activateOnStart) Activate();
        }

        public void Activate()
        {
            if (_activated) return;
            _activated = true;

            if (picture != null) picture.Straightened += OnPictureStraightened;
            else Debug.LogWarning("[Rätsel3] Keine PictureStraighten-Referenz gesetzt.");

            // Falls keine BookInteraction per Inspector gesetzt wurde, versuche sie automatisch
            // im Child-Objekt des `picture` zu finden (z.B. BookOnBack).
            if (book == null && picture != null)
            {
                var bi = picture.GetComponentInChildren<BookInteraction>(true);
                if (bi != null)
                {
                    book = bi;
                    Debug.Log("[Rätsel3] BookInteraction automatisch gefunden und zugewiesen.");
                }
            }

            if (pictureFall != null) pictureFall.Fallen += OnPictureFallen;

            if (book != null) book.Opened += OnBookOpened;
            else Debug.LogWarning("[Rätsel3] Keine BookInteraction-Referenz gesetzt.");

            Debug.Log("[Rätsel3] aktiviert. Schritt 1: Bild geraderücken.");
        }

        private void OnPictureStraightened()
        {
            Debug.Log("[Rätsel3] Bild gerade -> Schritt 2: Buch suchen & öffnen.");
            if (book != null) book.EnableInteraction();
        }

        private void OnPictureFallen()
        {
            Debug.Log("[Rätsel3] Das Bild ist gefallen. Schritt 28 abgeschlossen.");
        }

        private void OnBookOpened()
        {
            if (IsSolved) return;
            IsSolved = true;

            Debug.Log($"[Rätsel3] GELÖST -> Ziffer {rewardDigit}.");
            Solved?.Invoke(puzzleId, rewardDigit);

            if (CodeManager.Instance != null)
                CodeManager.Instance.SubmitDigit(puzzleId, rewardDigit);
            else
                Debug.LogWarning("[Rätsel3] Kein CodeManager in der Szene gefunden.");
        }

        private void OnDestroy()
        {
            if (picture != null) picture.Straightened -= OnPictureStraightened;
            if (pictureFall != null) pictureFall.Fallen -= OnPictureFallen;
            if (book != null) book.Opened -= OnBookOpened;
        }
    }
}
