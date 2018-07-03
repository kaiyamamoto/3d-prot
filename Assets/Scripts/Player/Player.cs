using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    protected Vector2 m_Movement;

    protected bool m_Jump;

    public float maxForwardSpeed = 8f;        // How fast Ellen can run. 速度
    public float gravity = 20f;               // How fast Ellen accelerates downwards when airborne. 重力
    public float jumpSpeed = 10f;             // How fast Ellen takes off when jumping. ジャンプ力
    protected bool m_IsGrounded = true;            // Whether or not Ellen is currently standing on the ground.地面にいるか？
    protected bool m_PreviouslyGrounded = true;    // Whether or not Ellen was standing on the ground last frame.前まで地面にいたか？

    protected float m_ForwardSpeed;                // How fast Ellen is currently going along the ground.どのくらい早く地面に向かっているか？
    protected float m_DesiredForwardSpeed;         // How fast Ellen aims be going along the ground based on input.　入力から地面に沿って進むための速度
    protected Animator m_Animator;                 // Reference used to make decisions based on Ellen's current animation and to set parameters.
    protected float m_VerticalSpeed;               // How fast Ellen is currently moving up or down.どのくらい上下に動くか？
    protected bool m_ReadyToJump;                  // Whether or not the input state and Ellen are correct to allow jumping.ジャンプ入力状態　

    const float k_JumpAbortSpeed = 10f;
    const float k_GroundedRayDistance = 1f;
    const float k_GroundAcceleration = 20f;
    const float k_GroundDeceleration = 25f;
    const float k_StickingGravityProportion = 0.3f;


    protected CharacterController m_CharCtrl;      // Reference used to actually move Ellen.動かすための参照

    public Vector2 MoveInput
    {
        get { return m_Movement; }
    }

    public bool JumpInput
    {
        get { return m_Jump; }
    }

    // Called automatically by Unity when the script first exists in the scene.
    // スクリプトがシーン内に最初に存在するとき、Unityによって自動的に呼び出されます
    void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_CharCtrl = GetComponent<CharacterController>();
    }

    void Update()
    {
        m_Movement.Set(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        m_Jump = Input.GetButton("Jump");

        CalculateForwardMovement();

        m_PreviouslyGrounded = m_IsGrounded;

    }

    // Called each physics step.
    // 各物理ステップを呼び出しました。
    void CalculateForwardMovement()
    {
        // Cache the move input and cap it's magnitude at 1.
        // 移動入力をキャッシュし、その大きさを1に設定します。
        Vector2 moveInput = m_Movement;
        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        // Calculate the speed intended by input.
        // 入力が意図する速度を計算します。
        m_DesiredForwardSpeed = moveInput.magnitude * maxForwardSpeed;

        // Determine change to speed based on whether there is currently any move input.
        // 現在の移動入力があるかどうかに基づいて速度の変更を決定します。
        float acceleration = !Mathf.Approximately(m_Movement.sqrMagnitude, 0f) ? k_GroundAcceleration : k_GroundDeceleration;

        // Adjust the forward speed towards the desired speed.
        // 前進速度を希望の速度に調整します。
        m_ForwardSpeed = Mathf.MoveTowards(m_ForwardSpeed, m_DesiredForwardSpeed, acceleration * Time.deltaTime);
    }

    // Called each physics step.
    // 各物理ステップを呼び出しました。
    void CalculateVerticalMovement()
    {
        // If jump is not currently held and Ellen is on the ground then she is ready to jump.
        // ジャンプが現在開催されておらず、エレンが地面にいる場合、彼女はジャンプする準備ができています。
        if (!JumpInput && m_IsGrounded)
            m_ReadyToJump = true;

        if (m_IsGrounded)
        {
            // When grounded we apply a slight negative vertical speed to make Ellen "stick" to the ground.
            // 接地されたときには、わずかに負の垂直速度を適用して、エレンを地面に「スティック」させます。
            m_VerticalSpeed = -gravity * k_StickingGravityProportion;

            // If jump is held, Ellen is ready to jump and not currently in the middle of a melee combo...
            // ジャンプが開催されている場合、Ellenはジャンプする準備ができており、現在は近接戦闘の途中ではありません...
            if (JumpInput && m_ReadyToJump)
            {
                // ... then override the previously set vertical speed and make sure she cannot jump again.
                // ..次に、以前に設定した垂直速度を上書きし、彼女が再びジャンプできないことを確認します。
                m_VerticalSpeed = jumpSpeed;
                m_IsGrounded = false;
                m_ReadyToJump = false;
            }
        }
        else
        {
            // If Ellen is airborne, the jump button is not held and Ellen is currently moving upwards...
            // エレンが空中であれば、ジャンプボタンは押されず、エレンは現在上向きに動いています...
            if (!JumpInput && m_VerticalSpeed > 0.0f)
            {
                // ... decrease Ellen's vertical speed.
                // This is what causes holding jump to jump higher that tapping jump.
                // ...エレンの垂直方向のスピードを落とす。
                // これは、ホールドジャンプがジャンプをジャンプするほど高くジャンプする原因です。
                m_VerticalSpeed -= k_JumpAbortSpeed * Time.deltaTime;
            }

            // If a jump is approximately peaking, make it absolute.
            // ジャンプがほぼピークに達している場合は、絶対にします
            if (Mathf.Approximately(m_VerticalSpeed, 0f))
            {
                m_VerticalSpeed = 0f;
            }

            // If Ellen is airborne, apply gravity.
            // エレンが空中であれば、重力をかける。
            m_VerticalSpeed -= gravity * Time.deltaTime;
        }
    }

    // Called each physics step (so long as the Animator component is set to Animate Physics) after FixedUpdate to override root motion.
    // FixedUpdateの後に、ルートモーションをオーバーライドするために、各フィジックスステップを呼び出す（Animatorコンポーネントがアニメーションフィジックスに設定されている限り）。
    void OnAnimatorMove()
    {
        Vector3 movement;

        // If Ellen is on the ground...
        // エレンが地面にいるなら...
        if (m_IsGrounded)
        {
            // ... raycast into the ground...
            // ...地面にレイキャスト...
            RaycastHit hit;
            Ray ray = new Ray(transform.position + Vector3.up * k_GroundedRayDistance * 0.5f, -Vector3.up);
            if (Physics.Raycast(ray, out hit, k_GroundedRayDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                // ... and get the movement of the root motion rotated to lie along the plane of the ground.
                // ...根の動きを地面に沿って回転させる。
                movement = Vector3.ProjectOnPlane(m_Animator.deltaPosition, hit.normal);
            }
            else
            {
                // If no ground is hit just get the movement as the root motion.
                // Theoretically this should rarely happen as when grounded the ray should always hit.
                // 地面に打撃がない場合は、根の動きとして動きを取得してください。
                // 理論的には、これは、レイがいつもぶつかるべきときのようにはめったに起こりません。
                movement = m_Animator.deltaPosition;
            }
        }
        else
        {
            // If not grounded the movement is just in the forward direction.
            // 接地されていない場合、動きは正方向になります。
            movement = m_ForwardSpeed * transform.forward * Time.deltaTime;
        }

        // Rotate the transform of the character controller by the animation's root rotation.
        // キャラクタコントローラの変換をアニメーションのルート回転で回転します。
        m_CharCtrl.transform.rotation *= m_Animator.deltaRotation;

        // Add to the movement with the calculated vertical speed.
        // 計算された垂直速度で動きに追加します。
        movement += m_VerticalSpeed * Vector3.up * Time.deltaTime;

        // Move the character controller.
        // キャラクターコントローラーを移動します。
        m_CharCtrl.Move(movement);

        // After the movement store whether or not the character controller is grounded.         
        // 移動後、文字コントローラが接地されているかどうかを記憶する。
        m_IsGrounded = m_CharCtrl.isGrounded;
    }
}
