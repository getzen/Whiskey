using Godot;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public enum GameAction
{
    Setup,
    ResetForNewHand,
    DealToHands,
    DealToExchange,
    DealToNest,
    GetBid,
    BidMade,
    EndBidding,
    MoveNestToMaker,
    GetExchanges,
    Exchange,
    Discard,
    EndExchanging,
    GetTrump,
    TrumpChosen,
    GetCardPlay,
    PlayCard,
    AwardTrick,
    AwardNest,
    PresentScore
}

public partial class Controller : Node
{
    private Game Game;
    private View View;
    private GameAction? NextAction;
    private double DelayBeforeNextAction;

    public Controller()
    {
        GD.Print("Controller constructor.");
    }

    public override void _Ready()
    {
        Game = new Game(new GameSettings());
        View = GetNode<View>("View");
        NextAction = GameAction.Setup;
        DelayBeforeNextAction = 1.0;

        // Subscribe to events.
        // This one is set during HandleStateLogic:
        // View.CardClicked +=...;

        View.HumanBidMade += OnHumanBid;

        TrumpChooser.SuitClicked += OnSuitClicked;

        Bot.BotBid += OnBotBid;
        Bot.BotDiscards += OnBotDiscards;
        Bot.BotTrumpSuit += OnBotTrumpSuit;
        Bot.BotPlayCard += OnBotCardPlayed;

        // var timer = new Timer();
        // timer.Autostart = true;
        // timer.WaitTime = 0.1;
        // AddChild(timer);
        // timer.Timeout += HandleStateLogic;
    }

    public override void _Process(double timeDelta)
    {
        DelayBeforeNextAction = Math.Max(DelayBeforeNextAction - timeDelta, 0.0);
        if (DelayBeforeNextAction == 0.0)
        {
            HandleStateLogic();
        }
    }

    void HandleStateLogic()
    {
        if (NextAction is null) return;

        GD.Print($"Action: {NextAction}");

        switch (NextAction)
        {
            case GameAction.Setup:
                Game.CreateDeck();

                foreach (var card in Game.Deck)
                {
                    View.CreateCardView(card);
                }
                View.UpdateDeck(Game);
                View.UpdateMessage("Welcome to Whiskey");

                NextAction = GameAction.ResetForNewHand;
                DelayBeforeNextAction = 1.0;
                break;

            case GameAction.ResetForNewHand:
                Game.ResetForNewHand();
                View.UpdateMessage("ResetForNewHand");
                View.ShowNextHandButton(false);
                View.ShowTrumpMarker(false);
                View.UpdateInfo(Game);
                View.UpdateDeck(Game);

                NextAction = GameAction.DealToHands;
                break;

            case GameAction.DealToHands:
                //GD.Print("hand, to deal: ", Game.ActiveHand().Count, ", " + Game.HandCardsToDeal);
                if (Game.HandCardsToDeal > 0)
                {
                    Game.HandCardsToDeal--;
                    Game.DealCardToHand();
                    View.UpdateHand(Game, Game.Active);
                    Game.NextPlayer();
                    DelayBeforeNextAction = 0.1;
                }
                else
                {
                    NextAction = GameAction.DealToExchange;
                }
                break;

            case GameAction.DealToExchange:
                if (Game.ExchangeCardsToDeal > 0)
                {
                    Game.ExchangeCardsToDeal--;
                    Game.DealCardToExchange();
                    View.UpdateExchange(Game);
                    DelayBeforeNextAction = 0.1;
                }
                else
                {
                    DelayBeforeNextAction = 0.1;
                    NextAction = GameAction.DealToNest;
                }
                break;

            case GameAction.DealToNest:
                if (Game.NestCardsToDeal > 0)
                {
                    Game.NestCardsToDeal--;
                    Game.DealCardToNest();
                    View.UpdateNest(Game, true);
                    DelayBeforeNextAction = 0.1;
                }
                else
                {
                    Game.SetFirstBidder();
                    DelayBeforeNextAction = 0.5;
                    NextAction = GameAction.GetBid;
                }
                break;

            case GameAction.GetBid:
                View.UpdateInfo(Game);
                View.UpdateBids(Game);

                if (Game.ActiveIsBot())
                {
                    SpawnBidBot();
                }
                else
                {
                    View.GetHumanBid(Game);
                }
                NextAction = null;
                break;

            case GameAction.BidMade:
                View.UpdateInfo(Game);
                View.UpdateBids(Game);

                if (Game.BiddingCompleted())
                {
                    DelayBeforeNextAction = 0.5;
                    NextAction = GameAction.EndBidding;
                }
                else
                {
                    Game.AdvanceToNextBiddingPlayer();
                    // Maybe display a thinking icon here instead
                    // of hiding the last bid made.
                    View.HideBidMarker(Game.Active);
                    DelayBeforeNextAction = 1.0;
                    NextAction = GameAction.GetBid;
                }
                break;

            case GameAction.EndBidding:
                Game.EndBidding();
                View.HideBidsExceptMaker(Game);
                NextAction = GameAction.MoveNestToMaker;
                break;

            case GameAction.MoveNestToMaker:
                Game.MoveExchangeCardsToMaker();
                Game.MarkEligibleDiscards();
                View.UpdateHand(Game, Game.Maker);
                if (!Game.ActiveIsBot())
                {
                    View.SetDiscardableHandCards(Game);
                }
                View.UpdateInfo(Game);
                NextAction = GameAction.GetExchanges;
                break;

            case GameAction.GetExchanges:
                if (Game.ActiveIsBot())
                {
                    View.GetBotDiscards(Game);
                    SpawnDiscardBot();
                }
                else
                {
                    View.CardClicked += OnHumanCardExchanged;
                    View.GetHumanExchanges(Game);
                }
                DelayBeforeNextAction = 1.0;
                NextAction = null;
                break;

            case GameAction.Exchange:
                View.ShowDoneExchangingButton(Game.IsExchangeFull());

                View.UpdateHand(Game, Game.Maker);
                View.UpdateExchange(Game);

                NextAction = null;
                break;

            case GameAction.Discard:
                View.UpdateHand(Game, Game.Maker);
                View.UpdateExchange(Game);

                DelayBeforeNextAction = 1.0;
                NextAction = GameAction.EndExchanging;
                break;

            case GameAction.EndExchanging:
                Game.ResetHandEligibility(Game.Active);
                Game.MoveExchangeToNest();

                View.CardClicked -= OnHumanCardExchanged;
                var hand = Game.Players[Game.Maker].Hand;
                View.ResetEligibility(hand);
                View.ResetEligibility(Game.Nest);
                View.ShowDoneExchangingButton(false);
                View.UpdateExchange(Game);
                View.UpdateNest(Game, true);

                NextAction = GameAction.GetTrump;
                break;

            case GameAction.GetTrump:
                if (Game.ActiveIsBot())
                {
                    View.GetBotTrump(Game);
                    SpawnTrumpBot();
                }
                else
                {
                    View.ShowTrumpChooser(true);
                }
                DelayBeforeNextAction = 1.0;
                NextAction = null;
                break;

            case GameAction.TrumpChosen:
                Game.SetFirstPlayer();

                View.UpdateInfo(Game);
                View.UpdateHand(Game, 0);
                View.ShowTrumpChooser(false);
                View.SetTrumpSuit(Game.TrumpSuit);
                View.ShowTrumpMarker(true);

                DelayBeforeNextAction = 1.0;
                NextAction = GameAction.GetCardPlay;
                break;

            case GameAction.GetCardPlay:
                //DelayBeforeNextAction = 1.0;
                NextAction = null;

                if (Game.ActiveIsBot())
                {
                    View.GetBotCardPlay(Game);
                    SpawnPlayBot();
                }
                else
                {
                    Game.GetPlayableCardIds();
                    View.CardClicked += OnHumanCardPlayed;
                    View.GetHumanCardPlay(Game);
                }
                break;

            case GameAction.PlayCard:
                var player = Game.Active;
                hand = Game.ActiveHand();
                Game.NextPlayer();

                View.UpdateHand(Game, player);
                View.UpdateTrick(Game);
                View.UpdateMessage("");
                View.UpdateInfo(Game);

                if (Game.Trick.Completed())
                {
                    GD.Print("trick completed ===");
                    DelayBeforeNextAction = 2.0;
                    NextAction = GameAction.AwardTrick;
                }
                else
                {
                    DelayBeforeNextAction = 1.0;
                    NextAction = GameAction.GetCardPlay;
                }
                break;

            case GameAction.AwardTrick:
                Game.AwardTrick();

                View.UpdateTaken(Game, Game.LastTrickWinner);
                View.UpdateInfo(Game);

                if (Game.HandCompleted())
                {
                    GD.Print("HandCompleted");
                    NextAction = GameAction.AwardNest;
                }
                else
                {
                    Game.ResetForNextTrick();
                    NextAction = GameAction.GetCardPlay;
                }
                break;

            case GameAction.AwardNest:
                var points = Game.AwardNestCards();

                var message = $"There were {points} points in the nest.";
                // View.ShowTrumpMarker(false);
                // View.UpdateMessage(new[] { message });
                View.UpdateInfo(Game);
                // View.UpdateNest(Game, false);
                // View.HideBidMarker(Game.Maker);

                DelayBeforeNextAction = 1.0;
                NextAction = GameAction.PresentScore;
                break;

            case GameAction.PresentScore:
                NextAction = null;
                Game.CompleteHand();
                GD.Print($"We: {Game.Scoring.WeHandScore()}, They: {Game.Scoring.TheyHandScore()}");
                // View.ShowNextHandButton(true);
                View.UpdateInfo(Game);
                break;
        }
    }

    /*
    === NOTE for async Task-calling methods below ===
    A Task does not end when it calls the MyEvent.Invoke(...). It ends _after_
    the event handler code is run So, do not update the View or other Godot Nodes
    in the event handlers.
    */

    private void SpawnBidBot()
    {
        var gameCopy = Game.DeepCopy();
        var bot = new Bot();
        Task.Run(() =>
        {
            bot.Bid(gameCopy);
        });
    }

    void OnBotBid(object sender, Bid bid) // from Bot
    {
        GD.Print("OnBotBid: ", bid.Points);
        Game.MakeBid(bid);
        NextAction = GameAction.BidMade;
        DelayBeforeNextAction = 0.2;
    }

    void OnHumanBid(object sender, Bid bid) // from View
    {
        GD.Print("OnHumanBid: ", bid.Points);
        Game.MakeBid(bid);
        View.EndHumanBid();
        NextAction = GameAction.BidMade;
    }

    private void SpawnDiscardBot()
    {
        var gameCopy = Game.DeepCopy();
        var bot = new Bot();
        Task.Run(() =>
        {
            bot.ChooseDiscards(gameCopy);
        });
    }

    void OnBotDiscards(object sender, List<Card> discards)
    {
        GD.Print("OnBotDiscards");
        foreach (var card in discards)
        {
            Game.SwapWithExchange(card.Id);
        }
        NextAction = GameAction.Discard;
    }

    private void SpawnTrumpBot()
    {
        var gameCopy = Game.DeepCopy();
        var bot = new Bot();
        Task.Run(() =>
        {
            bot.ChooseTrumpSuit(gameCopy);
        });
    }

    void OnBotTrumpSuit(object sender, Suit suit)
    {
        GD.Print("OnBotTrumpSuit: ", suit);
        Game.SetTrumpSuit(suit);
        NextAction = GameAction.TrumpChosen;
    }

    private void OnSuitClicked(object sender, Suit suit)
    {
        Game.SetTrumpSuit(suit);
        NextAction = GameAction.TrumpChosen;
    }

    private void SpawnPlayBot()
    {
        GD.Print("SpawnPlayBot");
        Task.Run(() =>
        {
            var bot = new Bot();
            var gameCopy = Game.DeepCopy();
            bot.GetPlayCard(gameCopy, 500);
        });
    }

    void OnHumanCardExchanged(object sender, int cardId)
    {
        GD.Print("OnHumanCardExchanged");
        Game.SwapWithExchange(cardId);
        NextAction = GameAction.Exchange;
    }

    void OnDoneButtonPressed()
    {
        GD.Print("OnDoneButtonPressed");
        NextAction = GameAction.EndExchanging;
    }

    void OnHumanCardPlayed(object sender, int cardId)
    {
        Game.ResetHandEligibility(Game.Active);
        Game.PlayCardId(cardId);

        View.CardClicked -= OnHumanCardPlayed;
        NextAction = GameAction.PlayCard;
    }

    void OnBotCardPlayed(object sender, int cardId)
    {
        Game.ResetHandEligibility(Game.Active);
        Game.PlayCardId(cardId);
        NextAction = GameAction.PlayCard;
    }
}
