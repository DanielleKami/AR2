using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EscapeRoom.UI
{
    /// <summary>
    /// Baut die Anleitungsseite (Mock 2) zur Laufzeit auf.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [DefaultExecutionOrder(-50)]
    public class InstructionsScreenView : MonoBehaviour
    {
        private static readonly Color32 Ink = new(0, 0, 0, 255);
        private static readonly Color Paper = Color.white;

        private const float SidePadding = 56f;
        private const float ButtonWidth = 920f;
        private const float ButtonHeight = 94f;
        private const float BorderWidth = 2f;
        private const float IconSize = 200f;
        private const float TextGap = 28f;

        [SerializeField] private StartMenuController menuController;

        private static TMP_FontAsset _font;

        private void Awake()
        {
            if (menuController == null)
                menuController = FindAnyObjectByType<StartMenuController>();

            _font ??= Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            Rebuild();
        }

        public void Rebuild()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);

            StretchRect(gameObject);

            var bg = CreateUiObject("Background", transform);
            StretchRect(bg);
            var bgImage = bg.AddComponent<Image>();
            bgImage.color = Paper;
            bgImage.raycastTarget = true;

            BuildHeader(transform);
            BuildStep(transform, "Step1", "UI/Instructions/step1_scan",
                "1. Räume scannen",
                "Suche eine horizontale Fläche, um virtuelle Objekte zu platzieren.",
                0.62f);
            BuildStep(transform, "Step2", "UI/Instructions/step2_puzzle",
                "2. Rätsel lösen",
                "Löse die Rätsel, um jeweils ein Zeichen für den Code zu erhalten.",
                0.455f);
            BuildStep(transform, "Step3", "UI/Instructions/step3_keypad",
                "3. Code eingeben",
                "Sammle 5 Zeichen und gib den 5-stelligen Code ein, um die Tür zu öffnen.",
                0.29f);
            BuildActionButton(transform, "BtnVerstanden", "VERSTANDEN", 0.115f, Close);
        }

        private void BuildHeader(Transform parent)
        {
            var header = CreateUiObject("Header", parent);
            var headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.sizeDelta = new Vector2(0f, 168f);

            var backButton = CreateUiObject("BtnBack", header.transform);
            var backRect = backButton.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0f, 1f);
            backRect.anchorMax = new Vector2(0f, 1f);
            backRect.pivot = new Vector2(0f, 1f);
            backRect.anchoredPosition = new Vector2(SidePadding, -48f);
            backRect.sizeDelta = new Vector2(80f, 80f);

            var backImage = backButton.AddComponent<Image>();
            backImage.color = Paper;
            backImage.preserveAspect = true;
            backImage.sprite = LoadSprite("UI/Instructions/back_arrow");

            var back = backButton.AddComponent<Button>();
            back.targetGraphic = backImage;
            back.onClick.AddListener(Close);

            var titleGo = CreateUiObject("Title", header.transform);
            var titleRect = titleGo.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -52f);
            titleRect.sizeDelta = new Vector2(0f, 72f);

            CreateText(titleGo.transform, "ANLEITUNG", 48f, FontStyles.Bold, TextAlignmentOptions.Center);

            var divider = CreateUiObject("Divider", header.transform);
            var dividerRect = divider.GetComponent<RectTransform>();
            dividerRect.anchorMin = new Vector2(0f, 0f);
            dividerRect.anchorMax = new Vector2(1f, 0f);
            dividerRect.pivot = new Vector2(0.5f, 0f);
            dividerRect.sizeDelta = new Vector2(-SidePadding * 2f, 3f);

            var dividerImage = divider.AddComponent<Image>();
            dividerImage.color = Ink;
        }

        private void BuildStep(
            Transform parent,
            string rowName,
            string iconResourcePath,
            string title,
            string body,
            float centerY)
        {
            var row = CreateUiObject(rowName, parent);
            var rowRect = row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0f, centerY);
            rowRect.anchorMax = new Vector2(1f, centerY);
            rowRect.pivot = new Vector2(0.5f, 0.5f);
            rowRect.offsetMin = new Vector2(SidePadding, -120f);
            rowRect.offsetMax = new Vector2(-SidePadding, 120f);

            var iconGo = CreateUiObject("Icon", row.transform);
            var iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(8f, 0f);
            iconRect.sizeDelta = new Vector2(IconSize, IconSize);

            var iconImage = iconGo.AddComponent<Image>();
            iconImage.color = Paper;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
            iconImage.sprite = LoadSprite(iconResourcePath);

            var textRoot = CreateUiObject("Text", row.transform);
            var textRect = textRoot.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(IconSize + TextGap, 0f);
            textRect.offsetMax = Vector2.zero;

            var titleGo = CreateUiObject("Title", textRoot.transform);
            var titleRect = titleGo.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0f, 1f);
            titleRect.sizeDelta = new Vector2(0f, 52f);

            CreateText(titleGo.transform, title, 38f, FontStyles.Bold, TextAlignmentOptions.TopLeft);

            var bodyGo = CreateUiObject("Body", textRoot.transform);
            var bodyRect = bodyGo.GetComponent<RectTransform>();
            bodyRect.anchorMin = Vector2.zero;
            bodyRect.anchorMax = Vector2.one;
            bodyRect.offsetMin = new Vector2(0f, 0f);
            bodyRect.offsetMax = new Vector2(0f, -56f);

            var bodyText = CreateText(bodyGo.transform, body, 32f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            bodyText.lineSpacing = 12f;
        }

        private void BuildActionButton(
            Transform parent,
            string name,
            string label,
            float anchorY,
            UnityEngine.Events.UnityAction onClick)
        {
            var buttonGo = CreateUiObject(name, parent);
            var rect = buttonGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, anchorY);
            rect.anchorMax = new Vector2(0.5f, anchorY);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);

            var fill = buttonGo.AddComponent<Image>();
            fill.color = Paper;

            var outline = buttonGo.AddComponent<Outline>();
            outline.effectColor = Ink;
            outline.effectDistance = new Vector2(BorderWidth, -BorderWidth);

            var labelGo = CreateUiObject("Label", buttonGo.transform);
            StretchRect(labelGo);
            CreateText(labelGo.transform, label, 36f, FontStyles.Bold, TextAlignmentOptions.Center);

            var button = buttonGo.AddComponent<Button>();
            button.targetGraphic = fill;
            button.onClick.AddListener(onClick);
        }

        private static TextMeshProUGUI CreateText(
            Transform parent,
            string value,
            float size,
            FontStyles style,
            TextAlignmentOptions alignment)
        {
            var go = CreateUiObject("Label", parent);
            StretchRect(go);

            var text = go.AddComponent<TextMeshProUGUI>();
            if (_font != null)
                text.font = _font;

            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = Ink;
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Overflow;
            text.richText = false;
            text.raycastTarget = false;
            text.UpdateMeshPadding();
            text.ForceMeshUpdate();

            return text;
        }

        private void Close()
        {
            if (menuController != null)
                menuController.OnCloseInstructions();
            else
                gameObject.SetActive(false);
        }

        private static Sprite LoadSprite(string resourcePath)
        {
            var sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite != null)
                return sprite;

            var texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null)
                return null;

            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);
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
    }
}
