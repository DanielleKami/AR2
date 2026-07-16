using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using EscapeRoom.Puzzles.Raetsel3;

namespace EscapeRoom.EditorTools
{
    /// <summary>
    /// Richtet ein ausgewähltes Bild (z. B. Painting_1b) für Rätsel 3 ein.
    /// Menü: Tools ▸ Escape Room ▸ Rätsel 3 – Buch-Hinweis am Bild einrichten
    /// </summary>
    public static class Raetsel3PictureSetup
    {
        private const string TexBookCover = "Assets/Textures/book_cover.png";

        [MenuItem("Tools/Escape Room/Rätsel 3 – Buch-Hinweis am Bild einrichten")]
        public static void SetupSelectedPicture()
        {
            var go = Selection.activeGameObject;
            if (go == null)
            {
                Debug.LogWarning("[Rätsel3] Bitte zuerst Painting_1b (oder das Bild) in der Hierarchy auswählen.");
                return;
            }

            var fall = go.GetComponent<PictureFall>() ?? go.AddComponent<PictureFall>();
            var reveal = go.GetComponent<PictureRevealBook>() ?? go.AddComponent<PictureRevealBook>();
            var cover = go.GetComponent<BookCoverHint>() ?? go.AddComponent<BookCoverHint>();

            var revealSo = new SerializedObject(reveal);
            revealSo.FindProperty("pictureFall").objectReferenceValue = fall;
            revealSo.FindProperty("bookCoverHint").objectReferenceValue = cover;
            revealSo.ApplyModifiedPropertiesWithoutUndo();

            var coverSo = new SerializedObject(cover);
            coverSo.FindProperty("hintText").stringValue =
                "Schätze lassen sich auch in Büchern finden";
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexBookCover);
            if (tex != null)
                coverSo.FindProperty("coverTexture").objectReferenceValue = tex;
            coverSo.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"[Rätsel3] Buch-Hinweis an '{go.name}' eingerichtet. Play drücken: Bild fällt → Cover erscheint.");
        }

        [MenuItem("Tools/Escape Room/Rätsel 3 – Buch-Hinweis am Bild einrichten", true)]
        private static bool SetupSelectedPictureValidate() => Selection.activeGameObject != null;
    }
}
