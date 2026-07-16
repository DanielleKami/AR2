using EscapeRoom.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EscapeRoom.Puzzles.Raetsel3
{
    /// <summary>
    /// Stellt Mock-28-HUD zur Laufzeit bereit, falls die Szene noch keins enthält.
    /// </summary>
    public static class Raetsel3SceneBootstrap
    {
        private const string SceneName = "raetsel_3";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AfterSceneLoad()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.name != SceneName)
                return;

            ConfigurePictureFall();
            EnsureCodeManager();
            EnsureHud();
        }

        private static void ConfigurePictureFall()
        {
            var fall = Object.FindAnyObjectByType<PictureFall>();
            if (fall == null)
                return;

            fall.ConfigureForHudTap();
        }

        private static void EnsureCodeManager()
        {
            if (Object.FindAnyObjectByType<CodeManager>() != null)
                return;

            new GameObject("CodeManager", typeof(CodeManager));
        }

        private static void EnsureHud()
        {
            if (Object.FindAnyObjectByType<Raetsel3HudView>() != null)
                return;

            EnsureEventSystem();
            var canvas = EnsureCanvas();

            var hudRoot = new GameObject(
                "Raetsel3Hud",
                typeof(RectTransform),
                typeof(Raetsel3HudView),
                typeof(Raetsel3HudController));
            hudRoot.layer = LayerMask.NameToLayer("UI");
            hudRoot.transform.SetParent(canvas.transform, false);
            Stretch(hudRoot);
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null)
                return;

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }

        private static Canvas EnsureCanvas()
        {
            var existing = GameObject.Find("Raetsel3Canvas");
            if (existing != null)
                return existing.GetComponent<Canvas>();

            var canvasGo = new GameObject(
                "Raetsel3Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvasGo.layer = LayerMask.NameToLayer("UI");

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            Stretch(canvasGo);
            return canvas;
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
