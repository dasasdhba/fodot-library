namespace Moon.Component;

/// <summary>
/// Manual Static Timer
/// </summary>
public class STimer(double waitTime) : MTimer
{
    public double WaitTime { get; set; } = waitTime;
    public override double GetWaitTime() => WaitTime;
}