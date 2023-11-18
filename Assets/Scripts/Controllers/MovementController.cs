using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
  [SerializeField] private Transform characterTransform;
  [SerializeField] private Transform opponentTransform;
  [SerializeField] private Transform originPoint;
  [SerializeField] private Transform middlePoint;
  [SerializeField] private Transform opponentOriginPoint;
  private CharacterController characterController;
  public CharacterController CharacterController => characterController;
  private bool moving = false;
  public bool Moving => moving;

  private void Awake()
  {
    this.characterController = GetComponent<CharacterController>();
  }

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.A))
    {
      MoveToOrigin();
    }
    if (Input.GetKeyDown(KeyCode.B))
    {
      MoveToMiddle();
    }
    if (Input.GetKeyDown(KeyCode.C))
    {
      MoveToOpponent();
    }
  }
  public void MoveToOrigin(float travelTime = .5f)
  {
    if (this.moving)
    {
      return;
    }

    this.moving = true;

    StartCoroutine(StartMovingToTransform(this.originPoint, travelTime));
  }

  public void MoveToMiddle(float travelTime = .5f)
  {
    if (this.moving)
    {
      return;
    }

    StartCoroutine(StartMovingToTransform(this.middlePoint, travelTime));
  }

  public void MoveToOpponent(float travelTime = .5f)
  {
    if (this.moving)
    {
      return;
    }

    StartCoroutine(StartMovingToTransform(this.opponentTransform, travelTime, true));
  }

  public void MoveToOpponentOrigin(float travelTime = .5f)
  {
    if (this.moving)
    {
      return;
    }

    StartCoroutine(StartMovingToTransform(this.opponentOriginPoint, travelTime));
  }

  public IEnumerator StartMovingToTransform(Transform destinationTransform, float travelTime, bool updateDestination = false)
  {
    float elapsedTime = 0;
    Vector3 currentPosition = this.transform.position;
    float currentMoveValue;
    Vector3 destination = destinationTransform.position;
    Vector3 direction = (currentPosition - destination).normalized;

    yield return new WaitUntil(() =>
    {
      elapsedTime += Time.deltaTime;

      if (updateDestination)
      {
        destination = new Vector3(destinationTransform.position.x + (direction.x * .2f), destinationTransform.position.y, destinationTransform.position.z);
      }

      currentMoveValue = Mathf.Lerp(currentPosition.x, destination.x, elapsedTime / travelTime);

      this.characterTransform.position = new Vector3(currentMoveValue, currentPosition.y, currentPosition.z);

      return elapsedTime >= travelTime;
    });

    this.moving = false;
  }
}
