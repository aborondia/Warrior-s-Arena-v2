using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateController : MonoBehaviour
{
  [SerializeField] private int attackPower = 10;
  public int AttackPower => attackPower;
  [SerializeField] private int healthPoints = 100;
  public int HealthPoints => healthPoints;
  [SerializeField][Range(0, 10)] private int advantagePoints = 5;
  public int AdvantagePoints => advantagePoints;
  private bool dead = false;
  public bool Dead => dead;

  public void TakeDamage(int damage)
  {
    this.healthPoints -= damage;

    if (this.healthPoints <= 0)
    {
      this.dead = true;
    }
  }
}
