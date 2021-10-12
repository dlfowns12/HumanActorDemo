using System;
using System.Runtime.InteropServices;


public class RTCPeer
{
#if UNITY_LINUX_RTC
    private const String libName = "libdrtc";

    public const int RTC_LOG_VERBOSE = 0;
    public const int RTC_LOG_INFO = 1;
    public const int RTC_LOG_WARNING = 2;
    public const int RTC_LOG_ERROR = 3;
    public const int RTC_LOG_NONE = 4;

    [System.Runtime.InteropServices.DllImport(libName)]
    public static extern IntPtr DVCClient_create(String uuild);


    [System.Runtime.InteropServices.DllImport(libName)]
    public static extern int DVCClient_setSignalServer(IntPtr handle, String url);


    [System.Runtime.InteropServices.DllImport(libName)]
    public static extern int DVCClient_setRtcHostPort(IntPtr handle, string hostPort);

    [System.Runtime.InteropServices.DllImport(libName)]
    public static extern int DVCClient_setRTPPortRange(IntPtr handle, int min_port, int max_port);

    [System.Runtime.InteropServices.DllImport(libName)]
    public static extern int DVCClient_addIceServer(IntPtr handle, String url, String userName, String password);


    [System.Runtime.InteropServices.DllImport(libName, CallingConvention = CallingConvention.StdCall)]
    public static extern int DVCClient_setOnDataMessage(IntPtr handle, IntPtr func);


    [System.Runtime.InteropServices.DllImport(libName)]
    public static extern int DVCClient_start(IntPtr handle, int enableVideo, int enableAudio, int enableData);


    [System.Runtime.InteropServices.DllImport(libName)]
    public static extern int DVCClient_setRtcLogLevel(IntPtr handle, int level);


    [System.Runtime.InteropServices.DllImport(libName)]
    public static extern bool DVCClient_isReady(IntPtr handle);


    [System.Runtime.InteropServices.DllImport(libName)]
    public static extern int DVCClient_sendAudio(IntPtr handle, float[] audioData, int nSampleRate, int nNumChannels, int nNumFrames);


    [System.Runtime.InteropServices.DllImport(libName)]
    public static extern int DVCClient_sendVideo(IntPtr handle, int width, int height, byte[] rgba_data, Int64 timestamp_us);


    [System.Runtime.InteropServices.DllImport(libName)]
    public static extern int DVCClient_sendMsg(IntPtr handle, byte[] msg, int msgLength);


    [System.Runtime.InteropServices.DllImport(libName)]
    public static extern int DVCClient_release(out IntPtr handle);
#endif
}