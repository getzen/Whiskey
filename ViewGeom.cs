using System;
using Godot;

public static class Constants
{
    public static readonly Vector2 SCREEN = new Vector2(1000f, 1000f);

    public static readonly Vector2 PLAY_TOP_LEFT = new Vector2(0f, 0f);
    public static readonly Vector2 PLAY_BOTTOM_RIGHT = new Vector2(SCREEN.X, SCREEN.Y);
    public static readonly Vector2 PLAY_CENTER = new Vector2(
        PLAY_TOP_LEFT.X + (PLAY_BOTTOM_RIGHT.X - PLAY_TOP_LEFT.X) * 0.5f,
        PLAY_TOP_LEFT.Y + (PLAY_BOTTOM_RIGHT.Y - PLAY_TOP_LEFT.Y) * 0.5f
    );

    public static readonly Vector2 NEST_EXCHANGE_POS = new Vector2(PLAY_CENTER.X, PLAY_CENTER.Y - 30f);
    public static readonly Vector2 NEST_ASIDE_POS = new Vector2(PLAY_TOP_LEFT.X + 90f, PLAY_BOTTOM_RIGHT.Y - 120f);
    public static readonly Vector2 MESSAGE_POS = new Vector2(PLAY_CENTER.X, PLAY_CENTER.Y + 110f);
    public static readonly Vector2 SCORE_TABLE_POS = new Vector2(10f, 10f);
    // public static readonly Vector2 PLAY_BUTTON_POS = new Vector2(PLAY_CENTER.X, PLAY_CENTER.Y + 100f);
    public static readonly Vector2 BID_PANEL_POS = new Vector2(PLAY_CENTER.X, PLAY_CENTER.Y + 220f);
    public static readonly Vector2 DONE_EXCHANGING_BUTTON_POS = new Vector2(PLAY_CENTER.X, PLAY_CENTER.Y + 60f);
    public static readonly Vector2 NEXT_HAND_BUTTON_POS = new Vector2(PLAY_CENTER.X, PLAY_CENTER.Y + 200f);
    public static readonly Vector2 TRUMP_CHOOSER_POS = new Vector2(PLAY_CENTER.X, PLAY_CENTER.Y + 70f);

    public const float CARD_SPEED = 800f;
    public const float ROT_SPEED = 10f;

    public const ushort CARD_Z = 100;
}

public struct ViewGeom
{
    public Vector2 Pos { get; set; }
    public float Rot { get; set; }
    public ushort Z { get; set; }

    // Default constructor
    public ViewGeom(Vector2 pos, float rot, ushort z)
    {
        Pos = pos;
        Rot = rot;
        Z = z;
    }

    // Equivalent to Rust's Default trait
    public static ViewGeom Default => new ViewGeom(Vector2.Zero, 0f, 0);
    
    public static ViewGeom TurnMarkerGeom(int player, int playerCount)
    {
        float rad = PlayerRadiansFromCenter(player, playerCount);
        return new ViewGeom
        {
            Pos = PositionFrom(Constants.PLAY_CENTER, rad, 435f),
            Rot = 0f,
            Z = 0
        };
    }

    public static ViewGeom BidMarkerGeom(int player, int playerCount)
    {
        float rad = PlayerRadiansFromCenter(player, playerCount);
        return new ViewGeom
        {
            Pos = PositionFrom(Constants.PLAY_CENTER, rad, 220f),
            Rot = 0f,
            Z = 0
        };
    }

    public static ViewGeom DeckGeom(int dealer, int playerCount, int _index)
    {
        if (dealer >= 0)
        {
            float rad = PlayerRadiansFromCenter(dealer, playerCount);
            return new ViewGeom
            {
                Pos = PositionFrom(Constants.PLAY_CENTER, rad, 190f),
                Rot = PlayerRotation(dealer, playerCount),
                Z = 0
            };
        }
        return new ViewGeom
        {
            Pos = Constants.SCREEN / 2.0f,
            Rot = 0f,
            Z = 0
        };
    }

    public static ViewGeom ExchangeGeom(int index, int count)
    {
        const float maxWidth = 300f;
        const float maxSpacing = 80f;

        float computedWidth = maxWidth / count;
        float xSpacing = Math.Min(maxSpacing, computedWidth);

        float xOffset = (count - 1) * -xSpacing / 2f;
        xOffset += index * xSpacing;
        Vector2 pos = Constants.NEST_EXCHANGE_POS + new Vector2(xOffset, 0f);

        return new ViewGeom
        {
            Pos = pos,
            Rot = 0f,
            Z = (ushort)(Constants.CARD_Z + index)
        };
    }

    public static ViewGeom NestGeom(int index, int count)
    {
        const float maxWidth = 130f;
        const float maxSpacing = 40f;

        float computedWidth = maxWidth / count;
        float xSpacing = Math.Min(maxSpacing, computedWidth);

        float offset = (count - 1) * -xSpacing / 2f;
        offset += index * xSpacing;
        Vector2 pos = Constants.NEST_ASIDE_POS + new Vector2(offset, -offset);

        return new ViewGeom
        {
            Pos = pos,
            Rot = -0.3f,
            Z = (ushort)(Constants.CARD_Z + index)
        };
    }

    public static float PlayerRotation(int player, int count)
    {
        return player * MathF.PI * 2f / count;
    }

    public static float PlayerRadiansFromCenter(int player, int count)
    {
        return player * MathF.PI * 2f / count + MathF.PI / 2f;
    }

    public static Vector2 PositionFrom(Vector2 startPos, float radians, float magnitude)
    {
        return new Vector2(
            startPos.X + MathF.Cos(radians) * magnitude,
            startPos.Y + MathF.Sin(radians) * magnitude
        );
    }

    public static ViewGeom HandCardGeom(int player, int index, int handCount, int playerCount, bool isBot)
    {
        const float distanceFromCenter = 400f;
        float maxWidth = isBot ? 500f : 530f; // 300f : 530f
        const float maxSpacing = 60f;

        float computedWidth = maxWidth / handCount;
        float xSpacing = Math.Min(maxSpacing, computedWidth);

        float xOffset = (handCount - 1) * -xSpacing / 2f;
        xOffset += index * xSpacing;

        float rad = PlayerRadiansFromCenter(player, playerCount);
        Vector2 pos = PositionFrom(Constants.PLAY_CENTER, rad, distanceFromCenter);

        float angle = PlayerRotation(player, playerCount);
        pos.X += xOffset * MathF.Cos(angle);
        pos.Y += xOffset * MathF.Sin(angle);

        return new ViewGeom
        {
            Pos = pos,
            Rot = angle,
            Z = (ushort)(Constants.CARD_Z + index)
        };
    }

    public static ViewGeom TrickCardGeom(int player, int playerCount)
    {
        const float distanceFromCenter = 120f;
        float rad = PlayerRadiansFromCenter(player, playerCount);
        Vector2 pos = PositionFrom(Constants.PLAY_CENTER, rad, distanceFromCenter);
        float angle = PlayerRotation(player, playerCount);

        return new ViewGeom
        {
            Pos = pos,
            Rot = angle,
            Z = (ushort)(Constants.CARD_Z + 200)
        };
    }

    public static ViewGeom TakenGeom(int player, int playerCount)
    {
        const float distanceFromCenter = 450f;
        float rad = PlayerRadiansFromCenter(player, playerCount);
        Vector2 pos = PositionFrom(Constants.PLAY_CENTER, rad, distanceFromCenter);
        float angle = PlayerRotation(player, playerCount);

        return new ViewGeom
        {
            Pos = pos,
            Rot = angle,
            Z = 0
        };
    }
}