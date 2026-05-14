using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Moon.Utils;

/// <summary>
/// math functions
/// </summary>
public static class Mathe
{
    public static Vector2 WithAxis(this Vector2 origin, Vector2 axis)
    {
        var dir = axis.Normalized();
        origin -= dir * dir.Dot(origin);
        return origin + axis;
    }
    
    /// <summary>
    /// Generate a quad crossing with (0,0), (center, maxHeight), (1, finalHeight)
    /// </summary>
    public static Func<float, float> UnitQuad(float maxHeight = 1f, 
        float finalHeight = 0f, float center = 0.5f)
    {
        if (center is <= 0f or >= 1f) 
            throw new ArgumentException("center should be between 0 and 1");
        
        ref var a = ref maxHeight;
        ref var b = ref finalHeight;
        ref var c = ref center;
        
        /* solve
            c^2x + cy = a,
            x + y = b
        */
            
        var cm1c = (c - 1f) * c;
        var bc = b * c;
        
        var x = (a - bc) / cm1c;
        var y = (bc * c - a) / cm1c;
        
        return p => p * p * x + p * y;
    }

    public static int[] PartitionInt(int total, int count)
    {
        var r = total / count;
        var b = total % count;
        var result = new int[count];
        Array.Fill(result, r);
        var rs = result.AsSpan();
        for (var i = 0; i < b; i++) rs[i]++;
        return result;
    }

    public static int[] PartitionIntRandom(int total, int count)
    {
        var result = PartitionInt(total, count);
        result.Shuffle();
        return result;
    }
    
    /// <summary>
    /// the search function should be like: search(x) = false if x &lt; t
    /// where t &gt; 0, true otherwise. the function will start at 0 and max
    /// to find the smallest x such that search(x) is true.
    /// </summary>
    public static float BinarySearch(Func<float, bool> search, float max = 256f, 
        float eps = 1f, int maxIter = 100)
    {
        if (search(0)) return 0;
        if (!search(max)) return max;
        
        var a = 0f;
        var b = max;
        
        var iter = 0;
        while (b - a > eps)
        {
            var c = (a + b) / 2f;
            if (search(c)) b = c;
            else a = c;

            if (maxIter > 0)
            {
                iter++;
                if (iter >= maxIter)
                {
                #if TOOLS
                    FD.PushWarning("Binary Search failed...");
                #endif                
                    break;
                }
            }
        }

        return b;
    }
    
    public static double Accelerate(double speed, double acc, double dec, double max, double delta)
        => Mathf.MoveToward(speed, max, speed < max ? acc * delta : dec * delta);
    
    public static float Accelerate(float speed, float acc, float dec, float max, float delta)
        => Mathf.MoveToward(speed, max, speed < max ? acc * delta : dec * delta);

    /// <summary>
    /// move to target in interval with closest direction
    /// </summary>
    public static double MoveTowardWrap(double current, double target, double min, double max, double delta)
    {
        var len = max - min;
        var half = len / 2d;
        var diff = Mathf.Wrap(current - target, -half, half);
        return Mathf.MoveToward(current, current - diff, delta);
    }
    
    /// <summary>
    /// <inheritdoc cref="MoveTowardWrap(double,double,double,double,double)"/>>
    /// </summary>
    public static float MoveTowardWrap(float current, float target, float min, float max, float delta)
    {
        var len = max - min;
        var half = len / 2f;
        var diff = Mathf.Wrap(current - target, -half, half);
        return Mathf.MoveToward(current, current - diff, delta);
    }
    
    /// <summary>
    /// move an angle towards another one in closest direction
    /// </summary>
    public static double MoveTowardAngle(double current, double target, double delta)
        => MoveTowardWrap(current, target, -double.Pi, double.Pi, delta);
    
    /// <summary>
    /// <inheritdoc cref="MoveTowardAngle(double,double,double)"/>>
    /// </summary>
    public static float MoveTowardAngle(float current, float target, float delta)
        => MoveTowardWrap(current, target, -float.Pi, float.Pi, delta);
    
    /// <summary>
    /// move a direction vector towards another one by angle
    /// </summary>
    public static Vector2 MoveTowardDir(Vector2 origin, Vector2 target, double delta)
    {
        var o = origin.Angle();
        var t = target.Angle();
        return Vector2.Right.Rotated((float)MoveTowardAngle(o, t, delta));
    }
    
    /// <summary>
    /// Moves this vector toward to x by the fixed delta amount.
    /// </summary>
    public static Vector2 MoveTowardX(this Vector2 origin, float x, float delta)
        => origin.MoveToward(origin with { X = x }, delta);
        
    public static Vector2 MoveTowardY(this Vector2 origin, float y, float delta)
        => origin.MoveToward(origin with { Y = y }, delta);
    
    /// <summary>
    /// clamp a value in center ± spread range
    /// </summary>
    public static double ClampWrap(double value, double center, double spread, double min, double max)
    {
        var diff = Mathf.Wrap(center - value, min, max);
        if (Math.Abs(diff) < spread) return Mathf.Wrap(value, min, max);

        var smin = center - spread;
        var smax = center + spread;
        var dmin = Mathf.Wrap(smin - value, min, max);
        var dmax = Mathf.Wrap(smax - value, min, max);
        var result = Math.Abs(dmin) < Math.Abs(dmax) ? smin : smax;
        return Mathf.Wrap(result, min, max);
    }
    
    /// <summary>
    /// <inheritdoc cref="ClampWrap(double,double,double,double,double)"/>>
    /// </summary>
    public static float ClampWrap(float value, float center, float spread, float min, float max)
    {
        var diff = Mathf.Wrap(center - value, min, max);
        if (Math.Abs(diff) < spread) return Mathf.Wrap(value, min, max);

        var smin = center - spread;
        var smax = center + spread;
        var dmin = Mathf.Wrap(smin - value, min, max);
        var dmax = Mathf.Wrap(smax - value, min, max);
        var result = Math.Abs(dmin) < Math.Abs(dmax) ? smin : smax;
        return Mathf.Wrap(result, min, max);
    }
    
    /// <summary>
    /// clamp an angle in center ± spread range
    /// </summary>
    public static double ClampAngle(double angle, double center, double spread)
        => ClampWrap(angle, center, spread, -double.Pi, double.Pi);
    
    /// <summary>
    /// <inheritdoc cref="ClampAngle(double,double,double)"/>>
    /// </summary>
    public static float ClampAngle(float angle, float center, float spread)
        => ClampWrap(angle, center, spread, -float.Pi, float.Pi);
    
    /// <summary>
    /// clamp a direction vector in normal.Rotate(±spread) range
    /// </summary>
    public static Vector2 ClampDir(Vector2 dir, Vector2 normal, double spread)
        => Vector2.Right.Rotated((float)ClampAngle(dir.Angle(), normal.Angle(), spread));

    public static T Max<T>(params T[] values) where T : IComparable<T>
        => values.Max();
    
    public static T Min<T>(params T[] values) where T : IComparable<T>
        => values.Min();
    
    // random
    private static RandomNumberGenerator RNG = new();

    /// <summary>
    /// 0~1 (inclusive)
    /// </summary>
    public static float Randf() => RNG.Randf();

    /// <summary>
    /// min~max (inclusive)
    /// </summary>
    public static float RandfRange(float min, float max) => RNG.RandfRange(min, max);

    /// <summary>
    /// 0~4294967295 (inclusive)
    /// </summary>
    public static uint Randi() => RNG.Randi();

    /// <summary>
    /// min~max (inclusive)
    /// </summary>
    public static int RandiRange(int min, int max) => RNG.RandiRange(min, max);
    
    /// <summary>
    /// Returns a random index with non-uniform weights.
    /// Prints an error and returns -1 if the array is empty.
    /// </summary>
    public static int RandWeighted(float[] weights) => (int)RNG.RandWeighted(weights);
    
    /// <summary>
    /// <inheritdoc cref="RandWeighted(float[])"/>
    /// </summary>
    public static int RandWeighted(IEnumerable<float> weights) => RandWeighted(weights.ToArray());
    
    /// <summary>
    /// Returns a random point within the given rect.
    /// </summary>
    public static Vector2 GetPointRandom(this Rect2 rect)
    {
        return new Vector2(
            RandfRange(rect.Position.X, rect.End.X),
            RandfRange(rect.Position.Y, rect.End.Y)
        );
    }
    
    public static T SelectRandom<T>(this IList<T> arr)
    {
        var count = arr.Count;
        if (count == 0) return default;
        return arr[RandiRange(0, count - 1)];
    }
    
    private static Random _Rand = new();
    public static void Shuffle<T>(this T[] arr) => _Rand.Shuffle(arr);
    public static void Shuffle<T>(this Span<T> arr) => _Rand.Shuffle(arr);
    public static void Shuffle<T>(this List<T> list)
        => CollectionsMarshal.AsSpan(list).Shuffle();
}