using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Godot;

struct CardClickDatum
{
    public int Id;
    public int ZIndex;
    public int Eligible;
}

public partial class View : CanvasLayer
{
    Sprite2D activePlayer;
    Control scoreTable;
    BidPanel bidPanel;
    List<BidMarker> bidMarkers = [];
    Panel trumpChooser;
    TrumpMarker trumpMarker;
    Label messageLabel;
    Button doneButton;

    // Holds a ref to the card nodes for easy searching.
    List<CardNode> cardNodes = [];

    // See OnCardNodeClicked() and _Process().
    List<CardClickDatum> cardClickedData = [];

    // Declare C# events (not Godot) for the Controller to subscribe to.
    public delegate void CardClickedEventHandler(object sender, int id);
    public static event CardClickedEventHandler CardClicked;

    public delegate void HumanBidMadeEventHandler(object sender, Bid bid);
    public static event HumanBidMadeEventHandler HumanBidMade;

    public View()
    {
    }

    public override void _Ready()
    {
        CardNode.CardNodeClicked += OnCardNodeClicked;

        GD.Print("View _Ready");
        activePlayer = GetNode<Sprite2D>("GUI/ActivePlayer");

        scoreTable = GetNode<Control>("GUI/ScoreTable");

        bidPanel = GetNode<BidPanel>("GUI/BidPanel");
        bidPanel.Visible = false;

        trumpChooser = GetNode<Panel>("GUI/TrumpChooser");
        trumpChooser.Visible = false;

        trumpMarker = GetNode<TrumpMarker>("GUI/TrumpMarker");
        trumpMarker.Visible = false;

        messageLabel = GetNode<Label>("GUI/MessageLabel");
        messageLabel.Visible = false;

        doneButton = GetNode<Button>("GUI/DoneButton");
        doneButton.Visible = false;

        // BidMarkers will be created when needed.

    }

    public override void _Process(double delta)
    {
        if (cardClickedData.Count > 0)
        {
            GD.Print("count: ", cardClickedData.Count);
            // Sort to find the top one.
            cardClickedData.Sort((card1, card2) => card1.ZIndex.CompareTo(card2.ZIndex));
            var topCard = cardClickedData.Last();
            if (topCard.Eligible == 1) // only sent the message if eligible
            {
                CardClicked.Invoke(this, topCard.Id); // picked up by Controller
            }
            cardClickedData.Clear();
        }
    }

    public override void _Input(InputEvent theEvent)
    {
    }

    public void CreateCardView(Card card)
    {
        var scene = GD.Load<PackedScene>("res://CardNode.tscn");
        var cardNode = scene.Instantiate() as CardNode;
        cardNode.Setup(card);
        cardNode.ZIndex = card.Id;
        cardNode.Position = new Vector2(200f, 200f);
        cardNode.Eligible = -1;

        var cardLayer = GetNode<CanvasLayer>("CardLayer");
        cardLayer.AddChild(cardNode);
        cardNodes.Add(cardNode);
    }

    CardNode FindCardNode(int id)
    {
        // This seems faster/better than calling FindChild(...).
        foreach (var cardNode in cardNodes)
        {
            if (cardNode.Id == id) return cardNode;
        }
        GD.Print($"Could not find cardNode.Id: {id}");
        return null;
    }

    // Event handler for CardNode event.
    private void OnCardNodeClicked(object sender, int id)
    {
        var cardNode = FindCardNode(id);
        var data = new CardClickDatum { Id = id, ZIndex = cardNode.ZIndex, Eligible = cardNode.Eligible };
        cardClickedData.Add(data);
    }

    public void UpdateInfo(Game game)
    {
        // active player marker
        activePlayer.Visible = true;
        var geom = ViewGeom.TurnMarkerGeom(game.Active, game.PlayerCount);
        activePlayer.Position = geom.Pos;

        // score table
        var weHand = GetNode<Label>("GUI/ScoreTable/HBoxContainer/WeColumn/Hand");
        weHand.Text = game.Scoring.WeHandScore().ToString();

        var theyHand = GetNode<Label>("GUI/ScoreTable/HBoxContainer/TheyColumn/Hand");
        theyHand.Text = game.Scoring.TheyHandScore().ToString();

        var weGame = GetNode<Label>("GUI/ScoreTable/HBoxContainer/WeColumn/Game");
        weGame.Text = game.Scoring.WeGameScore().ToString();

        var theyGame = GetNode<Label>("GUI/ScoreTable/HBoxContainer/TheyColumn/Game");
        theyGame.Text = game.Scoring.TheyGameScore().ToString();
    }

    internal void UpdateMessage(string message)
    {
        if (message == "")
        {
            messageLabel.Visible = false;
        }
        else
        {
            messageLabel.Visible = true;
            messageLabel.Text = message;
        }
    }

    void UpdateCard(Card card, ViewGeom geom)
    {
        var cardNode = FindCardNode(card.Id);
        cardNode.Eligible = card.Eligible;
        cardNode.SetFaceUp(card.FaceUp);
        cardNode.ZIndex = geom.Z;
        TweenCardPosition(cardNode, geom.Pos, 0.3f);
        TweenCardRotation(cardNode, geom.Rot, 0.3f);
    }

    void TweenCardPosition(CardNode cardNode, Vector2 newPos, float duration)
    {
        var tween = GetTree().CreateTween();
        tween.SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(cardNode, "position", newPos, duration).FromCurrent();
    }

    void TweenCardRotation(CardNode cardNode, float newRot, float duration)
    {
        var tween = GetTree().CreateTween();
        tween.SetEase(Tween.EaseType.InOut);
        // tween.TweenProperty(cardNode, "rotation", newRot, duration).FromCurrent();
        tween.Parallel().TweenProperty(cardNode, "rotation", newRot, duration).FromCurrent();
    }

    public void UpdateDeck(Game game)
    {
        foreach (var card in game.Deck)
        {
            var geom = ViewGeom.DeckGeom(-1, game.PlayerCount, 0);
            UpdateCard(card, geom);
        }
    }

    internal void UpdateHand(Game game, int player)
    {
        var cards = game.Players[player].Hand;
        var isBot = game.Players[player].IsBot;
        for (var i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            var geom = ViewGeom.HandCardGeom(player, i, cards.Count, game.PlayerCount, isBot);
            UpdateCard(card, geom);
        }
    }

    internal void ResetEligibility(List<Card> cards)
    {
        foreach (var card in cards)
        {
            var cardNode = FindCardNode(card.Id);
            cardNode.Eligible = -1;
        }
    }

    internal void UpdateExchange(Game game)
    {
        var cards = game.Exchange;
        for (var i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            var geom = ViewGeom.ExchangeGeom(i, cards.Count);
            UpdateCard(card, geom);
        }
    }

    internal void UpdateNest(Game game, bool v)
    {
        var cards = game.Nest;
        for (var i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            var geom = ViewGeom.NestGeom(i, cards.Count);
            UpdateCard(card, geom);
        }
    }

    internal void UpdateTrick(Game game)
    {
        var cards = game.Trick.Cards;
        for (var i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            if (card != null)
            {
                var geom = ViewGeom.TrickCardGeom(i, game.PlayerCount);
                UpdateCard(card, geom);
            }
        }
    }

    internal void UpdateTaken(Game game, int player)
    {
        var cards = game.Players[player].Taken;
        for (var i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            var geom = ViewGeom.TakenGeom(player, game.PlayerCount);
            UpdateCard(card, geom);
        }
    }

    internal void UpdateBids(Game game)
    {
        // Create and position the markers as needed.
        if (bidMarkers.Count == 0)
        {
            var layer = GetNode<CanvasLayer>("GUI");
            var scene = GD.Load<PackedScene>("res://BidMarker.tscn");
            for (var p = 0; p < game.PlayerCount; p++)
            {
                var marker = scene.Instantiate() as BidMarker;
                var geom = ViewGeom.BidMarkerGeom(p, game.PlayerCount);
                marker.Position = geom.Pos;
                bidMarkers.Add(marker);
                layer.AddChild(marker);
            }
        }

        for (var p = 0; p < game.PlayerCount; p++)
        {
            var bid = game.Players[p].Bid;
            switch (bid.Kind)
            {
                case BidKind.None:
                    bidMarkers[p].Visible = false;
                    break;
                default:
                    bidMarkers[p].Visible = true;
                    bidMarkers[p].SetBid(bid);
                    break;
            }
        }
    }

    internal void HideBidsExceptMaker(Game game)
    {
        for (var p = 0; p < game.PlayerCount; p++)
        {
            bidMarkers[p].Visible = p == game.Maker;
        }
    }

    public void HideBidMarker(int player) {
        bidMarkers[player].Visible = false;
    }

    internal void GetHumanBid(Game game)
    {
        HideBidMarker(game.Active);
        bidPanel.Visible = true;
        bidPanel.SetMinMaxPoints(game.MinCurrentBid(), game.PointsInHand);
    }

    internal void EndHumanBid()
    {
        bidPanel.Visible = false;
    }

    public void OnPassButton()
    {
        GD.Print("OnPassButton");
        var bid = new Bid(BidKind.Pass, 0, Suit.None);
        HumanBidMade.Invoke(this, bid);
    }

    public void OnBidButton()
    {
        GD.Print("OnBidButtonPressed");
        var bid = new Bid();
        bid.Kind = BidKind.Points;
        bid.Points = bidPanel.BidValue();
        bid.Suit = Suit.None;
        HumanBidMade.Invoke(this, bid);
    }

    internal void ShowTrumpChooser(bool visible)
    {
        trumpChooser.Visible = visible;
        // Update message... "Select trump suit"
    }

    internal void ShowTrumpMarker(bool visible)
    {
        trumpMarker.Visible = visible;
    }

    internal void SetTrumpSuit(Suit suit)
    {
        trumpMarker.SetSuit(suit);
    }





    internal void ShowNextHandButton(bool v)
    {

    }



    internal void GetBotDiscards(Game game)
    {
        // Showing thinking icon?
    }

    internal void GetHumanExchanges(Game game)
    {
        var count = game.Settings.ExchangeSize;
        messageLabel.Text = $"Exchange {count} cards.";
    }

    internal void ShowDoneExchangingButton(bool visible)
    {
        doneButton.Visible = visible;
    }

    internal void SetDiscardableHandCards(Game game)
    {
        //
    }

    internal void GetBotTrump(Game game)
    {
        //
    }

    internal void GetBotCardPlay(Game game)
    {
        //
    }

    internal void GetHumanCardPlay(Game game)
    {
        GD.Print("GetHumanCardPlay");
        UpdateHand(game, game.Active);
    }


}
