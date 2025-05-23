using Godot;
using System;

public partial class TrumpChooser : Panel
{
    // Subscriber: Controller
    public delegate void SuitClickedEventHandler(object sender, Suit suit);
    public static event SuitClickedEventHandler SuitClicked;

    public override void _Ready()
    {
        var club = GetNode<TextureButton>("ClubButton");
        club.Pressed += () => OnSuitButtonPressed(Suit.Club);

        var diamond = GetNode<TextureButton>("DiamondButton");
        diamond.Pressed += () => OnSuitButtonPressed(Suit.Diamond);

        var heart = GetNode<TextureButton>("HeartButton");
        heart.Pressed += () => OnSuitButtonPressed(Suit.Heart);

        var spade = GetNode<TextureButton>("SpadeButton");
        spade.Pressed += () => OnSuitButtonPressed(Suit.Spade);
    }

    void OnSuitButtonPressed(Suit suit)
    {
        GD.Print(suit);
        SuitClicked.Invoke(this, suit);
    }
}
