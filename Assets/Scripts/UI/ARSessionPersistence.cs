using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace EscapeRoom.UI
{
    /// <summary>
    /// Hält AR Session und XR Origin beim Szenenwechsel am Leben.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class ARSessionPersistence : MonoBehaviour
    {
        private static bool _initialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureBootstrap()
        {
            if (_initialized)
                return;

            var session = Object.FindAnyObjectByType<ARSession>(FindObjectsInactive.Include);
            var origin = Object.FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>(FindObjectsInactive.Include);
            if (session == null || origin == null)
                return;

            var root = session.gameObject;
            if (root.GetComponent<ARSessionPersistence>() == null)
                root.AddComponent<ARSessionPersistence>();

            Object.DontDestroyOnLoad(root);
            Object.DontDestroyOnLoad(origin.gameObject);
            _initialized = true;
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
