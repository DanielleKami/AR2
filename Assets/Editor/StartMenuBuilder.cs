using EscapeRoom.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EscapeRoom.EditorTools
{
    /// <summary>
    /// Baut das Startmenü pixelgenau gemäß Konzept-Mock 1 (MINI ESCAPE-ROOM).
    ///
    /// Menü: Tools ▸ Escape Room ▸ Startmenü – UI aufbauen
    /// </summary>
    public static class StartMenuBuilder
    {
        private const string ScenePath = "Assets/Scenes/StartMenu.unity";
        private const string HeroImagePath = "Assets/UI/start_menu_hero.png";
        private const string GearImagePath = "Assets/UI/settings_gear.png";
        private const string InstructionsBackPath = "Assets/UI/Instructions/back_arrow.png";
        private const string InstructionsStep1Path = "Assets/UI/Instructions/step1_scan.png";
        private const string InstructionsStep2Path = "Assets/UI/Instructions/step2_puzzle.png";
        private const string InstructionsStep3Path = "Assets/UI/Instructions/step3_keypad.png";
        private const string GameSceneName = "raetsel_3";

        private static readonly Color Ink = Color.black;
        private static readonly Color Paper = Color.white;

        private const float ButtonWidth = 920f;
        private const float ButtonHeight = 94f;
        private const float BorderWidth = 2f;

        [MenuItem("Tools/Escape Room/Startmenü – UI aufbauen")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError("[StartMenuBuilder] Szene nicht gefunden: " + ScenePath);
                return;
            }

            EnsureSpriteImportSettings(HeroImagePath);
            EnsureSpriteImportSettings(GearImagePath);
            EnsureInstructionsImportSettings();

            EnsureEventSystem();
            var canvas = EnsureCanvas();
            ClearCanvasChildren(canvas);

            var controller = EnsureController();

            BuildBackground(canvas);
            BuildTitle(canvas);
            BuildHeroImage(canvas);
            var instructionsPanel = BuildInstructionsPanel(canvas, controller);
            var settingsPanel = BuildSettingsPanel(canvas, controller);

            var startButton = BuildMenuButton(canvas.transform, "BtnSpielStarten", "SPIEL STARTEN", 0.228f);
            var instructionsButton = BuildMenuButton(canvas.transform, "BtnAnleitung", "ANLEITUNG", 0.168f);
            var settingsButton = BuildSettingsButton(canvas.transform);

            WireController(controller, startButton, instructionsButton, settingsButton, instructionsPanel, settingsPanel);

            SetupCamera();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = canvas.gameObject;
            Debug.Log("[StartMenuBuilder] Startmenü (Mock 1) aufgebaut.");
        }

        [MenuItem("Tools/Escape Room/Anleitung – Mock 2 UI aufbauen")]
        public static void BuildInstructionsOnly()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError("[StartMenuBuilder] Szene nicht gefunden: " + ScenePath);
                return;
            }

            EnsureInstructionsImportSettings();

            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[StartMenuBuilder] Kein Canvas in StartMenu gefunden.");
                return;
            }

            var controller = Object.FindAnyObjectByType<StartMenuController>();
            if (controller == null)
            {
                Debug.LogError("[StartMenuBuilder] Kein StartMenuController gefunden.");
                return;
            }

            var existing = GameObject.Find("InstructionsPanel");
            if (existing != null)
                Object.DestroyImmediate(existing);

            var instructionsButton = GameObject.Find("BtnAnleitung")?.GetComponent<Button>();
            var panel = BuildInstructionsPanel(canvas, controller);

            if (instructionsButton != null)
                WireButton(instructionsButton, controller, nameof(StartMenuController.OnShowInstructions));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = panel;
            Debug.Log("[StartMenuBuilder] Anleitung (Mock 2) aufgebaut.");
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null)
                return;

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        private static Canvas EnsureCanvas()
        {
            var canvasGo = GameObject.Find("Canvas") ?? new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.layer = LayerMask.NameToLayer("UI");

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            var rect = canvasGo.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return canvas;
        }

        private static void ClearCanvasChildren(Canvas canvas)
        {
            var transform = canvas.transform;
            for (var i = transform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(transform.GetChild(i).gameObject);
        }

        private static StartMenuController EnsureController()
        {
            var controllerGo = GameObject.Find("StartMenuController") ?? new GameObject("StartMenuController");
            var controller = controllerGo.GetComponent<StartMenuController>();
            if (controller == null)
                controller = controllerGo.AddComponent<StartMenuController>();

            controller.gameSceneName = GameSceneName;
            return controller;
        }

        private static void BuildBackground(Canvas canvas)
        {
            var bg = CreateUiObject("Background", canvas.transform);
            StretchFull(bg);
            var image = bg.AddComponent<Image>();
            image.color = Paper;
            image.raycastTarget = false;
        }

        private static void BuildTitle(Canvas canvas)
        {
            var titleRoot = CreateUiObject("Title", canvas.transform);
            var rect = titleRoot.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -120f);
            rect.sizeDelta = new Vector2(980f, 260f);

            var outlineGo = CreateUiObject("Outline", titleRoot.transform);
            StretchFull(outlineGo);
            var outlineText = outlineGo.AddComponent<TextMeshProUGUI>();
            ApplyTitleStyle(outlineText, "MINI\nESCAPE-ROOM", 96f, Ink);

            var fillGo = CreateUiObject("Fill", titleRoot.transform);
            StretchFull(fillGo);
            var fillText = fillGo.AddComponent<TextMeshProUGUI>();
            ApplyTitleStyle(fillText, "MINI\nESCAPE-ROOM", 84f, Paper);
        }

        private static void ApplyTitleStyle(TextMeshProUGUI text, string value, float size, Color face)
        {
            text.text = value;
            text.fontSize = size;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = face;
            text.characterSpacing = 1f;
            text.lineSpacing = -16f;
            text.raycastTarget = false;
        }

        private static void BuildHeroImage(Canvas canvas)
        {
            var heroGo = CreateUiObject("DoorIllustration", canvas.transform);
            var rect = heroGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.535f);
            rect.anchorMax = new Vector2(0.5f, 0.535f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(760f, 660f);

            var image = heroGo.AddComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.color = Color.white;
            image.sprite = LoadSprite(HeroImagePath);
        }

        private static Button BuildMenuButton(Transform parent, string name, string label, float anchorY)
        {
            var buttonGo = CreateUiObject(name, parent);
            var rect = buttonGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, anchorY);
            rect.anchorMax = new Vector2(0.5f, anchorY);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);

            var border = buttonGo.AddComponent<Image>();
            border.color = Ink;

            var fillGo = CreateUiObject("Fill", buttonGo.transform);
            var fillRect = fillGo.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(BorderWidth, BorderWidth);
            fillRect.offsetMax = new Vector2(-BorderWidth, -BorderWidth);
            var fill = fillGo.AddComponent<Image>();
            fill.color = Paper;
            fill.raycastTarget = false;

            var labelGo = CreateUiObject("Label", buttonGo.transform);
            StretchFull(labelGo);
            var text = labelGo.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 36f;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Ink;
            text.characterSpacing = 2f;
            text.raycastTarget = false;

            var button = buttonGo.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Ink;
            colors.highlightedColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            colors.pressedColor = new Color(0.35f, 0.35f, 0.35f, 1f);
            colors.selectedColor = Ink;
            button.colors = colors;
            button.targetGraphic = border;

            return button;
        }

        private static Button BuildSettingsButton(Transform parent)
        {
            var buttonGo = CreateUiObject("BtnSettings", parent);
            var rect = buttonGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-48f, -48f);
            rect.sizeDelta = new Vector2(56f, 56f);

            var image = buttonGo.AddComponent<Image>();
            image.color = Color.white;
            image.preserveAspect = true;
            image.raycastTarget = true;
            image.sprite = LoadSprite(GearImagePath);

            var button = buttonGo.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            button.colors = colors;
            button.targetGraphic = image;

            return button;
        }

        private static GameObject BuildInstructionsPanel(Canvas canvas, StartMenuController controller)
        {
            var panel = CreateUiObject("InstructionsPanel", canvas.transform);
            StretchFull(panel);
            panel.SetActive(false);

            var view = panel.AddComponent<InstructionsScreenView>();

            var serializedView = new SerializedObject(view);
            serializedView.FindProperty("menuController").objectReferenceValue = controller;
            serializedView.ApplyModifiedPropertiesWithoutUndo();

            controller.instructionsPanel = panel;
            return panel;
        }

        private static void EnsureInstructionsImportSettings()
        {
            EnsureSpriteImportSettings(InstructionsBackPath);
            EnsureSpriteImportSettings(InstructionsStep1Path);
            EnsureSpriteImportSettings(InstructionsStep2Path);
            EnsureSpriteImportSettings(InstructionsStep3Path);
            EnsureSpriteImportSettings("Assets/Resources/UI/Instructions/back_arrow.png");
            EnsureSpriteImportSettings("Assets/Resources/UI/Instructions/step1_scan.png");
            EnsureSpriteImportSettings("Assets/Resources/UI/Instructions/step2_puzzle.png");
            EnsureSpriteImportSettings("Assets/Resources/UI/Instructions/step3_keypad.png");
        }

        private static GameObject BuildSettingsPanel(Canvas canvas, StartMenuController controller)
        {
            var panel = CreateOverlayPanel(canvas.transform, "SettingsPanel", "EINSTELLUNGEN",
                "Einstellungen folgen in einer späteren Version.\n\n" +
                "Geplant sind unter anderem:\n" +
                "- Lautstärke\n" +
                "- Sprache\n" +
                "- Steuerung");

            controller.settingsPanel = panel;
            return panel;
        }

        private static GameObject CreateOverlayPanel(Transform parent, string name, string title, string body)
        {
            var panel = CreateUiObject(name, parent);
            StretchFull(panel);
            panel.SetActive(false);

            var dim = CreateUiObject("Dim", panel.transform);
            StretchFull(dim);
            var dimImage = dim.AddComponent<Image>();
            dimImage.color = new Color(0f, 0f, 0f, 0.55f);

            var card = CreateUiObject("Card", panel.transform);
            var cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(860f, 980f);

            var cardBorder = card.AddComponent<Image>();
            cardBorder.color = Ink;

            var cardFillGo = CreateUiObject("Fill", card.transform);
            var cardFillRect = cardFillGo.GetComponent<RectTransform>();
            cardFillRect.anchorMin = Vector2.zero;
            cardFillRect.anchorMax = Vector2.one;
            cardFillRect.offsetMin = new Vector2(BorderWidth, BorderWidth);
            cardFillRect.offsetMax = new Vector2(-BorderWidth, -BorderWidth);
            var cardFill = cardFillGo.AddComponent<Image>();
            cardFill.color = Paper;

            var titleGo = CreateUiObject("Title", card.transform);
            var titleRect = titleGo.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -36f);
            titleRect.sizeDelta = new Vector2(760f, 80f);
            var titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = 40f;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Ink;

            var bodyGo = CreateUiObject("Body", card.transform);
            var bodyRect = bodyGo.GetComponent<RectTransform>();
            bodyRect.anchorMin = new Vector2(0.5f, 0.5f);
            bodyRect.anchorMax = new Vector2(0.5f, 0.5f);
            bodyRect.pivot = new Vector2(0.5f, 0.5f);
            bodyRect.anchoredPosition = new Vector2(0f, 20f);
            bodyRect.sizeDelta = new Vector2(760f, 680f);
            var bodyText = bodyGo.AddComponent<TextMeshProUGUI>();
            bodyText.text = body;
            bodyText.fontSize = 30f;
            bodyText.alignment = TextAlignmentOptions.TopLeft;
            bodyText.color = Ink;
            bodyText.lineSpacing = 6f;

            var closeButton = BuildMenuButton(card.transform, "BtnClose", "SCHLIESSEN", 0.08f);
            var closeRect = closeButton.GetComponent<RectTransform>();
            closeRect.sizeDelta = new Vector2(420f, 80f);

            var controller = Object.FindAnyObjectByType<StartMenuController>();
            if (controller != null)
            {
                if (name == "InstructionsPanel")
                    WireButton(closeButton, controller, nameof(StartMenuController.OnCloseInstructions));
                else
                    WireButton(closeButton, controller, nameof(StartMenuController.OnCloseSettings));
            }

            return panel;
        }

        private static void WireController(
            StartMenuController controller,
            Button startButton,
            Button instructionsButton,
            Button settingsButton,
            GameObject instructionsPanel,
            GameObject settingsPanel)
        {
            controller.instructionsPanel = instructionsPanel;
            controller.settingsPanel = settingsPanel;
            controller.gameSceneName = GameSceneName;

            WireButton(startButton, controller, nameof(StartMenuController.OnStartGame));
            WireButton(instructionsButton, controller, nameof(StartMenuController.OnShowInstructions));
            WireButton(settingsButton, controller, nameof(StartMenuController.OnShowSettings));
        }

        private static void WireButton(Button button, Object target, string methodName)
        {
            var serializedButton = new SerializedObject(button);
            var onClick = serializedButton.FindProperty("m_OnClick");
            onClick.FindPropertyRelative("m_PersistentCalls.m_Calls").ClearArray();
            serializedButton.ApplyModifiedPropertiesWithoutUndo();

            button.onClick.RemoveAllListeners();

            switch (methodName)
            {
                case nameof(StartMenuController.OnStartGame) when target is StartMenuController start:
                    UnityEventTools.AddPersistentListener(button.onClick, start.OnStartGame);
                    break;
                case nameof(StartMenuController.OnShowInstructions) when target is StartMenuController instructions:
                    UnityEventTools.AddPersistentListener(button.onClick, instructions.OnShowInstructions);
                    break;
                case nameof(StartMenuController.OnShowSettings) when target is StartMenuController settings:
                    UnityEventTools.AddPersistentListener(button.onClick, settings.OnShowSettings);
                    break;
                case nameof(StartMenuController.OnCloseInstructions) when target is StartMenuController closeInstructions:
                    UnityEventTools.AddPersistentListener(button.onClick, closeInstructions.OnCloseInstructions);
                    break;
                case nameof(StartMenuController.OnCloseSettings) when target is StartMenuController closeSettings:
                    UnityEventTools.AddPersistentListener(button.onClick, closeSettings.OnCloseSettings);
                    break;
                default:
                    Debug.LogWarning("[StartMenuBuilder] Unbekannte Button-Methode: " + methodName);
                    break;
            }

            EditorUtility.SetDirty(button);
        }

        private static void EnsureSpriteImportSettings(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        private static Sprite LoadSprite(string assetPath)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite != null)
                return sprite;

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (texture == null)
            {
                Debug.LogWarning("[StartMenuBuilder] Sprite nicht gefunden: " + assetPath);
                return null;
            }

            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        }

        private static void SetupCamera()
        {
            var camera = Camera.main;
            if (camera == null)
                return;

            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Paper;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void StretchFull(GameObject go)
        {
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
