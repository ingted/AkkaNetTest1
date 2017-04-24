#r @"../packages/Akka/lib/net45/Akka.dll"
#r @"../packages/Hyperion/lib/net45/Hyperion.dll"
#r @"../packages/Newtonsoft.Json/lib/net45/Newtonsoft.Json.dll"
#r @"../packages/Akkling/lib/net45/Akkling.dll"
#r @"../packages/Akka.Streams/lib/net45/Akka.Streams.dll"
#r @"../packages/Akkling/lib/net45/Akkling.dll"
#r @"../packages/Reactive.Streams/lib/net45/Reactive.Streams.dll"
#r @"../packages/Akkling.Streams/lib/net45/Akkling.Streams.dll"

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

let source = Source.ofList [1..100]
let factorials = source.Scan(BigInteger(1), fun acc next -> acc * BigInteger(next))

let lineSink (fileName: string) =
    Flow.Create<string>()
    |> Flow.map (fun num -> ByteString.FromString(sprintf "%O\n" num))
    |> Flow.toMat (FileIO.ToFile(FileInfo(fileName))) (fun l r -> Keep.Right(l, r))

factorials
|> Source.zipWith (Source.ofList [0..100]) (fun num idx -> sprintf "%O! = %O" num idx)
|> Source.throttle ThrottleMode.Shaping 1 1 (TimeSpan.FromSeconds 1.)
|> Source.runForEach mat (printfn "%s")