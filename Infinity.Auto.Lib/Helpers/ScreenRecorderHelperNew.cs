using ScreenRecorderLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Automation.Lib.Helpers
{
    public class ScreenRecorderHelperNew : IScreenRecorderHelper, IDisposable
    {
        private Recorder _rec;
        private Stream _outStream;
        public void Dispose()
        {
            _rec.Stop();
            _rec = null;
            _outStream = null;
        }

        public void RecordScreen(RecorderParams recorderParams)
        {

            string videoPath = recorderParams.FileName;
            var recorderOptions = new RecorderOptions();
            recorderOptions.VideoOptions = new VideoOptions();
            recorderOptions.VideoOptions.BitrateMode = BitrateControlMode.Quality;
            recorderOptions.VideoOptions.Quality = recorderParams.Quality;
            _rec = Recorder.CreateRecorder(recorderOptions);
            
            //_rec.OnRecordingComplete += Rec_OnRecordingComplete;
            //_rec.OnRecordingFailed += Rec_OnRecordingFailed;
           // _rec.OnStatusChanged += Rec_OnStatusChanged;
            //Record to a file
            _rec.Record(videoPath);
        }
    }
}
