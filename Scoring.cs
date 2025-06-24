using System.Reflection.Metadata;
using Godot;

public struct Scoring
{
    public int WinScore;
    int[] Tricks = [0, 0, 0, 0];
    int[] Nest = [0, 0, 0, 0];
    int[] Game = [0, 0, 0, 0];

    public Scoring(int winScore)
    {
        WinScore = winScore;
        Tricks = [0, 0, 0, 0];
        Nest = [0, 0, 0, 0];
        Game = [0, 0, 0, 0];
    }

    public Scoring DeepCopy()
    {
        var copy = new Scoring(WinScore);
        // Handy method to copy Array elements (as long as they don't contain refs themselves).
        System.Array.Copy(Tricks, copy.Tricks, Tricks.Length);
        System.Array.Copy(Nest, copy.Nest, Nest.Length);
        System.Array.Copy(Game, copy.Game, Game.Length);
        return copy;
    }

    public void ResetForNewHand()
    {
        for (var p = 0; p < Tricks.Length; p++)
        {
            Tricks[p] = 0;
            Nest[p] = 0;
        }
    }

    public void AwardTrickPts(int points, int player)
    {
        Tricks[player] += points;
    }

    public void AwardNestPts(int points, int player)
    {
        Nest[player] += points;
    }

    public void AwardGamePts(int points, int player)
    {
        Game[player] += points;
    }

    public int CombinedHandScoreFor(int p1, int p2)
    {
        return Tricks[p1] + Nest[p1] + Tricks[p2] + Nest[p2];
    }

    public int WeHandScore()
    {
        return CombinedHandScoreFor(0, 2);
    }

    public int TheyHandScore()
    {
        return CombinedHandScoreFor(1, 3);
    }
    
    public int WeGameScore()
    {
        return Game[0];
    }

     public int TheyGameScore()
    {
        return Game[1];
    }
}