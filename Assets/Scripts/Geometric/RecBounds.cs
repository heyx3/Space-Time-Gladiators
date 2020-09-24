using System.Collections;
using UnityEngine;

[System.Serializable]
/// <summary>
/// Represents 2-dimensional, non-rotated rectangular bounds.
/// </summary>
public struct RecBounds
{
	public Vector2 center, size;
	public Vector2 extents { get { return size * 0.5f; } }
	
	public float left { get { return center.x - extents.x; } }
	public float right { get { return center.x + extents.x; } }
	public float top { get { return center.y + extents.y; } }
	public float bottom { get { return center.y - extents.y; } }
	
	public RecBounds(Vector2 center, Vector2 size) { this.center = center; this.size = size; }
    public RecBounds(Bounds bounds3D) : this(new Vector2(bounds3D.center.x, bounds3D.center.y), new Vector2(bounds3D.size.x, bounds3D.size.y)) { }
	
	/// <summary>
	/// Finds if this RecBounds intersects the given one.
	/// </summary>
	public bool Intersects(RecBounds other) {
		return !(left > other.right ||
			     right < other.left ||
				 bottom > other.top ||
			 	 top < other.bottom);
	}

    /// <summary>
    /// Finds whether or not the given coordinate is in/on this RecBounds.
    /// </summary>
    public bool Touches(Vector2 coord)
    {
        return coord.x >= left &&
               coord.x <= right &&
               coord.y >= bottom &&
               coord.y <= top;
    }
    /// <summary>
    /// Finds if the given coordinate is inside this RecBounds.
    /// </summary>
    public bool Inside(Vector2 coord)
    {
        return coord.x > left &&
               coord.x < right &&
               coord.y > bottom &&
               coord.y < top;
    }

	public override bool Equals (object obj)
	{
		RecBounds? r = obj as RecBounds?;
		
		return r.HasValue && r == this;
	}
	public override int GetHashCode ()
	{
		return new Bounds(new Vector3(center.x, center.y, 0.0f), new Vector3(size.x, size.y, 0.0f)).GetHashCode ();
	}
	
	public override string ToString ()
	{
		return "Center: " + center + ", Size: " + size;
	}
	
	/// <summary>
	/// Determines whether a specified instance of <see cref="RecBounds"/> is equal to another specified <see cref="RecBounds"/>.
	/// Uses an error margin of 0.01 for checking floating-point equality.
	/// </summary>
	public static bool operator ==(RecBounds one, RecBounds two) {
		
		const float error = 0.01f;
		
		return WithinError(one.center.x, two.center.x, error) &&
			   WithinError(one.center.y, two.center.y, error) &&
			   WithinError(one.size.x, two.size.x, error) &&
			   WithinError(one.size.y, two.size.y, error);
	}
	/// <summary>
	/// Determines whether a specified instance of <see cref="RecBounds"/> is not equal to another specified <see cref="RecBounds"/>.
	/// Uses an error margin of 0.01 for checking floating-point equality.
	/// </summary>
	public static bool operator !=(RecBounds one, RecBounds two) {
		return !(one == two);
	}

	private static bool WithinError(float one, float two, float error) { return Abs (one - two) < error; }
    private static float Abs(float a) { return (a < 0.0f) ? -a : a; }
}

