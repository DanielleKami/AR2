using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using EscapeRoom.Puzzles.Raetsel3;

namespace EscapeRoom.EditorTools
{
    public static class Painting1bHintSetup
    {
        [MenuItem("Tools/Escape Room/Rätsel 3 – Hinweis-Cover an Painting_1b anlegen")]
        public static void SetupSelectedPainting()
        {
            var painting = Selection.activeGameObject;
            if (painting == null)
            {
                EditorUtility.DisplayDialog("Rätsel 3", "Bitte Painting_1b in der Hierarchy auswählen.", "OK");
                return;
            }

            SetupPainting(painting);
        }

        [MenuItem("Tools/Escape Room/Rätsel 3 – Hinweis-Cover an Painting_1b anlegen", true)]
        private static bool SetupSelectedPaintingValidate() => Selection.activeGameObject != null;

        public static void SetupPainting(GameObject painting)
        {
            Undo.RegisterFullObjectHierarchyUndo(painting, "Rätsel 3 Hinweis-Cover");

            UnparentMisplacedObjects(painting.transform);
            CleanupBrokenSetup(painting.transform);
            RemoveMisplacedComponents(painting.transform);

            var fall = painting.GetComponent<PictureFall>() ?? Undo.AddComponent<PictureFall>(painting);
            var hint = painting.GetComponent<PictureHintCover>() ?? Undo.AddComponent<PictureHintCover>(painting);

            var coverRoot = PictureHintCover.BuildCoverCanvas(
                painting.transform,
                PictureHintCover.DefaultHint,
                new Vector2(0.45f, 0.32f));

            var so = new SerializedObject(hint);
            so.FindProperty("pictureFall").objectReferenceValue = fall;
            so.FindProperty("hintCoverRoot").objectReferenceValue = coverRoot;
            so.FindProperty("hintMessage").stringValue = PictureHintCover.DefaultHint;
            so.ApplyModifiedPropertiesWithoutUndo();

            var fallSo = new SerializedObject(fall);
            fallSo.FindProperty("fallAngleDeg").floatValue = 80f;
            fallSo.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(painting.scene);
            Selection.activeGameObject = painting;

            Debug.Log("[Rätsel3] Hinweis-Cover eingerichtet. Play drücken zum Testen.");
        }

        private static void UnparentMisplacedObjects(Transform painting)
        {
            for (int i = painting.childCount - 1; i >= 0; i--)
            {
                Transform child = painting.GetChild(i);
                if (child.name == "HintBookCover") continue;
                if (child.name.StartsWith("Table") || child.name.StartsWith("Book") || child.name == "Photo1")
                    Undo.SetTransformParent(child, null, "Szene bereinigen");
            }
        }

        private static void CleanupBrokenSetup(Transform painting)
        {
            for (int i = painting.childCount - 1; i >= 0; i--)
            {
                if (painting.GetChild(i).name == "HintBookCover")
                    Undo.DestroyObjectImmediate(painting.GetChild(i).gameObject);
            }
        }

        private static void RemoveMisplacedComponents(Transform painting)
        {
            foreach (var c in painting.GetComponentsInChildren<PictureHintCover>(true))
                if (c.transform != painting) Undo.DestroyObjectImmediate(c);

            foreach (var c in painting.GetComponentsInChildren<PictureFall>(true))
                if (c.transform != painting) Undo.DestroyObjectImmediate(c);
        }
    }
}
