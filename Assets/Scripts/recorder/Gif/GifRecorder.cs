using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DVCRecorder
{
    public class GifRecorder : Recorder
    {
        public GifRecorder()
        {
            m_RecorderName = "gif";
            
        }

        public override void init(int repeat, int quality, int width, int height)
        {
            m_Encoder = new GifEncoder(repeat, quality);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
