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
        if cnt > 0 then List.rev ((cnt |> string)::path)
        else List.rev path
        |> String.concat ""


    let findSub (s: string) =
        [2..20]
        |> Seq.collect (fun n ->
            Seq.windowed n s
            |> Seq.map asString
            |> Seq.groupBy id
            |> Seq.filter (fun (_, g) -> Seq.length g > 1)
            |> Seq.map fst)
        |> Seq.toArray

    let triples (s: string) options =
        options
        |> Array.choose (fun o ->
            let rest = splits [o] s
            let result = 
                Seq.groupBy id rest
                |> Seq.map fst
                |> Seq.filter (fun s -> s.Length <= 20)
                |> Seq.toArray
            if result.Length = 2 then
                Some <| Array.sortByDescending Seq.length [|o;result.[0];result.[1]|]
            else
                None)
        |> Array.sortByDescending (fun a -> a.[2].Length)

    let test1 = findSub final
    let test2 = triples final test1

    let routes (s: string) tokens =
        let a = tokens.[0]
        let b = tokens.[1]
        let c = tokens.[2]

        let path = 
            splits [a] s
            |> Array.map (fun between ->
                if between = b then [|b|]
                elif between = c then [|c|]
                elif between.StartsWith b then [|b;c|]
                elif between.StartsWith c then [|c;b|]
                else
                    splits [b] between)
    0