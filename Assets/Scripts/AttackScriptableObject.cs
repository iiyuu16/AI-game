using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Enemy Configuration", menuName = "ScriptableObject/Attack Configuration")]
public class AttackScriptableObject : ScriptableObject
{
    public bool isRanged = false;
    public int Damage = 5;
    public float AttackRadius = 1.5f;
    public float AttackDelay = 1.5f;

    //Ranged Config
    public Bullet BulletPrefab;
    public Vector3 BulletSpawnOffset = new Vector3(0, 1, 0);
    public LayerMask LineOfSightLayers;

    public void SetupEnemy(Enemy enemy)
    {
        (enemy.attackRadius.Collider == null ? enemy.attackRadius.GetComponent<SphereCollider>() : enemy.attackRadius.Collider).radius = AttackRadius;
        enemy.attackRadius.attackDelay = AttackDelay;
        enemy.attackRadius.damage = Damage;

        if(isRanged)
        {
            RangedAttackRadius rangedAttackRadius = enemy.attackRadius.GetComponent<RangedAttackRadius>();

            rangedAttackRadius.bulletPrefab = BulletPrefab;
            rangedAttackRadius.bulletSpawnOffset = BulletSpawnOffset;
            rangedAttackRadius.mask = LineOfSightLayers;

            rangedAttackRadius.CreateBulletPool();
        }
    }
}
