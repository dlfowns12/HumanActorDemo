
using System;
using System.Threading;
using UnityEngine;
using ThreadPriority = System.Threading.ThreadPriority;

namespace DVCRecorder
{
	internal sealed class GeneratorWorker
	{
		#region Private fields

		private readonly Thread _thread;

		private FixedSizedQueue<DVCFrame> _capturedFrames;
		private Recorder _recorder;
		private readonly string _filePath;
		private readonly Action _onFileSaved;
        private readonly int _playbackFrameRate;
        private readonly int _repeat;
		private EncoderBase _encoder;

		private int _width;
		private int _height;
		#endregion

		#region Internal methods

		internal GeneratorWorker(Recorder recorder, bool loop, int playbackFrameRate, ThreadPriority priority, FixedSizedQueue<DVCFrame> capturedFrames, string filePath, Action onFileSaved)
		{
			_capturedFrames = capturedFrames;
			_filePath = filePath;
			_onFileSaved = onFileSaved;
            // 0: loop, -1 play once
            _repeat = loop ? 0 : -1;
            _playbackFrameRate = playbackFrameRate;

			_recorder = recorder;
			
			_thread = new Thread(Run) {Priority = priority};

			if (_capturedFrames.Count() > 0) {
				DVCFrame f = _capturedFrames.ElementAt(0);
				_width = f.Width;
				_height = f.Height;
				Debug.Log("GeneratorWorker _width:" + _width + " _height:" + _height);
			}
		}

		internal void Start()
		{

			_recorder.init(_repeat, 20, _width, _height);
			_encoder = _recorder.m_Encoder;
			_encoder.SetFrameRate(_playbackFrameRate);
			_thread.Start();
		}

		#endregion

		#region Private methods

		private void Run()
		{

			var startTimestamp = DateTime.Now;
            _encoder.Start(_filePath);
			_encoder.BuildPalette(ref _capturedFrames);
	
			for (int i = 0; i < _capturedFrames.Count(); i++)
            {
				Debug.Log("i:" + i);
                _encoder.AddFrame(_capturedFrames.ElementAt(i));

            }
            _encoder.Finish();
            Debug.Log(_recorder.m_RecorderName + "生成完毕，耗时： " + (DateTime.Now - startTimestamp).Milliseconds + " 毫秒");

			_onFileSaved?.Invoke();

			//MsgEvent.SendCallBackMsg((int)AvatarID.Suc_gif_recording_stop, AvatarID.Suc_gif_recording_stop.ToString());
		}

		#endregion
		
	}
}
