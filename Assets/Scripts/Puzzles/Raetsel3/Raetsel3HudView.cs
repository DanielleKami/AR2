using EscapeRoom.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EscapeRoom.Puzzles.Raetsel3
{
    /// <summary>
    /// Screen-HUD für Rätsel 3 – Mock 28 (Etwas fällt herunter).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class Raetsel3HudView : MonoBehaviour
    {
        private static readonly Color32 Ink = new(0, 0, 0, 255);
        private static readonly Color Paper = Color.white;

        private const float SidePadding = 48f;
        private const float FooterHeight = 220f;

        [SerializeField] private string[] initialCodeDigits = { "3", "7", "5", "." };
        [SerializeField] private int initialHintCount;

        private static TMP_FontAsset _font;
        private TextMeshProUGUI _hintCountText;
        private TextMeshProUGUI[] _codeDigitTexts;
        private GameObject _tapIcon;
        private GameObject _illustration;

        public Button TapButton { get; private set; }

        private void Awake()
        {
            _font ??= Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            Rebuild();
            RefreshFromCodeManager();
        }

        private void OnEnable()
        {
            if (CodeManager.Instance != null)
                CodeManager.Instance.DigitSubmitted += OnDigitSubmitted;
        }

        private void OnDisable()
        {
            if (CodeManager.Instance != null)
                CodeManager.Instance.DigitSubmitted -= OnDigitSubmitted;
        }

        public void Rebuild()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);

            StretchRect(gameObject);

            var bg = CreateUiObject("Background", transform);
            StretchRect(bg);
            bg.AddComponent<Image>().color = Paper;

            BuildHeader(transform);
            BuildIllustrationArea(transform);
            BuildFooter(transform);
        }

        public void SetTapIconVisible(bool visible)
        {
            if (_tapIcon != null)
                _tapIcon.SetActive(visible);
        }

        public void SetIllustrationVisible(bool visible)
        {
            if (_illustration != null)
                _illustration.SetActive(visible);
        }

        public void SetHintCount(int count)
        {
            if (_hintCountText != null)
                _hintCountText.text = count.ToString();
        }

        public void RefreshFromCodeManager()
        {
            if (_codeDigitTexts == null || _codeDigitTexts.Length == 0)
                return;

            if (CodeManager.Instance == null)
            {
                for (var i = 0; i < _codeDigitTexts.Length && i < initialCodeDigits.Length; i++)
                    _codeDigitTexts[i].text = initialCodeDigits[i];
                return;
            }

            string code = CodeManager.Instance.GetCode();
            for (var i = 0; i < _codeDigitTexts.Length; i++)
            {
                char c = i < code.Length ? code[i] : '_';
                if (c == '_')
                    _codeDigitTexts[i].text = i < initialCodeDigits.Length ? initialCodeDigits[i] : ".";
                else
                    _codeDigitTexts[i].text = c.ToString();
            }
        }

        private void OnDigitSubmitted(int puzzleId, int digit)
        {
            RefreshFromCodeManager();
        }

        private void BuildHeader(Transform parent)
        {
            var header = CreateUiObject("Header", parent);
            var rect = header.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, 220f);

            var titleGo = CreateUiObject("PuzzleTitle", header.transform);
            var titleRect = titleGo.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -36f);
            titleRect.sizeDelta = new Vector2(0f, 64f);
            CreateText(titleGo.transform, "RÄTSEL 3/5", 52f, FontStyles.Bold, TextAlignmentOptions.Center);

            var subtitleGo = CreateUiObject("Subtitle", header.transform);
            var subtitleRect = subtitleGo.GetComponent<RectTransform>();
            subtitleRect.anchorMin = new Vector2(0f, 1f);
            subtitleRect.anchorMax = new Vector2(1f, 1f);
            subtitleRect.pivot = new Vector2(0.5f, 1f);
            subtitleRect.anchoredPosition = new Vector2(0f, -108f);
            subtitleRect.sizeDelta = new Vector2(-SidePadding * 2f, 100f);
            var subtitle = CreateText(subtitleGo.transform, "Plötzlich fällt etwas\nvon der Wand!", 34f,
                FontStyles.Normal, TextAlignmentOptions.Center);
            subtitle.lineSpacing = 6f;
        }

        private void BuildIllustrationArea(Transform parent)
        {
            var area = CreateUiObject("IllustrationArea", parent);
            var rect = area.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(SidePadding, FooterHeight + 12f);
            rect.offsetMax = new Vector2(-SidePadding, -230f);

            _illustration = CreateUiObject("Illustration", area.transform);
            StretchRect(_illustration);
            var illImage = _illustration.AddComponent<Image>();
            illImage.color = Paper;
            illImage.preserveAspect = true;
            illImage.raycastTarget = false;
            illImage.sprite = LoadSprite("UI/Raetsel3/fall_illustration");

            _tapIcon = CreateUiObject("TapIcon", area.transform);
            var tapRect = _tapIcon.GetComponent<RectTransform>();
            tapRect.anchorMin = new Vector2(1f, 0f);
            tapRect.anchorMax = new Vector2(1f, 0f);
            tapRect.pivot = new Vector2(1f, 0f);
            tapRect.anchoredPosition = new Vector2(-8f, 16f);
            tapRect.sizeDelta = new Vector2(140f, 140f);

            var tapImage = _tapIcon.AddComponent<Image>();
            tapImage.color = Paper;
            tapImage.preserveAspect = true;
            tapImage.sprite = LoadSprite("UI/Raetsel3/tap_icon");

            TapButton = _tapIcon.AddComponent<Button>();
            TapButton.targetGraphic = tapImage;
        }

        private void BuildFooter(Transform parent)
        {
            var separator = CreateUiObject("FooterSeparator", parent);
            var sepRect = separator.GetComponent<RectTransform>();
            sepRect.anchorMin = new Vector2(0f, 0f);
            sepRect.anchorMax = new Vector2(1f, 0f);
            sepRect.pivot = new Vector2(0.5f, 0f);
            sepRect.anchoredPosition = new Vector2(0f, FooterHeight + 24f);
            sepRect.sizeDelta = new Vector2(-SidePadding * 2f, 2f);
            separator.AddComponent<Image>().color = Ink;

            var footer = CreateUiObject("Footer", parent);
            var footerRect = footer.GetComponent<RectTransform>();
            footerRect.anchorMin = new Vector2(0f, 0f);
            footerRect.anchorMax = new Vector2(1f, 0f);
            footerRect.pivot = new Vector2(0.5f, 0f);
            footerRect.anchoredPosition = new Vector2(0f, 24f);
            footerRect.sizeDelta = new Vector2(-SidePadding * 2f, FooterHeight);

            AddRectBorder(footer, Ink, 2f);

            var divider = CreateUiObject("Divider", footer.transform);
            var divRect = divider.GetComponent<RectTransform>();
            divRect.anchorMin = new Vector2(0.5f, 0f);
            divRect.anchorMax = new Vector2(0.5f, 1f);
            divRect.pivot = new Vector2(0.5f, 0.5f);
            divRect.sizeDelta = new Vector2(2f, 0f);
            divider.AddComponent<Image>().color = Ink;

            BuildFooterHalf(footer.transform, "HintPanel", "HINWEIS", true);
            BuildFooterHalf(footer.transform, "CodePanel", "CODE", false);
        }

        private void BuildFooterHalf(Transform parent, string name, string label, bool isHint)
        {
            var panel = CreateUiObject(name, parent);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = isHint ? new Vector2(0f, 0f) : new Vector2(0.5f, 0f);
            rect.anchorMax = isHint ? new Vector2(0.5f, 1f) : new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(12f, 12f);
            rect.offsetMax = new Vector2(-12f, -12f);

            var titleGo = CreateUiObject("Label", panel.transform);
            var titleRect = titleGo.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -4f);
            titleRect.sizeDelta = new Vector2(0f, 40f);
            CreateText(titleGo.transform, label, 30f, FontStyles.Bold, TextAlignmentOptions.Center);

            if (isHint)
            {
                var row = CreateUiObject("HintRow", panel.transform);
                var rowRect = row.GetComponent<RectTransform>();
                rowRect.anchorMin = new Vector2(0.5f, 0.5f);
                rowRect.anchorMax = new Vector2(0.5f, 0.5f);
                rowRect.pivot = new Vector2(0.5f, 0.5f);
                rowRect.anchoredPosition = new Vector2(0f, -12f);
                rowRect.sizeDelta = new Vector2(180f, 72f);

                var bulbGo = CreateUiObject("Bulb", row.transform);
                var bulbRect = bulbGo.GetComponent<RectTransform>();
                bulbRect.anchorMin = new Vector2(0f, 0.5f);
                bulbRect.anchorMax = new Vector2(0f, 0.5f);
                bulbRect.pivot = new Vector2(0f, 0.5f);
                bulbRect.sizeDelta = new Vector2(56f, 56f);
                var bulbImage = bulbGo.AddComponent<Image>();
                bulbImage.sprite = LoadSprite("UI/Raetsel3/lightbulb");
                bulbImage.preserveAspect = true;
                bulbImage.color = Paper;

                var countGo = CreateUiObject("Count", row.transform);
                var countRect = countGo.GetComponent<RectTransform>();
                countRect.anchorMin = new Vector2(1f, 0.5f);
                countRect.anchorMax = new Vector2(1f, 0.5f);
                countRect.pivot = new Vector2(1f, 0.5f);
                countRect.sizeDelta = new Vector2(80f, 64f);
                _hintCountText = CreateText(countGo.transform, initialHintCount.ToString(), 44f,
                    FontStyles.Bold, TextAlignmentOptions.Center);
            }
            else
            {
                var boxes = CreateUiObject("CodeBoxes", panel.transform);
                var boxesRect = boxes.GetComponent<RectTransform>();
                boxesRect.anchorMin = new Vector2(0.5f, 0.5f);
                boxesRect.anchorMax = new Vector2(0.5f, 0.5f);
                boxesRect.pivot = new Vector2(0.5f, 0.5f);
                boxesRect.anchoredPosition = new Vector2(0f, -16f);
                boxesRect.sizeDelta = new Vector2(360f, 72f);

                _codeDigitTexts = new TextMeshProUGUI[4];
                for (var i = 0; i < 4; i++)
                {
                    var box = CreateUiObject($"Digit{i + 1}", boxes.transform);
                    var boxRect = box.GetComponent<RectTransform>();
                    boxRect.anchorMin = new Vector2(i / 4f, 0f);
                    boxRect.anchorMax = new Vector2((i + 1) / 4f, 1f);
                    boxRect.offsetMin = new Vector2(4f, 0f);
                    boxRect.offsetMax = new Vector2(-4f, 0f);

                    AddRectBorder(box, Ink, 2f);

                    var digitGo = CreateUiObject("Value", box.transform);
                    StretchRect(digitGo);
                    _codeDigitTexts[i] = CreateText(digitGo.transform,
                        i < initialCodeDigits.Length ? initialCodeDigits[i] : ".",
                        36f, FontStyles.Bold, TextAlignmentOptions.Center);
                }
            }
        }

        private static TextMeshProUGUI CreateText(
            Transform parent,
            string value,
            float size,
            FontStyles style,
            TextAlignmentOptions alignment)
        {
            var go = CreateUiObject("Text", parent);
            StretchRect(go);
            var text = go.AddComponent<TextMeshProUGUI>();
            if (_font != null) text.font = _font;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = Ink;
            text.richText = false;
            text.raycastTarget = false;
            return text;
        }

        private static Sprite LoadSprite(string resourcePath)
        {
            var sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite != null) return sprite;
            var texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null) return null;
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), 100f);
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void StretchRect(GameObject go)
        {
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void AddRectBorder(GameObject target, Color32 borderColor, float thickness)
        {
            var fill = target.AddComponent<Image>();
            fill.color = borderColor;
            fill.raycastTarget = false;

            var inner = CreateUiObject("Fill", target.transform);
            var innerRect = inner.GetComponent<RectTransform>();
            innerRect.anchorMin = Vector2.zero;
            innerRect.anchorMax = Vector2.one;
            innerRect.offsetMin = new Vector2(thickness, thickness);
            innerRect.offsetMax = new Vector2(-thickness, -thickness);
            inner.AddComponent<Image>().color = Paper;
        }
    }
}
