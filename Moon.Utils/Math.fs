module Moon.Utils.Math

/// pick(0) is expected to be Some, while pick(1) = None is also expected
/// but not necessary. returns inside(pick = Some), outside(pick = None), pickedValue
let binarySearchAndPick maxStep eps pick =
    let mutable inside = 0f
    let mutable outside = 1f
    let mutable lastPick = None
    
    let mutable value = pick outside
    while value |> Option.isSome do
        inside <- outside
        outside <- outside * 2f
        lastPick <- value
        value <- pick outside
    
    let mutable lastInside = false
    let mutable mid = inside * 0.5f + outside * 0.5f
    let rec update step =
        let value = pick mid
        let isInside = value |> Option.isSome
        if isInside then
            inside <- mid
            lastPick <- value
        else
            outside <- mid
        
        if step >= maxStep || outside - inside <= eps then
            (inside, outside), lastPick
        else
        
        if isInside = lastInside then
            mid <-
                if isInside then
                    inside * 0.25f + outside * 0.75f
                else
                    inside * 0.75f + outside * 0.25f
        else
            mid <- inside * 0.5f + outside * 0.5f
        lastInside <- isInside
        update (step + 1)
    
    update 0

/// check(0) is expected to be true, while check(1) = false is also expected
/// but not necessary. returns inside(check = true), outside(check = false)
let binarySearch maxStep eps check =
    binarySearchAndPick maxStep eps (fun x -> if check x then Some () else None)
    |> fst

/// Generate a quad crossing with (0,0), (center, maxHeight), (1, finalHeight)
let inline createUnitQuad (maxHeight: ^a) (finalHeight : ^a) (center : ^a) =
    let zero = LanguagePrimitives.GenericZero< ^a>
    let one = LanguagePrimitives.GenericOne< ^a>
    if center < zero || center > one then
        failwith "center should be between 0 and 1"
    
    let a = maxHeight;
    let b = finalHeight;
    let c = center;
    
    (* solve
        c^2x + cy = a,
        x + y = b
    *)
        
    let cm1c = (c - one) * c;
    let bc = b * c;
    
    let x = (a - bc) / cm1c;
    let y = (bc * c - a) / cm1c;
    
    fun (p : ^a) -> p * p * x + p * y

let unitQuad = createUnitQuad 1.0 0.0 0.5
let unitQuadf = createUnitQuad 1.0f 0.0f 0.5f

let inline flip (flag: bool) (value : ^a) =
    if flag then
        LanguagePrimitives.GenericZero< ^a> - value
    else
        value

let partitionInt (total : int) (count : int) =
    let r = total / count
    let b = total % count
    [ for i in 1 .. count do yield if i <= b then r + 1 else r ]
    
let partitionIntRandom (total : int) (count : int) =
    partitionInt total count
    |> List.randomShuffle