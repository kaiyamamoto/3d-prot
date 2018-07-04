using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : Player.PlayerStateBase
{
    protected Vector2 _movement;

    public float maxForwardSpeed = 8f;

    public PlayerMove(Player player) : base(player)
    {

    }

    public override Player.PlayerStateBase KeyInput()
    {
        return null;
    }

    public override void Enter()
    {

    }
}
