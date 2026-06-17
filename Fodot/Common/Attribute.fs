namespace Fodot.Common

open System

[<AttributeUsage(AttributeTargets.Class, AllowMultiple = false)>]
type FScriptAttribute(tag : obj) =
    inherit Attribute()
    member this.Tag = tag

[<AttributeUsage(AttributeTargets.Field, AllowMultiple = false)>]
type GDProperty(name: string) =
    inherit Attribute()
    member this.Name = name