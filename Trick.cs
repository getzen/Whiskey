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

    public Card WinningCard()
    {
        if (Winner != -1)
        {
            return Cards[Winner];
        }
        return null;
    }

    public bool Completed()
    {
        return CardsPlayed == PlayerCount;
    }

    public void Add(int player, Card card, Suit trumpSuit, JokerKind jokerKind)
    {
        Points += card.Points;
        Cards[player] = card;
        CardsPlayed += 1;

        switch (jokerKind)
        {
            case JokerKind.Trump:
                if (LeadCard == null)
                {
                    LeadCard = card;
                    Winner = player;
                    return;
                }
                break;

            case JokerKind.Phoenix:
                if (LeadCard == null)
                {
                    if (card.Suit == Suit.Joker)
                    {
                        return; // do nothing
                    }

                    LeadCard = card;
                    Winner = player;
                    return;
                }
                // Not the lead card
                if (card.Suit == Suit.Joker)
                {
                    // Adopt rank and suit of winning card.
                    card.Rank = Cards[Winner].Rank;
                    card.Suit = Cards[Winner].Suit;
                }
                break;
        }

        var winningCard = Cards[Winner];
                
        // Trump suit
        if (trumpSuit != Suit.None)
        {
            if (winningCard.Suit == trumpSuit)
            {
                // !!! Note the >= sign below.
                if (card.Suit == trumpSuit && card.Rank >= winningCard.Rank)
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
                    // !!! Note the >= sign below.
                    if (card.Suit == winningCard.Suit && card.Rank >= winningCard.Rank)
                    {
                        Winner = player;
                    }
                }
            }
        }
        else
        {
            // Hand does not have a trump suit.
            if (card.Suit == winningCard.Suit && card.Rank >= winningCard.Rank)
            {
                Winner = player;
            }
        }
        
    }

    // public void AddForPhoenixJokers(int player, Card card, Suit trumpSuit)
    // {
        
    //     var winningCard = Cards[Winner];

    //     // Not the lead card.
    //     if (card.Suit == Suit.Joker)
    //     {
    //         // Takes the lead.
    //         Winner = player;
    //         card.Rank = winningCard.Rank;
    //         card.Suit = winningCard.Suit;
    //         return;
    //     }
        

    // }

    // public void AddForTrumpJokers(int player, Card card, Suit trumpSuit)
    // {
    //     var winningCard = Cards[Winner];

    //     // Trump suit
    //     if (trumpSuit != Suit.None)
    //     {
    //         if (winningCard.Suit == trumpSuit)
    //         {
    //             // !!! Note the sign below to determine ties in the case of
    //             // jokers.
    //             if (card.Suit == trumpSuit && card.Rank > winningCard.Rank)
    //             {
    //                 Winner = player;
    //             }
    //         }
    //         else
    //         {
    //             // Winning card is not trump.
    //             if (card.Suit == trumpSuit)
    //             {
    //                 Winner = player;
    //             }
    //             else
    //             {
    //                 if (card.Suit == winningCard.Suit && card.Rank > winningCard.Rank)
    //                 {
    //                     Winner = player;
    //                 }
    //             }
    //         }
    //     }
    //     else
    //     {
    //         // Hand does not have a trump suit.
    //         if (card.Suit == winningCard.Suit && card.Rank > winningCard.Rank)
    //         {
    //             Winner = player;
    //         }
    //     }
    // }
}
