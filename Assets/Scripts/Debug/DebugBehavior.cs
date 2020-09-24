using UnityEngine;
//using UnityEditor;
using System.Collections;

public class DebugBehavior : MonoBehaviour {
	
//	CollisionTracker tracker;
//	
//	public bool run = false;
//	
//	// Use this for initialization
//	void Awake () {
//		tracker = GetComponent<CollisionTracker>();
//	}
//	
//	// Update is called once per frame
//	void Update () {
//		
//		if (!run) return;
//		
//		if (Selection.activeGameObject != null &&
//			Selection.activeGameObject.collider != null)
//		{
//			RecBounds b = new RecBounds(Selection.activeGameObject.collider.bounds);
//			RecBounds b2;
//			Line l;
//			
//			//for (float x = b.left; x < b.right; x += 1.0f)
//			//	for (float y = b.bottom; y < b.top; y += 1.0f)
//			//  {
//			//		b2 = new RecBounds(new Vector2(x + 0.5f, y + 0.5f), new Vector2(1.0f, 1.0f));
//			b2 = new RecBounds(new Vector2(b.right - 0.5f, b.top - 0.5f), new Vector2(1.0f, 1.0f));
//					if (tracker.Lines.WallExists(b2))
//					{
//						foreach (ColType c in tracker.Lines.GetSides (b2)) {
//							
//							l = tracker.Lines.GetLine (b2, c);
//						
//							Debug.DrawLine (l.Endpoint1, l.Endpoint2, ToCol (c));
//						}
//					}
//			//  }
//		}
//	}
//	
//	Color ToCol(ColType c)
//	{
//		switch (c)
//		{
//			case ColType.Bottom:
//				return Color.black;
//			case ColType.Top:
//				return Color.white;
//			case ColType.Left:
//				return Color.yellow;
//			case ColType.Right:
//				return Color.green;
//			
//			default: throw new System.NotImplementedException();
//		}
//	}
}
