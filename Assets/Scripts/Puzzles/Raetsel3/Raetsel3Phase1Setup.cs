using EscapeRoom.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeRoom.Puzzles.Raetsel3
{
    /// <summary>
    /// Phase 1: nur Wand mit Gemälde sichtbar (schrittweiser Aufbau von Rätsel 3).
    /// </summary>
    public static class Raetsel3Phase1Setup
    {
        private const string WallName = "Wall_1a";
        private const string PaintingName = "Painting_1b";
        private const string PaintingTextureResource = "Raetsel3/painting";

        public static void Apply(Transform contentRoot = null)
        {
            var wall = FindObject(WallName, contentRoot);
            var painting = FindObject(PaintingName, contentRoot);

            HideExtraObjects(contentRoot, wall, painting);

            if (wall == null || painting == null)
            {
                Debug.LogWarning("[Rätsel3] Phase 1: Wall_1a oder Painting_1b nicht gefunden.");
                return;
            }

            AttachPaintingToWall(wall.transform, painting.transform);
            ApplyPaintingTexture(painting);
            DisablePictureInteractionForNow(painting);
        }

        public static bool IsPhase1Root(GameObject root)
        {
            if (root == null)
                return false;

            return root.name == WallName || root.name == PaintingName;
        }

        private static GameObject FindObject(string objectName, Transform contentRoot)
        {
            if (contentRoot != null)
            {
                foreach (Transform child in contentRoot)
                {
                    if (child.name == objectName)
                        return child.gameObject;
                }
            }

            return GameObject.Find(objectName);
        }

        private static void HideExtraObjects(Transform contentRoot, GameObject wall, GameObject painting)
        {
            if (contentRoot != null)
            {
                foreach (Transform child in contentRoot)
                {
                    if (child.gameObject == wall || child.gameObject == painting)
                        continue;

                    child.gameObject.SetActive(false);
                }

                return;
            }

            var scene = SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root == wall || root == painting)
                    continue;

                if (root.name is "Directional Light" or "XR Origin" or "AR Session" or "ARPlacedContent")
                    continue;

                if (root.GetComponent<ARGameBootstrapMarker>() != null)
                    continue;

                root.SetActive(false);
            }
        }

        private static void AttachPaintingToWall(Transform wall, Transform painting)
        {
            if (painting.parent == wall)
                return;

            var worldPosition = painting.position;
            var worldRotation = painting.rotation;
            painting.SetParent(wall, true);
            
            // Maintain world position when parenting to ensure proper placement
            painting.SetPositionAndRotation(worldPosition, worldRotation);
            
            var paintingBounds = GetObjectBounds(painting);
            Debug.Log($"[Rätsel3] Gemälde an Wand befestigt: " +
                      $"Position={painting.position}, Größe={paintingBounds.size.x:F2}m x {paintingBounds.size.y:F2}m");
        }
        
        private static Bounds GetObjectBounds(Transform obj)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                return new Bounds(obj.position, Vector3.one);

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        private static void ApplyPaintingTexture(GameObject painting)
        {
            var texture = Resources.Load<Texture2D>(PaintingTextureResource);
            if (texture == null)
            {
                Debug.LogWarning($"[Rätsel3] Textur nicht gefunden: Resources/{PaintingTextureResource}");
                return;
            }

            foreach (var renderer in painting.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.name.Contains("Frame") || renderer.name.Contains("Rahmen"))
                    continue;

                var material = renderer.material;
                if (material.HasProperty("_BaseMap"))
                    material.SetTexture("_BaseMap", texture);
                else if (material.HasProperty("_MainTex"))
                    material.SetTexture("_MainTex", texture);
            }
        }

        private static void DisablePictureInteractionForNow(GameObject painting)
        {
            var hintCover = painting.GetComponentInChildren<PictureHintCover>(true);
            if (hintCover != null)
                hintCover.gameObject.SetActive(false);

            var hintRoot = painting.transform.Find("HintBookCover");
            if (hintRoot != null)
                hintRoot.gameObject.SetActive(false);

            var fall = painting.GetComponent<PictureFall>();
            if (fall != null)
                fall.ConfigureForHudTap();
        }
    }
}
