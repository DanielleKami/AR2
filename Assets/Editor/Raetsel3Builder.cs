using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using EscapeRoom.Core;
using EscapeRoom.Puzzles.Raetsel3;

namespace EscapeRoom.EditorTools
{
    /// <summary>
    /// Baut die komplette Test-Szene für Rätsel 3 (Bild &amp; Buch) per Menüklick auf:
    /// gerahmtes Bild mit Gemälde, aufklappbares Buch mit Ledercover, Buchseite mit "2",
    /// Kamera, CodeManager und Raetsel3Controller inkl. verdrahteter Referenzen.
    ///
    /// Menü:  Tools ▸ Escape Room ▸ Rätsel 3 – Test-Setup bauen
    /// </summary>
    public static class Raetsel3Builder
    {
        private const string RootName = "Raetsel3_Root";
        private const string TexPainting = "Assets/Textures/painting.png";
        private const string TexBookCover = "Assets/Textures/book_cover.png";
        private const string TexPage2 = "Assets/Textures/page2.png";

        [MenuItem("Tools/Escape Room/Rätsel 3 – Test-Setup bauen")]
        public static void Build()
        {
            // Vorhandenes Setup entfernen (idempotent)
            var existing = GameObject.Find(RootName);
            if (existing != null) Object.DestroyImmediate(existing);

            var root = new GameObject(RootName);

            BuildEnvironment(root.transform);
            var picture = BuildPicture(root.transform);
            // Erzeuge sowohl ein Tisch-Buch (zur Demonstration) als auch das aufgeklebte Buch auf der Rückseite
            var tableBook = BuildBook(root.transform, out GameObject page2, out Transform coverPivot, out BoxCollider bookCollider);
            var hintCover = BuildHintCover(picture.transform);

            var codeManager = BuildCodeManager(root.transform);
            BuildController(root.transform, picture, tableBook);

            WirePicture(picture);
            WireBook(tableBook, bookCollider, coverPivot, page2);
            WireHintCover(picture, hintCover);

            SetupCamera();

            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[Raetsel3Builder] Test-Setup aufgebaut. Jetzt auf ▶ Play drücken: " +
                      "Bild geradeziehen, dann auf das Buch klicken.");
        }

        // ---------- Aufbau ----------

        private static void BuildEnvironment(Transform parent)
        {
            // Boden
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Boden";
            floor.transform.SetParent(parent, false);
            floor.transform.position = new Vector3(0f, 0f, 2.8f);
            floor.GetComponent<Renderer>().sharedMaterial =
                MakeMaterial(false, null, new Color(0.55f, 0.55f, 0.58f));

            // Wand hinter dem Bild
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wand";
            Object.DestroyImmediate(wall.GetComponent<Collider>());
            wall.transform.SetParent(parent, false);
            wall.transform.position = new Vector3(0f, 1.6f, 4.25f);
            wall.transform.localScale = new Vector3(6f, 3.6f, 0.1f);
            wall.GetComponent<Renderer>().sharedMaterial =
                MakeMaterial(false, null, new Color(0.78f, 0.74f, 0.68f));

            // Licht ist in einer Basic-URP-Szene bereits vorhanden (Directional Light).
        }

        private static PictureStraighten BuildPicture(Transform parent)
        {
            var bild = new GameObject("Bild");
            bild.transform.SetParent(parent, false);
            bild.transform.position = new Vector3(0f, 1.6f, 4f);

            // Rahmen (dunkles Holz) leicht hinter der Leinwand
            var frame = MakeChildPrimitive(PrimitiveType.Cube, "Rahmen", bild.transform);
            frame.transform.localPosition = new Vector3(0f, 0f, 0.04f);
            frame.transform.localScale = new Vector3(1.42f, 1.12f, 0.06f);
            frame.GetComponent<Renderer>().sharedMaterial =
                MakeMaterial(false, null, new Color(0.20f, 0.12f, 0.06f));

            // Leinwand (Gemälde)
            var canvas = MakeChildPrimitive(PrimitiveType.Quad, "Leinwand", bild.transform);
            canvas.transform.localPosition = Vector3.zero;
            canvas.transform.localScale = new Vector3(1.3f, 1.0f, 1f);
            canvas.GetComponent<Renderer>().sharedMaterial =
                MakeMaterial(false, LoadTex(TexPainting), Color.white);

            // Collider zum Greifen/Drehen auf dem Bild-Root
            var col = bild.AddComponent<BoxCollider>();
            col.size = new Vector3(1.3f, 1.0f, 0.12f);

            return bild.AddComponent<PictureStraighten>();
        }

        private static BookInteraction BuildBook(Transform parent, out GameObject page2,
            out Transform coverPivot, out BoxCollider bookCollider)
        {
            var buch = new GameObject("Buch");
            buch.transform.SetParent(parent, false);
            buch.transform.position = new Vector3(0.75f, 0.12f, 2.6f);

            var leather = MakeMaterial(false, LoadTex(TexBookCover), Color.white);

            // Seitenblock
            var pages = MakeChildPrimitive(PrimitiveType.Cube, "Seiten", buch.transform);
            pages.transform.localPosition = Vector3.zero;
            pages.transform.localScale = new Vector3(0.95f, 0.16f, 0.65f);
            pages.GetComponent<Renderer>().sharedMaterial =
                MakeMaterial(false, null, new Color(0.93f, 0.90f, 0.82f));

            // Unterer Deckel
            var coverBottom = MakeChildPrimitive(PrimitiveType.Cube, "Deckel_unten", buch.transform);
            coverBottom.transform.localPosition = new Vector3(0f, -0.10f, 0f);
            coverBottom.transform.localScale = new Vector3(1.02f, 0.04f, 0.72f);
            coverBottom.GetComponent<Renderer>().sharedMaterial = leather;

            // Deckel-Pivot an der Buchrücken-Kante (links)
            var pivot = new GameObject("Deckel_Pivot");
            pivot.transform.SetParent(buch.transform, false);
            pivot.transform.localPosition = new Vector3(-0.5f, 0.10f, 0f);
            coverPivot = pivot.transform;

            // Oberer Deckel (klappt auf)
            var coverTop = MakeChildPrimitive(PrimitiveType.Cube, "Deckel_oben", pivot.transform);
            coverTop.transform.localPosition = new Vector3(0.5f, 0f, 0f);
            coverTop.transform.localScale = new Vector3(1.02f, 0.04f, 0.72f);
            coverTop.GetComponent<Renderer>().sharedMaterial = leather;

            // Buchseite mit "2" (anfangs ausgeblendet)
            page2 = MakeChildPrimitive(PrimitiveType.Quad, "Ziffer2", buch.transform);
            page2.transform.localPosition = new Vector3(0f, 0.10f, 0f);
            page2.transform.localRotation = Quaternion.Euler(60f, 0f, 0f);
            page2.transform.localScale = new Vector3(0.8f, 0.55f, 1f);
            page2.GetComponent<Renderer>().sharedMaterial =
                MakeMaterial(true, LoadTex(TexPage2), Color.white);
            page2.SetActive(false);

            // Collider zum Antippen auf dem Buch-Root
            bookCollider = buch.AddComponent<BoxCollider>();
            bookCollider.size = new Vector3(1.02f, 0.28f, 0.72f);

            return buch.AddComponent<BookInteraction>();
        }

        private static GameObject BuildHintCover(Transform pictureTransform)
        {
            return PictureHintCover.BuildCoverCanvas(
                pictureTransform,
                PictureHintCover.DefaultHint,
                new Vector2(0.45f, 0.32f));
        }

        private static CodeManager BuildCodeManager(Transform parent)
        {
            var go = new GameObject("CodeManager");
            go.transform.SetParent(parent, false);
            return go.AddComponent<CodeManager>();
        }

        private static void BuildController(Transform parent, PictureStraighten picture, BookInteraction book)
        {
            var go = new GameObject("Raetsel3Controller");
            go.transform.SetParent(parent, false);
            var ctrl = go.AddComponent<Raetsel3Controller>();

            var so = new SerializedObject(ctrl);
            so.FindProperty("picture").objectReferenceValue = picture;
            so.FindProperty("book").objectReferenceValue = book;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- Verdrahtung ----------

        private static void WirePicture(PictureStraighten picture)
        {
            var so = new SerializedObject(picture);
            so.FindProperty("startTiltAngle").floatValue = -22f;
            so.FindProperty("toleranceDeg").floatValue = 6f;
            so.FindProperty("rotationSpeed").floatValue = 0.3f;
            so.FindProperty("pictureCollider").objectReferenceValue =
                picture.GetComponent<BoxCollider>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireBook(BookInteraction book, BoxCollider bookCollider,
            Transform coverPivot, GameObject page2)
        {
            var so = new SerializedObject(book);
            so.FindProperty("bookCollider").objectReferenceValue = bookCollider;
            so.FindProperty("coverToOpen").objectReferenceValue = coverPivot;
            so.FindProperty("openAngle").floatValue = 150f;
            so.FindProperty("openDuration").floatValue = 0.7f;
            so.FindProperty("openAxis").vector3Value = Vector3.forward;
            so.FindProperty("digitReveal").objectReferenceValue = page2;
            so.FindProperty("interactableFromStart").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireHintCover(PictureStraighten picture, GameObject hintCoverRoot)
        {
            var pictureGo = picture.gameObject;
            var pf = pictureGo.GetComponent<PictureFall>();
            if (pf == null) pf = pictureGo.AddComponent<PictureFall>();

            var hint = pictureGo.GetComponent<PictureHintCover>();
            if (hint == null) hint = pictureGo.AddComponent<PictureHintCover>();

            var soHint = new SerializedObject(hint);
            soHint.FindProperty("pictureFall").objectReferenceValue = pf;
            soHint.FindProperty("hintCoverRoot").objectReferenceValue = hintCoverRoot;
            soHint.FindProperty("hintMessage").stringValue = PictureHintCover.DefaultHint;
            soHint.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var found = Object.FindFirstObjectByType<Camera>();
                cam = found;
            }
            if (cam == null) return;

            cam.transform.position = new Vector3(0.2f, 1.25f, 0.6f);
            cam.transform.LookAt(new Vector3(0.35f, 0.95f, 3.3f));
            cam.fieldOfView = 60f;
        }

        // ---------- Helfer ----------

        private static GameObject MakeChildPrimitive(PrimitiveType type, string name, Transform parent)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            var col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col); // nur Root-Collider sollen treffen
            go.transform.SetParent(parent, false);
            return go;
        }

        private static Texture2D LoadTex(string path)
        {
            var t = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (t == null) Debug.LogWarning($"[Raetsel3Builder] Textur nicht gefunden: {path}");
            return t;
        }

        private static Material MakeMaterial(bool unlit, Texture tex, Color color)
        {
            string shaderName = unlit
                ? "Universal Render Pipeline/Unlit"
                : "Universal Render Pipeline/Lit";
            var shader = Shader.Find(shaderName) ?? Shader.Find("Unlit/Texture") ?? Shader.Find("Standard");
            var m = new Material(shader);

            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            m.color = color;

            if (tex != null)
            {
                if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", tex);
                m.mainTexture = tex;
            }
            return m;
        }
    }
}
