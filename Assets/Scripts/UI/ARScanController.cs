using System.Collections;
using System.Collections.Generic;
using EscapeRoom.Puzzles.Raetsel3;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace EscapeRoom.UI
{
    public enum ARScanPlacementMode
    {
        /// <summary>Genug Fläche → Button → Nutzer tippt auf Fläche.</summary>
        ButtonThenTap = 0,
        /// <summary>Genug Fläche → Button → automatische Platzierung.</summary>
        ButtonThenAuto = 1,
        /// <summary>Genug Fläche → sofort automatische Platzierung.</summary>
        AutoWhenReady = 2
    }

    /// <summary>
    /// Erkennt gescannte Fläche, bietet Platzierung an und lädt die Rätsel-Szene.
    /// </summary>
    public class ARScanController : MonoBehaviour
    {
        [Header("Szene")]
        [SerializeField] private string gameSceneName = "raetsel_3";

        [Header("Referenzen")]
        [SerializeField] private GameObject menuCanvas;
        [SerializeField] private GameObject menuCamera;
        [SerializeField] private GameObject xrOrigin;
        [SerializeField] private ARScanOverlayView scanOverlay;

        [Header("Scan-Schwellwerte")]
        [SerializeField] private float minimumPlaneAreaSqM = 1.5f;
        [SerializeField] private float editorMinimumPlaneAreaSqM = 6f;
        [SerializeField] private int minimumPlaneCount = 2;

        [Header("Scan-Fortschritt (Balken)")]
        [SerializeField] private float progressCurveExponent = 2f;
        [Tooltip("Max. m² pro Frame, die zum Balken zählen (kleiner = langsamer).")]
        [SerializeField] private float maxAreaGrowthPerFrameSqM = 0.009f;

        [Header("Platzierung")]
        [SerializeField] private ARScanPlacementMode placementMode = ARScanPlacementMode.ButtonThenAuto;

        private ARRaycastManager _raycastManager;
        private ARAnchorManager _anchorManager;
        private ARPlaneManager _planeManager;
        private ARSession _arSession;

        private ScanPhase _phase = ScanPhase.Idle;
        private bool _isLoading;
        private float _accumulatedScanAreaSqM;
        private GameObject _previewWall;
        private readonly Dictionary<TrackableId, float> _knownPlaneAreas = new();
        private readonly List<ARRaycastHit> _hits = new();

        private enum ScanPhase
        {
            Idle,
            Scanning,
            Ready,
            Placing
        }

        private void Awake()
        {
            ResolveReferences();
            EnsureScanOverlay();
            EnsureXrComponents();
            SetPlaneDetectionEnabled(false);

            if (scanOverlay != null)
                scanOverlay.gameObject.SetActive(false);
        }

        public void BeginScan()
        {
            if (_phase != ScanPhase.Idle || _isLoading)
                return;

            ResolveReferences();
            EnsureScanOverlay();
            EnsureXrComponents();

            ARPlacementState.Clear();
            StartCoroutine(BeginScanRoutine());
        }

        private IEnumerator BeginScanRoutine()
        {
            if (menuCanvas != null)
                menuCanvas.SetActive(false);

            if (menuCamera != null)
                menuCamera.SetActive(false);

            if (xrOrigin != null)
                xrOrigin.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            ResetScanTrackables();
            yield return null;
            yield return null;

            SetPlaneDetectionEnabled(true);

            if (_arSession != null)
                _arSession.Reset();

            yield return null;
            yield return null;

            if (scanOverlay != null)
            {
                scanOverlay.EnsureCanvas();
                scanOverlay.gameObject.SetActive(true);
                scanOverlay.Build(OnPlaceButtonClicked);
                scanOverlay.SetScanning(0f);
            }

            _accumulatedScanAreaSqM = 0f;
            _knownPlaneAreas.Clear();
            _phase = ScanPhase.Scanning;
        }

        private void Update()
        {
            if (_isLoading)
                return;

            switch (_phase)
            {
                case ScanPhase.Scanning:
                    UpdateScanning();
                    break;
                case ScanPhase.Placing:
                    TryPlaceFromTap();
                    break;
            }
        }

        private void UpdateScanning()
        {
            UpdateAreaAccumulator();
            var barProgress = EvaluateScanProgress(out var ready);
            scanOverlay?.SetScanning(barProgress);

            if (!ready)
                return;

            EnterReadyState();
        }

        private void EnterReadyState()
        {
            _phase = ScanPhase.Ready;

            switch (placementMode)
            {
                case ARScanPlacementMode.AutoWhenReady:
                    scanOverlay?.SetReadyToPlace(false, false, 1f);
                    TryAutoPlace();
                    break;

                case ARScanPlacementMode.ButtonThenAuto:
                    scanOverlay?.SetReadyToPlace(true, false, 1f);
                    break;

                default:
                    scanOverlay?.SetReadyToPlace(true, placementMode == ARScanPlacementMode.ButtonThenTap, 1f);
                    break;
            }
        }

        private void OnPlaceButtonClicked()
        {
            if (_phase != ScanPhase.Ready || _isLoading)
                return;

            if (placementMode == ARScanPlacementMode.ButtonThenTap)
            {
                _phase = ScanPhase.Placing;
                scanOverlay?.SetPlacing();
                return;
            }

            TryAutoPlace();
        }

        private void TryPlaceFromTap()
        {
            if (!TryGetTapPosition(out var screenPosition))
                return;

            TryPlaceAtScreenPosition(screenPosition);
        }

        private static bool TryGetTapPosition(out Vector2 screenPosition)
        {
            screenPosition = default;

            var pointer = Pointer.current;
            if (pointer != null && pointer.press.wasPressedThisFrame)
            {
                screenPosition = pointer.position.ReadValue();
                return true;
            }

            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                screenPosition = mouse.position.ReadValue();
                return true;
            }

            return false;
        }

        private void TryAutoPlace()
        {
            if (!TryGetBestPlane(out var plane, out var effectiveSize))
            {
                _phase = ScanPhase.Placing;
                scanOverlay?.SetPlacing();
                return;
            }

            var surfacePose = new Pose(plane.center, plane.transform.rotation);
            CompletePlacement(surfacePose, plane, effectiveSize);
        }

        private void TryPlaceAtScreenPosition(Vector2 screenPosition)
        {
            if (_raycastManager == null || _anchorManager == null)
                return;

            if (!_raycastManager.Raycast(screenPosition, _hits, TrackableType.PlaneWithinPolygon))
                return;

            var hit = _hits[0];
            if (hit.trackable is not ARPlane plane)
                return;

            CompletePlacement(hit.pose, plane, GetEffectivePlaneSize(plane));
        }

        private void CompletePlacement(Pose surfacePose, ARPlane plane, Vector2 effectivePlaneSize)
        {
            if (_anchorManager == null)
                return;

            var anchor = _anchorManager.AttachAnchor(plane, surfacePose);
            if (anchor == null)
                return;

            if (xrOrigin != null)
                anchor.transform.SetParent(xrOrigin.transform, true);

            var wallPose = Raetsel3WallPlacer.ComputeWallPose(surfacePose, effectivePlaneSize, plane);
            ARPlacementState.SetPlacement(wallPose, anchor.transform, effectivePlaneSize);

            _phase = ScanPhase.Idle;
            SetPlaneDetectionEnabled(false);
            HideDetectedPlanes();
            scanOverlay?.SetDone();

            if (_previewWall != null)
                Destroy(_previewWall);

            _previewWall = Raetsel3WallPlacer.SpawnWall(anchor.transform, wallPose, effectivePlaneSize);
            StartCoroutine(LoadPuzzleSceneAfterPreview());
        }

        private IEnumerator LoadPuzzleSceneAfterPreview()
        {
            _isLoading = true;
            yield return new WaitForSeconds(0.75f);

            if (_previewWall != null)
            {
                Destroy(_previewWall);
                _previewWall = null;
            }

            SceneManager.LoadScene(gameSceneName);
        }

        private void HideDetectedPlanes()
        {
            if (_planeManager == null)
                return;

            foreach (var plane in _planeManager.trackables)
                plane.gameObject.SetActive(false);
        }

        private float EvaluateScanProgress(out bool ready)
        {
            ready = false;

            if (_planeManager == null)
                return 0f;

            var requiredArea = Application.isEditor ? editorMinimumPlaneAreaSqM : minimumPlaneAreaSqM;
            var validPlanes = CountValidScanPlanes();
            var barProgress = ComputeBarProgress(requiredArea, validPlanes);

            var areaReady = _accumulatedScanAreaSqM >= requiredArea;
            var planesReady = validPlanes >= minimumPlaneCount;
            ready = areaReady && planesReady && barProgress >= 0.995f;

            return barProgress;
        }

        private float ComputeBarProgress(float requiredArea, int validPlanes)
        {
            var rawProgress = Mathf.Clamp01(_accumulatedScanAreaSqM / requiredArea);

            if (validPlanes < minimumPlaneCount && minimumPlaneCount > 0)
            {
                var planeFactor = validPlanes / (float)minimumPlaneCount;
                rawProgress = Mathf.Min(rawProgress, planeFactor * 0.9f);
            }

            return Mathf.Pow(rawProgress, progressCurveExponent);
        }

        private void UpdateAreaAccumulator()
        {
            if (_planeManager == null)
                return;

            var seenThisFrame = new HashSet<TrackableId>();

            foreach (var plane in _planeManager.trackables)
            {
                if (!IsValidScanPlane(plane))
                    continue;

                var id = plane.trackableId;
                seenThisFrame.Add(id);

                var area = GetPlaneVisibleAreaSqM(plane);
                var hasPrevious = _knownPlaneAreas.TryGetValue(id, out var previousArea);

                float growth;
                if (!hasPrevious)
                    growth = Mathf.Min(area, maxAreaGrowthPerFrameSqM);
                else
                    growth = Mathf.Max(0f, area - previousArea);

                growth = Mathf.Min(growth, maxAreaGrowthPerFrameSqM);
                _accumulatedScanAreaSqM += growth;
                _knownPlaneAreas[id] = area;
            }

            var removed = new List<TrackableId>();
            foreach (var id in _knownPlaneAreas.Keys)
            {
                if (!seenThisFrame.Contains(id))
                    removed.Add(id);
            }

            foreach (var id in removed)
                _knownPlaneAreas.Remove(id);
        }

        private int CountValidScanPlanes()
        {
            if (_planeManager == null)
                return 0;

            var count = 0;
            foreach (var plane in _planeManager.trackables)
            {
                if (IsValidScanPlane(plane))
                    count++;
            }

            return count;
        }

        private static bool IsValidScanPlane(ARPlane plane)
        {
            if (plane.trackingState != TrackingState.Tracking)
                return false;

            if (plane.alignment != PlaneAlignment.HorizontalUp &&
                plane.alignment != PlaneAlignment.HorizontalDown)
                return false;

            return GetPlaneVisibleAreaSqM(plane) > 0.01f;
        }

        private static float GetPlaneVisibleAreaSqM(ARPlane plane)
        {
            var meshFilter = plane.GetComponentInChildren<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
                return CalculateMeshSurfaceArea(meshFilter.sharedMesh, meshFilter.transform);

            return GetPlaneBoundaryAreaSqM(plane);
        }

        private static float GetPlaneBoundaryAreaSqM(ARPlane plane)
        {
            var boundary = plane.boundary;
            if (!boundary.IsCreated || boundary.Length < 3)
                return plane.extents.x * plane.extents.y * 4f * 0.25f;

            var area = 0d;
            for (var i = 0; i < boundary.Length; i++)
            {
                var a = boundary[i];
                var b = boundary[(i + 1) % boundary.Length];
                area += a.x * b.y - b.x * a.y;
            }

            return Mathf.Abs((float)area * 0.5f);
        }

        private static float CalculateMeshSurfaceArea(Mesh mesh, Transform meshTransform)
        {
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;
            if (vertices == null || triangles == null || triangles.Length < 3)
                return 0f;

            var area = 0d;
            for (var i = 0; i < triangles.Length; i += 3)
            {
                var v0 = meshTransform.TransformPoint(vertices[triangles[i]]);
                var v1 = meshTransform.TransformPoint(vertices[triangles[i + 1]]);
                var v2 = meshTransform.TransformPoint(vertices[triangles[i + 2]]);
                area += Vector3.Cross(v1 - v0, v2 - v0).magnitude * 0.5d;
            }

            return (float)area;
        }

        private void ResetScanTrackables()
        {
            SetPlaneDetectionEnabled(false);

            if (_planeManager != null)
            {
                var planes = new List<ARPlane>();
                foreach (var plane in _planeManager.trackables)
                    planes.Add(plane);

                foreach (var plane in planes)
                    Destroy(plane.gameObject);
            }

            SetPlaneDetectionEnabled(true);
        }

        private bool TryGetBestPlane(out ARPlane bestPlane, out Vector2 effectiveSize)
        {
            bestPlane = null;
            effectiveSize = Vector2.zero;
            var bestScore = 0f;

            if (_planeManager == null)
                return false;

            var camera = Camera.main;

            foreach (var plane in _planeManager.trackables)
            {
                if (!IsValidScanPlane(plane))
                    continue;

                var visibleArea = GetPlaneVisibleAreaSqM(plane);
                _knownPlaneAreas.TryGetValue(plane.trackableId, out var scannedArea);
                var score = visibleArea * 0.55f + scannedArea * 0.45f;

                if (camera != null)
                {
                    var toPlane = plane.center - camera.transform.position;
                    var distance = toPlane.magnitude;
                    if (distance is > 0.35f and < 4.5f)
                        score *= 1.15f;

                    var viewDir = camera.transform.forward;
                    if (Vector3.Dot(viewDir.normalized, toPlane.normalized) > 0.25f)
                        score *= 1.1f;
                }

                if (score <= bestScore)
                    continue;

                bestScore = score;
                bestPlane = plane;
                effectiveSize = GetEffectivePlaneSize(plane);
            }

            if (bestPlane != null && effectiveSize.sqrMagnitude < 0.01f)
                effectiveSize = GetEffectivePlaneSize(bestPlane);

            return bestPlane != null;
        }

        private static Vector2 GetEffectivePlaneSize(ARPlane plane)
        {
            var boundary = plane.boundary;
            if (boundary.IsCreated && boundary.Length >= 3)
            {
                var minX = float.MaxValue;
                var maxX = float.MinValue;
                var minY = float.MaxValue;
                var maxY = float.MinValue;

                foreach (var point in boundary)
                {
                    if (point.x < minX) minX = point.x;
                    if (point.x > maxX) maxX = point.x;
                    if (point.y < minY) minY = point.y;
                    if (point.y > maxY) maxY = point.y;
                }

                return new Vector2(maxX - minX, maxY - minY);
            }

            var meshFilter = plane.GetComponentInChildren<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                var meshBounds = meshFilter.sharedMesh.bounds;
                var scale = meshFilter.transform.lossyScale;
                return new Vector2(
                    meshBounds.size.x * Mathf.Abs(scale.x),
                    meshBounds.size.z * Mathf.Abs(scale.z));
            }

            return plane.size;
        }

        private void SetPlaneDetectionEnabled(bool enabled)
        {
            if (_planeManager != null)
                _planeManager.enabled = enabled;
        }

        private void ResolveReferences()
        {
            menuCanvas ??= GameObject.Find("Canvas");
            menuCamera ??= FindMenuCamera();
            xrOrigin ??= GameObject.Find("XR Origin");
            _arSession ??= Object.FindAnyObjectByType<ARSession>();

            if (xrOrigin != null)
            {
                _planeManager ??= xrOrigin.GetComponent<ARPlaneManager>();
                _raycastManager ??= xrOrigin.GetComponent<ARRaycastManager>();
                _anchorManager ??= xrOrigin.GetComponent<ARAnchorManager>();
            }
        }

        private static GameObject FindMenuCamera()
        {
            var xrOrigin = Object.FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
            var cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (var cam in cameras)
            {
                if (xrOrigin != null && cam.transform.IsChildOf(xrOrigin.transform))
                    continue;

                if (cam.GetComponent<ARCameraManager>() != null)
                    continue;

                if (cam.CompareTag("MainCamera"))
                    return cam.gameObject;
            }

            return null;
        }

        private void EnsureScanOverlay()
        {
            if (scanOverlay != null)
            {
                scanOverlay.EnsureCanvas();
                if (scanOverlay.transform.parent != null)
                    scanOverlay.transform.SetParent(null, false);
                return;
            }

            var existing = GameObject.Find("ARScanOverlay");
            if (existing != null)
            {
                scanOverlay = existing.GetComponent<ARScanOverlayView>();
                if (scanOverlay == null)
                    scanOverlay = existing.AddComponent<ARScanOverlayView>();
                scanOverlay.EnsureCanvas();
                return;
            }

            var overlayGo = new GameObject("ARScanOverlay");
            overlayGo.layer = LayerMask.NameToLayer("UI");
            scanOverlay = overlayGo.AddComponent<ARScanOverlayView>();
            scanOverlay.EnsureCanvas();
        }

        private void EnsureXrComponents()
        {
            if (xrOrigin == null)
                return;

            _raycastManager ??= xrOrigin.GetComponent<ARRaycastManager>();
            if (_raycastManager == null)
                _raycastManager = xrOrigin.AddComponent<ARRaycastManager>();

            _anchorManager ??= xrOrigin.GetComponent<ARAnchorManager>();
            if (_anchorManager == null)
                _anchorManager = xrOrigin.AddComponent<ARAnchorManager>();

            _planeManager ??= xrOrigin.GetComponent<ARPlaneManager>();
        }
    }
}
