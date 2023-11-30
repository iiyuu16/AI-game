using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RangedAttackRadius : AttackRadius
{
    public NavMeshAgent agent;
    public Bullet bulletPrefab;
    private Bullet bullet;
    public Vector3 bulletSpawnOffset = new Vector3(0, 1, 0);
    public LayerMask mask;
    private ObjectPool bulletPool;
    [SerializeField]
    private float SphereCastRadius = 0.1f;
    private RaycastHit hit;
    private IDamageable targetDamageable;

    protected override void Awake()
    {
        base.Awake();
        bulletPool = ObjectPool.CreateInstance(bulletPrefab, Mathf.CeilToInt((1 / attackDelay) * bulletPrefab.AutoDestroyTime));
    }

    protected override IEnumerator Attack()
    {
        WaitForSeconds Wait = new WaitForSeconds(attackDelay);
        yield return Wait;
        while (Damageables.Count > 0)
        {
            for (int i = 0; i < Damageables.Count; i++)
            {
                if (HasLineOfSight(Damageables[i].GetTransform()))
                {
                    targetDamageable = Damageables[i];
                    onAttack?.Invoke(Damageables[i]);
                    agent.isStopped = true;
                    break;
                }
            }

            if (targetDamageable != null)
            {
                PoolableObject poolableObject = bulletPool.GetObject();
                if (poolableObject != null)
                {
                    bullet = poolableObject.GetComponent<Bullet>();
                    bullet.Damage = damage;
                    bullet.transform.position = transform.position + bulletSpawnOffset;
                    bullet.transform.rotation = agent.transform.rotation;
                    bullet.Rigidbody.AddForce(agent.transform.forward * bulletPrefab.MoveSpeed, ForceMode.VelocityChange);
                }
            }
            else
            {
                agent.isStopped = false;
            }

            yield return Wait;

            if (targetDamageable == null || !HasLineOfSight(targetDamageable.GetTransform()))
            {
                agent.isStopped = false;
            }

            Damageables.RemoveAll(DisableDamageables);
        }

        agent.isStopped = false;
        attackCoroutine = null;
    }

    private bool HasLineOfSight(Transform target)
    {
        if (Physics.SphereCast(transform.position + bulletSpawnOffset, SphereCastRadius,
            ((target.position + bulletSpawnOffset) - (transform.position + bulletSpawnOffset)).normalized, out hit, Collider.radius, mask))
        {
            IDamageable damageable;
            if (hit.collider.TryGetComponent<IDamageable>(out damageable))
            {
                return damageable.GetTransform() == target;
            }
        }
        return false;
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        if (attackCoroutine == null)
        {
            agent.enabled = true;
        }
    }

}
