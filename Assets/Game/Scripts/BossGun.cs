using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

public class BossGun : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private MMF_Player attackFeedbacks;
    [SerializeField] private MMSimpleObjectPooler pool;
    [SerializeField] private List<Transform> attackPositions;
    [SerializeField] private float attackCooldown = 0.1f;
    [SerializeField] private float startCooldown = 3f;

    private GameObject _owner;
    private float _internalAttackTimer;
    private float _internalStartTimer;

    private void Start()
    {
        _internalStartTimer = startCooldown;
    }

    private void Update()
    {
        // Update cooldowns
        if (_internalStartTimer > 0) { _internalStartTimer -= Time.deltaTime; }
        if (_internalAttackTimer > 0) { _internalAttackTimer -= Time.deltaTime; }

        var sine = MathF.Sin(Time.time * 4);
        if (sine > 0) return;
        
        Fire();
    }

    private void Fire()
    {
        if (_internalStartTimer > 0) return;
        if (_internalAttackTimer > 0) return;
        _internalAttackTimer = attackCooldown;
        
        var projectile = pool.GetPooledGameObject();
        
        if (projectile.TryGetComponent<PooledProjectile>(out var pooledProjectile))
        {
            pooledProjectile.Owner = gameObject;
        }

        if (attackPositions.Count > 0)
        {
            var attackTransform = attackPositions[0];
            
            projectile.transform.position = attackTransform.position;
            projectile.transform.forward = attackTransform.forward;

            attackPositions.Remove(attackTransform);
            attackPositions.Add(attackTransform);
        }
        else
        {
            projectile.transform.position = transform.position;
            projectile.transform.forward = transform.forward;
        }

        projectile.SetActive(true);
        
        if (attackFeedbacks != null) { attackFeedbacks.PlayFeedbacks(); }
    }
}
