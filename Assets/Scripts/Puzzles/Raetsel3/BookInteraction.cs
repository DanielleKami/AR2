using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EscapeRoom.Puzzles.Raetsel3
{
    /// <summary>
    /// Buch suchen &amp; öffnen: Erst nach Freischaltung (Bild gerade) interaktiv. Ein Tap auf
    /// das Buch klappt den Deckel auf (einfache Code-Animation), blendet die Ziffer "2" ein
    /// und feuert <see cref="Opened"/>. Funktioniert im Editor mit der Maus (Pointer.current).
    /// </summary>
    public class BookInteraction : MonoBehaviour
    {
        [Header("Referenzen")]
        [Tooltip("Collider des Buchs. Leer = Collider an diesem GameObject.")]
        [SerializeField] private Collider bookCollider;

        [Tooltip("Optional: Animator mit Trigger zum Öffnen.")]
        [SerializeField] private Animator animator;
        [SerializeField] private string openTrigger = "Open";

        [Header("Cover-Animation (ohne Animator)")]
        [Tooltip("Transform des Buchdeckels, der beim Öffnen aufklappt (optional).")]
        [SerializeField] private Transform coverToOpen;
        [Tooltip("Aufklapp-Winkel in Grad.")]
        [SerializeField] private float openAngle = 110f;
        [Tooltip("Dauer der Aufklapp-Animation in Sekunden.")]
        [SerializeField] private float openDuration = 0.6f;
        [Tooltip("Achse, um die der Deckel aufklappt (lokal).")]
        [SerializeField] private Vector3 openAxis = Vector3.forward;

        [Tooltip("Objekt mit der sichtbaren Ziffer '2' (anfangs inaktiv).")]
        [SerializeField] private GameObject digitReveal;

        [Tooltip("Optional: Highlight/Outline als Such-Hinweis (anfangs inaktiv).")]
        [SerializeField] private GameObject highlight;

        [Tooltip("Optional: Sound beim Öffnen.")]
        [SerializeField] private AudioSource openSound;

        [Tooltip("Wenn true, ist das Buch sofort antippbar (zum isolierten Testen).")]
        [SerializeField] private bool interactableFromStart = false;

        public event Action Opened;
        public bool IsOpen { get; private set; }

        private Camera _cam;
        private bool _interactable;

        private void Awake()
        {
            _cam = Camera.main;
            if (bookCollider == null) bookCollider = GetComponent<Collider>();
            if (digitReveal != null) digitReveal.SetActive(false);
            if (highlight != null) highlight.SetActive(false);
            _interactable = interactableFromStart;
        }

        /// <summary>Macht das Buch antippbar und zeigt den Such-Hinweis.</summary>
        public void EnableInteraction()
        {
            _interactable = true;
            if (highlight != null) highlight.SetActive(true);
            Debug.Log("[Rätsel3] Buch ist jetzt antippbar.");
        }

        private void Update()
        {
            if (!_interactable || IsOpen) return;

            var pointer = Pointer.current;
            if (pointer == null) return;

            if (pointer.press.wasPressedThisFrame && Hits(pointer.position.ReadValue()))
                Open();
        }

        private bool Hits(Vector2 screenPos)
        {
            if (_cam == null || bookCollider == null) return false;
            Ray ray = _cam.ScreenPointToRay(screenPos);
            return Physics.Raycast(ray, out RaycastHit hit) && hit.collider == bookCollider;
        }

        private void Open()
        {
            IsOpen = true;
            if (animator != null && !string.IsNullOrEmpty(openTrigger)) animator.SetTrigger(openTrigger);
            if (highlight != null) highlight.SetActive(false);
            if (openSound != null) openSound.Play();
            Debug.Log("[Rätsel3] Buch geöffnet -> Ziffer 2 sichtbar.");

            if (coverToOpen != null)
                StartCoroutine(OpenCoverRoutine());
            else if (digitReveal != null)
                digitReveal.SetActive(true);

            Opened?.Invoke();
        }

        private IEnumerator OpenCoverRoutine()
        {
            Quaternion from = coverToOpen.localRotation;
            Quaternion to = from * Quaternion.AngleAxis(openAngle, openAxis.normalized);
            float t = 0f;
            while (t < openDuration)
            {
                t += Time.deltaTime;
                coverToOpen.localRotation = Quaternion.Slerp(from, to, Mathf.Clamp01(t / openDuration));
                yield return null;
            }
            coverToOpen.localRotation = to;
            if (digitReveal != null) digitReveal.SetActive(true);
        }
    }
}
