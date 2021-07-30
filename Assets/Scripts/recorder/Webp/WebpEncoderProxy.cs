
namespace DVCRecorder {

    using System;
    using System.Runtime.InteropServices;

    public static class WebpEncoderProxy {

        private const string Assembly =
        #if UNITY_IOS && !UNITY_EDITOR
        @"__Internal";
        #else
        "webp";
        #endif

        [DllImport(Assembly, EntryPoint = @"WebpEncoder_create")]
        public static extern IntPtr WebpEncoder_create(int width, int height, int kmin, int kmax, int lossless);
        [DllImport(Assembly, EntryPoint = @"WebpEncoder_destroy")]
        public static extern void WebpEncoder_destroy(this IntPtr recorder);
        [DllImport(Assembly, EntryPoint = @"WebpEncoder_process")]
        public static extern void WebpEncoder_process(this IntPtr recorder, byte[] pixelBuffer, long timestamp);
        [DllImport(Assembly, EntryPoint = @"WebpEncoder_finalize")]
        public static extern void WebpEncoder_finalize(this IntPtr recorder, char[] outpath);
    }
}
