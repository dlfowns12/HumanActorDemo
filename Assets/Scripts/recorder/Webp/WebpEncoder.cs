using System;
using System.IO;
using UnityEngine;

namespace DVCRecorder
{
    public sealed class WebpEncoder : EncoderBase
    {
        protected int m_Width;
        protected int m_Height;
        protected int m_Repeat = -1;                  // -1: no repeat, 0: infinite, >0: repeat count
        protected int m_FrameDelay = 0;               // Frame delay (milliseconds)
        protected bool m_HasStarted = false;          // Ready to output frames
        protected FileStream m_FileStream;

        protected int m_SampleInterval = 30;          // Default sample interval for quantizer

        protected DVCFrame m_CurrentFrame;
        protected byte[] m_Pixels;                    // BGRA byte array from frame

        private string m_FilePath;

        private IntPtr m_Encoder;

        public WebpEncoder() : this(-1, 10, 0, 0)
        {

        }
        public WebpEncoder(int repeat, int quality, int width, int height)
        {
            if (repeat >= 0)
                m_Repeat = repeat;

            m_SampleInterval = (int)Mathf.Clamp(quality, 1, 100);
            m_Width = width;
            m_Height = height;
            m_Encoder = WebpEncoderProxy.WebpEncoder_create(m_Width, m_Height, 0, 1, 0);
            Debug.Log("WebpEncoder :" + m_Encoder);
        }

        ~WebpEncoder()
        {
            Debug.Log("~WebpEncoder :" + m_Encoder);
            WebpEncoderProxy.WebpEncoder_destroy(m_Encoder);
        }

        protected void SetSize(int w, int h)
        {
            m_Width = w;
            m_Height = h;

        }

        public void SetDelay(int ms)
        {
            m_FrameDelay = Mathf.RoundToInt(ms / 10f);
        }

        public void SetFrameRate(float fps)
        {
            if (fps > 0f)
                m_FrameDelay = Mathf.RoundToInt(100f / fps);
        }

        public void AddFrame(DVCFrame frame)
        {
            if ((frame == null))
                throw new ArgumentNullException("Can't add a null frame to the webp.");

            if (!m_HasStarted)
                throw new InvalidOperationException("Call Start() before adding frames to the webp.");
            m_CurrentFrame = frame;
            GetImagePixels();
            WebpEncoderProxy.WebpEncoder_process(m_Encoder, m_Pixels, frame.TimeStamp);
            Debug.Log("WebpEncoder AddFrame:" + m_Encoder);
        }

        public void Start(string file)
        {
            m_FilePath = file;
            m_HasStarted = true;
        }

        public void BuildPalette(ref FixedSizedQueue<DVCFrame> frames)
        {

        }
        public void Finish()
        {
            Debug.Log("WebpEncoder Finish: m_FilePath" + m_FilePath);
            char[] byteArray = m_FilePath.ToCharArray();
            char[] filePath = new char[byteArray.Length + 1];
            for (int i = 0; i < byteArray.Length; i++)
            {
                filePath[i] = byteArray[i];
            }
            filePath[byteArray.Length] = '\0';
            Debug.Log("WebpEncoder Finish: m_FilePath filePath:" + filePath);
            WebpEncoderProxy.WebpEncoder_finalize(m_Encoder, filePath);
        }

        protected void GetImagePixels()
        {
            if (m_Pixels == null)
            {
                m_Pixels = new Byte[4 * m_CurrentFrame.Width * m_CurrentFrame.Height];
            }

            Color32[] p = m_CurrentFrame.Data;
            int count = 0;
            Debug.Log("WebpEncoder GetImagePixels Width:" + m_CurrentFrame.Width + " Height:" + m_CurrentFrame.Height);
            for (int th = 0; th < m_CurrentFrame.Height; ++th)
            {
                for (int tw = 0; tw < m_CurrentFrame.Width; ++tw)
                {
                    Color32 color = p[th * m_CurrentFrame.Width + tw];
                    m_Pixels[count] = color.r; count++;
                    m_Pixels[count] = color.g; count++;
                    m_Pixels[count] = color.b; count++;
                    m_Pixels[count] = color.a; count++;
                }
            }
        }
    }
}
