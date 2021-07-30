namespace DVCRecorder
{
    public interface EncoderBase {
        void SetDelay(int ms);

        void SetFrameRate(float fps);

        void AddFrame(DVCFrame frame);

        void Start(string file);

        void BuildPalette(ref FixedSizedQueue<DVCFrame> frames);

        void Finish();
    }
}