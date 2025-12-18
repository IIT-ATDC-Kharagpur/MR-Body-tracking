using System.Collections;
using Meta.XR;
using Meta.XR.Samples;
using UnityEngine;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    [MetaCodeSample("PassthroughCameraApiSamples-MultiObjectDetection")]
    public class DetectionManager : MonoBehaviour
    {
        [Header("Passthrough Camera")]
        [SerializeField] private PassthroughCameraAccess m_cameraAccess;

        [Header("Sentis")]
        [SerializeField] private SentisInferenceRunManager m_runInference;

        // IMPORTANT: start unpaused (this was breaking builds)
        private bool m_isPaused = false;
        private bool m_started = false;

        private IEnumerator Start()
        {
            Debug.Log("[DetectionManager] Waiting for Sentis model...");

            while (!m_runInference.IsModelLoaded)
                yield return null;

            Debug.Log("[DetectionManager] Sentis ready");
        }

        private void Update()
        {
            if (m_cameraAccess == null || m_runInference == null)
                return;

            if (m_isPaused)
                return;

            if (!m_started)
            {
                if (m_cameraAccess.IsPlaying)
                {
                    m_started = true;
                    Debug.Log("[DetectionManager] Passthrough started");
                }
                return;
            }

            if (!m_runInference.IsRunning())
            {
                m_runInference.RunInference(m_cameraAccess);
            }
        }

        // Keep method for compatibility, but not required anymore
        public void OnPause(bool pause)
        {
            m_isPaused = pause;
        }
    }
}
