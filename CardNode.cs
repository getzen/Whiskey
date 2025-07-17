using Godot;
using System;
using System.Collections;

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
            var button_active = _eligible switch
            {
                1 => true,
                _ => false,
            };
            GetNode<TextureButton>("TextureButton").Disabled = !button_active;
            // Debugging:
            //GetNode<ReferenceRect>("ReferenceRect").Visible = button_active;
        }
    }

    private bool _hightlight;
    public bool Highlight
    {
        get => _hightlight;
        set
        {
            _hightlight = value;
            var highlightNode = GetNode<Sprite2D>("Highlight");
            highlightNode.Visible = value;
        }
    }

    public bool IsJoker;

    private Texture2D FaceTexture;
    private Texture2D BackTexture;

    public delegate void CardNodeClickedEventHandler(object sender, int id);
    public static event CardNodeClickedEventHandler CardNodeClicked;

    public CardNode()
    {
    }

    public override void _Ready()
    {
        Highlight = false;
    }

    public void Setup(Card card)
    {
        Id = card.Id;
        Name = card.Id.ToString();
        FaceTexture = GD.Load<Texture2D>(TexturePath(card));
        BackTexture = GD.Load<Texture2D>("res://images/cards/back.png");
        Texture = FaceTexture;
        Scale = new Vector2(0.4f, 0.4f); // 240 x 360
        IsJoker = card.Suit == Suit.Joker;
        SetPoints(card.Points);

        // Rather than use an Area2D with a CollisionShape2D and then parse InputEvents,
        // we will use TextureButton with no texture. We set it to the size of the FaceTexture.
        // Using TextureButton without the Sprite2D might be possible, but it's awkward and
        // not worth the fiddling and trouble.
        var button = GetNode<TextureButton>("TextureButton");
        var texSize = FaceTexture.GetSize();
        button.Size = texSize;
        button.Position = -(texSize * 0.5f);
        button.Pressed += OnButtonPressed;

        // Hit testing
        var rect = GetNode<ReferenceRect>("ReferenceRect");
        rect.Size = button.Size;
        rect.Position = button.Position;
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

    private void OnButtonPressed()
    {
        if (Eligible == 1)
        {
            CardNodeClicked.Invoke(this, Id);
        }
    }

    // This is the input event way.
    // public void OnArea2DInputEvent(Node viewport, InputEvent inputEvent, int shapeIndex)
    // {
    //     if (inputEvent is InputEventMouseButton && inputEvent.IsReleased())
    //     {
    //         CardNodeClicked.Invoke(this, Id);
    //     }
    // }
}
