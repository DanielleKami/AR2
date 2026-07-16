using EscapeRoom.Puzzles.Raetsel3;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils;

namespace EscapeRoom.UI
{
    /// <summary>
    /// Platziert Rätsel-3-Inhalt (Ziegelwand) auf der gewählten AR-Fläche.
    /// </summary>
    public static class ARGameBootstrap
    {
        private const string WallName = "Wall_1a";
        private const string PaintingName = "Painting_1b";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AfterSceneLoad()
        {
            if (!ARPlacementState.HasPlacement)
                return;

            var scene = SceneManager.GetActiveScene();
            if (scene.name != "raetsel_3")
                return;

            ApplyPlacement(scene);
        }

        private static void ApplyPlacement(Scene scene)
        {
            var xrOrigin = Object.FindAnyObjectByType<XROrigin>();
            if (xrOrigin != null)
                xrOrigin.gameObject.SetActive(true);

            DisableSceneCamera(scene);

            var anchor = ARPlacementState.AnchorTransform;
            var wallPose = ARPlacementState.PlacementPose;
            var planeSize = ARPlacementState.PlaneSize;

            var contentRoot = new GameObject("ARPlacedContent");
            if (anchor != null)
            {
                contentRoot.transform.SetParent(anchor, false);
                contentRoot.transform.localPosition = Vector3.zero;
                contentRoot.transform.localRotation = Quaternion.identity;
            }
            else
            {
                contentRoot.transform.SetPositionAndRotation(wallPose.position, wallPose.rotation);
            }

            var wall = FindSceneObject(scene, WallName);
            var painting = FindSceneObject(scene, PaintingName);

            if (wall != null)
            {
                wall.transform.SetParent(contentRoot.transform, true);
                Raetsel3WallPlacer.PlaceWall(wall.transform, wallPose, planeSize);
            }
            else
            {
                Raetsel3WallPlacer.SpawnWall(contentRoot.transform, wallPose, planeSize);
            }

            if (painting != null)
                painting.transform.SetParent(contentRoot.transform, true);

            Raetsel3Phase1Setup.Apply(contentRoot.transform);
        }

        private static GameObject FindSceneObject(Scene scene, string objectName)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == objectName)
                    return root;

                var nested = FindInChildren(root.transform, objectName);
                if (nested != null)
                    return nested;
            }

            return GameObject.Find(objectName);
        }

        private static GameObject FindInChildren(Transform parent, string objectName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == objectName)
                    return child.gameObject;

                var nested = FindInChildren(child, objectName);
                if (nested != null)
                    return nested;
            }

            return null;
        }

        private static void DisableSceneCamera(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var cam = root.GetComponentInChildren<Camera>(true);
                if (cam == null || cam.GetComponent<ARCameraManager>() != null)
                    continue;

                cam.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Marker für Objekte, die nicht unter ARPlacedContent hängen sollen.
    /// </summary>
    public class ARGameBootstrapMarker : MonoBehaviour
    {
    }
}
