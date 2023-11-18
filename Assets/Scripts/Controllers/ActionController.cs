using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionController : MonoBehaviour
{
    public enum ActionsEnum
    {
        Default = 0,
        Attack_Regular = 1,
        Attack_Special = 2,
        Challenge = 3,
        Cheer = 4,
        Dash = 5,
        Death = 6,
        Evade = 7,
        Flinch = 8,
        Guard = 9,
        Hold_Weapon = 10,
        Idle = 11,
        Idle_Hurt = 12,
        Raise_Hand = 13,
    }

    private CharacterController characterController;
    public CharacterController CharacterController => characterController;

    [SerializeField]
    private ActionsEnum currentAction = ActionsEnum.Default;
    public ActionsEnum CurrentAction => currentAction;
    private bool attacking = false;
    public bool Attacking => attacking;

    public void SetAction(ActionsEnum action)
    {
        this.currentAction = action;
    }

    private void Awake()
    {
        this.characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            StartActionSequence();
        }
    }

    public void OnAttackStart()
    {
        this.attacking = true;
    }

    public void OnAttackEnd()
    {
        this.attacking = false;
    }

    #region Actions

    public void StartActionSequence()
    {
        StartCoroutine(this.characterController.AnimationController.StartAnimationSequence());
    }

    public void OnActionEnd()
    {
        this.currentAction = ActionsEnum.Default;
    }

    #endregion

    #region Getters

    public bool IsAttacking()
    {
        switch (this.currentAction)
        {
            case ActionsEnum.Attack_Regular:
                return true;
            case ActionsEnum.Attack_Special:
                return true;
            default:
                return false;
        }
    }

    public bool ShouldMove()
    {
        if (!IsAttacking())
        {
            return false;
        }
        else if (!this.characterController.OpponentController.ActionController.IsAttacking())
        {
            return true;
        }
        else if (ShouldClash())
        {
            return true;
        }
        else
        {
            if (this.characterController.CharacterStateController.AdvantagePoints
            >= this.characterController.OpponentController.CharacterStateController.AdvantagePoints)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public bool ShouldFlinch()
    {
        if (this.currentAction != ActionsEnum.Guard
            && this.characterController.OpponentController.ActionController.IsAttacking())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool ShouldCounter()
    {
        if (this.currentAction == ActionsEnum.Guard
        && this.characterController.OpponentController.ActionController.CurrentAction == ActionsEnum.Attack_Special)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool ShouldClash()
    {
        if (this.currentAction == ActionsEnum.Attack_Special
        && this.characterController.OpponentController.ActionController.CurrentAction == ActionsEnum.Attack_Special)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool HaveAdvantage()
    {
        if (this.characterController.CharacterStateController.AdvantagePoints > this.characterController.OpponentController.CharacterStateController.AdvantagePoints)
        {
            return true;
        }
        else if (this.characterController.CharacterStateController.AdvantagePoints < this.characterController.OpponentController.CharacterStateController.AdvantagePoints)
        {
            return false;
        }
        else
        {
            if (this.characterController.IsPlayerCharacter)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    #endregion
}
