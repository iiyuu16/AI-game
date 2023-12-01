using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Enemy Configuration", menuName = "ScriptableObject/Enemy Configuration")]
public class EnemyScriptableObject : ScriptableObject
{
    public float AIUpdateInterval = 0.1f;
    public float Acceleration = 8f;
    public float AngularSpeed = 120f;
    public int AreaMask = -1;
    public int AvoidancePriority = 50;
    public float BaseOffset = 0f;
    public float Height = 2f;
    public ObstacleAvoidanceType ObstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
    public float Radius = 0.5f;
    public float Speed = 3f;
    public float StoppingDistance = 0.5f;

    //base stats
    public int health = 100;
    public Enemy Prefab;
    public AttackScriptableObject AttackConfiguration;

    public void SetupEnemy(Enemy enemy)
    {
        enemy.agent.acceleration = Acceleration;
        enemy.agent.angularSpeed = AngularSpeed;
        enemy.agent.areaMask = AreaMask;
        enemy.agent.avoidancePriority = AvoidancePriority;
        enemy.agent.baseOffset = BaseOffset;
        enemy.agent.height = Height;
        enemy.agent.obstacleAvoidanceType = ObstacleAvoidanceType;
        enemy.agent.radius = Radius;
        enemy.agent.speed = Speed;
        enemy.agent.stoppingDistance = StoppingDistance;

        enemy.movement.updateSpeed = AIUpdateInterval;

        AttackConfiguration.SetupEnemy(enemy);
    }
}
