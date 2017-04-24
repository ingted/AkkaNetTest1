#r @"../packages/Reactive.Streams/lib/net45/Reactive.Streams.dll"
#r @"../packages/Akka/lib/net45/Akka.dll"
#r @"../packages/Akka.Streams/lib/net45/Akka.Streams.dll"
#r @"../packages/Akkling/lib/net45/Akkling.dll"
#r @"../packages/Akka.Streams/lib/net45/Akka.Streams.dll"

open Akka.Streams
open Akka.Streams.Dsl
open Akka
open System.Numerics
open Akka.IO
open System.IO
open System
open Akkling
open Akkling.Streams

Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
let system = System.create "system" (Configuration.defaultConfig())
let mat = system.Materializer()

let source: Source<int, NotUsed> = ofArray From [1..100]
let factorials = source.Scan(BigInteger(1), fun acc next -> acc * BigInteger(next))

let lineSink (fileName: string) =
    Flow.Create<string>()
        .Select(fun num -> ByteString.FromString(sprintf "%O\n" num))
        .ToMaterialized(FileIO.ToFile(FileInfo(fileName)), fun l r -> Keep.Right(l, r))

factorials
    .ZipWith(Source.From [0..100], (fun num idx -> sprintf "%O! = %O" num idx))
    .Throttle(1, TimeSpan.FromSeconds 1., 1, ThrottleMode.Shaping)
    .RunForeach(Action<_> (printfn "%s"), mat)