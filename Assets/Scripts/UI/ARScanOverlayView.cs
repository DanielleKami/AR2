using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EscapeRoom.UI
{
    /// <summary>
    /// Scan-Hinweis mit Fortschritt und Platzieren-Button.
    /// </summary>
    public class ARScanOverlayView : MonoBehaviour
    {
        private static readonly Color ProgressEmptyColor = new(0.78f, 0.78f, 0.78f, 1f);
        private static readonly Color ProgressFullColor = new(0.15f, 0.72f, 0.28f, 1f);

        private static TMP_FontAsset _font;
        private TextMeshProUGUI _statusText;
        private Image _progressFill;
        private RectTransform _progressFillRect;
        private Button _placeButton;
        private GameObject _panel;
        private GameObject _placingHint;
        private GraphicRaycaster _raycaster;
        private Action _onPlaceClicked;

        private void Awake()
        {
            EnsureCanvas();
        }

        private void OnEnable()
        {
            EnsureCanvas();
        }

        public void Build(Action onPlaceClicked)
        {
            _onPlaceClicked = onPlaceClicked;
            _font ??= Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

            for (var i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);

            EnsureCanvas();

            _panel = CreateChild("Panel");
            var panel = _panel;
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -48f);
            panelRect.sizeDelta = new Vector2(960f, 320f);
            panel.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.92f);

            var textGo = CreateChild("Status", panel.transform);
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0.45f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.offsetMin = new Vector2(24f, 0f);
            textRect.offsetMax = new Vector2(-24f, -24f);
            _statusText = textGo.AddComponent<TextMeshProUGUI>();
            if (_font != null) _statusText.font = _font;
            _statusText.fontSize = 32f;
            _statusText.alignment = TextAlignmentOptions.Center;
            _statusText.color = Color.black;
            _statusText.raycastTarget = false;

            var progressBg = CreateChild("ProgressBg", panel.transform);
            var progressBgRect = progressBg.GetComponent<RectTransform>();
            progressBgRect.anchorMin = new Vector2(0.08f, 0.28f);
            progressBgRect.anchorMax = new Vector2(0.92f, 0.36f);
            progressBgRect.offsetMin = Vector2.zero;
            progressBgRect.offsetMax = Vector2.zero;
            progressBg.AddComponent<Image>().color = ProgressEmptyColor;

            var progressFill = CreateChild("ProgressFill", progressBg.transform);
            _progressFillRect = progressFill.GetComponent<RectTransform>();
            _progressFillRect.anchorMin = Vector2.zero;
            _progressFillRect.anchorMax = new Vector2(0f, 1f);
            _progressFillRect.pivot = new Vector2(0f, 0.5f);
            _progressFillRect.offsetMin = Vector2.zero;
            _progressFillRect.offsetMax = Vector2.zero;
            _progressFill = progressFill.AddComponent<Image>();
            _progressFill.color = ProgressEmptyColor;
            UpdateProgress(0f);

            var buttonGo = CreateChild("PlaceButton", panel.transform);
            var buttonRect = buttonGo.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.15f, 0.04f);
            buttonRect.anchorMax = new Vector2(0.85f, 0.22f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            var buttonImage = buttonGo.AddComponent<Image>();
            buttonImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            _placeButton = buttonGo.AddComponent<Button>();
            _placeButton.targetGraphic = buttonImage;
            _placeButton.onClick.AddListener(HandlePlaceClicked);
            _placeButton.interactable = false;
            _placeButton.gameObject.SetActive(false);

            var buttonLabelGo = CreateChild("Label", buttonGo.transform);
            Stretch(buttonLabelGo);
            var buttonLabel = buttonLabelGo.AddComponent<TextMeshProUGUI>();
            if (_font != null) buttonLabel.font = _font;
            buttonLabel.text = "OBJEKT PLATZIEREN";
            buttonLabel.fontSize = 30f;
            buttonLabel.fontStyle = FontStyles.Bold;
            buttonLabel.alignment = TextAlignmentOptions.Center;
            buttonLabel.color = Color.white;
            buttonLabel.raycastTarget = false;

            SetScanning(0f);

            _placingHint = CreateChild("PlacingHint");
            var hintRect = _placingHint.GetComponent<RectTransform>();
            hintRect.anchorMin = new Vector2(0.5f, 1f);
            hintRect.anchorMax = new Vector2(0.5f, 1f);
            hintRect.pivot = new Vector2(0.5f, 1f);
            hintRect.anchoredPosition = new Vector2(0f, -32f);
            hintRect.sizeDelta = new Vector2(900f, 120f);
            var hintText = _placingHint.AddComponent<TextMeshProUGUI>();
            if (_font != null) hintText.font = _font;
            hintText.fontSize = 30f;
            hintText.alignment = TextAlignmentOptions.Center;
            hintText.color = Color.white;
            hintText.raycastTarget = false;
            hintText.text = "Tippe auf die erkannte Fläche,\num das Rätsel zu starten.";
            _placingHint.SetActive(false);
        }

        public void SetScanning(float progress01)
        {
            if (_panel != null)
                _panel.SetActive(true);

            if (_placingHint != null)
                _placingHint.SetActive(false);

            SetUiBlocking(true);
            UpdateProgress(progress01);
            if (_statusText != null)
            {
                _statusText.text =
                    "Scanne die Umgebung…\nBewege die Kamera langsam über eine\nflache Fläche (Boden oder Tisch).";
            }

            SetPlaceButtonVisible(false);
        }

        public void SetReadyToPlace(bool showButton, bool tapAfterButton, float progress01)
        {
            UpdateProgress(progress01);
            if (_statusText != null)
            {
                _statusText.text = showButton
                    ? tapAfterButton
                        ? "Genug Fläche erkannt!\nTippe auf „Objekt platzieren“,\ndann auf die Fläche."
                        : "Genug Fläche erkannt!\nTippe auf „Objekt platzieren“."
                    : tapAfterButton
                        ? "Genug Fläche erkannt!\nTippe auf die Fläche zum Platzieren."
                        : "Genug Fläche erkannt!\nObjekt wird platziert…";
            }

            SetPlaceButtonVisible(showButton);
            if (_placeButton != null)
                _placeButton.interactable = showButton;
        }

        public void SetPlacing()
        {
            if (_panel != null)
                _panel.SetActive(false);

            if (_placingHint != null)
                _placingHint.SetActive(true);

            SetPlaceButtonVisible(false);
            SetUiBlocking(false);
        }

        public void SetDone()
        {
            if (_statusText != null)
                _statusText.text = "Fläche platziert.\nSpiel wird geladen…";

            SetPlaceButtonVisible(false);
        }

        private void HandlePlaceClicked()
        {
            _onPlaceClicked?.Invoke();
        }

        private void UpdateProgress(float progress01)
        {
            var progress = Mathf.Clamp01(progress01);

            if (_progressFillRect != null)
                _progressFillRect.anchorMax = new Vector2(progress, 1f);

            if (_progressFill != null)
                _progressFill.color = Color.Lerp(ProgressEmptyColor, ProgressFullColor, progress);
        }

        private void SetPlaceButtonVisible(bool visible)
        {
            if (_placeButton != null)
                _placeButton.gameObject.SetActive(visible);
        }

        private void SetUiBlocking(bool block)
        {
            _raycaster ??= GetComponent<GraphicRaycaster>();
            if (_raycaster != null)
                _raycaster.enabled = block;
        }

        private GameObject CreateChild(string name, Transform parent = null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent == null ? transform : parent, false);
            return go;
        }

        private static void Stretch(GameObject go)
        {
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public void EnsureCanvas()
        {
            if (transform.parent != null)
                transform.SetParent(null, false);

            var canvas = GetComponent<Canvas>();
            if (canvas == null)
                canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

            var scaler = GetComponent<CanvasScaler>();
            if (scaler == null)
                scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            _raycaster = GetComponent<GraphicRaycaster>();
            if (_raycaster == null)
                _raycaster = gameObject.AddComponent<GraphicRaycaster>();
        }
    }
}
