using System;
using MoreMountains.Feedbacks;
using UnityEngine;

public class TestTarget : MonoBehaviour
{
    public MMF_Player hitFeedbacks;

    public void OnDamage()
    {
        if (hitFeedbacks != null)
        {
            hitFeedbacks.PlayFeedbacks();
        }
    }
    
    // private void OnTriggerEnter(Collider other)
    // {
    //     if (hitFeedbacks != null)
    //     {
    //         hitFeedbacks.PlayFeedbacks();
    //     }
    // }
}
