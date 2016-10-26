using System.Diagnostics;

namespace PinBall {
    public class Time {

        private Stopwatch stopwatch = new Stopwatch();
        private double lastUpdate;

        public double ElapseTime => stopwatch.ElapsedMilliseconds * 0.001;

        public Time() { }

        public void Start() {
            stopwatch.Start();
            lastUpdate = 0;
        }

        public void Stop() {
            stopwatch.Stop();
        }

        public double Update() {
            double now = ElapseTime;
            double updateTime = now - lastUpdate;
            lastUpdate = now;
            return updateTime;
        }
    }
}
