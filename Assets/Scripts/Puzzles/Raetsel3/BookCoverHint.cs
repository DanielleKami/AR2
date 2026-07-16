using TMPro;
using UnityEngine;

namespace EscapeRoom.Puzzles.Raetsel3
{
    /// <summary>
    /// Zeigt auf der Rückseite des Bildes ein Buchcover mit Hinweistext an
    /// (Mock 3: „Schätze lassen sich auch in Büchern finden“).
    /// </summary>
    public class BookCoverHint : MonoBehaviour
    {
        [Header("Inhalt")]
        [TextArea(2, 4)]
        [SerializeField] private string hintText = "Schätze lassen sich auch in Büchern finden";

        [Header("Layout (lokal am Bild)")]
        [Tooltip("Versatz zur Rückseite des Bildes (lokal).")]
        [SerializeField] private Vector3 localOffset = new Vector3(0f, 0f, -0.026f);

        [SerializeField] private Vector2 coverSize = new Vector2(0.28f, 0.2f);

        [Tooltip("Feinjustierung der Textgröße.")]
        [SerializeField] private float fontSize = 0.045f;

        [Header("Optik")]
        [SerializeField] private Texture2D coverTexture;
        [SerializeField] private Color coverColor = new Color(0.42f, 0.26f, 0.14f);
        [SerializeField] private Color textColor = new Color(0.95f, 0.92f, 0.84f);

        private GameObject _root;
        private bool _built;

        public Transform CoverRoot => _root != null ? _root.transform : null;

        private void Awake()
        {
            Hide();
        }

        public void Show()
        {
            EnsureBuilt();
            if (_root != null)
                _root.SetActive(true);
        }

        public void Hide()
        {
            if (_root != null)
                _root.SetActive(false);
        }

        public void EnsureBuilt()
        {
            if (_built) return;
            _built = true;

            _root = new GameObject("BookCoverHint");
            _root.transform.SetParent(transform, false);
            _root.transform.localPosition = localOffset;
            _root.transform.localScale = Vector3.one;

            var cover = GameObject.CreatePrimitive(PrimitiveType.Quad);
            cover.name = "Cover";
            Object.Destroy(cover.GetComponent<Collider>());
            cover.transform.SetParent(_root.transform, false);
            cover.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            cover.transform.localScale = new Vector3(coverSize.x, coverSize.y, 1f);
            cover.GetComponent<Renderer>().sharedMaterial = CreateCoverMaterial();

            var textGo = new GameObject("HintText");
            textGo.transform.SetParent(_root.transform, false);
            textGo.transform.localPosition = new Vector3(0f, 0f, -0.002f);
            textGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            var tmp = textGo.AddComponent<TextMeshPro>();
            tmp.text = hintText;
            tmp.fontSize = fontSize;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;
            tmp.rectTransform.sizeDelta = coverSize;
        }

        private Material CreateCoverMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Unlit/Texture")
                         ?? Shader.Find("Standard");
            var mat = new Material(shader);

            if (coverTexture != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", coverTexture);
                mat.mainTexture = coverTexture;
            }
            else if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", coverColor);
            }

            mat.color = coverColor;
            return mat;
        }
    }
}
