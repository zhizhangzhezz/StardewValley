using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementAnimationParameterControl : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        // 订阅运动事件
        EventHandler.MovementEvent += SetAnimationParameters;
    }

    private void OnDisable()
    {
        // 取消订阅
        EventHandler.MovementEvent -= SetAnimationParameters;
    }

    // 接收事件数据，设置动画参数
    private void SetAnimationParameters(MovementEventParams param)
    {
        //Debug.Log("收到事件！");
        // 输入方向
        animator.SetFloat(Settings.xInput, param.inputX);
        animator.SetFloat(Settings.yInput, param.inputY);

        // 运动状态
        animator.SetBool(Settings.isWalking, param.isWalking);
        animator.SetBool(Settings.isRunning, param.isRunning);

        // 工具效果
        animator.SetInteger(Settings.toolEffect, (int)param.toolEffect);

        // 使用工具方向
        animator.SetBool(Settings.isUsingToolRight, param.isUsingToolRight);
        animator.SetBool(Settings.isUsingToolLeft, param.isUsingToolLeft);
        animator.SetBool(Settings.isUsingToolUp, param.isUsingToolUp);
        animator.SetBool(Settings.isUsingToolDown, param.isUsingToolDown);

        // 举起工具方向
        animator.SetBool(Settings.isLiftingToolRight, param.isLiftingToolRight);
        animator.SetBool(Settings.isLiftingToolLeft, param.isLiftingToolLeft);
        animator.SetBool(Settings.isLiftingToolUp, param.isLiftingToolUp);
        animator.SetBool(Settings.isLiftingToolDown, param.isLiftingToolDown);

        // 挥舞工具方向
        animator.SetBool(Settings.isSwingingToolRight, param.isSwingingToolRight);
        animator.SetBool(Settings.isSwingingToolLeft, param.isSwingingToolLeft);
        animator.SetBool(Settings.isSwingingToolUp, param.isSwingingToolUp);
        animator.SetBool(Settings.isSwingingToolDown, param.isSwingingToolDown);

        // 拾取方向
        animator.SetBool(Settings.isPickingRight, param.isPickingRight);
        animator.SetBool(Settings.isPickingLeft, param.isPickingLeft);
        animator.SetBool(Settings.isPickingUp, param.isPickingUp);
        animator.SetBool(Settings.isPickingDown, param.isPickingDown);

        // 空闲方向
        animator.SetBool(Settings.idleUp, param.idleUp);
        animator.SetBool(Settings.idleDown, param.idleDown);
        animator.SetBool(Settings.idleLeft, param.idleLeft);
        animator.SetBool(Settings.idleRight, param.idleRight);
    }

    // 动画事件：播放脚步声
    private void AnimationEventPlayFootstepSound()
    {
        // 这里填播放脚步声逻辑
        //AudioManager.Instance?.PlaySound(SoundName.Footstep);
    }
}