

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

public delegate void BotPlayCardEventHandler(object sender, int cardId);

public class Bot
{
    // Declare C# events
    public delegate void BotBidEventHandler(object sender, Bid bid);
    public static event BotBidEventHandler BotBid;

    public delegate void BotDiscardsEventHandler(object sender, List<Card> cards);
    public static event BotDiscardsEventHandler BotDiscards;

    public delegate void BotTrumpSuitEventHandler(object sender, Suit suit);
    public static event BotTrumpSuitEventHandler BotTrumpSuit;


    public static event BotPlayCardEventHandler BotPlayCard;

    public Suit StrongestSuit(List<Card> cards)
    {
        var suits = new List<Suit>([Suit.Club, Suit.Diamond, Suit.Heart, Suit.Spade]);
        var bestSuitScore = 0;
        var bestSuit = Suit.Club;

        foreach (var suit in suits)
        {
            var suitScore = 0;

            foreach (var card in cards)
            {
                if (card.Suit == suit)
                {
                    suitScore += card.Rank;
                }
            }
            if (suitScore > bestSuitScore)
            {
                bestSuitScore = suitScore;
                bestSuit = suit;
            }
        }

        return bestSuit;
    }

    Card LowestNonTrumpCard(List<Card> cards, Suit trumpSuit)
    {
        var lowestRank = 99;
        Card lowestCard = null;

        foreach (var card in cards)
        {
            if (card.Eligible != 1) continue;
            if (card.Suit != trumpSuit && card.Rank < lowestRank)
            {
                lowestRank = card.Rank;
                lowestCard = card;
            }
        }
        return lowestCard;
    }

    public void Bid(Game gameCopy)
    {
        var bid = ChooseBidSimple(gameCopy);
        BotBid?.Invoke(this, bid);
    }

    Bid ChooseBidSimple(Game gameCopy)
    {
        var minBid = gameCopy.MinCurrentBid();
        var maxBid = gameCopy.PointsInHand;

        var hand = gameCopy.ActiveHand();

        /////////////////////////////// 0  1  2  3  4  5  6  7  8  9 10 11 12  13  14  15
        var rankValues = new List<int>([0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 10, 15, 20]);

        var bidPoints = 60; // base bid

        foreach (var card in hand)
        {
            bidPoints += rankValues[card.Rank];
        }

        // Round to 10.
        bidPoints = bidPoints / 10 * 10;

        Bid bid;
        if (bidPoints < minBid)
        {
            bid = new Bid(BidKind.Pass, 0, Suit.None);
        }
        else if (bidPoints > maxBid)
        {
            bid = new Bid(BidKind.Points, maxBid, Suit.None);
        }
        else
        {
            bid = new Bid(BidKind.Points, bidPoints, Suit.None);
        }
        return bid;
    }

    public void ChooseDiscards(Game gameCopy)
    {
        var discards = ChooseDiscardsSimple(gameCopy);
        BotDiscards?.Invoke(this, discards);
    }

    List<Card> ChooseDiscardsSimple(Game gameCopy)
    {
        var hand = gameCopy.ActiveHand();
        var trumpSuit = gameCopy.TrumpSuit;
        var discards = new List<Card>();

        while (discards.Count < gameCopy.Settings.ExchangeSize)
        {
            var lowestCard = LowestNonTrumpCard(hand, trumpSuit);
            // Okay to misuse Eligible here since we are working with
            // a Game copy. LowestNonTrumpCard skips ineligibles.
            if (lowestCard is Card card)
            {
                card.Eligible = 0;
                discards.Add(card);
            }
        }
        return discards;
    }

    public void ChooseTrumpSuit(Game gameCopy)
    {
        var hand = gameCopy.ActiveHand();
        GD.Print("Choosing Trump, active: ", gameCopy.Active);
        var trumpSuit = StrongestSuit(hand);
        BotTrumpSuit?.Invoke(this, trumpSuit);
    }

    public void GetPlayCard(Game gameCopy, int simulations)
    {
        var cardId = PlayCardMonte(gameCopy, simulations);
        BotPlayCard?.Invoke(this, cardId);

        // var playableIds = gameCopy.GetPlayableCardIds();
        // BotPlayCard?.Invoke(this, playableIds[0]);
    }

    int PlayCardMonte(Game gameCopy, int simulations)
    {
        var montePlayer = gameCopy.Active;
        var montePlayableIds = gameCopy.GetPlayableCardIds();

        int bestCardId = 0;
        int bestScore = int.MinValue;

        // Create a List holding all the cards this player doesn't know about
        // which means all the other hand cards, plus the nest (??).
        var hiddenCards = new List<Card>();
        var playerHandCounts = new List<int>();


        for (var p = 0; p < gameCopy.PlayerCount; p++)
        {
            playerHandCounts.Add(gameCopy.Players[p].Hand.Count);
            if (p == montePlayer) continue;
            while (gameCopy.Players[p].Hand.Count > 0)
            {
                var card = gameCopy.Players[p].Hand.Pop();
                hiddenCards.Add(card);
            }
        }

        // var nestCount = 0;
        // if (montePlayer != gameCopy.Maker)
        // {
        //     nestCount = gameCopy.Nest.Count;
        //     while (gameCopy.Nest.Count > 0)
        //     {
        //         var card = gameCopy.Nest.Pop();
        //         hiddenCards.Add(card);
        //     }
        // }

        GD.Print("P:", montePlayer, ", hiddenCards: ", hiddenCards.Count, " eligible: ", montePlayableIds.Count);

        var stopWatch = new Stopwatch();
        stopWatch.Start();

        foreach (var cardId in montePlayableIds)
        {
            var simScore = 0;

            for (var i = 0; i < simulations; i++)
            {
                var simGame = gameCopy.DeepCopy();

                // Assign random cards to all players but the active player.
                hiddenCards.Shuffle();
                var hiddenIdx = 0;

                for (var p = 0; p < gameCopy.PlayerCount; p++)
                {
                    if (p == montePlayer) continue;

                    for (var j = 0; j < playerHandCounts[p]; j++)
                    {
                        simGame.Players[p].Hand.Add(hiddenCards[hiddenIdx]);
                        hiddenIdx += 1;
                    }
                }
                // The nest
                // if (montePlayer != gameCopy.Maker)
                // {
                //     for (var j = 0; j < nestCount; j++)
                //     {
                //         simGame.Nest.Add(hiddenCards[hiddenIdx].DeepCopy());
                //         hiddenIdx += 1;
                //     }
                // }

                simGame.PlayCardId(cardId); // advances player

                while (!simGame.HandCompleted())
                {
                    if (simGame.Trick.Completed())
                    {
                        simGame.AwardTrick();
                        simGame.ResetForNextTrick();
                    }

                    var ids = simGame.GetPlayableCardIds();
                    var randIdx = new Random().Next(ids.Count);
                    simGame.PlayCardId(ids[randIdx]);
                }

                simGame.AwardNestCards();
                simGame.CompleteHand();

                if (montePlayer == 0 || montePlayer == 2)
                {
                    simScore += simGame.WeHandScore - simGame.TheyHandScore;
                }
                else
                {
                    simScore += simGame.TheyHandScore - simGame.WeHandScore;
                }
            }

            if (simScore > bestScore)
            {
                bestScore = simScore;
                bestCardId = cardId;
            }
        }

        stopWatch.Stop();
        TimeSpan ts = stopWatch.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
        ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
        GD.Print($"sims: {simulations}, ms: {ts.Milliseconds}");

        return bestCardId;
    }
}