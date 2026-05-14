namespace Moon.Component;

/// <summary>
/// basic manual timer
/// </summary>
public abstract class MTimer()
{
    public abstract double GetWaitTime();
    public double Time { get ;set; } = 0d;
    
    /// <summary>
    /// return true when timeout
    /// </summary>
    public bool Update(double delta)
    {
        double waitTime = GetWaitTime();
        if (Time >= waitTime)
        {
            Time = 0d;
            return true;
        }
        
        Time += delta;
        if (Time >= waitTime)
        {
            Time -= waitTime;
            return true;
        }
        
        return false;
    }
    
    public void Clear() => Time = 0d;
}