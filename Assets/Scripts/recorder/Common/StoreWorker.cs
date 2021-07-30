using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DVCRecorder
{
    /// <summary>
    /// 帧队列缓存
    /// </summary>
    public sealed class StoreWorker
    {
        // 帧队列
        public FixedSizedQueue<DVCFrame> StoredFrames { get; private set; }



        public Material mirror = null;//用于水平镜像翻转的shader

        internal static StoreWorker Instance
        {
            get { return _instance ?? (_instance = new StoreWorker()); }
        }

        private static StoreWorker _instance;

        internal void Clear()
        {
            if (StoredFrames != null)
            {
                StoredFrames.Clear();
                StoredFrames = null;
            }
        }

        internal void Start(int maxCapturedFrames)
        {
            Clear();
            StoredFrames = new FixedSizedQueue<DVCFrame>(maxCapturedFrames);


            //Debug.Log("start to load shader");
            if (mirror == null )
               mirror = new Material(Shader.Find("Unlit/mirror"));


            //if (mirror == null)
            //    Debug.Log("not find mirror");

        }

        /// <summary>
        /// 缓存帧数据到队列中
        /// </summary>
        /// <param name="renderTexture"></param>
        /// <param name="resizeRatio"></param>
        internal void StoreFrame(RenderTexture renderTexture, double resizeRatio, long timestamp)
        {
            var newWidth = Convert.ToInt32(renderTexture.width * resizeRatio);
            var newHeight = Convert.ToInt32(renderTexture.height * resizeRatio);
       
            renderTexture.filterMode = FilterMode.Bilinear;

            var resizedRenderTexture = RenderTexture.GetTemporary(newWidth, newHeight);
            resizedRenderTexture.filterMode = FilterMode.Bilinear;

            RenderTexture.active = resizedRenderTexture;
            //Graphics.Blit(renderTexture, resizedRenderTexture);

            mirror.SetInt("_MirrorV",1);//是否竖直镜像翻转
            Graphics.Blit(renderTexture, resizedRenderTexture, mirror);


            // 转化为Texture2D
            var resizedTexture2D =
                new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear,
                    anisoLevel = 0       //这个值主要针对于地面，其他地方可低， 值越大 显卡开销越大；
                };

            resizedTexture2D.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
            resizedTexture2D.Apply();
            RenderTexture.active = null;


            //#if UNITY_ANDROID
            //            Texture2D flipTexture = new Texture2D(newWidth, newHeight);
            //            for (int i = 0; i < newHeight; i++)
            //                flipTexture.SetPixels(0, i, newWidth, 1, resizedTexture2D.GetPixels(0, newHeight - i - 1, newWidth, 1));
            //#endif




            var frame = new DVCFrame
            {
                Width = resizedTexture2D.width,
                Height = resizedTexture2D.height,
                //#if UNITY_ANDROID
                //                Data = flipTexture.GetPixels32(),
                //#else
                //                Data = resizedTexture2D.GetPixels32(),
                //#endif
                Data = resizedTexture2D.GetPixels32(),

                TimeStamp = timestamp
            };

            resizedRenderTexture.Release();
            Object.Destroy(resizedTexture2D);
            
            StoredFrames.Enqueue(frame);
        }
    }
}