using System;

namespace Moon.Component;

/// <summary>
/// Manual Dynamic Timer
/// </summary>
public class DTimer(Func<double> waitTimeFunc) : MTimer
{
    public Func<double> WaitTimeFunc { get; set; } = waitTimeFunc;
    public override double GetWaitTime() => WaitTimeFunc();
}