using System;
using System.Collections.Generic;

public enum TeamKind
{
    Unassigned,
    Makers,
    Defenders
}

public class Player
{
    public bool IsBot;
    public TeamKind Team = TeamKind.Unassigned;
    public int HandPoints;
    public int GameScore;
    public bool HasPassed;
    public Bid Bid;
    public List<Card> Hand = [];
    public List<Card> Taken = [];

    public Player(bool isBot)
    {
        IsBot = isBot;
    }

    public Player DeepCopy()
    {
        var copy = (Player)this.MemberwiseClone();

        copy.Hand = [];
        foreach (var card in Hand)
        {
            copy.Hand.Add(card.DeepCopy());
        }
        copy.Taken = [];
        foreach (var card in Taken)
        {
            copy.Taken.Add(card.DeepCopy());
        }
        return copy;
    }

    public void ResetForNewHand()
    {
        Team = TeamKind.Unassigned;
        HandPoints = 0;
        HasPassed = false;
        Bid = new Bid(BidKind.None, 0, Suit.None);
        Hand.Clear();
        Taken.Clear();
    }
}
