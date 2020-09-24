using UnityEngine;
using System.Collections;

public class FloatingTextBehavior : MonoBehaviour {

    public float FadePerSec = 1.0f;
    public float YSpeed = 0.5f;

    Material mater;
    void Start()
    {
        mater = renderer.material;
    }

    Color mat;
    Vector3 pos;
    void FixedUpdate()
    {
        mat = mater.color;
        mater.color = new Color(mat.r, mat.g, mat.b, mat.a - (FadePerSec * Time.fixedDeltaTime));
        if (mater.color.a <= 0.0f)
        {
            GameObject.Destroy(gameObject);
            return;
        }

        pos = transform.position;
        transform.position = new Vector3(pos.x, pos.y + (YSpeed * Time.fixedDeltaTime), pos.y);
    }
}
