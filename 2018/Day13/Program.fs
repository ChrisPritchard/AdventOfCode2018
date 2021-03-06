﻿open System
open System.IO
open System

type Cart = {
    pos: int * int
    dir: char
    inter: int
}

let move cart =
    let x,y = cart.pos
    match cart.dir with
    | '^' -> { cart with pos = x,y-1 }
    | 'v' -> { cart with pos = x,y+1 }
    | '<' -> { cart with pos = x-1,y }
    | '>' -> { cart with pos = x+1,y }
    | _ -> failwith "invalid cart"

let advance (rails : Map<int * int, char>) cart =
    let tile = rails.[cart.pos]
    match cart.dir with
    | '^' ->
        match tile with 
        | '/' -> { cart with dir = '>' }
        | '\\' -> { cart with dir = '<' }
        | '|' -> { cart with dir = '^' }
        | _ -> 
            match cart.inter % 3 with
            | 0 -> { cart with dir = '<'; inter = cart.inter + 1 }
            | 1 -> { cart with dir = '^'; inter = cart.inter + 1 }
            | _ -> { cart with dir = '>'; inter = cart.inter + 1 }
    | 'v' ->
        match tile with 
        | '/' -> { cart with dir = '<' }
        | '\\' -> { cart with dir = '>' }
        | '|' -> { cart with dir = 'v' }
        | _ -> 
            match cart.inter % 3 with
            | 0 -> { cart with dir = '>'; inter = cart.inter + 1 }
            | 1 -> { cart with dir = 'v'; inter = cart.inter + 1 }
            | _ -> { cart with dir = '<'; inter = cart.inter + 1 }
    | '<' ->
        match tile with 
        | '/' -> { cart with dir = 'v' }
        | '\\' -> { cart with dir = '^' }
        | '-' -> { cart with dir = '<' }
        | _ -> 
            match cart.inter % 3 with
            | 0 -> { cart with dir = 'v'; inter = cart.inter + 1 }
            | 1 -> { cart with dir = '<'; inter = cart.inter + 1 }
            | _ -> { cart with dir = '^'; inter = cart.inter + 1 }
    | '>' ->
        match tile with 
        | '/' -> { cart with dir = '^' }
        | '\\' -> { cart with dir = 'v' }
        | '-' -> { cart with dir = '>' }
        | _ -> 
            match cart.inter % 3 with
            | 0 -> { cart with dir = '^'; inter = cart.inter + 1 }
            | 1 -> { cart with dir = '>'; inter = cart.inter + 1 }
            | _ -> { cart with dir = 'v'; inter = cart.inter + 1 }
    | _ -> failwith "invalid cart"

// let render (rails : Map<int * int, char>) carts =
//     Console.CursorVisible <- false
//     System.Threading.Thread.Sleep 1000
//     Console.CursorLeft <- 0
//     for ((x, y), t) in rails |> Map.toList do
//         Console.CursorLeft <- x
//         Console.CursorTop <- y
//         Console.Write t
//     for c in carts do
//         let x, y = c.pos
//         Console.CursorLeft <- x
//         Console.CursorTop <- y
//         Console.Write c.dir

let rec part1 (rails : Map<int * int, char>) carts =
    // render rails carts
    let next, crash = 
        carts
        |> List.sortBy (fun c -> snd c.pos, fst c.pos)
        |> List.fold (fun (n, crash) cart -> 
            match crash with
            | Some _ -> n, crash
            | None ->
                let moved = move cart
                match List.tryFind (fun c -> c.pos = moved.pos) n with
                | Some _ -> n, Some moved.pos
                | None -> 
                    (advance rails moved)::n, None) ([], None)
    match crash with
    | Some p -> p
    | None -> part1 rails next

let rec part2 (rails : Map<int * int, char>) carts =
    // render rails carts
    match carts with
    | [allAlone] -> allAlone.pos
    | _ ->
        let next, crashed, _ = 
            carts
            |> List.sortBy (fun c -> snd c.pos, fst c.pos)
            |> List.fold (fun (next, crashed, prev) cart ->
                if List.contains cart crashed then next, crashed, cart::prev
                else
                    let moved = move cart |> advance rails
                    let toCheck = next @ (List.except prev carts)
                    match List.tryFind (fun c -> c.pos = moved.pos) toCheck with
                    | Some other -> next |> List.except [other], moved::other::crashed, cart::prev
                    | None -> moved::next, crashed, cart::prev) 
                    ([], [], [])
        part2 rails (List.except crashed next)

[<EntryPoint>]
let main _ =
    
    let input = File.ReadAllLines "input.txt"
    let tiles =
        input |> Seq.mapi (fun y line -> 
        line |> Seq.mapi (fun x c -> (x, y), c))
        |> Seq.collect id |> Seq.toList

    let rails = 
        tiles 
        |> List.map (fun (p, t) -> p, t |> function | '^' | 'v' -> '|' | '<' | '>' -> '-' | c -> c) 
        |> Map.ofList
    let carts = 
        tiles 
        |> List.filter (snd >> function | '^' | 'v' | '<' | '>' -> true | _ -> false) 
        |> List.map (fun (p,c) -> { pos = p; dir = c; inter = 0 })

    printfn "part 1: %i,%i" <|| part1 rails carts
    printfn "part 2: %i,%i" <|| part2 rails carts

    0
