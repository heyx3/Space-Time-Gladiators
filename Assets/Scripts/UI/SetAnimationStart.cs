using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
/// <summary>
/// Sets the animation for the GameObject.
/// </summary>
public class SetAnimationStart : MonoBehaviour {

    public Animations Anim = Animations.MM_Wall;

	// Use this for initialization
	void Start () {
        GetComponent<Animator>().CurrentAnimation = Anim;
	}
}
