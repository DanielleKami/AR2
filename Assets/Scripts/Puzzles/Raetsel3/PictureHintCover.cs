using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EscapeRoom.Puzzles.Raetsel3
{
    /// <summary>
    /// Zeigt nach dem Bildfall ein Buchcover mit Hinweistext (Billboard zur Kamera).
    /// </summary>
    public class PictureHintCover : MonoBehaviour
    {
        public const string DefaultHint =
            "Schätze lassen sich auch in Bücher finden";

        [Header("Referenzen")]
        [SerializeField] private PictureFall pictureFall;
        [SerializeField] private GameObject hintCoverRoot;

        [Header("Inhalt")]
        [TextArea(2, 4)]
        [SerializeField] private string hintMessage = DefaultHint;

        [Header("Layout")]
        [SerializeField] private Vector2 coverSizeMeters = new Vector2(0.55f, 0.38f);
        [SerializeField] private float heightAbovePainting = 0.06f;

        [Header("Animation")]
        [SerializeField] private float revealDelay = 0.35f;
        [SerializeField] private float popDuration = 0.4f;

        private bool _revealed;
        private Collider _pictureCollider;

        private void Awake()
        {
            if (transform.name == "HintBookCover")
            {
                enabled = false;
                return;
            }

            if (pictureFall == null) pictureFall = GetComponent<PictureFall>();
            _pictureCollider = GetComponent<Collider>();
            EnsureCover();

            if (hintCoverRoot != null)
            {
                ApplyHintText();
                hintCoverRoot.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (pictureFall != null)
                pictureFall.Fallen += OnPictureFallen;
        }

        private void OnDisable()
        {
            if (pictureFall != null)
                pictureFall.Fallen -= OnPictureFallen;
        }

        private void LateUpdate()
        {
            if (!_revealed || hintCoverRoot == null || !hintCoverRoot.activeSelf) return;
            UpdateCoverTransform();
        }

        private void EnsureCover()
        {
            if (hintCoverRoot == null)
            {
                Transform existing = transform.Find("HintBookCover");
                if (existing != null && existing.GetComponent<Canvas>() != null)
                    hintCoverRoot = existing.gameObject;
            }

            if (hintCoverRoot != null && hintCoverRoot.GetComponent<Canvas>() != null)
                return;

            hintCoverRoot = BuildCoverCanvas(transform, hintMessage, coverSizeMeters);
        }

        private void OnPictureFallen()
        {
            if (_revealed) return;
            _revealed = true;
            StartCoroutine(RevealRoutine());
        }

        private IEnumerator RevealRoutine()
        {
            EnsureCover();
            if (hintCoverRoot == null) yield break;

            if (revealDelay > 0f)
                yield return new WaitForSeconds(revealDelay);

            ApplyHintText();
            UpdateCoverTransform();
            hintCoverRoot.SetActive(true);

            Transform panel = hintCoverRoot.transform.Find("Panel");
            Vector3 targetScale = panel != null ? panel.localScale : Vector3.one;
            if (panel != null) panel.localScale = Vector3.zero;

            float elapsed = 0f;
            while (elapsed < popDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / popDuration);
                if (panel != null)
                    panel.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, t);
                yield return null;
            }

            if (panel != null)
                panel.localScale = targetScale;

            Debug.Log("[Rätsel3] Buch-Hinweis sichtbar.");
        }

        private void UpdateCoverTransform()
        {
            if (_pictureCollider == null) _pictureCollider = GetComponent<Collider>();
            if (_pictureCollider == null || hintCoverRoot == null) return;

            Bounds bounds = _pictureCollider.bounds;
            Vector3 center = bounds.center + Vector3.up * heightAbovePainting;

            Camera cam = Camera.main;
            if (cam == null) return;

            hintCoverRoot.transform.position = center;
            hintCoverRoot.transform.rotation = Quaternion.LookRotation(
                center - cam.transform.position, Vector3.up);

            var canvas = hintCoverRoot.GetComponent<Canvas>();
            if (canvas != null)
                canvas.worldCamera = cam;
        }

        private void ApplyHintText()
        {
            if (hintCoverRoot == null) return;
            var text = hintCoverRoot.GetComponentInChildren<Text>(true);
            if (text != null)
                text.text = hintMessage.ToUpperInvariant();
        }

        public static GameObject BuildCoverCanvas(Transform painting, string message, Vector2 sizeMeters)
        {
            Transform existing = painting.Find("HintBookCover");
            if (existing != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Object.DestroyImmediate(existing.gameObject);
                else
#endif
                    Object.Destroy(existing.gameObject);
            }

            const float scale = 0.001f;
            var root = new GameObject("HintBookCover");
            root.transform.SetParent(painting, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            if (Camera.main != null)
                canvas.worldCamera = Camera.main;

            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(sizeMeters.x / scale, sizeMeters.y / scale);
            rect.localScale = new Vector3(scale, scale, scale);

            root.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 100f;

            var panel = new GameObject("Panel");
            panel.transform.SetParent(root.transform, false);
            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0.52f, 0.32f, 0.16f);

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(panel.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(16f, 12f);
            textRect.offsetMax = new Vector2(-16f, -12f);

            var text = textGo.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = message.ToUpperInvariant();
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 32;
            text.fontStyle = FontStyle.Bold;
            text.color = new Color(0.95f, 0.90f, 0.82f);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;

            root.SetActive(false);
            return root;
        }
    }
}
