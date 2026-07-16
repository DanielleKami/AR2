using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EscapeRoom.Puzzles.Raetsel3
{
    /// <summary>
    /// Bild geraderücken: Der Spieler dreht das schief hängende Bild per Touch/Maus-Drag
    /// um seine lokale Vorwärtsachse (Wand-Normale). Liegt der Winkel innerhalb der
    /// Toleranz, rastet das Bild auf 0° ein und feuert <see cref="Straightened"/>.
    /// Funktioniert im Editor mit der Maus (neues Input System: Pointer.current).
    /// </summary>
    public class PictureStraighten : MonoBehaviour
    {
        [Header("Verhalten")]
        [Tooltip("Start-Schiefwinkel in Grad (z. B. -25).")]
        [SerializeField] private float startTiltAngle = -25f;

        [Tooltip("Toleranz in Grad, ab der das Bild als 'gerade' gilt.")]
        [SerializeField] private float toleranceDeg = 5f;

        [Tooltip("Grad pro Pixel Drag (Empfindlichkeit).")]
        [SerializeField] private float rotationSpeed = 0.3f;

        [Header("Referenzen")]
        [Tooltip("Collider des Bildes. Leer = Collider an diesem GameObject.")]
        [SerializeField] private Collider pictureCollider;

        [Tooltip("Optional: Sound beim Einrasten.")]
        [SerializeField] private AudioSource snapSound;

        public event Action Straightened;
        public bool IsStraight { get; private set; }

        private Camera _cam;
        private Quaternion _baseRotation;
        private float _currentAngle;
        private bool _dragging;
        private Vector2 _lastPointer;

        private void Awake()
        {
            _cam = Camera.main;
            if (pictureCollider == null) pictureCollider = GetComponent<Collider>();
            _baseRotation = transform.localRotation;
            _currentAngle = startTiltAngle;
            ApplyRotation();
        }

        private void Update()
        {
            if (IsStraight) return;

            var pictureFall = GetComponent<PictureFall>();
            if (pictureFall != null && (pictureFall.IsFalling || pictureFall.HasFallen))
                return;

            var pointer = Pointer.current;
            if (pointer == null) return;

            Vector2 pos = pointer.position.ReadValue();

            if (pointer.press.wasPressedThisFrame && HitsPicture(pos))
            {
                _dragging = true;
                _lastPointer = pos;
            }
            else if (pointer.press.wasReleasedThisFrame)
            {
                _dragging = false;
            }

            if (_dragging && pointer.press.isPressed)
            {
                float deltaX = pos.x - _lastPointer.x;
                _lastPointer = pos;

                _currentAngle += deltaX * rotationSpeed;
                ApplyRotation();

                if (Mathf.Abs(_currentAngle) <= toleranceDeg)
                    Snap();
            }
        }

        private bool HitsPicture(Vector2 screenPos)
        {
            if (_cam == null || pictureCollider == null) return false;
            Ray ray = _cam.ScreenPointToRay(screenPos);
            return Physics.Raycast(ray, out RaycastHit hit) && hit.collider == pictureCollider;
        }

        private void ApplyRotation()
        {
            // Drehung um die lokale Vorwärtsachse (Wand-Normale) als 'Roll'.
            transform.localRotation = _baseRotation * Quaternion.AngleAxis(_currentAngle, Vector3.forward);
        }

        private void Snap()
        {
            _currentAngle = 0f;
            ApplyRotation();
            IsStraight = true;
            _dragging = false;
            if (snapSound != null) snapSound.Play();
            Debug.Log("[Rätsel3] Bild ist gerade -> eingerastet.");
            Straightened?.Invoke();
        }

        /// <summary>Entfernt Schiefwinkel vor dem Fallen, damit das Bild sauber auf den Boden liegt.</summary>
        public void ResetTiltForFall()
        {
            _dragging = false;
            _currentAngle = 0f;
            transform.localRotation = _baseRotation;
        }
    }
}
