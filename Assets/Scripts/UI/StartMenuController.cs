using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeRoom.UI
{
    /// <summary>
    /// Steuert das Startmenü und lädt die Rätsel-Szene oder zeigt die Anleitung an.
    ///</summary>
    public class StartMenuController : MonoBehaviour
    {
        [Header("Szenen")]
        [Tooltip("Name der Szene, die beim Starten des Spiels geladen wird.")]
        public string gameSceneName = "raetsel_3";

        [Header("UI-Panels")]
        [Tooltip("Optionales Panel mit der Anleitung. Wird im Menü ein-/ausgeblendet.")]
        public GameObject instructionsPanel;

        [Tooltip("Optionales Panel mit den Einstellungen.")]
        public GameObject settingsPanel;

        [Header("AR-Scan")]
        [SerializeField] private ARScanController arScanController;

        private void Awake()
        {
            EnsureMenuCanvasVisible();

            if (instructionsPanel != null)
                instructionsPanel.SetActive(false);

            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            if (arScanController == null)
                arScanController = GetComponent<ARScanController>();
            if (arScanController == null)
                arScanController = gameObject.AddComponent<ARScanController>();
        }

        private void EnsureMenuCanvasVisible()
        {
            var canvas = GameObject.Find("Canvas");
            if (canvas == null)
                return;

            var rect = canvas.GetComponent<RectTransform>();
            if (rect != null)
            {
                if (rect.localScale.sqrMagnitude < 0.01f)
                    rect.localScale = Vector3.one;

                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
        }

        /// <summary>
        /// Wird vom Start-Button aufgerufen.
        /// </summary>
        public void OnStartGame()
        {
            if (arScanController == null)
                arScanController = GetComponent<ARScanController>();

            if (arScanController != null)
            {
                arScanController.BeginScan();
                return;
            }

            if (string.IsNullOrWhiteSpace(gameSceneName))
            {
                Debug.LogWarning("StartMenuController: Kein Spiel-Szenenname gesetzt.");
                return;
            }

            SceneManager.LoadScene(gameSceneName);
        }

        /// <summary>
        /// Zeigt das Anleitungs-Panel an.
        /// </summary>
        public void OnShowInstructions()
        {
            if (instructionsPanel == null)
            {
                Debug.LogWarning("StartMenuController: Kein InstructionsPanel gesetzt.");
                return;
            }

            instructionsPanel.SetActive(true);
            instructionsPanel.transform.SetAsLastSibling();

            var view = instructionsPanel.GetComponent<InstructionsScreenView>();
            if (view != null && instructionsPanel.transform.Find("Step1") == null)
                view.Rebuild();
        }

        /// <summary>
        /// Schließt das Anleitungs-Panel.
        /// </summary>
        public void OnCloseInstructions()
        {
            if (instructionsPanel == null) return;
            instructionsPanel.SetActive(false);
        }

        /// <summary>
        /// Zeigt das Einstellungs-Panel an.
        /// </summary>
        public void OnShowSettings()
        {
            if (settingsPanel == null)
            {
                Debug.LogWarning("StartMenuController: Kein SettingsPanel gesetzt.");
                return;
            }

            settingsPanel.SetActive(true);
        }

        /// <summary>
        /// Schließt das Einstellungs-Panel.
        /// </summary>
        public void OnCloseSettings()
        {
            if (settingsPanel == null) return;
            settingsPanel.SetActive(false);
        }
    }
}
