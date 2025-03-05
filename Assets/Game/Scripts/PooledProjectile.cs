using System;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

public class PooledProjectile : MMPoolableObject
{
    [Header("Attributes")]
    [SerializeField] private float speed = 150f;
    
    [FormerlySerializedAs("core")]
    [Header("Main GameObject")]
    [SerializeField] private GameObject coreVisuals;
    [SerializeField] private Rigidbody mainRigidBody;
    [SerializeField] private Collider mainCollider;
    
    [Header("Feedbacks")]
    [SerializeField] private MMF_Player spawnFeedbacks;
    [SerializeField] private MMF_Player destroyFeedbacks;
    [SerializeField] private MMF_Player lifeTimeFeedbacks;
    [SerializeField] private float destroyDelay = 3f;

    public GameObject Owner { get; set; }
    
    private bool _isDead = true;

    private void Start()
    {
        if (TryGetComponent<Rigidbody>(out var rb))
        { mainRigidBody = rb; }
        else { gameObject.AddComponent<Rigidbody>(); }
        
        if (TryGetComponent<Collider>(out var col))
        { mainCollider = col; }
        else { gameObject.AddComponent<BoxCollider>(); }
    }

    protected override void OnEnable()
    {
        _isDead = false;
        
        Size = GetBounds().extents * 2;
        if (LifeTime > 0f)
        {
            Invoke(nameof(OnLifeTime), LifeTime);
        }
        ExecuteOnEnable?.Invoke();
        
        coreVisuals.SetActive(true);
        mainCollider.enabled = true;
        
        if (destroyFeedbacks != null)
        {
            spawnFeedbacks.PlayFeedbacks();
        }

        SetRotation();

        SetHeight();
        
        mainRigidBody.linearVelocity = transform.forward * speed;
    }
    
    protected override void Update()
    {
        if (_isDead) return;

        if (transform.rotation.z != 0f || transform.rotation.x != 0f) { SetRotation(); }
        if (transform.position.y != 0f) { SetHeight(); }
    }
    
    private void OnLifeTime()
    {
        if (_isDead) return;
        _isDead = true;
        
        // disable visuals and collision
        coreVisuals.SetActive(false);
        mainCollider.enabled = false;
        
        if (destroyFeedbacks != null)
        {
            lifeTimeFeedbacks.PlayFeedbacks();
        }
        
        mainRigidBody.linearVelocity = Vector3.zero;
        
        Invoke(nameof(Destroy), destroyDelay);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == Owner) return;
        if (_isDead) return;
        _isDead = true;

        if (other.TryGetComponent<TestTarget>(out var target))
        {
            target.OnDamage();
        }
        
        // disable visuals and collision
        coreVisuals.SetActive(false);
        mainCollider.enabled = false;
        
        if (destroyFeedbacks != null)
        {
            destroyFeedbacks.PlayFeedbacks();
        }
        
        mainRigidBody.linearVelocity = Vector3.zero;
        
        Invoke(nameof(Destroy), destroyDelay);
    }

    private void SetHeight()
    {
        var thisTransform = transform;
        
        var currentPosition = thisTransform.position;
        var newPosition = new Vector3(currentPosition.x,0,currentPosition.z);
        thisTransform.position = newPosition;
    }

    private void SetRotation()
    {
        var thisTransform = transform;
        
        var currentRotation = thisTransform.rotation;
        var newRotation = new Quaternion(0, currentRotation.y, 0, currentRotation.w);
        thisTransform.rotation = newRotation;
    }
}
