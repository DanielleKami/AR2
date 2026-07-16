using EscapeRoom.Core;
using EscapeRoom.Puzzles.Raetsel3;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EscapeRoom.EditorTools
{
    /// <summary>
    /// Richtet Mock 28 (Rätsel-3-HUD) in raetsel_3 ein.
    ///
    /// Menü: Tools ▸ Escape Room ▸ Rätsel 3 – Mock 28 HUD aufbauen
    /// </summary>
    public static class Raetsel3HudBuilder
    {
        private const string ScenePath = "Assets/Scenes/raetsel_3.unity";

        [MenuItem("Tools/Escape Room/Rätsel 3 – Mock 28 HUD aufbauen")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError("[Raetsel3HudBuilder] Szene nicht gefunden: " + ScenePath);
                return;
            }

            EnsureEventSystem();
            var canvas = EnsureCanvas();
            EnsureCodeManager();
            EnsureController();

            var existing = GameObject.Find("Raetsel3Hud");
            if (existing != null)
                Object.DestroyImmediate(existing);

            var hudRoot = new GameObject("Raetsel3Hud", typeof(RectTransform), typeof(Raetsel3HudView), typeof(Raetsel3HudController));
            hudRoot.layer = LayerMask.NameToLayer("UI");
            hudRoot.transform.SetParent(canvas.transform, false);
            Stretch(hudRoot);

            var view = hudRoot.GetComponent<Raetsel3HudView>();
            var controller = hudRoot.GetComponent<Raetsel3HudController>();

            var pictureFall = Object.FindAnyObjectByType<PictureFall>();
            if (pictureFall != null)
                pictureFall.ConfigureForHudTap();

            SetSerializedField(controller, "hudView", view);
            SetSerializedField(controller, "pictureFall", pictureFall);

            WireControllerReferences(pictureFall);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = hudRoot;
            Debug.Log("[Raetsel3HudBuilder] Mock-28-HUD aufgebaut.");
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null)
                return;

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        private static Canvas EnsureCanvas()
        {
            var canvasGo = GameObject.Find("Canvas") ?? new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.layer = LayerMask.NameToLayer("UI");

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            Stretch(canvasGo);
            return canvas;
        }

        private static void EnsureCodeManager()
        {
            if (Object.FindAnyObjectByType<CodeManager>() != null)
                return;

            new GameObject("CodeManager", typeof(CodeManager));
        }

        private static void EnsureController()
        {
            var controller = Object.FindAnyObjectByType<Raetsel3Controller>();
            if (controller != null)
                return;

            var go = new GameObject("Raetsel3Controller", typeof(Raetsel3Controller));
            var newController = go.GetComponent<Raetsel3Controller>();
            var picture = Object.FindAnyObjectByType<PictureStraighten>();
            var fall = Object.FindAnyObjectByType<PictureFall>();
            var book = Object.FindAnyObjectByType<BookInteraction>();

            SetSerializedField(newController, "picture", picture);
            SetSerializedField(newController, "pictureFall", fall);
            SetSerializedField(newController, "book", book);
        }

        private static void WireControllerReferences(PictureFall pictureFall)
        {
            var controller = Object.FindAnyObjectByType<Raetsel3Controller>();
            if (controller == null)
                return;

            var picture = Object.FindAnyObjectByType<PictureStraighten>();
            var book = Object.FindAnyObjectByType<BookInteraction>();
            SetSerializedField(controller, "picture", picture);
            SetSerializedField(controller, "pictureFall", pictureFall);
            SetSerializedField(controller, "book", book);
        }

        private static void SetSerializedField(Object target, string fieldName, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetSerializedField(Object target, string fieldName, float value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.floatValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetSerializedField(Object target, string fieldName, bool value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.boolValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void Stretch(GameObject go)
        {
            var rect = go.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
