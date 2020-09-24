using UnityEngine;
using System.Collections;

/// <summary>
/// The behavior/traits of a flag base.
/// </summary>
[RequireComponent(typeof(Animator))]
public class FlagBaseBehavior : MonoBehaviour
{
	public Color Team;
	public FlagBehavior Flag;
	public CTFRules ctfRules { get { return Flag.ctfRules; } }
	
	public void SetData(Color team, FlagBehavior flag) {
		Team = team;
		Flag = flag;
		renderer.material.SetColor ("_TeamCol", Team);
	}
}

