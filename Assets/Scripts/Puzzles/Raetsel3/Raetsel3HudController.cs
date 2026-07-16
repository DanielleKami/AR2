using UnityEngine;

namespace EscapeRoom.Puzzles.Raetsel3
{
    /// <summary>
    /// Verbindet Mock-28-HUD mit dem Bild-Fall (PictureFall).
    /// </summary>
    public class Raetsel3HudController : MonoBehaviour
    {
        [SerializeField] private Raetsel3HudView hudView;
        [SerializeField] private PictureFall pictureFall;

        private void Awake()
        {
            if (hudView == null)
                hudView = GetComponentInChildren<Raetsel3HudView>(true);
            if (pictureFall == null)
                pictureFall = FindAnyObjectByType<PictureFall>();
        }

        private void Start()
        {
            if (pictureFall != null)
            {
                pictureFall.Fallen += OnPictureFallen;
                if (hudView != null)
                    hudView.SetTapIconVisible(!pictureFall.HasFallen);
            }

            if (hudView?.TapButton != null)
                hudView.TapButton.onClick.AddListener(OnTapIconPressed);
        }

        private void OnDestroy()
        {
            if (pictureFall != null)
                pictureFall.Fallen -= OnPictureFallen;
            if (hudView?.TapButton != null)
                hudView.TapButton.onClick.RemoveListener(OnTapIconPressed);
        }

        private void OnTapIconPressed()
        {
            if (pictureFall == null || pictureFall.HasFallen || pictureFall.IsFalling)
                return;

            pictureFall.StartFall();
            hudView?.SetTapIconVisible(false);
        }

        private void OnPictureFallen()
        {
            hudView?.SetTapIconVisible(false);
        }
    }
}
