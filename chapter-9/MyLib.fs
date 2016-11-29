module MyLib

let add x y =
    x + y

let multiply x y =
    x * y

let tryDivide x y =
    if y = 0
    then None
    else Some(x / y)

type ICustomer =
    abstract Name: string
    abstract HasBoughtItem: itemId: int -> bool

/// Checks customer names do not contain spaces
let validateCustomer (customer: ICustomer) =
    customer.Name.IndexOf(" ") = -1