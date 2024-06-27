# Whiskey
A trick-taking card game, with hints of Rook and Euchre. Rook since capturing cards with point values is more important than simply taking tricks. Euchre since a player can abruptly call a trump suit instead of back-and-forth bidding.

It's a Godot game written in GDScript. The bot players use the Monte Carlo method to determine discards and best hand play. They use a rule-based system for bidding. GDScript is fast enough on my machine, but might be sluggish on older hardware. I may convert the whole thing to C# or SwiftGodot or Godot-Rust someday.

# To-Do
The game is playable, but incomplete. It needs:

- End-of-hand handling. Right now, the game just abruptly stops at the end of the hand instead of continuing to the next.
- Correct handling of the Jokers in no-trump hands.
- Better UX to guide player about what is happening.
- Lots more.
