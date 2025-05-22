using System;
using System.Collections.Generic;

public class Trick
{
    int PlayerCount;
    int CardsPlayed = 0;
    public List<Card> Cards = [];
    public Card LeadCard;
    public int Winner = -1;
    public int Points;

    public Trick(int playerCount)
    {
        PlayerCount = playerCount;
        for (var p = 0; p < playerCount; p++)
        {
            Cards.Add(null);
        }
    }

    public Trick DeepCopy()
    {
        var copy = (Trick)this.MemberwiseClone();
        
        copy.Cards = [];
        foreach (var card in Cards)
        {
            if (card != null)
            {
                copy.Cards.Add(card.DeepCopy());
            }
            else
            {
                copy.Cards.Add(null);
            }
        }
        if (LeadCard != null)
        {
            copy.LeadCard = LeadCard.DeepCopy();
        }
        return copy;
    }

    public void Reset()
    {
        CardsPlayed = 0;
        for (var i = 0; i < PlayerCount; i++)
        {
            Cards[i] = null;
        }
        LeadCard = null;
        Winner = -1;
        Points = 0;
    }

    public bool IsEmpty()
    {
        return CardsPlayed == 0;
    }

    public bool Completed()
    {
        return CardsPlayed == PlayerCount;
    }

    public void Add(int player, Card card, Suit trumpSuit)
    {
        if (CardsPlayed == 0)
        {
            // This is the lead card.
            LeadCard = card;
            Winner = player;
        }
        else
        {
            if (Cards[Winner] is Card winningCard)
            {
                // Trump suit
                if (trumpSuit != Suit.None)
                {
                    if (winningCard.Suit == trumpSuit)
                    {
                        // !!! Note the sign below to determine ties in the case of
                        // jokers.
                        if (card.Suit == trumpSuit && card.Rank > winningCard.Rank)
                        {
                            Winner = player;
                        }
                    }
                    else
                    {
                        // Winning card is not trump.
                        if (card.Suit == trumpSuit)
                        {
                            Winner = player;
                        }
                        else
                        {
                            if (card.Suit == winningCard.Suit && card.Rank > winningCard.Rank)
                            {
                                Winner = player;
                            }
                        }
                    }
                }
                else
                {
                    // Hand does not have a trump suit.
                    if (card.Suit == winningCard.Suit && card.Rank > winningCard.Rank)
                    {
                        Winner = player;
                    }
                }
            }
        }
        Points += card.Points;
        Cards[player] = card;
        CardsPlayed += 1;
    }
}
