namespace Fodot.Common

open System

[<AttributeUsage(AttributeTargets.Class, AllowMultiple = false)>]
type FScriptAttribute(name: string) =
    inherit Attribute()
    member this.Name = name

[<AttributeUsage(AttributeTargets.Field, AllowMultiple = false)>]
type GDProperty(name: string) =
    inherit Attribute()
    member this.Name = name