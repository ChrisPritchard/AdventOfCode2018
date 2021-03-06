﻿module Day17

open Common
open System.IO

let input = (File.ReadAllText ("./inputs/day17.txt")).Split ','

let mem = Intcode.memFrom input
let io = Intcode.IO.create ()
Intcode.run 0L 0L mem io |> ignore

let rec mapper acc line = 
    let canRead, next = io.read ()
    if not canRead then List.rev acc |> List.toArray
    else
        if next = 10L then
            let line = List.rev line |> List.toArray
            let acc = if line.Length > 0 then line::acc else acc
            mapper acc []
        else
            mapper acc (char next::line)
            
let map = mapper [] []

let part1 () =

    //for y = 0 to map.Length - 1 do
    //    for x = 0 to map.[y].Length - 1 do
    //        printf "%c" map.[y].[x]
    //    printfn ""

    let intersection x y =  
        if map.[y].[x] <> '#' then None
        else
            let isIntersection =
                [
                    -1, 0
                    1, 0
                    0, -1
                    0, 1
                ] 
                |> Seq.map (fun (dx, dy) -> x + dx, y + dy)
                |> Seq.filter (fun (x, y) -> x >= 0 && y >= 0 && x < map.[0].Length && y < map.Length)
                |> Seq.filter (fun (x, y) -> map.[y].[x] = '#')
                |> Seq.length >= 3
            if isIntersection then Some (y * x) else None

    seq {
        for y = 0 to map.Length - 1 do
            for x = 0 to map.[y].Length - 1 do
                match intersection x y with
                | Some n -> yield n
                | None -> ()
    } |> Seq.sum

let part2 () =

    let start = (3, 0, '^')
    let goal = (34, 24)

    let edges (x, y, currentDir) =
        let options = 
            [
                -1, 0, '<'
                1, 0, '>'
                0, -1, '^'
                0, 1, 'v'
            ] 
            |> Seq.map (fun (dx, dy, dir) -> x + dx, y + dy, dir)
            |> Seq.filter (fun (x, y, _) -> x >= 0 && y >= 0 && x < map.[0].Length && y < map.Length)
            |> Seq.filter (fun (x, y, _) -> map.[y].[x] = '#')
            |> Seq.toArray
        let straight = 
            Array.tryFind (fun (_, _, dir) -> dir = currentDir) options
        seq {
            match straight with
            | Some o -> yield o
            | None ->
                yield! options
        }

    let isGoal (x, y, _) = (x, y) = goal

    let change lastDir dir =
        match lastDir, dir with
        | '^', '>' | '>', 'v' | 'v', '<' | '<', '^' -> "R"
        | '^', '<' | '<', 'v' | 'v', '>' | '>', '^' -> "L"
        | _ -> failwithf "unexpected: %c %c" lastDir dir

    let fullPath = BFS.run isGoal edges start |> Option.defaultValue []
    let (path, cnt, _) = 
        (([], 0, '^'), fullPath.[1..]) 
        ||> List.fold (fun (acc, cnt, lstDir) (x, y, dir) ->
            if dir = lstDir then
                acc, cnt + 1, lstDir
            elif cnt <> 0 then
                (change lstDir dir)::(cnt + 1 |> string)::acc, 0, dir
            else
                (change lstDir dir)::acc, 1, dir)
    let final =
        if cnt > 0 then List.rev ((cnt + 1 |> string)::path)
        else List.rev path
        |> List.toArray

    let triples (s: string []) =
        let tokens = 
            [5..10]
            |> Seq.collect (fun n -> Array.windowed n s)
            |> Seq.distinct
            |> Seq.toArray
        seq {
            for a in tokens do
                for b in tokens do
                    for c in tokens do
                        if a <> b && b <> c && c <> a then
                            let A, B, C = String.concat "" a, String.concat "" b, String.concat "" c
                            let S = String.concat "" s
                            if replace A "" S |> replace B "" |> replace C "" = "" then
                                yield (a, b, c)
        } |> Seq.head

    let (a, b, c) = triples final
    let mainRoutine = 
        replace (String.concat "" a) "A," (String.concat "" final) 
        |> replace (String.concat "" b) "B," 
        |> replace (String.concat "" c) "C," 
        |> fun s -> s.Trim ',' + "\n"
    let A = String.concat "," a + "\n"
    let B = String.concat "," b + "\n"
    let C = String.concat "," c + "\n"

    let mem = Intcode.memFrom input
    mem.[0L] <- 2L
    let io = Intcode.IO.create ()
    String.concat "" [mainRoutine;A;B;C;"n\n"] |> Seq.iter (fun c -> int64 c |> io.write)
    Intcode.run 0L 0L mem io |> ignore

    io.output |> Seq.toArray |> Array.last |> string