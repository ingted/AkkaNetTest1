#r @"../packages/Reactive.Streams/lib/net45/Reactive.Streams.dll"
#r @"../packages/Akka/lib/net45/Akka.dll"
#r @"../packages/Akka.Streams/lib/net45/Akka.Streams.dll"
#r @"../packages/Akka.FSharp/lib/net45/Akka.FSharp.dll"

open Akka.Streams
open Akka.Streams.Dsl
open Akka
open Akka.Actor
open System.Numerics
open Akka.IO
open System.IO
open System
open Akka.Streams.Dsl
open Akka.FSharp

Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
let system = ActorSystem.Create "system"
let materializer = system.Materializer()

let source: Source<int, NotUsed> = Source.From [1..100]
let factorials = source.Scan(BigInteger(1), fun acc next -> acc * BigInteger(next))

let lineSink (fileName: string) =
    Flow.Create<string>()
        .Select(fun num -> ByteString.FromString(sprintf "%O\n" num))
        .ToMaterialized(FileIO.ToFile(FileInfo(fileName)), fun l r -> Keep.Right(l, r))

factorials
    .ZipWith(Source.From [0..100], (fun num idx -> sprintf "%O! = %O" num idx))
    .Throttle(1, TimeSpan.FromSeconds 1., 1, ThrottleMode.Shaping)
    .RunForeach(Action<_> (printfn "%s"), materializer)