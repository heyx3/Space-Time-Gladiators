using UnityEngine;
using System.Collections;

/// <summary>
/// Removes this game object from the collision tracker once it is destroyed.
/// </summary>
public class RemoveFromTracker : MonoBehaviour
{
    void OnDestroy()
    {
        StateMachine st = GetComponent<StateMachine>();

        if (st != null)
        {
            WorldConstants.ColTracker.RemoveActor(st);
        }
        else
        {
            WorldConstants.ColTracker.RemoveOther(gameObject);
        }
    }
}