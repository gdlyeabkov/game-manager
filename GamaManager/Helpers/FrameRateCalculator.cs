using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Windows.Media;

namespace GamaManager.Helpers
{
    class FrameRateCalculator : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private readonly ISubject<FrameRate> _rateSubject = new Subject<FrameRate>();

        private readonly Stopwatch _frameRateCalcStopWatch = new Stopwatch();
        private int _frameCount;
        private int _previousFrameCount;

        private readonly Timer _frameRateCalcTimer;

        private const int IsNotRunning = 0;
        private const int IsRunning = 1;
        private int _isRunning = IsNotRunning;

        public FrameRateCalculator()
        {
            _frameRateCalcTimer = new Timer(CalculateFrameRate, null, TimeSpan.Zero, TimeSpan.Zero);
        }

        public void Start()
        {
            if (Interlocked.CompareExchange(ref _isRunning, IsRunning, IsNotRunning) == IsNotRunning)
            {
                _frameRateCalcStopWatch.Start();
                _frameRateCalcTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(1));
                CompositionTarget.Rendering += CompositionTargetRendering;
            }
        }

        private void CompositionTargetRendering(object sender, EventArgs e)
        {
            _lock.EnterWriteLock();
            try
            {
                _frameCount++;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Stop()
        {
            StopOrReset(true);
        }

        public void Reset()
        {
            StopOrReset(false);
        }

        public IObservable<FrameRate> Observable
        {
            get { return _rateSubject.AsObservable(); }
        }

        private void StopOrReset(bool stop)
        {
            if (Interlocked.CompareExchange(ref _isRunning, IsNotRunning, IsRunning) == IsRunning)
            {
                _frameRateCalcTimer.Change(TimeSpan.Zero, TimeSpan.Zero);

                if (stop)
                    _frameRateCalcStopWatch.Stop();
                else
                    _frameRateCalcStopWatch.Reset();

                CompositionTarget.Rendering -= CompositionTargetRendering;
            }
        }

        private void CalculateFrameRate(object state)
        {
            int framesThisTick;

            _lock.EnterReadLock();
            try
            {
                var tempFrameCount = _frameCount;
                framesThisTick = (_frameCount - _previousFrameCount);
                _previousFrameCount = tempFrameCount;
            }
            finally
            {
                _lock.ExitReadLock();
            }

            var tickTime = _frameRateCalcStopWatch.Elapsed.ToString(@"mm\:ss");
            var frameRate = new FrameRate(tickTime, framesThisTick);
            _rateSubject.OnNext(frameRate);
        }

        public void Dispose()
        {
            _frameRateCalcTimer.Dispose();
        }
    }
}
