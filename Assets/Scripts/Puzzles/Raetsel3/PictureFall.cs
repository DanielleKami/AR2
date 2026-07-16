using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace EscapeRoom.Puzzles.Raetsel3
{
    public class PictureFall : MonoBehaviour
    {
        [Header("Fall-Setup")]
        [SerializeField] private float autoFallDelay = 1f;
        [SerializeField] private float fallAngleDeg = 80f;
        [SerializeField] private float groundY = 0f;
        [SerializeField] private float groundClearance = 0.03f;
        [SerializeField] private bool flipBackSide;
        [SerializeField] private float fallDuration = 0.8f;
        [SerializeField] private bool tapToStart = true;
        [SerializeField] private Collider pictureCollider;
        [SerializeField] private AudioSource fallSound;

        [HideInInspector, FormerlySerializedAs("tiltTowardCameraDeg")]
        [SerializeField] private float _legacyTiltTowardCamera;

        public event Action Fallen;
        public bool HasFallen { get; private set; }
        public bool IsFalling { get; private set; }

        private Vector3 _startWorldPosition;
        private Quaternion _startWorldRotation;
        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
            if (pictureCollider == null) pictureCollider = GetComponent<Collider>();
            if (fallSound == null) fallSound = GetComponent<AudioSource>();
            _startWorldPosition = transform.position;
            _startWorldRotation = transform.rotation;

            DetachForeignChildren();
        }

        private void DetachForeignChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child.name == "HintBookCover") continue;
                if (child.name.StartsWith("Table") || child.name.StartsWith("Book"))
                    child.SetParent(null, true);
            }
        }

        private void Start()
        {
            if (autoFallDelay >= 0f)
                StartCoroutine(StartFallDelayed());
        }

        /// <summary>
        /// Deaktiviert Auto-Fall und 3D-Tap – Mock-28-HUD übernimmt den Start.
        /// </summary>
        public void ConfigureForHudTap()
        {
            autoFallDelay = -1f;
            tapToStart = false;
        }

        private IEnumerator StartFallDelayed()
        {
            if (autoFallDelay > 0f)
                yield return new WaitForSeconds(autoFallDelay);

            if (!HasFallen)
                StartFall();
        }

        private void Update()
        {
            if (HasFallen || !tapToStart || IsFalling) return;

            var pointer = Pointer.current;
            if (pointer == null) return;
            if (pointer.press.wasPressedThisFrame && Hits(pointer.position.ReadValue()))
                StartFall();
        }

        public void StartFall()
        {
            if (HasFallen || IsFalling) return;
            StartCoroutine(FallRoutine());
        }

        private bool Hits(Vector2 screenPosition)
        {
            if (_camera == null || pictureCollider == null) return false;
            Ray ray = _camera.ScreenPointToRay(screenPosition);
            return Physics.Raycast(ray, out RaycastHit hit) && hit.collider == pictureCollider;
        }

        private IEnumerator FallRoutine()
        {
            IsFalling = true;

            var straighten = GetComponent<PictureStraighten>();
            if (straighten != null)
                straighten.ResetTiltForFall();

            Quaternion wallRotation = transform.rotation;
            Vector3 startPosition = transform.position;
            ComputeFallEndPose(wallRotation, startPosition, out Vector3 to, out Quaternion targetRotation);

            Quaternion startRotation = transform.rotation;
            float elapsed = 0f;
            while (elapsed < fallDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / fallDuration);
                transform.position = Vector3.Lerp(startPosition, to, t);
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                yield return null;
            }

            transform.SetPositionAndRotation(to, targetRotation);
            IsFalling = false;
            HasFallen = true;

            if (fallSound != null)
                fallSound.Play();

            Fallen?.Invoke();
            Debug.Log("[Rätsel3] Das Bild ist heruntergefallen.");
        }

        private void ComputeFallEndPose(Quaternion wallRotation, Vector3 startPosition,
            out Vector3 endPosition, out Quaternion endRotation)
        {
            Vector3 fallDirection = wallRotation * Vector3.forward;
            fallDirection.y = 0f;
            if (fallDirection.sqrMagnitude < 0.0001f)
                fallDirection = Vector3.forward;

            endRotation = GetFlatRotationOnGround(fallDirection.normalized);
            if (flipBackSide)
                endRotation *= Quaternion.Euler(0f, 180f, 0f);

            Vector3 hingeAxis = wallRotation * Vector3.right;
            Vector3 hingePoint = GetBottomHingePoint(wallRotation);
            float angle = Mathf.Clamp(fallAngleDeg, 0f, 88f);
            Quaternion fallDelta = Quaternion.AngleAxis(-angle, hingeAxis);

            endPosition = hingePoint + fallDelta * (startPosition - hingePoint);
            endPosition = SnapToGround(transform, pictureCollider, endRotation, endPosition, groundY, groundClearance);
        }

        private Vector3 GetBottomHingePoint(Quaternion wallRotation)
        {
            Quaternion saved = transform.rotation;
            transform.rotation = wallRotation;

            Vector3 hinge;
            if (pictureCollider is BoxCollider box)
            {
                Vector3 localBottom = box.center - new Vector3(0f, box.size.y * 0.5f, 0f);
                hinge = transform.TransformPoint(localBottom);
            }
            else if (pictureCollider != null)
            {
                Bounds bounds = pictureCollider.bounds;
                hinge = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            }
            else
            {
                hinge = transform.position;
            }

            transform.rotation = saved;
            return hinge;
        }

        public static Quaternion GetFlatRotationOnGround(Vector3 horizontalFallDirection)
        {
            horizontalFallDirection.y = 0f;
            if (horizontalFallDirection.sqrMagnitude < 0.0001f)
                horizontalFallDirection = Vector3.forward;

            return Quaternion.LookRotation(Vector3.down, horizontalFallDirection.normalized);
        }

        public static Vector3 SnapToGround(Transform target, Collider collider, Quaternion rotation,
            Vector3 worldPosition, float groundY, float clearance = 0.001f)
        {
            Quaternion savedRotation = target.rotation;
            Vector3 savedPosition = target.position;

            target.SetPositionAndRotation(worldPosition, rotation);

            float deltaY = groundY + clearance;
            if (collider != null)
                deltaY = groundY + clearance - collider.bounds.min.y;

            target.SetPositionAndRotation(savedPosition, savedRotation);
            worldPosition.y += deltaY;
            return worldPosition;
        }

        public void ResetPicture()
        {
            StopAllCoroutines();
            transform.position = _startWorldPosition;
            transform.rotation = _startWorldRotation;
            IsFalling = false;
            HasFallen = false;
        }
    }
}
