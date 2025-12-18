// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Collections.Generic;
using Unity.InferenceEngine;
using UnityEngine;
using UnityEngine.UI;

namespace PassthroughCameraSamples.MultiObjectDetection
{
    public class SentisInferenceUiManager : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private RawImage m_displayImage;
        [SerializeField] private GameObject m_jointPrefab;
        [SerializeField] private GameObject m_linePrefab;

        [Header("Thresholds")]
        [SerializeField] private float m_minPersonScore = 0.4f;
        [SerializeField] private float m_minJointScore = 0.5f;

        private readonly List<GameObject> m_jointPool = new();
        private readonly List<GameObject> m_linePool = new();

        // COCO-17 skeleton
        private readonly int[,] m_skeleton =
        {
            {0,1},{1,3},{0,2},{2,4},
            {5,6},
            {5,7},{7,9},
            {6,8},{8,10},
            {5,11},{6,12},
            {11,12},
            {11,13},{13,15},
            {12,14},{14,16}
        };

        public void DrawPose2D(Tensor<float> output, float imageW, float imageH)
        {
            ClearAll();

            int detections = output.shape[2];
            float uiW = m_displayImage.rectTransform.rect.width;
            float uiH = m_displayImage.rectTransform.rect.height;

            for (int d = 0; d < detections; d++)
            {
                float personScore = output[0, 4, d];
                if (personScore < m_minPersonScore)
                    continue;

                Vector2?[] joints = new Vector2?[17];

                for (int k = 0; k < 17; k++)
                {
                    float x = output[0, 5 + k * 3 + 0, d];
                    float y = output[0, 5 + k * 3 + 1, d];
                    float c = output[0, 5 + k * 3 + 2, d];

                    if (c < m_minJointScore) continue;

                    float nx = x / imageW;
                    float ny = y / imageH;

                    float px = (nx - 0.5f) * uiW;
                    float py = (0.5f - ny) * uiH;

                    joints[k] = new Vector2(px, py);
                    DrawJoint(joints[k].Value);
                }

                DrawSkeleton(joints);
            }
        }

        private void DrawJoint(Vector2 pos)
        {
            var joint = GetFromPool(m_jointPool, m_jointPrefab);
            joint.transform.SetParent(m_displayImage.transform, false);
            joint.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
        }

        private void DrawSkeleton(Vector2?[] joints)
        {
            for (int i = 0; i < m_skeleton.GetLength(0); i++)
            {
                int a = m_skeleton[i, 0];
                int b = m_skeleton[i, 1];

                if (!joints[a].HasValue || !joints[b].HasValue)
                    continue;

                var line = GetFromPool(m_linePool, m_linePrefab);
                line.transform.SetParent(m_displayImage.transform, false);

                var rt = line.GetComponent<RectTransform>();
                Vector2 p1 = joints[a].Value;
                Vector2 p2 = joints[b].Value;

                Vector2 dir = p2 - p1;
                float len = dir.magnitude;

                rt.sizeDelta = new Vector2(len, 4f);
                rt.localPosition = (p1 + p2) * 0.5f;
                rt.localRotation = Quaternion.Euler(
                    0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg
                );
            }
        }

        private GameObject GetFromPool(List<GameObject> pool, GameObject prefab)
        {
            foreach (var go in pool)
            {
                if (!go.activeSelf)
                {
                    go.SetActive(true);
                    return go;
                }
            }

            var obj = Instantiate(prefab);
            pool.Add(obj);
            return obj;
        }

        private void ClearAll()
        {
            foreach (var j in m_jointPool) j.SetActive(false);
            foreach (var l in m_linePool) l.SetActive(false);
        }
    }
}
