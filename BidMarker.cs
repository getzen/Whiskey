using System;
using Godot;

public partial class BidMarker : Node2D
{
    public void SetBid(Bid bid)
    {
        string text;
        Texture2D texture;

        if (bid.Kind == BidKind.Pass)
        {
            text = "pass";
            texture = GD.Load<Texture2D>("res://images/circle.png");
        }
        else 
        {
            text = bid.Points.ToString();

            var path = "res://images/";
            path += bid.Suit switch
            {
                Suit.Club => "club.png",
                Suit.Diamond => "diamond.png",
                Suit.Heart => "heart.png",
                Suit.Spade => "spade.png",
                Suit.None => "circle.png",
                _ => throw new Exception(""),
            };
            texture = GD.Load<Texture2D>(path);
        }

        GetNode<Label>("Label").Text = text;
        GetNode<Sprite2D>("Suit").Texture = texture;
    }
}
