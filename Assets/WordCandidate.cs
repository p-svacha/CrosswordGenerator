using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordCandidate
{

    public int X;
    public int Y;
    public bool Horizontal;
    public KeyValuePair<string, string> Question;
    public int ReusedLetters;

    public WordCandidate(int x, int y, bool hor, KeyValuePair<string, string> q, int reuse)
    {
        X = x;
        Y = y;
        Horizontal = hor;
        Question = q;
        ReusedLetters = reuse;
    }

}
