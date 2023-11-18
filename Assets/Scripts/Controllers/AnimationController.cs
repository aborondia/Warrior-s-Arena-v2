using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ActionsEnum = ActionController.ActionsEnum;

public class AnimationController : MonoBehaviour
{


  [SerializeField] private Animator animator;
  public Animator Animator => animator;
  private CharacterController characterController;
  public CharacterController CharacterController => characterController;
  private bool playingAnimation = false;
  public bool PlayingAnimation => playingAnimation;
  private bool waitingToFlinch = false;
  private bool waitingToCounter = false;

  private void Awake()
  {
    this.characterController = GetComponent<CharacterController>();
  }

  #region Animation Sequences

  public IEnumerator StartAnimationSequence()
  {
    ActionsEnum characterAction = this.characterController.ActionController.CurrentAction;
    ActionsEnum enemyAction = this.characterController.OpponentController.ActionController.CurrentAction;
    bool shouldAttack = this.characterController.ActionController.IsAttacking();
    bool shouldClash = this.characterController.ActionController.ShouldClash();
    bool shouldMove = this.characterController.ActionController.ShouldMove();
    bool shouldCounter = this.characterController.ActionController.ShouldCounter();
    bool shouldFlinch = this.characterController.ActionController.ShouldFlinch();
    bool opponentShouldFlinch = this.characterController.OpponentController.ActionController.ShouldFlinch();

    this.waitingToFlinch = shouldFlinch;
    this.waitingToCounter = shouldCounter;

    if (shouldAttack)
    {
      this.CharacterController.ActionController.OnAttackStart();
    }
    else if (characterAction == ActionsEnum.Guard)
    {
      this.animator.SetTrigger(GetAnimationTrigger(ActionsEnum.Guard));
    }

    yield return new WaitForEndOfFrame();
    yield return new WaitUntil(() => !this.playingAnimation);

    if (shouldMove)
    {
      this.animator.SetTrigger(GetAnimationTrigger(ActionsEnum.Dash));

      yield return new WaitUntil(() => GetCurrentAnimationName() == GetAnimationName(ActionsEnum.Dash));

      this.CharacterController.MovementController.MoveToOpponent();

      yield return new WaitUntil(() =>
      {
        return !this.CharacterController.MovementController.Moving && !this.playingAnimation;
      });
    }
    else
    {
      if (this.waitingToFlinch)
      {
        yield return new WaitUntil(() => !this.waitingToFlinch);

        this.animator.SetTrigger(GetAnimationTrigger(ActionsEnum.Flinch));

        shouldFlinch = false;
      }
      else if (this.waitingToCounter)
      {
        // ???
      }

      yield return new WaitUntil(() => !this.CharacterController.OpponentController.ActionController.Attacking);
    }

    if (shouldClash)
    {
      this.animator.SetTrigger(GetAnimationTrigger(characterAction));
      this.CharacterController.MovementController.MoveToOpponentOrigin();

      yield return new WaitUntil(() => !this.CharacterController.MovementController.Moving && !this.playingAnimation);

      this.animator.SetTrigger(GetAnimationTrigger(ActionsEnum.Flinch));

      yield return new WaitUntil(() => GetCurrentAnimationName() == GetAnimationName(ActionsEnum.Flinch) && !this.playingAnimation);
      // Death Check
    }
    else if (shouldAttack)
    {
      this.animator.SetTrigger(GetAnimationTrigger(characterAction));

      yield return new WaitUntil(() => GetCurrentAnimationName() == GetAnimationName(characterAction));

      if (opponentShouldFlinch)
      {
        this.characterController.OpponentController.AnimationController.OnReceivingAttack();
      }

      yield return new WaitUntil(() => !this.playingAnimation && !this.CharacterController.MovementController.Moving);

    }

    this.characterController.ActionController.OnAttackEnd();

    if (shouldFlinch)
    {
      yield return new WaitUntil(() => !this.waitingToFlinch);

      this.animator.SetTrigger(GetAnimationTrigger(ActionsEnum.Flinch));

      yield return new WaitUntil(() => GetCurrentAnimationName() == GetAnimationName(ActionsEnum.Flinch) && !this.playingAnimation);

      shouldFlinch = false;
    }
    // Death Check

    yield return new WaitUntil(() => !this.CharacterController.OpponentController.ActionController.Attacking);

    this.animator.SetTrigger(GetAnimationTrigger(ActionsEnum.Evade));

    yield return new WaitUntil(() => GetCurrentAnimationName() == GetAnimationName(ActionsEnum.Evade));

    this.CharacterController.MovementController.MoveToOrigin();

    yield return new WaitUntil(() =>
    {
      return !this.CharacterController.MovementController.Moving && !this.playingAnimation;
    });

    this.animator.SetTrigger(GetAnimationTrigger(ActionsEnum.Idle));
  }

  #endregion

  public void OnReceivingAttack()
  {
    this.waitingToFlinch = false;
  }

  #region Animation Events

  public void AnimationStarted()
  {
    this.playingAnimation = true;
  }

  public void AnimationEnded()
  {
    this.playingAnimation = false;
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
