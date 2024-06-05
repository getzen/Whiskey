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


// There seems to be no way to access this from GDScipt/.
// NONE is used when bidding to pass.
	
public enum SuitNew {CLUB, DIAMOND, HEART, SPADE, JOKER, NONE}


[GlobalClass] // Required to access this class from GDScript
public partial class CardNew : GodotObject
{
	
	
	public int id;
	private SuitNew _suit;
	public int Suit => (int)_suit;
	public double rank; // double is equivalent to GDScript's float.
	public int Points { get; set; } // Apparently, can't see fields in GDScript. Need to be properties.
	public bool face_up;

	public static CardNew Create(int id, double rank) 
	{
		var card = new CardNew();
		card._suit = SuitNew.NONE;
		return card;
	}

}