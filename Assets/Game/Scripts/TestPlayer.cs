using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class TestPlayer : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float speed = 40f;
    [SerializeField] private float diveRadians = 0.1f;
    [SerializeField] private float turnRadians = 0.2f;
    
    [Header("Bounding Box")]
    [SerializeField] private float minX = 40f;
    [FormerlySerializedAs("maximumX")][SerializeField] private float maxX = 40f;
    [SerializeField] private float minZ = 40f;
    [FormerlySerializedAs("maximumZ")] [SerializeField] private float maxZ = 40f;
    
    [Header("Attack")]
    [SerializeField] private MMF_Player attackFeedbacks;
    [SerializeField] private MMSimpleObjectPooler pool;
    [SerializeField] private List<Transform> attackPositions;
    [SerializeField] private float attackCooldown = 0.1f;
    
    [Header("Dodge")]
    [SerializeField] private MMF_Player dodgeLeftFeedbacks;
    [SerializeField] private MMF_Player dodgeRightFeedbacks;
    [SerializeField] private float dodgeDuration = 2.0f;
    [SerializeField] private float dodgeCooldown = 2.5f;
    [SerializeField] private Collider dodgeCollider;
    
    [Header("Boost")]
    [SerializeField] private MMF_Player accelerationFeedbacks;
    [SerializeField] private MMF_Player decelerationFeedbacks;
    
    private Vector2 _movementInput;

    private bool _attackPressed;
    private bool _dodging;
    private bool _boosting;
    
    private float _internalAttackTimer;
    private float _internalDodgeTimer;
    private float _internalDodgeDuration;
    
    private void Update()
    {
        // Update cooldowns
        if (_internalAttackTimer > 0) { _internalAttackTimer -= Time.deltaTime; }
        if (_internalDodgeTimer > 0) { _internalDodgeTimer -= Time.deltaTime; }
        
        // Update Dodge duration
        if (_internalDodgeDuration > 0) { _internalDodgeDuration -= Time.deltaTime; }
        
        Move();
        
        if (_attackPressed) { Fire(); }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _movementInput = context.ReadValue<Vector2>();
    }

    private void Move()
    {
        var finalX = _movementInput.x;
        var finalY = _movementInput.y;
        
        if (transform.localPosition.x > maxX)
        {
            finalY = 1;
        }

        if (transform.localPosition.x < minX)
        {
            finalY = -1;
        }

        if (transform.localPosition.z > maxZ)
        {
            finalX = -1;
        }
        
        if (transform.localPosition.z < minZ)
        {
            finalX = 1;
        }
        
        var movement = new Vector3(-1 * finalY, 0, finalX);
        
        transform.Translate(movement * (speed * Time.deltaTime), Space.World);

        if (_internalDodgeDuration > 0) return;
        
        switch (_movementInput.y)
        {
            case < 0:
                var transform1 = transform;
                var rotation1 = transform1.rotation;
                rotation1 = new Quaternion(rotation1.x, rotation1.y, -turnRadians, rotation1.w);
                transform1.rotation = rotation1;
                break;
            case > 0:
                var transform2 = transform;
                var rotation2 = transform2.rotation;
                rotation2 = new Quaternion(rotation2.x, rotation2.y, turnRadians, rotation2.w);
                transform2.rotation = rotation2;
                break;
            default:
                var transform3 = transform;
                var rotation3 = transform3.rotation;
                rotation3 = new Quaternion(rotation3.x, rotation3.y, 0, rotation3.w);
                transform3.rotation = rotation3;
                break;
        }

        switch (_movementInput.x)
        {
            case < 0:
                var transform1 = transform;
                var rotation1 = transform1.rotation;
                rotation1 = new Quaternion(-diveRadians, rotation1.y, rotation1.z, rotation1.w);
                transform1.rotation = rotation1;
                break;
            case > 0:
                var transform2 = transform;
                var rotation2 = transform2.rotation;
                rotation2 = new Quaternion(diveRadians, rotation2.y, rotation2.z, rotation2.w);
                transform2.rotation = rotation2;
                break;
            default:
                var transform3 = transform;
                var rotation3 = transform3.rotation;
                rotation3 = new Quaternion(0, rotation3.y, rotation3.z, rotation3.w);
                transform3.rotation = rotation3;
                break;
        }
    }
    
    public void OnAttack(InputAction.CallbackContext context)
    {
        //Debug.Log("Started: " + context.started);
        //Debug.Log("Performed: " + context.performed);
        //Debug.Log("Read as Button: " + context.ReadValueAsButton());

        _attackPressed = context.ReadValueAsButton();
    }

    private void Fire()
    {
        if (_internalDodgeDuration > 0) return;
        if (_internalAttackTimer > 0) return;
        _internalAttackTimer = attackCooldown;
        
        var projectile = pool.GetPooledGameObject();

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
        
        if (attackFeedbacks != null)
        {
            attackFeedbacks.PlayFeedbacks();
        }
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (_internalDodgeTimer > 0) return;
        _internalDodgeTimer = dodgeCooldown;
        _internalDodgeDuration = dodgeDuration;
        
        StartCoroutine(DodgeDurationCo());
        
        // switch to play feedback direction according to movement on X axis
        switch (_movementInput.y)
        {
            case > 0:
            {
                if (dodgeLeftFeedbacks != null) { dodgeLeftFeedbacks.PlayFeedbacks(); }

                break;
            }
            case < 0:
            {
                if (dodgeRightFeedbacks != null) { dodgeRightFeedbacks.PlayFeedbacks(); }

                break;
            }
            case 0:
            {
                var coin = Random.value;
                if (coin > 0.5)
                {
                    if (dodgeLeftFeedbacks != null) { dodgeLeftFeedbacks.PlayFeedbacks(); }
                }
                else
                {
                    if (dodgeRightFeedbacks != null) { dodgeRightFeedbacks.PlayFeedbacks(); }
                }

                break;
            }
        }
    }

    private IEnumerator DodgeDurationCo()
    {
        dodgeCollider.enabled = false;
        
        while (_internalDodgeDuration > 0)
        {
            yield return null;
        }
        
        dodgeCollider.enabled = true;
    }
    
    public void OnBoost(InputAction.CallbackContext context)
    {
        Debug.Log("Boosting: " + context.ReadValueAsButton());
        
        if (context.ReadValueAsButton())
        {
            if (_boosting) return;
            _boosting = true;
            
            if (accelerationFeedbacks == null) return;
            accelerationFeedbacks.PlayFeedbacks();
        }
        else
        {
            if (!_boosting) return;
            _boosting = false;
            
            if (accelerationFeedbacks == null || decelerationFeedbacks == null) return;
            accelerationFeedbacks.StopFeedbacks();
            decelerationFeedbacks.PlayFeedbacks();
        }
    }
}
