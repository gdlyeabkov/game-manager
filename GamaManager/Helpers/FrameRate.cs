using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamaManager.Helpers
{
    internal class FrameRate
    {
        public FrameRate(string time, int frames)
        {
            Time = time;
            Frames = frames;
        }

        public string Time { get; private set; }
        public int Frames { get; private set; }

    }
}
