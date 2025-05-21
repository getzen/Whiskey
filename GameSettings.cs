using System.Collections.Generic;

public struct GameSettings
{
    public int PlayerCount = 4;
    public int HandSize = 9;
    public int ExchangeSize = 2;
    public int NestSize = 0;

    // (Rank, Points)
    public (int, int)[] Clubs;
    public (int, int)[] Diamonds;
    public (int, int)[] Hearts;
    public (int, int)[] Spades;
    public (int, int)[] Jokers;

    public GameSettings()
    {
        // 6s omitted
        Clubs = [(5, 10), (7, 0), (8, 0), (9, 0), (10, 10),
        (11, 0), (12, 0), (13, 10), (14, 10)];

        Diamonds = [(5, 10), (7, 0), (8, 0), (9, 0), (10, 10),
        (11, 0), (12, 0), (13, 10), (14, 10)];

        Hearts = [(5, 10), (7, 0), (8, 0), (9, 0), (10, 10),
        (11, 0), (12, 0), (13, 10), (14, 10)];

        Spades = [(5, 10), (7, 0), (8, 0), (9, 0), (10, 10),
        (11, 0), (12, 0), (13, 10), (14, 10)];

        Jokers = [(15, 0), (15, 0)];
    }
}