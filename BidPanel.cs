using Godot;
using System;

public partial class BidPanel : Panel
{

    public int BidValue()
    {
        return (int)GetNode<SpinBox>("SpinBox").Value;
    }

    public void SetMinMaxPoints(int min, int max) {
        var spinner = GetNode<SpinBox>("SpinBox");
        spinner.Value = min;
        spinner.MinValue = min;
        spinner.MaxValue = max;
    }

   
}
