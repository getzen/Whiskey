
public enum JokerKind
{
    // "Normal" joker behavior: it's a member of the trump suit for the hand
    // and maintains its fixed rank.
    Trump,
    // Kinda like the Phoenix in Tichu: it has a rank of zero when led and does not
    // set the lead suit (the next first suited card does). Cannot be played to an
    // ongoing trick unless void of the lead suit, but when played it takes on the
    // suit of the then-winning card and its rank is set slightly (0.1) higher.
    Phoenix
}

public struct GameSettings
{
    public int PlayerCount = 4;
    public int HandSize = 9;
    public int ExchangeSize = 2;
    public int NestSize = 0;
    public JokerKind JokerKind = JokerKind.Phoenix;

    // (Rank, Points)
    public (int, int)[] Clubs;
    public (int, int)[] Diamonds;
    public (int, int)[] Hearts;
    public (int, int)[] Spades;
    public (int, int)[] Jokers;

    public GameSettings()
    {
        // 6s omitted
        Clubs = [(5, 5), (7, 0), (8, 0), (9, 0), (10, 10),
        (11, 0), (12, 0), (13, 0), (14, 10)];

        Diamonds = [(5, 5), (7, 0), (8, 0), (9, 0), (10, 10),
        (11, 0), (12, 0), (13, 0), (14, 10)];

        Hearts = [(5, 5), (7, 0), (8, 0), (9, 0), (10, 10),
        (11, 0), (12, 0), (13, 0), (14, 10)];

        Spades = [(5, 5), (7, 0), (8, 0), (9, 0), (10, 10),
        (11, 0), (12, 0), (13, 0), (14, 10)];

        Jokers = [(0, 10), (0, 10)];
    }
}