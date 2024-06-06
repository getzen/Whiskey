using Godot;
using System;

// public partial class card_new : Node
// {
// 	// Called when the node enters the scene tree for the first time.
// 	public override void _Ready()
// 	{
// 	}

// 	// Called every frame. 'delta' is the elapsed time since the previous frame.
// 	public override void _Process(double delta)
// 	{
// 	}
// }


// NONE is used when bidding to pass.
public enum SuitNew {Club, Diamond, Heart, Spade, Joker, None}


[GlobalClass] // Required to access this class from GDScript.
public partial class CardNew : GodotObject // Required, or a subclass of it.
{
	// Apparently, we can't see C# fields in GDScript. They need to be properties.

	public int Id { get; set;}
	private SuitNew _suit;
	public int Suit => (int)_suit; // Cast the enum to its int.
	public double Rank { get; set; } // double is equivalent to GDScript's float.
	public int Points { get; set; }
	public bool FaceUp { get; set; }

	public static CardNew Create(int id, int suit, double rank, int points) 
	{
		var card = new CardNew();
		card.Id = id;
		card._suit = (SuitNew)suit; // Cast the int to the enum.
		card.Rank = rank;
		card.Points = points;
		card.FaceUp = false;
		return card;
	}

}