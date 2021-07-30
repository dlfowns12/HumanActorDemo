using System;
using System.Collections;
using System.IO;
using UnityEngine;
using NatSuite.Recorders.Clocks;

using ThreadPriority = System.Threading.ThreadPriority;

namespace DVCRecorder
{
    //[RequireComponent(typeof(Camera)), DisallowMultipleComponent]
    public class Recorder : MonoBehaviour
    {
        internal enum RecordingState
        {
            OnHold = 0,
            Recording = 1,
        }


        // 录制状态
        internal RecordingState CurrentState;


        /// <summary>
        /// 每秒采样次数
        /// </summary>
        public int captureFrameRate = 30;

        /// <summary>
        /// 最帧数量
        /// </summary>
        public int maxCapturedFrames = 1000;

        /// <summary>
        /// 每秒播放帧数
        /// </summary>
        public int playbackFrameRate = 25;

        /// <summary>
        /// 生成的GIF是否循环播放
        /// </summary>
        public bool loopPlayback = true;

        /// <summary>
        /// 主摄像机
        /// </summary>
        //private Camera capturedCamera;

        private int _cameraWidth;

        private int _cameraHeight;


        /// <summary>
        ///  计时器
        /// </summary>
        private float _elapsedTime = 0;

        /// <summary>
        /// 生成的GIF尺寸与原图的尺寸比例
        /// </summary>
        private static double RESIZE_RATIO = 0.5;

        /// <summary>
        /// 生成的GIF ID
        /// </summary>
        private string _captureId;
        /// <summary>
        /// GIF保存路径
        /// </summary>
        private string _resultFilePath;

        /// <summary>
        /// 生成的GIF保存文件夹
        /// </summary>
        private const string GeneratedContentFolderName = "Output";

        /// <summary>
        /// 录制协程
        /// </summary>
        private Coroutine _recordCoroutine;

        private int nWidth;
        private int nHeight;

        public string m_RecorderName;
        public EncoderBase m_Encoder = null;

        private RealtimeClock m_RealtimeClock;

        public virtual void init(int repeat, int quality, int width, int height)
        { 
            
        }

        public void setCameraPixel(int width, int height)
        {
            _cameraWidth = width;
            _cameraHeight = height;
        }

        /// <summary>
        /// 开始录制
        /// </summary>
        public void StartRecord(string strfilePath,int width,int height,int fps)
        {
            if (_captureId != null)
            {
              
                CleanUp();
            }
            InitSession(strfilePath);

            nWidth = width;
            nHeight = height;
            m_RealtimeClock = new RealtimeClock();
            Debug.Log("Recorder StartRecord ...");
            //float rato = (float)fps / captureFrameRate;

            //captureFrameRate =(int)( captureFrameRate / rato);


            playbackFrameRate = fps;

  

            _recordCoroutine = StartCoroutine(RunRecord());
        }

        /// <summary>
        /// 停止录制
        /// </summary>
        public void StopRecord()
        {
            StopCoroutine(_recordCoroutine);
        }

        /// <summary>
        /// 生成GIF
        /// </summary>
        /// <param name="result"></param>
        public void GenerateOutFile(Action<byte[], string> result)
        {
            if (StoreWorker.Instance.StoredFrames.Count() > 0)
            {
                var generator = new GeneratorWorker(this, loopPlayback, playbackFrameRate, ThreadPriority.Normal, StoreWorker.Instance.StoredFrames,
                     _resultFilePath,
                    () =>
                    {
                        MainThreadExecutor.Queue(() =>
                        {
                            result(File.ReadAllBytes(_resultFilePath), _resultFilePath);

                        });
                    });

                generator.Start();
            }
            else
            {
                Debug.LogError("生成 " + m_RecorderName + " 失败");
                result(new byte[0], _resultFilePath);
            }
        }

        private void Awake()
        {
            /*
            if (capturedCamera == null)
            {
                GameObject camObj = GameObject.Find("Main Camera");
                capturedCamera = camObj.GetComponent<Camera>();
            }

            if (capturedCamera == null)
            {
                Debug.LogError("Camera is not set");
                return;
            }
            */
        }


        private void OnDestroy()
        {
            StoreWorker.Instance.Clear();
        }


        private static string GetResultDirectory()
        {
            string resultDirPath;
#if UNITY_EDITOR
            resultDirPath = Application.dataPath;
#else
            resultDirPath = Application.persistentDataPath; 
#endif
            resultDirPath += Path.DirectorySeparatorChar + GeneratedContentFolderName;

            if (!Directory.Exists(resultDirPath))
            {
                Directory.CreateDirectory(resultDirPath);
            }
            return resultDirPath;
        }

        private void InitSession(string strFilePath)
        {
            //_captureId = Guid.NewGuid().ToString();
            //var fileName = string.Format("result-{0}.gif", _captureId);
            //_resultFilePath = GetResultDirectory() + Path.DirectorySeparatorChar + fileName;

            _resultFilePath = strFilePath + "." + m_RecorderName;
            Debug.Log("InitSession _resultFilePath:" + _resultFilePath);
            StoreWorker.Instance.Start(maxCapturedFrames);
        }

        private void CleanUp()
        {
            if (File.Exists(_resultFilePath))
            {
                File.Delete(_resultFilePath);
            }
        }

        int tcount = 0;
        /// <summary>
        /// 运行录制
        /// </summary>
        /// <returns></returns>
        IEnumerator RunRecord()
        {
           
            while (true)
            {
               
                yield return new WaitForEndOfFrame();
                _elapsedTime += Time.unscaledDeltaTime;
               
                if (_elapsedTime >= 1.0f / captureFrameRate)
                {
                    _elapsedTime = 0;
                    long timestamp = m_RealtimeClock.timestamp;
                    RenderTexture rt = GetTemporaryRenderTexture();
                    Graphics.Blit(null, rt);


                    RESIZE_RATIO =  (float)nHeight / _cameraHeight;
                   // Debug.Log("RunRecord RESIZE_RATIO:" + RESIZE_RATIO + " rt.w:" + rt.width + " rt.h:" + rt.height);
                    tcount = tcount + 1;
                   // Debug.Log("RunRecord tcount:" + tcount);

                    StoreWorker.Instance.StoreFrame(rt, RESIZE_RATIO, timestamp / 1000000);
                    yield return null;
                    RenderTexture.ReleaseTemporary(rt);

             
                }
            }
        }

        private RenderTexture GetTemporaryRenderTexture()
        {
            int newWidth = (int)(nWidth * RESIZE_RATIO);

             var rt = RenderTexture.GetTemporary(_cameraWidth, _cameraHeight, 0, RenderTextureFormat.ARGB32); 
           // var rt = RenderTexture.GetTemporary(Convert.ToInt32(_cameraWidth * RESIZE_RATIO), _cameraHeight, 0, RenderTextureFormat.ARGB32);

            //nWidth =(int) (nHeight * capturedCamera.pixelWidth / capturedCamera.pixelHeight);
            //var rt = RenderTexture.GetTemporary(nWidth,nHeight, 0, RenderTextureFormat.ARGB32);
            rt.wrapMode = TextureWrapMode.Clamp;
            rt.filterMode = FilterMode.Bilinear;
            rt.anisoLevel = 0;
            return rt;
        }

    }

}


