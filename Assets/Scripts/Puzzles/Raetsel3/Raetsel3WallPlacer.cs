using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace EscapeRoom.Puzzles.Raetsel3
{
    /// <summary>
    /// Platziert Wall_1a (Ziegelwand / Bricks_diffuse) auf einer erkannten AR-Fläche.
    /// </summary>
    public static class Raetsel3WallPlacer
    {
        private const string WallResourcePath = "Raetsel3/Wall_1a";
        private const float MinWallWidthMeters = 1.2f;
        private const float MaxWallWidthMeters = 3.6f;
        private const float TargetWallHeightMeters = 2.35f;
        private const float PlaneWidthUsage = 0.85f;

        public static GameObject SpawnWall(Transform parent, Pose wallPose, Vector2 planeSize)
        {
            var prefab = Resources.Load<GameObject>(WallResourcePath);
            if (prefab == null)
            {
                Debug.LogWarning($"[Rätsel3] Prefab nicht gefunden: Resources/{WallResourcePath}");
                return null;
            }

            var wall = Object.Instantiate(prefab, parent);
            wall.name = "Wall_1a";
            ApplyBrickMaterial(wall);
            PlaceWall(wall.transform, wallPose, planeSize);
            return wall;
        }

        private static void ApplyBrickMaterial(GameObject wall)
        {
            var material = Resources.Load<Material>("Raetsel3/Bricks_01_mat");
            if (material == null)
                return;

            foreach (var renderer in wall.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterial = material;
        }

        public static Pose ComputeWallPose(Pose surfacePose, Vector2 planeSize, ARPlane plane)
        {
            var up = plane.normal;
            if (Vector3.Dot(up, Vector3.up) < 0f)
                up = -up;

            var cam = Camera.main;
            var toCamera = cam != null
                ? cam.transform.position - surfacePose.position
                : Vector3.forward;
            var forward = Vector3.ProjectOnPlane(toCamera, up);
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.ProjectOnPlane(plane.transform.forward, up);
            forward.Normalize();

            return new Pose(surfacePose.position, Quaternion.LookRotation(forward, up));
        }

        public static void PlaceWall(Transform wall, Pose wallPose, Vector2 planeSize)
        {
            if (wall == null)
                return;

            ApplyBrickMaterial(wall.gameObject);

            wall.SetPositionAndRotation(wallPose.position, wallPose.rotation);
            wall.localScale = Vector3.one;

            var bounds = GetRendererBounds(wall);
            if (bounds.size.sqrMagnitude < 0.0001f)
                return;

            var targetWidth = Mathf.Clamp(
                Mathf.Max(planeSize.x, planeSize.y) * PlaneWidthUsage,
                MinWallWidthMeters,
                MaxWallWidthMeters);

            var scaleX = targetWidth / Mathf.Max(bounds.size.x, 0.01f);
            var scaleY = TargetWallHeightMeters / Mathf.Max(bounds.size.y, 0.01f);
            var scaleZ = scaleX * 0.4f;

            wall.localScale = new Vector3(scaleX, scaleY, scaleZ);
            AlignBottomToSurface(wall, wallPose.position, wallPose.up);
            AlignHorizontalCenter(wall, wallPose.position, wallPose.up, wallPose.forward);
        }

        private static void AlignBottomToSurface(Transform wall, Vector3 surfacePoint, Vector3 surfaceUp)
        {
            var bounds = GetRendererBounds(wall);
            var bottomCenter = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            wall.position += surfacePoint - bottomCenter;

            var remaining = Vector3.Dot(wall.position - surfacePoint, surfaceUp);
            if (Mathf.Abs(remaining) > 0.001f)
                wall.position -= surfaceUp * remaining;
        }

        private static void AlignHorizontalCenter(
            Transform wall,
            Vector3 targetCenter,
            Vector3 surfaceUp,
            Vector3 forward)
        {
            forward = Vector3.ProjectOnPlane(forward, surfaceUp);
            if (forward.sqrMagnitude < 0.0001f)
                return;

            forward.Normalize();

            var bounds = GetRendererBounds(wall);
            var delta = targetCenter - bounds.center;
            delta -= Vector3.Dot(delta, surfaceUp) * surfaceUp;

            if (delta.sqrMagnitude > 0.0001f)
                wall.position += delta;
        }

        private static Bounds GetRendererBounds(Transform root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                return new Bounds(root.position, Vector3.one);

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }
    }
}
