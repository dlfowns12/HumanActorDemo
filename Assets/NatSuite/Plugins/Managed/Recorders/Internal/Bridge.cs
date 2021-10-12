/* 
*   NatCorder
*   Copyright (c) 2020 Yusuf Olokoba.
*/

namespace NatSuite.Recorders.Internal {

    using System;
    using System.Runtime.InteropServices;

    public static class Bridge {

        private const string Assembly =
#if UNITY_IOS && !UNITY_EDITOR
        @"__Internal";
#elif UNITY_LINUX_RECORDER
        "mp4recorder";
#else
        @"NatCorder";
#endif
#if UNITY_LINUX_RECORDER
        public delegate void RecordingHandler(IntPtr context, IntPtr path);
        [DllImport(Assembly, EntryPoint = @"DVC_CreateMP4Recorder")]
        public static extern IntPtr CreateMP4Recorder(int width, int height, float framerate, int bitrate, int keyframeInterval, int sampleRate, int channelCount, string recordingPath);
       [DllImport(Assembly, EntryPoint = @"DVC_FrameSize")]
        public static extern void FrameSize(this IntPtr recorder, out int width, out int height);
        [DllImport(Assembly, EntryPoint = @"DVC_CommitFrame")]
        public static extern void CommitFrame(this IntPtr recorder, IntPtr pixelBuffer, long timestamp);
        [DllImport(Assembly, EntryPoint = @"DVC_CommitSamples")]
        public static extern void CommitSamples(this IntPtr recorder, float[] sampleBuffer, int sampleCount, long timestamp);
        [DllImport(Assembly, EntryPoint = @"DVC_FinishWriting")]
        public static extern void FinishWriting(this IntPtr recorder);
#else
        public delegate void RecordingHandler (IntPtr context, IntPtr path); // CHECK // Marshal string directly?

        [DllImport(Assembly, EntryPoint = @"NCCreateMP4Recorder")]
        public static extern IntPtr CreateMP4Recorder (int width, int height, float framerate, int bitrate, int keyframeInterval, int sampleRate, int channelCount, [MarshalAs(UnmanagedType.LPStr)] string recordingPath, RecordingHandler callback, IntPtr context);
        [DllImport(Assembly, EntryPoint = @"NCCreateHEVCRecorder")]
        public static extern IntPtr CreateHEVCRecorder (int width, int height, float framerate, int bitrate, int keyframeInterval, int sampleRate, int channelCount, [MarshalAs(UnmanagedType.LPStr)] string recordingPath, RecordingHandler callback, IntPtr context);
        [DllImport(Assembly, EntryPoint = @"NCCreateGIFRecorder")]
        public static extern IntPtr CreateGIFRecorder (int width, int height, float frameDuration, [MarshalAs(UnmanagedType.LPStr)] string recordingPath, RecordingHandler callback, IntPtr context);
        [DllImport(Assembly, EntryPoint = @"NCFrameSize")]
        public static extern void FrameSize (this IntPtr recorder, out int width, out int height);
        [DllImport(Assembly, EntryPoint = @"NCCommitFrame")]
        public static extern void CommitFrame (this IntPtr recorder, IntPtr pixelBuffer, long timestamp);
        [DllImport(Assembly, EntryPoint = @"NCCommitSamples")]
        public static extern void CommitSamples (this IntPtr recorder, float[] sampleBuffer, int sampleCount, long timestamp);
        [DllImport(Assembly, EntryPoint = @"NCFinishWriting")]
        public static extern void FinishWriting (this IntPtr recorder);
#endif
    }
}