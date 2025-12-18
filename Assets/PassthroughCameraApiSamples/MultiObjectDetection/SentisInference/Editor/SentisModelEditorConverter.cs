// Copyright (c) Meta Platforms, Inc. and affiliates.
/*using Meta.XR.Samples;
using Unity.InferenceEngine;
using UnityEditor;
using UnityEngine;

namespace PassthroughCameraSamples.MultiObjectDetection.Editor
{
    [MetaCodeSample("PassthroughCameraApiSamples-MultiObjectDetection")]
    [CustomEditor(typeof(SentisInferenceRunManager))]
    public class SentisModelEditorConverter : UnityEditor.Editor
    {
        private const string FILEPATH =
            "Assets/PassthroughCameraApiSamples/MultiObjectDetection/SentisInference/Model/yolov8n-pose.sentis";

        private SentisInferenceRunManager m_targetClass;

        private void OnEnable()
        {
            m_targetClass = (SentisInferenceRunManager)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "YOLOv8 POSE MODE\n" +
                "This converts a YOLOv8 pose ONNX model to Sentis.\n" +
                "The pose output tensor is preserved exactly (no NMS, no box filtering).",
                MessageType.Info
            );

            if (GUILayout.Button("Convert YOLOv8 Pose ONNX → Sentis"))
            {
                ConvertPoseModel();
            }
        }

        private void ConvertPoseModel()
        {
            if (m_targetClass == null || m_targetClass.OnnxModel == null)
            {
                Debug.LogError("ONNX pose model is not assigned in SentisInferenceRunManager.");
                return;
            }

            // Load ONNX model
            var model = ModelLoader.Load(m_targetClass.OnnxModel);

            // IMPORTANT:
            // For YOLO POSE we do NOT modify the graph.
            // We forward the model exactly as-is.
            var graph = new FunctionalGraph();
            var input = graph.AddInput(model, 0);

            // Forward the original model
            var outputs = Functional.Forward(model, input);

            // Compile ONLY the first output tensor (pose tensor)
            var poseModel = graph.Compile(outputs[0]);

            // Save as Sentis
            ModelWriter.Save(FILEPATH, poseModel);
            AssetDatabase.Refresh();

            Debug.Log(
                "YOLOv8 Pose Sentis model generated successfully.\n" +
                "Expected output shape: (1, 56, 8400)\n" +
                "Saved at: " + FILEPATH
            );
        }
    }
}*/
