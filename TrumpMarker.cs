using Godot;
using System;

public partial class TrumpMarker : Sprite2D
{
    public void SetSuit(Suit suit) {
        Texture = suit switch
        {
            Suit.Club => Texture = GD.Load<Texture2D>("res://images/club.png"),
            Suit.Diamond => Texture = GD.Load<Texture2D>("res://images/diamond.png"),
            Suit.Heart => Texture = GD.Load<Texture2D>("res://images/heart.png"),
            Suit.Spade => Texture = GD.Load<Texture2D>("res://images/spade.png"),
            _ => throw new NotImplementedException(),
        };
    }
}
