using System.Collections;
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Object = UnityEngine.Object;

public class DVCStreamer : MonoBehaviour
{
#if UNITY_LINUX_RTC
    private Camera mCamera = null;
    private IntPtr mStreamClient;
    private OnDataMessagePtr mMessageListener = null;
    private RenderTextureDescriptor mFrameDescriptor;
    private Texture2D mReadbackBuffer;
    private byte[] mPixelBuffer;
    float mWaitSec = 1f;
    float mAddTime;
    int mFrames = 0;
    private int mCount = 0;
    public int mWidth = 1280;
    public int mHeight = 720;
    protected System.Threading.Timer mTimer;
    private UIManager mUIManager;
    private MessageQueue mMessageQueue = new MessageQueue();

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate void OnDataMessagePtr(IntPtr data, int len);

    [Serializable]
    public class DataChannelCommand
    {
        public string command;
        public string type;
        public bool repeat;
        public int key;
        public int ascii;
    }

    private class MessageQueue : Object
    {
        private ArrayList mMessageList;
        private Mutex mMutex;

        public MessageQueue()
        {
            mMutex = new Mutex();
            mMessageList = new ArrayList();
        }

        public void QueueMessage(DataChannelCommand msg)
        {
            mMutex.WaitOne();
            mMessageList.Add(msg);
            mMutex.ReleaseMutex();

        }

        public DataChannelCommand DequeueMessage()
        {
            DataChannelCommand msg = null;
            mMutex.WaitOne();
            if (mMessageList.Count > 0)
            {
                msg = (DataChannelCommand)mMessageList[0];
                mMessageList.RemoveAt(0);
            }
            mMutex.ReleaseMutex();

            return msg;
        }

        void Destroy()
        {

        }


    }

    void Awake()
    {
        Application.targetFrameRate = -1;
        GameObject camObj = GameObject.Find("Main Camera");
        mCamera = camObj.GetComponent<Camera>();
        Debug.Log("DVCStreamer Start ...");

        DateTime dateTime = DateTime.Now;
        string strNowTime = string.Format("{0:D}{1:D}{2:D}{3:D}{4:D}{5:D}", dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
        string hostname = Environment.GetEnvironmentVariable("HOSTNAME");
        string roomID = hostname + strNowTime;

        string mediaPublicPort = Environment.GetEnvironmentVariable("MEDIAPUBLICPORT");
        string signalServer = Environment.GetEnvironmentVariable("SIGNALSERVER");

        if (signalServer == null)
        {
            Debug.Log("!!!!rtc signal server is null!!!!!");
        }



        //Camera.onPostRender += OnPostRenderCallback;
        mStreamClient = RTCPeer.DVCClient_create(roomID);
        Debug.Log("Start-----" + System.DateTime.Now + ", mStreamClient=" + mStreamClient + ", roomId=" + roomID + ", publicport=" + mediaPublicPort + ",signalserver=" + signalServer);
        if (mStreamClient != IntPtr.Zero)
        {
            RTCPeer.DVCClient_setSignalServer(mStreamClient, signalServer);
            if ((mediaPublicPort != null) && (mediaPublicPort.Length > 0))
            {
                RTCPeer.DVCClient_setRtcHostPort(mStreamClient, mediaPublicPort);
            }
            RTCPeer.DVCClient_setRTPPortRange(mStreamClient, 30000, 30000);
            //RTCPeer.DVCClient_addIceServer(mStreamClient, mTrunServerUrl, mTrunServerUser, mTrunServerPwd);
            mMessageListener += new OnDataMessagePtr(OnDataCnannel);
            RTCPeer.DVCClient_setOnDataMessage(mStreamClient, Marshal.GetFunctionPointerForDelegate(mMessageListener));
            RTCPeer.DVCClient_start(mStreamClient, 1, 0, 1);
            Debug.Log("Start+++++" + System.DateTime.Now);
        }

        mFrameDescriptor = new RenderTextureDescriptor(mWidth, mHeight, RenderTextureFormat.ARGB32, 24);
        mReadbackBuffer = new Texture2D(mWidth, mHeight, TextureFormat.RGBA32, false, false);
        mPixelBuffer = new byte[mWidth * mHeight * 4];
        // Start recording
        mFrameDescriptor.sRGB = true;

        mUIManager = GameObject.Find("Canvas").GetComponent<UIManager>();
    }

    // Start is called before the first frame update
    void Start()
    {

        
        StartCoroutine(OnFrame());
    }

    // Update is called once per frame
    void Update()
    {
        mFrames++;
        mAddTime += Time.deltaTime;
        if (mAddTime > mWaitSec)
        {
            RealFPS = mFrames / mAddTime;
            pushFPS = mCount / mAddTime;
            mFrames = 0;
            mAddTime = 0;
            mCount = 0;
        }

        DataChannelCommand jData = mMessageQueue.DequeueMessage();
        if (jData != null)
        {
            int ckey = jData.key;
            Debug.Log("DVCStreamer Update jData.command:" + jData.command);
            switch (jData.command)
            {
                case "finalize":
                    Application.Quit();
                    break;
                case "createavatar":
                    mUIManager.loadshader_click();
                    mUIManager.loadAvatar_click();
                    break;
                case "background":
                    break;
                case "animation":
                    mUIManager.anim_play();
                    break;
                case "suit":
                    break;
                case "hair":
                    break;
                default:
                    Debug.Log("unknown data channel cmd");
                    break;
            }
        }
    }

    void OnDestroy()
    {
        if (mTimer != null)
        {
            mTimer.Dispose();
        }

        //Camera.onPostRender -= OnPostRenderCallback;
        Texture2D.Destroy(mReadbackBuffer);
        mPixelBuffer = null;
        releaseRTC();

    }


    private void releaseRTC()
    {
        Debug.Log("DVCClient_free------" + mStreamClient + " @ " + System.DateTime.Now);
        if (mStreamClient != IntPtr.Zero)
        {
            IntPtr tmp = mStreamClient;
            mStreamClient = IntPtr.Zero;
            RTCPeer.DVCClient_release(out tmp);

        }
    }
    public void OnDataCnannel(IntPtr data, int len)
    {

        byte[] bArray = new byte[len];
        Marshal.Copy(data, bArray, 0, len);
        string str = Encoding.Default.GetString(bArray, 0, len);
        Debug.Log("OnDataCnannel:" + str);
        Console.WriteLine("receive message:" + str);
        //string replay = ReversalString(str);
        //byte[] byteArray = System.Text.Encoding.Default.GetBytes(replay);
        //RTCPeer.DVCClient_sendMsg(rtcclient, byteArray, byteArray.Length);
        
        string cmd_str = str.ToLower();
        AvatarThreadExecutor.Queue(() =>
        {
            if (mCamera != null)
            {
                DispathCmd(cmd_str);
            }
            else
            {
                Debug.Log("mCamera is null");
            }


        });
    }

    private void DispathCmd(string cmd)
    {
        Debug.Log("DispathCmd:" + cmd);

        var jData = JsonConvert.DeserializeObject<DataChannelCommand>(cmd);
        mMessageQueue.QueueMessage(jData);

    }
    public void Quit()
    {
        releaseRTC();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else

        Application.Quit();

#endif
    }

    public long GetTimeStampMilliSecond()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        try
        {
            return Convert.ToInt64(ts.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            Debug.Log($"GetTimeStampMilliSecond Error = {ex}");
            return 0;
        }
    }

    public static string ReversalString(string input)
    {
        string result = "";
        for (int i = input.Length - 1; i >= 0; i--)
        {
            result += input[i];
        }
        return result;
    }


    public Texture2D FlipTexture(Texture2D original, bool upSideDown = true)
    {

        Texture2D flipped = new Texture2D(original.width, original.height);

        int xN = original.width;
        int yN = original.height;


        for (int i = 0; i < xN; i++)
        {
            for (int j = 0; j < yN; j++)
            {
                if (upSideDown)
                {
                    flipped.SetPixel(j, xN - i - 1, original.GetPixel(j, i));
                }
                else
                {
                    flipped.SetPixel(xN - i - 1, j, original.GetPixel(i, j));
                }
            }
        }
        flipped.Apply();

        return flipped;
    }

    public void Capture(Camera _camera)
    {
        RenderTexture activeRenderTexture = RenderTexture.active;
        Debug.Log(_camera);
        RenderTexture.active = _camera.targetTexture;

        _camera.Render();

        Texture2D image = new Texture2D(_camera.targetTexture.width, _camera.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, _camera.targetTexture.width, _camera.targetTexture.height), 0, 0);
        image.Apply();
        RenderTexture.active = activeRenderTexture;

        byte[] bytes = image.EncodeToPNG();
        Destroy(image);

        Debug.Log(bytes);

        File.WriteAllBytes(Path.Combine(Application.persistentDataPath, "output.png"), bytes);
    }

    private IEnumerator OnFrame()
    {
        Debug.Log("DVCStreamer OnFrame ...");
        var endOfFrame = new WaitForEndOfFrame();
        while (true)
        {
            // Check frame index
            yield return endOfFrame;

            if (!RTCPeer.DVCClient_isReady(mStreamClient))
            {
                if (mCount % 100 == 0)
                {
                        Debug.Log("DVCClient not Ready");
                }
                Thread.Sleep(300);
                continue;
                
            }
            mCount++;


            Camera camera1 = mCamera;

            var frameBuffer = RenderTexture.GetTemporary(mFrameDescriptor);
            var prevTarget = camera1.targetTexture;
            camera1.targetTexture = frameBuffer;
            camera1.Render();
            camera1.targetTexture = prevTarget;
            // Readback and commit

            var prevActive = RenderTexture.active;
            RenderTexture.active = frameBuffer;
            mReadbackBuffer.ReadPixels(new Rect(0, 0, frameBuffer.width, frameBuffer.height), 0, 0, false);
            TextureHelper.rotate(mReadbackBuffer);
            mReadbackBuffer.GetRawTextureData<byte>().CopyTo(mPixelBuffer);

            //Debug.Log("DVCStreamer DVCClient_sendVideo ...");
            RTCPeer.DVCClient_sendVideo(mStreamClient, frameBuffer.width, frameBuffer.height, mPixelBuffer, GetTimeStampMilliSecond());

            RenderTexture.active = prevActive;

            RenderTexture.ReleaseTemporary(frameBuffer);
        }
    }


    public static float RealFPS { get; private set; }
    public static float pushFPS { get; private set; }

#endif
}
