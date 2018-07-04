using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Player : MonoBehaviourEx
{
    private PlayerStateBase _stateBase;

    private CharacterController _charCtrl = null;

    private DynamicAnimator _animator = null;

    private float _gravity = 9.81f;

    public float _jumpSpeed = 10f;             // How fast Ellen takes off when jumping. ジャンプ力


    private bool _isGrounded = true;
    protected bool _readyToJump;                  // Whether or not the input state and Ellen are correct to allow jumping.ジャンプ入力状態　


    protected float _forwardSpeed;                // How fast Ellen is currently going along the ground.どのくらい早く地面に向かっているか？
    protected float _verticalSpeed;               // How fast Ellen is currently moving up or down.どのくらい上下に動くか？
    protected float VerticalSpeed
    {
        get { return _verticalSpeed; }
        set { _verticalSpeed = value; }
    }

    const float k_GroundedRayDistance = 1f;
    const float k_StickingGravityProportion = 0.3f;

    const float k_JumpAbortSpeed = 10f;

    void Start()
    {
        _animator = GetComponent<DynamicAnimator>();
        _charCtrl = GetComponent<CharacterController>();
        _stateBase = new PlayerIdle(this);
    }

    private void Update()
    {
        // 状態の更新
        StateUpdate();

        // 移動関連座標の更新
        MoveUpdate();
    }

    /// <summary>
    /// 状態の更新
    /// </summary>
    private void StateUpdate()
    {
        if (_stateBase == null) return;
        // 状態入力確認
        _stateBase.Update();

        var state = _stateBase.KeyInput();

        if (state == null) return;

        Debug.Log(state);

        // 更新がある場合変更
        _stateBase.Exit();

        _stateBase = state;

        _stateBase.Enter();
    }
    public void SetState(PlayerStateBase state)
    {
        if (state == null) return;

        Debug.Log(state);

        // 更新がある場合変更
        if (_stateBase != null) _stateBase.Exit();

        _stateBase = state;

        _stateBase.Enter();
    }
    public void StateLose()
    {
        _stateBase.Exit();

        _stateBase = null;
    }

    /// <summary>
    /// 移動関連の更新
    /// </summary>
    private void MoveUpdate()
    {
        Vector3 movement = Vector3.zero;

        // 地面にいるとき
        if (_isGrounded)
        {
            // 地面にレイキャスト
            RaycastHit hit;
            Ray ray = new Ray(transform.position + Vector3.up * k_GroundedRayDistance * 0.5f, -Vector3.up);
            if (Physics.Raycast(ray, out hit, k_GroundedRayDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                // 地面に沿って回転させる。
                movement = Vector3.ProjectOnPlane(_animator.Animator.deltaPosition, hit.normal);
            }
            else
            {
                movement = _animator.Animator.deltaPosition;
            }
        }
        else
        {
            // 空中であれば、重力をかける
            _verticalSpeed -= _gravity * Time.deltaTime;

            // 地面以外の時動きは正方向
            movement = _forwardSpeed * transform.forward * Time.deltaTime;
        }

        // キャラクタコントローラの変換をアニメーションのルート回転で回転
        _charCtrl.transform.rotation *= _animator.Animator.deltaRotation;

        // 計算された垂直速度を動きに追加
        movement += _verticalSpeed * Vector3.up * Time.deltaTime;

        // キャラクターコントローラーを移動
        _charCtrl.Move(movement);

        // 移動後、キャラクターコントローラの地面接点取得
        _isGrounded = _charCtrl.isGrounded;
    }

    #region state
    public abstract class PlayerStateBase
    {
        protected Player _player;

        public PlayerStateBase(Player player)
        {
            _player = player;
        }

        public abstract PlayerStateBase KeyInput();

        public virtual void Update() { }

        public virtual void Enter() { }

        public virtual void Exit() { }

        protected PlayerStateBase ReadyJump(bool isJump)
        {
            PlayerStateBase state = null;

            // ジャンプしていなくて、地面にいる場合はジャンプ。
            if (!isJump && _player._isGrounded)
                _player._readyToJump = true;

            if (_player._isGrounded)
            {
                // わずかに負の垂直速度を適用して、スティック
                _player._verticalSpeed = -_player._gravity * k_StickingGravityProportion;

                // ジャンプ可能
                if (isJump && _player._readyToJump)
                {
                    state = new PlayerJump(_player);
                }
            }

            return state;
        }
    }
    #endregion
}
