namespace Moon.Interface

type IPlatformShape =
    abstract member OneWayCollision : bool with get, set
    abstract member OneWayCollisionMargin : float32 with get, set