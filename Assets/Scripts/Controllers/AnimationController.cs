using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ActionsEnum = ActionController.ActionsEnum;
using MovementDestinations = MovementController.MovementDestinations;

public class AnimationController : MonoBehaviour
{
    public enum LoopingAnimationStates
    {
        cheer,
        guard,
        idle,
        idle_hurt,
    }
    [SerializeField] private Animator animator;
    public Animator Animator => animator;
    private CharacterController characterController;
    public CharacterController CharacterController => characterController;
    private Dictionary<ActionsEnum, string> animations = new Dictionary<ActionsEnum, string>();
    private Dictionary<ActionsEnum, float> animationSpeeds = new Dictionary<ActionsEnum, float>();
    private bool playingAnimation = false;
    public bool PlayingAnimation => playingAnimation;
    private UnityEvent playCheerEvent = new UnityEvent();
    private UnityEvent playGuardEvent = new UnityEvent();
    private UnityEvent playIdleEvent = new UnityEvent();
    private UnityEvent playIdleHurtEvent = new UnityEvent();
    private UnityEvent stopLoopingAnimationEvent = new UnityEvent();

    #region Initilization

    private void Awake()
    {
        this.characterController = GetComponent<CharacterController>();

        Initialize();
    }

    private void Initialize()
    {
        foreach (ActionsEnum value in Enum.GetValues(typeof(ActionsEnum)))
        {
            this.animations.Add(value, value.ToString());
        }

        InitializeLoopingEvent(this.playCheerEvent, LoopingAnimationStates.cheer);
        InitializeLoopingEvent(this.playGuardEvent, LoopingAnimationStates.guard);
        InitializeLoopingEvent(this.playIdleEvent, LoopingAnimationStates.idle);
        InitializeLoopingEvent(this.playIdleHurtEvent, LoopingAnimationStates.idle_hurt);
        InitializeLoopingAnimationCancelEvent();
        InitializeAnimationSpeeds();
    }

    private void InitializeLoopingEvent(UnityEvent unityEvent, LoopingAnimationStates state)
    {
        foreach (LoopingAnimationStates value in Enum.GetValues(typeof(LoopingAnimationStates)))
        {
            if (value == state)
            {
                unityEvent.AddListener(() => this.animator.SetBool(value.ToString(), true));
            }
            else
            {
                unityEvent.AddListener(() => this.animator.SetBool(value.ToString(), false));
            }
        }
    }

    private void InitializeLoopingAnimationCancelEvent()
    {
        foreach (LoopingAnimationStates value in Enum.GetValues(typeof(LoopingAnimationStates)))
        {
            this.stopLoopingAnimationEvent.AddListener(() => this.animator.SetBool(value.ToString(), false));
        }
    }

    private void InitializeAnimationSpeeds()
    {
        this.animationSpeeds.Add(ActionsEnum.Default, 0.5f);
        this.animationSpeeds.Add(ActionsEnum.Attack_Regular, 0.75f);
        this.animationSpeeds.Add(ActionsEnum.Attack_Special, 0.75f);
        this.animationSpeeds.Add(ActionsEnum.Dash, 1f);
        this.animationSpeeds.Add(ActionsEnum.Evade, 1f);
        this.animationSpeeds.Add(ActionsEnum.Flinch, 0.5f);
    }

    #endregion

    #region Animation Sequences

    public IEnumerator StartAnimationSequence()
    {
        ActionsEnum characterAction = this.characterController.ActionController.CurrentAction;
        ActionsEnum enemyAction = this.characterController.OpponentController.ActionController.CurrentAction;
        bool shouldAttack = this.characterController.ActionController.IsAttacking();
        bool shouldClash = this.characterController.ActionController.ShouldClash();
        bool shouldMove = this.characterController.ActionController.ShouldMove();
        bool shouldCounter = this.characterController.ActionController.ShouldCounter();
        bool haveAdvantage = this.characterController.ActionController.HaveAdvantage();
        bool opponentShouldFlinch = this.characterController.OpponentController.ActionController.ShouldFlinch();

        if (shouldAttack)
        {
            this.CharacterController.ActionController.OnAttackStart();
        }

        yield return new WaitUntil(() => !this.playingAnimation);

        if (shouldMove || shouldClash)
        {
            PlayAnimation(ActionsEnum.Dash);

            this.CharacterController.MovementController.StartMoving(MovementDestinations.Opponent);

            yield return new WaitUntil(() => !this.CharacterController.MovementController.Moving && !this.playingAnimation);
        }
        else
        {
            if (characterAction == ActionsEnum.Guard)
            {
                StartLoopingAnimation(LoopingAnimationStates.guard);
            }
            else
            {
                StartLoopingAnimation(LoopingAnimationStates.idle);
            }
        }

        if (shouldCounter)
        {

        }

        // add delay
        if (shouldClash)
        {
            PlayAnimation(characterAction);
            this.CharacterController.MovementController.StartMoving(MovementDestinations.OpponentOrigin);

            yield return new WaitUntil(() => !this.CharacterController.MovementController.Moving && !this.playingAnimation);

            PlayAnimation(ActionsEnum.Flinch);

            yield return new WaitUntil(() => !this.playingAnimation);
            // Death Check
        }
        else if (shouldAttack)
        {
            if (!haveAdvantage)
            {
                yield return new WaitUntil(() => !this.CharacterController.OpponentController.ActionController.Attacking && !this.playingAnimation);
            }

            PlayAnimation(characterAction);

            if (opponentShouldFlinch)
            {
                this.characterController.OpponentController.AnimationController.OnReceivingAttack();
            }

            yield return new WaitUntil(() => !this.playingAnimation);
        }

        this.characterController.ActionController.OnAttackEnd();

        yield return new WaitUntil(() => !this.CharacterController.OpponentController.ActionController.Attacking && !this.playingAnimation);

        // Death Check

        if (shouldMove || shouldClash)
        {
            PlayAnimation(ActionsEnum.Evade);
        }

        this.CharacterController.MovementController.StartMoving(MovementDestinations.Origin);

        yield return new WaitUntil(() => !this.CharacterController.MovementController.Moving && !this.playingAnimation);

        StartLoopingAnimation(LoopingAnimationStates.idle);
    }

    #endregion

    #region Animation Control

    private void PlayAnimation(ActionsEnum action)
    {
        if (!this.animations.ContainsKey(action))
        {
            Debug.Log("Animations does not contain " + action);

            return;
        }

        Debug.Log($"{this.gameObject.name} - {action}");

        StopLoopingAnimation();
        SetAnimationSpeed(action);
        this.animator.Play(this.animations[action], 0, 0f);
    }

    private void StartLoopingAnimation(LoopingAnimationStates state)
    {
        Debug.Log($"{this.gameObject.name} - {state}");

        SetAnimationSpeed(ActionsEnum.Default);

        switch (state)
        {
            case LoopingAnimationStates.cheer:
                this.playCheerEvent.Invoke();
                break;
            case LoopingAnimationStates.guard:
                this.playGuardEvent.Invoke();
                break;
            case LoopingAnimationStates.idle:
                this.playIdleEvent.Invoke();
                break;
            case LoopingAnimationStates.idle_hurt:
                this.playIdleHurtEvent.Invoke();
                break;
        }
    }

    private void StopLoopingAnimation()
    {
        this.stopLoopingAnimationEvent.Invoke();
    }

    public void SetLoopingState()
    {

    }

    public void OnReceivingAttack()
    {
        PlayAnimation(ActionsEnum.Flinch);
    }

    #endregion

    #region Animation Events

    public void AnimationStarted()
    {
        Debug.Log("anim started");
        this.playingAnimation = true;
    }

    public void AnimationEnded()
    {
        Debug.Log("anim ended");
        this.playingAnimation = false;
    }

    #endregion

    #region Setters

    private void SetAnimationSpeed(ActionsEnum action)
    {
        if (this.animationSpeeds.ContainsKey(action))
        {
            this.animator.speed = this.animationSpeeds[action];
        }
        else
        {
            this.animator.speed = this.animationSpeeds[ActionsEnum.Default];
        }
    }

    #endregion

    #region Getters

    private string GetCurrentAnimationName()
    {
        return this.animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
    }

    private string GetAnimationTrigger(ActionsEnum animationName)
    {
        return animationName.ToString().ToLower();
    }

    private string GetAnimationName(ActionsEnum animationName)
    {
        return animationName.ToString();
    }

    #endregion
}
