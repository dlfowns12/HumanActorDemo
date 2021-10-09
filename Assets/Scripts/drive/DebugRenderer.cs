
using System.Collections.Generic;
using UnityEngine;



    public class DebugRenderer : MonoBehaviour
    {
        public class SubRenderer : MonoBehaviour
        {
            public LineRenderer mLineRenderer;
            public List<int> mIndices;

            public void Initialize()
            {
                mLineRenderer = gameObject.AddComponent<LineRenderer>();
                mLineRenderer.widthMultiplier = 0.01f;
                mLineRenderer.sharedMaterial = new Material(Shader.Find("Diffuse"));
                mLineRenderer.sharedMaterial.color = Color.white;
                mIndices = new List<int>();
            }
        }
        public List<SubRenderer> mSubRenderers = new List<SubRenderer>();
        protected int mActiveSubRendererIndex = -1;

        public void Start()
        {
            var subRenderers = gameObject.GetComponentsInChildren<SubRenderer>();
            mSubRenderers = new List<SubRenderer>();
            foreach (var renderer in subRenderers)
            {
                mSubRenderers.Add(renderer);
            }
        }
        public void Initialize(int subRendererCount)
        {
            mSubRenderers = new List<SubRenderer>();
            for (int i = 0; i < subRendererCount; i++)
            {
                AddSubRenderer(string.Format("DebugRenderer{0}", i));
            }
            ActiveSubRenderer(0);
        }
        public void AddSubRenderer(string name)
        {
            //var subObject = JointsDriveHelper.GetObject(gameObject, name);
            //var subRenderer = subObject.AddComponent<SubRenderer>();
            //subRenderer.Initialize();
            //mSubRenderers.Add(subRenderer);

            ActiveSubRenderer(mSubRenderers.Count - 1);
        }
        public void ActiveSubRenderer(int index, string name = null)
        {
            mActiveSubRendererIndex = index;
            if (name != null)
            {
                mSubRenderers[mActiveSubRendererIndex].gameObject.name = name;
            }
        }
        public LineRenderer GetLineRenderer()
        {
            return mSubRenderers[mActiveSubRendererIndex].mLineRenderer;
        }
        public void AddIndices(int startIndex, int count = 1)
        {
            var renderer = mSubRenderers[mActiveSubRendererIndex];
            for (int i = 0; i < count; i++)
            {
                renderer.mIndices.Add(startIndex + i);
            }
        }
        public void UpdateData(Vector3[] joints, Vector3 offset, Vector3 euler, Vector3 scale)
        {
            var rotate = Quaternion.Euler(euler);
            foreach (var renderer in mSubRenderers)
            {
                var indices = renderer.mIndices;
                if (indices.Count == 0)
                {
                    renderer.mLineRenderer.positionCount = 0;
                    continue;
                }
                var selectJoints = new Vector3[indices.Count];
                for (int i = 0; i < indices.Count; i++)
                {
                    int index = indices[i];
                    selectJoints[i] = joints[index];
                    selectJoints[i] = rotate * selectJoints[i];
                    selectJoints[i].Scale(scale);
                    selectJoints[i] = selectJoints[i] + offset;
                }
                renderer.mLineRenderer.positionCount = indices.Count;
                renderer.mLineRenderer.SetPositions(selectJoints);
            }
        }
}