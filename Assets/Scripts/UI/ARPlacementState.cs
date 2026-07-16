using UnityEngine;

namespace EscapeRoom.UI
{
    /// <summary>
    /// Speichert die vom Spieler gewählte AR-Platzierung zwischen Szenen.
    /// </summary>
    public static class ARPlacementState
    {
        public static bool HasPlacement { get; private set; }
        public static Pose PlacementPose { get; private set; }
        public static Transform AnchorTransform { get; private set; }
        public static Vector2 PlaneSize { get; private set; }

        public static void SetPlacement(Pose pose, Transform anchor, Vector2 planeSize)
        {
            HasPlacement = true;
            PlacementPose = pose;
            AnchorTransform = anchor;
            PlaneSize = planeSize;
        }

        public static void Clear()
        {
            HasPlacement = false;
            AnchorTransform = null;
            PlaneSize = Vector2.zero;
        }
    }
}
