using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DVCRecorder
{
    public class WebpRecorder : Recorder
    {
        public WebpRecorder()
        {
            m_RecorderName = "webp";
            
        }

        public override void init(int repeat, int quality, int width, int height)
        {
            m_Encoder = new WebpEncoder(repeat, quality, width, height);
        }

    }
}
