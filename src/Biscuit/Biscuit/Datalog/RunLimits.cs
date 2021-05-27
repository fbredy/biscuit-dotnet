using System;

namespace Biscuit.Datalog
{
    public class RunLimits
    {
        public int MaxFacts { get; set; }
        public int MaxIterations { get; set; }
        public TimeSpan MaxTime { get; set; }

        public RunLimits()
        {
            MaxFacts = 1000;
            MaxIterations = 100;
            MaxTime = TimeSpan.FromMilliseconds(5000);// Duration.ofMillis(5);
        }

        public RunLimits(int maxFacts, int maxIterations, TimeSpan maxTime)
        {
            this.MaxFacts = maxFacts;
            this.MaxIterations = maxIterations;
            this.MaxTime = maxTime;
        }
    }

}
