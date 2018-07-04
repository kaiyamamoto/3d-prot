using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Player
{
    public class PlayerIdle : Player.PlayerStateBase
    {
        public PlayerIdle(Player player) : base(player) { }

        public override Player.PlayerStateBase KeyInput()
        {
            Player.PlayerStateBase state = null;

            bool isJump = false;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                isJump = true;
            }

            state = ReadyJump(isJump);

            return state;
        }

        public override void Enter()
        {
            _player._animator.CrossFadeInFixedTime("Idle_01");
        }
    }
}