module Moon.Utils.Math

/// returns inside(check = true), outside(check = false)
let binarySearch maxStep check =
    let mutable inside = 0f
    let mutable outside = 1f

    while check outside do
        inside <- outside
        outside <- outside * 2f
    
    for _ in 1 .. maxStep do
        let mid = inside * 0.5f + outside * 0.5f
        
        if check mid then
            inside <- mid
        else
            outside <- mid
    
    inside, outside

/// Generate a quad crossing with (0,0), (center, maxHeight), (1, finalHeight)
let inline unitQuad (maxHeight: ^a) (finalHeight : ^a) (center : ^a) =
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