using Godot;
using System;
using System.Collections.Generic;

public class Game
{
	public GameSettings Settings;
	public int PlayerCount;

	public Scoring Scoring = new Scoring(300);

	public List<Card> Deck = [];
	public List<Card> Exchange = [];
	public List<Card> Nest = [];
	public List<Player> Players = [];

	public int Dealer;
	public int Active;
	public int Maker;
	public Bid? HighBid;
	public Suit TrumpSuit;
	public Trick Trick;
	public int PointsInHand;
	public int LastTrickWinner;

	public int HandCardsToDeal;
	public int ExchangeCardsToDeal;
	public int NestCardsToDeal;

	public Game(GameSettings settings)
	{
		Settings = settings;
		PlayerCount = settings.PlayerCount;
		for (var p = 0; p < PlayerCount; p++)
		{
			Players.Add(new Player(p > 0));
		}
		Dealer = new Random().Next(PlayerCount);

		Trick = new Trick(PlayerCount);
	}

	public Game DeepCopy()
	{
		// If Game were a struct, all value-type fields would be automatically copied with:
		// var clone = this;

		// Handy method to copy Array elements (as long as they don't contain refs themselves).
		// System.Array.Copy(SomeArray, clone.SomeArray, SomeArray.Length);

		var copy = (Game)this.MemberwiseClone();

		copy.Scoring = Scoring.DeepCopy();

		// The '[]' creates a new list, which we need.
		// It's not the same as copy.Deck.Clear().
		copy.Deck = [];
		foreach (var card in Deck)
		{
			copy.Deck.Add(card.DeepCopy());
		}
		copy.Exchange = [];
		foreach (var card in Exchange)
		{
			copy.Exchange.Add(card.DeepCopy());
		}
		copy.Nest = [];
		foreach (var card in Nest)
		{
			copy.Nest.Add(card.DeepCopy());
		}
		copy.Players = [];
		foreach (var player in Players)
		{
			copy.Players.Add(player.DeepCopy());
		}
		copy.Trick = Trick.DeepCopy();

		return copy;
	}

	public bool ActiveIsBot()
	{
		return Players[Active].IsBot;
	}

	public List<Card> ActiveHand()
	{
		return Players[Active].Hand;
	}

	public void NextPlayer()
	{
		Active = (Active + 1) % PlayerCount;
	}

	void AddCardToDeck(int id, Suit suit, int rank, int points)
	{
		var card = new Card(id, suit, rank, points);
		Deck.Add(card);
	}


	public void CreateDeck()
	{
		int id = 0;
		int cardCount = 0;
		PointsInHand = 0;

		foreach (var item in Settings.Clubs)
		{
			AddCardToDeck(id, Suit.Club, item.Item1, item.Item2);
			id++; cardCount += 1; PointsInHand += item.Item2; // forgive me
		}
		foreach (var item in Settings.Diamonds)
		{
			AddCardToDeck(id, Suit.Diamond, item.Item1, item.Item2);
			id++; cardCount += 1; PointsInHand += item.Item2;
		}
		foreach (var item in Settings.Hearts)
		{
			AddCardToDeck(id, Suit.Heart, item.Item1, item.Item2);
			id++; cardCount += 1; PointsInHand += item.Item2;
		}
		foreach (var item in Settings.Spades)
		{
			AddCardToDeck(id, Suit.Spade, item.Item1, item.Item2);
			id++; cardCount += 1; PointsInHand += item.Item2;
		}
		foreach (var item in Settings.Jokers)
		{
			AddCardToDeck(id, Suit.Joker, item.Item1, item.Item2);
			id++; cardCount += 1; PointsInHand += item.Item2;
		}
		GD.Print($"Card count: {cardCount}. Points in hand: {PointsInHand}");
	}

	public void ResetForNewHand()
	{
		foreach (var player in Players)
		{
			Deck.AddRange(player.Hand);
			Deck.AddRange(player.Taken);
			player.ResetForNewHand();
		}

		Deck.AddRange(Nest);

		foreach (Card card in Deck)
		{
			card.FaceUp = false;
		}
		SetJokerSuit(Deck, Suit.Joker);
		Deck.Shuffle();

		Dealer = (Dealer + 1) % PlayerCount;
		Active = Dealer;
		Maker = -1;
		TrumpSuit = Suit.None;
		HighBid = null;
		Trick.Reset();

		HandCardsToDeal = Settings.PlayerCount * Settings.HandSize;
		ExchangeCardsToDeal = Settings.ExchangeSize;
		NestCardsToDeal = Settings.NestSize;
	}

	void SetJokerSuit(List<Card> cards, Suit suit)
	{
		foreach (Card card in cards)
		{
			if (card.IsJoker) card.Suit = suit;
		}
	}

	public void SetTrumpSuit(Suit suit)
	{
		TrumpSuit = suit;
		if (Settings.JokerKind == JokerKind.Trump)
		{
			for (var p = 0; p < PlayerCount; p++)
			{
				SetJokerSuit(Players[p].Hand, suit);
				if (!Players[p].IsBot) SortHand(p);
			}
		}
	}

	public void DealCardToHand()
	{
		var card = Deck.Pop();
		//card.FaceUp = !ActiveIsBot(); /////////////////
		card.FaceUp = true;
		ActiveHand().Add(card);
		if (!ActiveIsBot())
		{
			SortHand(Active);
		}
	}

	public void DealCardToExchange()
	{
		var card = Deck.Pop();
		card.FaceUp = true; ///////////////////////////
		Exchange.Add(card);
	}

	public void DealCardToNest()
	{
		var card = Deck.Pop();
		Nest.Add(card);
	}

	public void SetFirstBidder()
	{
		Active = Dealer;
		NextPlayer();
	}

	public void SortHand(int p)
	{
		var hand = Players[p].Hand;
		hand.Sort((x, y) => x.SortOrder().CompareTo(y.SortOrder()));
	}

	public int MinCurrentBid()
	{
		if (HighBid is Bid highBid)
		{
			return highBid.Points + 5;
		}
		else
		{
			return 80; // need to read from options
		}
	}

	public void MakeBid(Bid bid)
	{
		Players[Active].Bid = bid;
		if (bid.Kind == BidKind.Pass)
		{
			Players[Active].HasPassed = true;
		}
		else
		{
			HighBid = bid;
			Maker = Active;
		}
	}

	public void AdvanceToNextBiddingPlayer()
	{
		while (true)
		{
			NextPlayer();
			if (!Players[Active].HasPassed)
			{
				break;
			}
		}
	}

	public bool BiddingCompleted()
	{
		var bids = 0;
		var passes = 0;

		foreach (var player in Players)
		{
			switch (player.Bid.Kind)
			{
				case BidKind.None: return false;
				case BidKind.Pass: passes += 1; break;
				case BidKind.Points: bids += 1; break;
			}
		}
		return bids == 1;
	}

	public void EndBidding()
	{
		Active = Maker;
		var makerPartner = (Maker + 2) % PlayerCount;

		for (var p = 0; p < PlayerCount; p++)
		{
			if (p == Maker || p == makerPartner)
			{
				Players[p].Team = TeamKind.Makers;
			}
			else
			{
				Players[p].Team = TeamKind.Defenders;
			}
		}
	}

	public bool ArePartners(int p1, int p2)
	{
		return Players[p1].Team == Players[p2].Team;
	}

	public void MoveExchangeCardsToMaker()
	{
		var isBot = Players[Maker].IsBot;
		while (Exchange.Count > 0)
		{
			var card = Exchange.Pop();
			card.FaceUp = true; ///////////////////////////////!isBot;
			Players[Maker].Hand.Add(card);
		}
		if (!isBot)
		{
			SortHand(Maker);
		}
	}

	public void MarkEligibleDiscards()
	{
		// See Rust version for different options.

		// All cards are eligible except the joker.
		foreach (var card in Players[Maker].Hand)
		{
			card.Eligible = card.IsJoker ? 0 : 1;
		}
	}

	public bool IsExchangeFull()
	{
		return Exchange.Count == Settings.ExchangeSize;
	}

	public void SwapWithExchange(int id)
	{
		var hand = Players[Maker].Hand;
		var idx = 0;
		// Check if hand card.
		idx = hand.FindIndex(card => card.Id == id);
		if (idx != -1)
		{
			if (!IsExchangeFull())
			{
				var card = hand[idx];
				hand.RemoveAt(idx);
				Exchange.Add(card);
			}

		}
		else
		{
			idx = Exchange.FindIndex(card => card.Id == id);
			if (idx != -1)
			{
				var card = Exchange[idx];
				Exchange.RemoveAt(idx);
				hand.Add(card);
				SortHand(Maker);
			}
		}
	}

	public void MoveExchangeToNest()
	{
		while (Exchange.Count > 0)
		{
			var card = Exchange.Pop();
			card.FaceUp = true;
			card.Eligible = -1;
			Nest.Add(card);
		}
	}

	public void SetFirstPlayer()
	{
		// Player after the maker.
		Active = Maker;
		NextPlayer();
	}

	public bool HasCardInLeadSuit()
	{
		if (Trick.LeadCard is Card leadCard)
		{
			foreach (var card in ActiveHand())
			{
				if (card.Suit == leadCard.Suit)
				{
					return true;
				}
			}
		}
		return false;
	}


	public List<int> GetPlayableCardIds()
	{
		var eligibleIds = new List<int>();
		var hasCardInLeadSuit = HasCardInLeadSuit();
		var trickIsEmpty = Trick.IsEmpty();

		var leadCard = Trick.LeadCard;

		foreach (var card in ActiveHand())
		{
			var eligible = 0;

			// Is this the first card to play or there are no matching cards in hand?
			if (trickIsEmpty || !hasCardInLeadSuit)
			{
				eligible = 1;
			}
			// Not the first card in play.
			else if (leadCard != null && card.Suit == leadCard.Suit)
			{
				eligible = 1;
			}

			if (eligible == 1) eligibleIds.Add(card.Id);

			card.Eligible = eligible;
		}
		return eligibleIds;
	}

	public void PlayCardId(int id)
	{
		var hand = ActiveHand();
		var idx = hand.FindIndex(card => card.Id == id);
		if (idx == -1)
		{
			GD.Print("******************************** Can't find id ", id);
			return;
		}
		var card = hand[idx];
		hand.RemoveAt(idx);
		card.FaceUp = true;
		card.Eligible = -1;
		Trick.Add(Active, card, TrumpSuit, Settings.JokerKind);
	}

	public void ResetHandEligibility(int p)
	{
		foreach (var card in Players[p].Hand)
		{
			card.Eligible = -1;
		}
	}

	public void AwardTrick()
	{
		LastTrickWinner = Trick.Winner;
		Scoring.AwardTrickPts(Trick.Points, Trick.Winner);

		foreach (var card in Trick.Cards)
		{
			if (card != null)
			{
				card.FaceUp = false;
				Players[Trick.Winner].Taken.Add(card);
			}
			else
			{
				GD.Print("Card is null ***");
			}
		}
	}

	public void ResetForNextTrick()
	{
		Active = LastTrickWinner;
		Trick.Reset();
	}

	public bool HandCompleted()
	{
		return Players[Active].Hand.Count == 0;
	}

	public int AwardNestCards()
	{
		var points = 0;
		foreach (var card in Nest)
		{
			card.FaceUp = true;
			points += card.Points;
		}
		Scoring.AwardNestPts(points, LastTrickWinner);
		return points;
	}

	public void CompleteHand()
	{
		var theBid = Players[Maker].Bid.Points;

		var bidders = Maker switch
		{
			0 => (0, 2),
			1 => (1, 3),
			2 => (0, 2),
			3 => (1, 3),
			_ => (-1, -1),
		};
		var bidderPts = Scoring.CombinedHandScoreFor(bidders.Item1, bidders.Item2);

		var defenders = bidders switch
		{
			(0, 2) => (1, 3),
			(1, 3) => (0, 2),
			_ => (-1, -1),
		};
		var defenderPts = Scoring.CombinedHandScoreFor(defenders.Item1, defenders.Item2);

		if (bidderPts >= theBid)
		{
			// Bid success
			Scoring.AwardGamePts(theBid, bidders.Item1);
			Scoring.AwardGamePts(theBid, bidders.Item2);

			Scoring.AwardGamePts(defenderPts / 1, defenders.Item1);
			Scoring.AwardGamePts(defenderPts / 1, defenders.Item2);
		}
		else
		{
			// Bid failure
			Scoring.AwardGamePts(defenderPts, defenders.Item1);
			Scoring.AwardGamePts(defenderPts, defenders.Item2);
		}
	}

	
}
