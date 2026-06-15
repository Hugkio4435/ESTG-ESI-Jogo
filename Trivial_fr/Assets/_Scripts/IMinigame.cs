using System.Collections.Generic;

// Does not inherit from MonoBehaviour! It's just a "contract".
public interface IMinigame
{
    // Any script that signs this contract must have this function.
    // It returns a Dictionary linking the Player's Name (string) to the Points Earned (int).
    Dictionary<string, int> CalculateRoundScores();
}