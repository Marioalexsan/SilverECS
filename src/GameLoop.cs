using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SilverECS
{
    public class GameLoop
    {
        public bool Running { get; private set; } = false;

        private Thread _updateThread;

        private Stopwatch _stopwatch = new();

        private long _accumulatedTicks = 0;

        public TimeSpan TargetDeltaTime
        {
            get;
            set;
        } = TimeSpan.FromSeconds(1.0 / 60.0);

        public long _lastTimeInTicks = 0;

        public TimeSpan TimeRunning => _stopwatch.Elapsed;

        public GameLoop()
        {

        }

        public void Run()
        {
            if (Running)
            {
                throw new InvalidOperationException("Game is already running!");
            }

            _updateThread = new Thread(this.UpdateInternal);

            Running = true;
            _stopwatch.Start();
            _updateThread.Start();
        }

        public void Stop()
        {
            if (!Running)
            {
                throw new InvalidOperationException("Game is not running!");
            }

            Running = false;
            _updateThread.Join();
            _stopwatch.Reset();

            _updateThread = null;
        }

        protected virtual void Initialize()
        {

        }

        protected virtual void Update(TimeSpan deltaTime)
        {

        }

        protected virtual void Cleanup()
        {

        }

        private void UpdateInternal()
        {
            TimeSpan worstSleepTime = TimeSpan.FromMilliseconds(4);

            Initialize();

            while (Running)
            {
                long frameTicks = TargetDeltaTime.Ticks;

                AdvanceTime();

                while (_accumulatedTicks + worstSleepTime.Ticks < frameTicks)
                {
                    var lol = _stopwatch.Elapsed;
                    Thread.Sleep(1);
                    Debug.WriteLine((_stopwatch.Elapsed - lol).TotalMilliseconds);

                    AdvanceTime();
                }

                while (_accumulatedTicks < frameTicks)
                {
                    Thread.SpinWait(1);
                    AdvanceTime();
                }

                while (_accumulatedTicks > frameTicks)
                {
                    Update(TimeSpan.FromTicks(frameTicks));
                    _accumulatedTicks -= frameTicks;
                }
            }

            Cleanup();
        }

        private long AdvanceTime()
        {
            long ticksNow = _stopwatch.Elapsed.Ticks;
            long deltaTicks = ticksNow - _lastTimeInTicks;
            _lastTimeInTicks = ticksNow;

            _accumulatedTicks += deltaTicks;

            return deltaTicks;
        }
    }
}
