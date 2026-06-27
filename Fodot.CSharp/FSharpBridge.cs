using System;
using Microsoft.FSharp.Core;

namespace Fodot.CSharp;

public static class FSharpBridge
{
    public static T AsObj<T>(this FSharpOption<T> option)
    {
        return OptionModule.IsSome(option) ? option.Value : default;
    }

    public static FSharpFunc<TU, TV> AsFSharpFunc<TU, TV>(this Func<TU, TV> func)
    {
        return FuncConvert.ToFSharpFunc<TU, TV>(func.Invoke);
    }
    
    public static FSharpFunc<Unit, T> AsFSharpFunc<T>(this Func<T> func)
    {
        return FuncConvert.ToFSharpFunc<Unit, T>(unit => func());
    }
    
    public static FSharpFunc<T, Unit> AsFSharpFunc<T>(this Action<T> action)
    {
        return FuncConvert.ToFSharpFunc<T, Unit>(t => 
        {
            action.Invoke(t);
            return null;
        });
    }
    
    public static FSharpFunc<Unit, Unit> AsFSharpFunc(this Action action)
    {
        return FuncConvert.ToFSharpFunc<Unit, Unit>(t => 
        {
            action.Invoke();
            return null;
        });
    }
}
