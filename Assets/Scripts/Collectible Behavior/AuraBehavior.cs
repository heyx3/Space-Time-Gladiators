using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class AuraBehavior : MonoBehaviour
{	
	bool init = false;
	void Update ()
    {
        if (init)
        {
            return;
        }
		
		GetComponent<Animator>().CurrentAnimation = Animations.Ob_Aura;
		init = true;
	}

    public ParticleHandler ParticleEffects = null;

    public void TookDamage()
    {

    }
    public void DealtDamage()
    {

    }

    public void VIPChanged()
    {
		if (transform.parent == null)
		{
			return;
		}
		
		Transform t = transform;
		t.position = t.parent.position;
		
        renderer.material.SetColor("_TeamCol", t.parent.GetComponent<IDData>().Team);
    }
}
