using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pipes editor/New pipe...")]
public class Pipe : ScriptableObject
{
    public Color color=Color.white;
    public int appearingLevel;
    public Sprite sprite_assigned;
}
