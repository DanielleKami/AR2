using System.Collections;
using UnityEngine;

namespace EscapeRoom.Puzzles.Raetsel3
{
    /// <summary>
    /// Spawnt ein Buch vor dem Bild wenn dieses fällt, lässt es physikalisch fallen
    /// und sorgt dafür, dass es auf dem Boden liegt mit der Rückseite sichtbar und hervorgehoben.
    /// </summary>
    public class BookDrop : MonoBehaviour
    {
        [Tooltip("Prefab des Buchs, das fallen soll (z.B. Book_1c).")]
        public GameObject bookPrefab;

        [Tooltip("Lokaler Versatz relativ zum Bild-Transform für den Spawnpunkt (x,y,z).")]
        public Vector3 spawnOffset = new Vector3(0f, 0.3f, 0.5f);

        [Tooltip("Impuls-Kraft nach vorne beim Spawnen (Impulse).")]
        public float forwardForce = 1f;

        [Tooltip("Drehmoment (Torque) als Impuls, um das Buch beim Fall leicht zu drehen.")]
        public Vector3 spinTorque = new Vector3(1f, 0.2f, 0.2f);

        [Tooltip("Wartezeit in Sekunden bis zur Korrektur/Finalisierung nach dem Fallen.")]
        public float finalizeDelay = 1.2f;

        [Tooltip("Optionales Highlight-Objekt (z.B. Quad/Glow) das nach dem Landen aktiviert wird.")]
        public GameObject highlightToEnable;

        [Tooltip("Soll das Buch bei Spawn als Rigidbody/Collider ergänzt werden, falls nicht vorhanden?")]
        public bool ensurePhysics = true;

        private PictureFall _pictureFall;
        private bool _spawned;

        private void Awake()
        {
            _pictureFall = GetComponent<PictureFall>();
        }

        private void OnEnable()
        {
            if (_pictureFall != null)
                _pictureFall.Fallen += OnPictureFallen;
        }

        private void OnDisable()
        {
            if (_pictureFall != null)
                _pictureFall.Fallen -= OnPictureFallen;
        }

        private void OnPictureFallen()
        {
            if (_spawned) return;
            _spawned = true;
            SpawnAndDrop();
        }

        private void SpawnAndDrop()
        {
            if (bookPrefab == null)
            {
                Debug.LogWarning("[BookDrop] Kein bookPrefab gesetzt.");
                return;
            }

            // Berechne Spawnposition in Weltkoordinaten
            Vector3 spawnWorld = transform.TransformPoint(spawnOffset);
            Quaternion spawnRot = Quaternion.LookRotation(-transform.forward); // Buch vorne zur Kamera ausrichten

            GameObject book = Instantiate(bookPrefab, spawnWorld, spawnRot);
            book.name = bookPrefab.name + "_dropped";

            // Stelle sicher, dass ein Collider vorhanden ist
            Collider col = book.GetComponentInChildren<Collider>();
            if (col == null && ensurePhysics)
            {
                var bc = book.AddComponent<BoxCollider>();
                bc.isTrigger = false;
            }

            Rigidbody rb = book.GetComponent<Rigidbody>();
            if (rb == null && ensurePhysics)
            {
                rb = book.AddComponent<Rigidbody>();
                rb.mass = 0.5f;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
            }

            // Kleiner Aufwärtsimpuls + Vorwärts
            if (rb != null)
            {
                Vector3 impulse = transform.forward * forwardForce + Vector3.up * 0.25f;
                rb.AddForce(impulse, ForceMode.Impulse);
                rb.AddTorque(spinTorque, ForceMode.Impulse);
            }

            // Deaktiviere Highlight/Reveal falls vorhanden
            if (highlightToEnable != null) highlightToEnable.SetActive(false);

            // Nach einer Verzögerung das Buch final auf den Boden drehen und Highlight aktivieren
            StartCoroutine(FinalizeAfterDelay(book));
        }

        private IEnumerator FinalizeAfterDelay(GameObject book)
        {
            yield return new WaitForSeconds(finalizeDelay);

            if (book == null) yield break;

            // Versuche die Position/Zustand zu korrigieren: lege das Buch flach aufs Level
            Vector3 pos = book.transform.position;
            pos.y = 0.02f; // knapp über dem Boden (anpassen falls Bodenhöhe anders)
            book.transform.position = pos;

            // Drehe das Buch so, dass die Rückseite oben liegt (auf dem Rücken)
            // Wir nutzen eine flache Rotation: X = 90 -> flach, Y = 0/180 um Rückseite zu zeigen
            book.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            // Stoppe Physik falls vorhanden, damit es stabil liegen bleibt
            Rigidbody rb = book.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            // Aktiviere Highlight / Auffälligkeit
            if (highlightToEnable != null)
                highlightToEnable.SetActive(true);

            Debug.Log("[BookDrop] Buch gelandet und auf der Rückseite sichtbar.");
        }
    }
}
