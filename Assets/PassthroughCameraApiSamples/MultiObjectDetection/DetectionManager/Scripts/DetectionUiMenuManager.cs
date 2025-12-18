using UnityEngine;
using UnityEngine.UI;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    public class DetectionUiMenuManager : MonoBehaviour
    {
        [SerializeField] private GameObject m_loadingPanel;
        [SerializeField] private Text m_labelInformation;

        private float autoHideDelay = 2f;

        private void Start()
        {
            if (m_loadingPanel != null)
                m_loadingPanel.SetActive(true);

            if (m_labelInformation != null)
                m_labelInformation.text = "Starting pose detection...";

            Invoke(nameof(HideLoading), autoHideDelay);
        }

        private void HideLoading()
        {
            if (m_loadingPanel != null)
                m_loadingPanel.SetActive(false);

            if (m_labelInformation != null)
                m_labelInformation.text = "Pose detection running";
        }
    }
}
