using System;
using System.Collections;
using UnityEngine;

namespace EscapeRoom.Puzzles.Raetsel3
{
    /// <summary>
    /// Nach dem Fallen legt sich das Bild auf den Boden und zeigt auf der Rückseite ein verborgenes Buch.
    /// </summary>
    public class PictureRevealBook : MonoBehaviour
    {
        [Header("Referenzen")]
        [Tooltip("Das Skript, das den Bildfall auslöst.")]
        [SerializeField] private PictureFall pictureFall;

        [Tooltip("GameObject des Buchs auf der Rückseite. Es wird nach dem Fallen aktiv.")]
        [SerializeField] private GameObject bookOnBack;

        [Tooltip("Optionaler auffälliger Effekt für das Buch (z. B. Glow oder Outline).")]
        [SerializeField] private GameObject highlight;

        [Header("Animation")]
        [Tooltip("Dauer, bis das Bild auf den Boden gerollt ist.")]
        [SerializeField] private float layDownDuration = 0.6f;

        [Tooltip("Verzögerung, bevor das Buch auf der Rückseite angezeigt wird.")]
        [SerializeField] private float revealDelay = 0.25f;

        [Tooltip("Zusätzliche Rotation nach dem Flachlegen (nur Feinjustierung).")]
        [SerializeField] private Vector3 targetEulerOnGround = Vector3.zero;

        private bool _revealed;

        private void Awake()
        {
            // PictureHintCover übernimmt die Anzeige, wenn vorhanden.
            if (GetComponent<PictureHintCover>() != null)
            {
                enabled = false;
                return;
            }

            if (pictureFall == null) pictureFall = GetComponent<PictureFall>();
            if (bookOnBack != null) bookOnBack.SetActive(false);
            if (highlight != null) highlight.SetActive(false);
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

        private void OnPictureFallen()
        {
            if (_revealed) return;
            _revealed = true;
            StartCoroutine(LayDownAndReveal());
        }

        private IEnumerator LayDownAndReveal()
        {
            // PictureFall legt das Bild bereits flach auf den Boden.
            if (pictureFall == null || !pictureFall.HasFallen)
            {
                float elapsed = 0f;
                Quaternion startRot = transform.rotation;
                Vector3 startPos = transform.position;
                Vector3 fallDirection = transform.forward;
                fallDirection.y = 0f;
                if (fallDirection.sqrMagnitude < 0.0001f)
                    fallDirection = Vector3.forward;
                Quaternion endRot = PictureFall.GetFlatRotationOnGround(fallDirection) * Quaternion.Euler(targetEulerOnGround);
                Vector3 endPos = PictureFall.SnapToGround(transform, GetComponent<Collider>(), endRot, startPos, 0f);

                while (elapsed < layDownDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, elapsed / layDownDuration);
                    transform.rotation = Quaternion.Slerp(startRot, endRot, t);
                    transform.position = Vector3.Lerp(startPos, endPos, t);
                    yield return null;
                }

                transform.SetPositionAndRotation(endPos, endRot);
            }

            if (revealDelay > 0f)
                yield return new WaitForSeconds(revealDelay);

            if (bookOnBack != null)
                bookOnBack.SetActive(true);
            if (highlight != null)
                highlight.SetActive(true);

            Debug.Log("[Rätsel3] Buch auf der Rückseite ist jetzt sichtbar.");
        }
    }
}
