using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeManager : MonoBehaviour
{
    public SpriteRenderer child_lower, child_upper;

    public void SetupThePipe(Color assigned_color, Sprite assigned_sprite)
    {
        child_lower.color = assigned_color;
        child_lower.sprite = assigned_sprite;
        child_upper.sprite = assigned_sprite;
        child_upper.color = assigned_color;
    }
}
