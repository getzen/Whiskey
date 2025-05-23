using Godot;
using System;

public partial class CardNode : Sprite2D
{
    public int Id;

    private int _eligible;
    public int Eligible
    {
        get => _eligible;
        set
        {
            _eligible = value;
            Modulate = _eligible switch
            { // -1, 0, or 1
                0 => Colors.Gray,
                _ => Colors.White,
            };
        }
    }

    public bool IsJoker;

    private Texture2D FaceTexture;
    private Texture2D BackTexture;

    // Declare C# events (not Godot). See Area2D node's signals in the editor.
    public delegate void CardNodeClickedEventHandler(object sender, int id);
    public static event CardNodeClickedEventHandler CardNodeClicked;

    public CardNode()
    {
    }

    public override void _Ready()
    {
    }

    public void Setup(Card card)
    {
        Id = card.Id;
        Name = card.Id.ToString();
        FaceTexture = GD.Load<Texture2D>(TexturePath(card));
        BackTexture = GD.Load<Texture2D>("res://images/cards/back.png");
        Texture = FaceTexture;
        Scale = new Vector2(0.4f, 0.4f);
        IsJoker = card.Suit == Suit.Joker;
        SetPoints(card.Points);

        // Set the Area2D click detection shape.
        var shape = new RectangleShape2D();
        shape.Size = GetRect().Size; // GetRect() accounts for scale.
        var collision = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
        collision.Shape = shape;
    }

    string TexturePath(Card card)
    {
        var path = "res://images/cards/";

        path += card.Suit switch
        {
            Suit.Club => "clb",
            Suit.Diamond => "dia",
            Suit.Heart => "hrt",
            Suit.Spade => "spd",
            Suit.Joker => "joker",
            _ => "",
        };

        if (card.Suit != Suit.Joker)
        {
            path += card.Rank switch
            {
                11 => "J",
                12 => "Q",
                13 => "K",
                14 => "A",
                _ => card.Rank.ToString()
            };
        }

        path += ".png";
        return path;
    }

    public float Width()
    {
        return Texture.GetWidth() * Scale.X;
    }

    void SetPoints(int points)
    {
        var label = GetNode<Label>("PointsLabel");
        if (points == 0)
        {
            label.Text = "";
        }
        else
        {
            label.Text = points.ToString() + " pts";
        }
    }

    public void SetFaceUp(bool up)
    {
        Texture = up ? FaceTexture : BackTexture;
        GetNode<Label>("PointsLabel").Visible = up;
    }

    // Send an event if the node has been clicked.
    public void OnArea2DInputEvent(Node viewport, InputEvent inputEvent, int shapeIndex)
    {
        if (inputEvent is InputEventMouseButton && inputEvent.IsReleased())
        {
            CardNodeClicked.Invoke(this, Id);
        }
    }


}
