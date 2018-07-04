using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Player
{
    public class PlayerJump : Player.PlayerStateBase
    {
        private bool _isJump = false;

        public PlayerJump(Player player) : base(player)
        {

        }

        public override Player.PlayerStateBase KeyInput()
        {
            PlayerStateBase state = null;

            if (_isJump == false) return null;

            // 上移動中
            if (_player._verticalSpeed > 0.0f)
            {
                // 垂直方向のスピードを落とす
                _player._verticalSpeed -= k_JumpAbortSpeed * Time.deltaTime;
            }
            else if (Mathf.Approximately(_player._verticalSpeed, 0f))
            {
                // ジャンプがピークに達している場合は、絶対値に
                _player._verticalSpeed = 0f;
            }
            else
            {
                // 落下状態
                state = new PlayerFall(_player);
            }

            return state;
        }

        public override void Enter()
        {
            _player._animator.Play("Jump");

            _player.StartCoroutine(enterCorutine(0.3f));
        }

        private IEnumerator enterCorutine(float time)
        {
            yield return new WaitForSeconds(time);

            // 垂直速度を上書きし、再びジャンプしないように。
            _player._verticalSpeed = _player._jumpSpeed;
            _player._isGrounded = false;
            _player._readyToJump = false;
            _isJump = true;
        }
    }

    public class PlayerFall : Player.PlayerStateBase
    {
        public PlayerFall(Player player) : base(player)
        {

        }

        public override void Enter()
        {
            _player._animator.CrossFadeInFixedTime("Falling");
        }

        public override Player.PlayerStateBase KeyInput()
        {
            // 地面にレイキャスト
            RaycastHit hit;
            Ray ray = new Ray(_player.transform.position + Vector3.up * k_GroundedRayDistance / 2, -Vector3.up);
            if (Physics.Raycast(ray, out hit, k_GroundedRayDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {

                Debug.Log("hit");

                // 状態無効
                _player.StateLose();

                _player._animator.Play("Landing", () =>
                  {
                      _player.SetState(new PlayerIdle(_player));
                  });
            }

            return null;
        }
    }
}
