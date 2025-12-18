// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections;
using System.IO;
using Meta.XR;
using Unity.InferenceEngine;
using UnityEngine;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    public class SentisInferenceRunManager : MonoBehaviour
    {
        [Header("Sentis Config")]
        [SerializeField] private Vector2Int m_inputSize = new(640, 640);
        [SerializeField] private BackendType m_backend = BackendType.CPU;
        [SerializeField] private int m_layersPerFrame = 25;

        [Header("StreamingAssets Model")]
        [Tooltip("Filename only, must exist in Assets/StreamingAssets")]
        [SerializeField] private string m_sentisFileName = "yolov8n-pose.sentis";

        [Header("UI")]
        [SerializeField] private SentisInferenceUiManager m_uiInference;

        public bool IsModelLoaded { get; private set; }

        private Worker m_worker;
        private Tensor<float> m_input;
        private Tensor<float> m_output;
        private IEnumerator m_schedule;

        private bool m_running = false;
        private bool m_waiting = false;

        private void Start()
        {
            LoadModelFromStreamingAssets();
        }

        private void Update()
        {
            InferenceUpdate();
        }

        private void OnDestroy()
        {
            m_input?.Dispose();
            m_output?.Dispose();
            m_worker?.Dispose();
        }

        // 🔹 LOAD MODEL FROM STREAMINGASSETS
        private void LoadModelFromStreamingAssets()
        {
            string modelPath = Path.Combine(
                Application.streamingAssetsPath,
                m_sentisFileName
            );

            if (!File.Exists(modelPath))
            {
                Debug.LogError($"Sentis model not found at:\n{modelPath}");
                return;
            }

            Debug.Log($"Loading Sentis model from:\n{modelPath}");

            var model = ModelLoader.Load(modelPath);
            m_worker = new Worker(model, m_backend);

            // Warm-up
            m_input = new Tensor<float>(
                new TensorShape(1, 3, m_inputSize.x, m_inputSize.y)
            );
            m_worker.Schedule(m_input);

            IsModelLoaded = true;
            Debug.Log("Sentis model loaded successfully from StreamingAssets");
        }

        public bool IsRunning()
        {
            return m_running;
        }

        public void RunInference(PassthroughCameraAccess cameraAccess)
        {
            if (m_running || !IsModelLoaded)
                return;

            Texture cameraTexture = cameraAccess.GetTexture();
            if (cameraTexture == null)
                return;

            m_input?.Dispose();
            m_input = new Tensor<float>(
                new TensorShape(1, 3, m_inputSize.x, m_inputSize.y)
            );

            var tt = new TextureTransform().SetDimensions(cameraTexture.width, cameraTexture.height, 3);

            TextureConverter.ToTensor(cameraTexture, m_input, tt);

            m_schedule = m_worker.ScheduleIterable(m_input);
            m_running = true;
        }

        private void InferenceUpdate()
        {
            if (!m_running) return;

            try
            {
                int it = 0;
                while (m_schedule.MoveNext())
                {
                    if (++it % m_layersPerFrame == 0)
                        return;
                }

                if (!m_waiting)
                {
                    var pull = m_worker.PeekOutput(0) as Tensor<float>;
                    pull.ReadbackRequest();
                    m_waiting = true;
                }
                else
                {
                    var pull = m_worker.PeekOutput(0) as Tensor<float>;
                    if (pull.IsReadbackRequestDone())
                    {
                        m_output?.Dispose();
                        m_output = pull.ReadbackAndClone();

                        // 🔍 Optional verification log
                        Debug.Log($"Sentis output shape: {m_output.shape}");

                        m_uiInference.DrawPose2D(
                            m_output,
                            m_inputSize.x,
                            m_inputSize.y
                        );

                        m_running = false;
                        m_waiting = false;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                m_running = false;
                m_waiting = false;
            }
        }
    }
}
