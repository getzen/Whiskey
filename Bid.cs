
public enum BidKind
{
    None,
    Pass,
    Points
}

public struct Bid
{
    public BidKind Kind;
    public int Points;
    public Suit Suit;

    public Bid(BidKind kind, int points, Suit suit)
    {
        Kind = kind;
        Points = points;
        Suit = suit;
    }
}
