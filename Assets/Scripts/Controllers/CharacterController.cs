using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AnimationController))]
[RequireComponent(typeof(MovementController))]
[RequireComponent(typeof(CharacterStateController))]
[RequireComponent(typeof(ActionController))]
public class CharacterController : MonoBehaviour
{
    [SerializeField] private bool isPlayerCharacter = false;
    public bool IsPlayerCharacter => isPlayerCharacter;
    [SerializeField] private CharacterController opponentController;
    public CharacterController OpponentController => opponentController;
    private AnimationController animationController;
    public AnimationController AnimationController => animationController;
    private MovementController movementController;
    public MovementController MovementController => movementController;
    private CharacterStateController characterStateController;
    public CharacterStateController CharacterStateController => characterStateController;
    private ActionController actionController;
    public ActionController ActionController => actionController;

    private void Awake()
    {
        this.characterStateController = GetComponent<CharacterStateController>();
        this.movementController = GetComponent<MovementController>();
        this.animationController = GetComponent<AnimationController>();
        this.actionController = GetComponent<ActionController>();
    }
}
