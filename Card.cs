using System;

public enum Suit
{
    // 'None' is here for a No-Trump bid.
    None, Club, Diamond, Heart, Spade, Joker,
}

public class Card
{
    public int Id;
    public Suit Suit;
    public int Rank;
    public int Points;
    public bool IsJoker;
    public bool FaceUp;
    // -1 = N/A, 0 = ineligible, 1 = eligible
    public int Eligible;

    public Card(int id, Suit suit, int rank, int points)
    {
        Id = id;
        Suit = suit;
        Rank = rank;
        Points = points;
        Eligible = -1;
        if (suit == Suit.Joker)
        {
            IsJoker = true;
        }
    }

    public Card DeepCopy() {
        var copy = (Card)this.MemberwiseClone();
        return copy;
    }
    
    public int SortOrder()
    {
        int[] suitOrder = [0, 20, 40, 60, 80, 100];
        return Rank + suitOrder[(int)Suit];
    }

    public override string ToString()
    {
        var rank = Rank.ToString();
        switch (Rank)
        {
            case 11:
                rank = "J";
                break;

            case 12:
                rank = "Q";
                break;
            
            case 13:
                rank = "K";
                break;

            case 14:
                rank = "A";
                break;
        }
        
        var suit = "";
        switch (Suit)
        {
            case Suit.Club:
                suit = "c"; //♣️
                break;

            case Suit.Diamond:
                suit = "d"; //♦️
                break;

            case Suit.Heart:
                suit = "h"; //♥️
                break;

            case Suit.Spade:
                suit = "s"; //♠️
            break;

            case Suit.Joker:
                suit = "jkr";
                break;
        }
        return rank + suit;
    }
}
