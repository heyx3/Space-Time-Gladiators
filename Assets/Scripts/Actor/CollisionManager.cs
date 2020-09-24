using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Handles collision events.
/// Sends the following messages:
/// ActorCollision(int),
/// OtherCollision(int),
/// WallCollision(ColIndexPair)
/// The int values in the messages are the index in the correct collision list of the collision.
/// </summary>
public class CollisionManager : MonoBehaviour
{
	public static string ToString(ColType c)
	{
		switch (c)
		{
			case ColType.Bottom:
				return "Bottom";
			case ColType.Top:
				return "Top";
			case ColType.Left:
				return "Left";
			case ColType.Right:
				return "Right";
				
			default: throw new NotImplementedException();
		}
	}
	
    public static ColType CollisionType(RecBounds actor, Vector2 velocity, RecBounds wall)
    {
        //TODO: [SO FAR, THIS DOESN'T SEEM NECESSARY]. If the player hit a wall but only barely, and the y interval below/above the wall that the player is mainly occupying is clear, let him pass through the wall.

        //Figure out which edge was collided with first: top/bottom, or side. Return that edge.

        //1) Get the closest wall corner to the actor's center, and the opposite corner on the actor's bounds.

        Vector2 lastCenter = actor.center - (velocity * Time.fixedDeltaTime);
        ColType topOrBottom = (lastCenter.y > wall.center.y) ? ColType.Top : ColType.Bottom;
		ColType leftOrRight = (lastCenter.x > wall.center.x) ? ColType.Right : ColType.Left;
        //If the actor is going at least half a unit per second, use
		
        Vector2 playerCenter = new Vector2(actor.center.x, actor.center.y);
        Vector2 wallCorner = new Vector2(wall.center.x - wall.extents.x, wall.center.y - wall.extents.y);
        Vector2 actorOppositeCorner = new Vector2(actor.center.x + actor.extents.x, actor.center.y + actor.extents.y);
        float temp, tempDistSqr = System.Single.MaxValue;

        //Go through every corner searching for the closest.
        Vector2 tempDir, tempCorner;
        for (int i = 0; i < 4; ++i)
        {
            switch (i)
            {
                case 0:
                    tempDir = new Vector2(-1.0f, -1.0f);
                    break;

                case 1:
                    tempDir = new Vector2(-1.0f, 1.0f);
                    break;

                case 2:
                    tempDir = new Vector2(1.0f, -1.0f);
                    break;

                case 3:
                    tempDir = new Vector2(1.0f, 1.0f);
                    break;

                default:
                    tempDir = Vector2.zero;
                    break;
            }

            tempCorner = new Vector2(wall.center.x + (tempDir.x * wall.extents.x),
                                     wall.center.y + (tempDir.y * wall.extents.y));
            temp = Mathf.Pow(playerCenter.x - tempCorner.x, 2.0f) + Mathf.Pow(playerCenter.y - tempCorner.y, 2.0f);

            if (temp < tempDistSqr)
            {
                tempDistSqr = temp;
                actorOppositeCorner = new Vector2(actor.center.x - (tempDir.x * actor.extents.x),
                                                  actor.center.y - (tempDir.y * actor.extents.y));
                wallCorner = tempCorner;
            }
        }

        //2) Get the amount of overlap along both dimensions.
        Vector2 overlap = wallCorner - actorOppositeCorner;

        //3) Handle edge cases for the overlap.
        const float marginOfErrorOverlap = 0.01f;
        if (Mathf.Abs(overlap.x) <= marginOfErrorOverlap)
            return leftOrRight;
        if (Mathf.Abs(overlap.y) <= marginOfErrorOverlap)
            return topOrBottom;

        //4) Handle edge cases for velocity.
        //No margin of error is needed because StateMachines already set the velocity to 0 when under a certain magnitude.
        if (velocity.x == 0)
			return topOrBottom;
        if (velocity.y == 0)
			return leftOrRight;
		
        //5) Get the amount of time needed at the actor's current velocity
        //   to move both the side and top/bottom edges back to bare contact with the wall.
        Vector2 t = new Vector2(Mathf.Abs(overlap.x / velocity.x), Mathf.Abs(overlap.y / velocity.y));

        //6) Depending on which edge would take longer to move back to bare contact
        //   (i.e. which edge was collided with first), return the correct value.
        if (t.x < t.y) return leftOrRight;
        else return topOrBottom;
    }
    public static bool OnLine(Line l, ColType side, RecBounds b)
    {
		const float error = 0.01f;
		
        switch (side)
        {
            case ColType.Bottom:

                //Make sure this line makes sense.
                if (l.Dir == Line.Orientation.Vertical)
                    throw new ArgumentException();

                //Next, if the bound's top edge isn't touching the line, exit.
                float top = b.center.y + b.extents.y;
                float lineTop = l.ConstValue;
                if (!StateMachine.WithinError(top, lineTop, error)) return false;
			
                //Finally, check to make sure the bounds are within the range of the line.
                return l.LineRange.Touches(new Interval(b.center.x - b.extents.x, b.center.x + b.extents.x, true, 2));

            case ColType.Top:

                //Make sure this line makes sense.
                if (l.Dir == Line.Orientation.Vertical)
                    throw new ArgumentException();

                //Next, if the bound's bottom edge isn't touching the line, exit.
                float bottom = b.center.y - b.extents.y;
                float lineBottom = l.ConstValue;
                if (!StateMachine.WithinError(bottom, lineBottom, error)) return false;

                //Finally, check to make sure the bounds are within the range of the line.
                return l.LineRange.Touches(new Interval(b.center.x - b.extents.x, b.center.x + b.extents.x, true, 2));

            case ColType.Left:

                //Make sure this line makes sense.
                if (l.Dir == Line.Orientation.Horizontal)
                    throw new ArgumentException();

                //Next, if the bound's right edge isn't touching the line, exit.
                float right = b.center.x + b.extents.x;
                float lineRight = l.ConstValue;
                if (!StateMachine.WithinError(right, lineRight, error)) return false;

                //Finally, check to make sure the bounds are within the range of the line.
                return l.LineRange.Touches(new Interval(b.center.y - b.extents.y, b.center.y + b.extents.y, true, 2));

            case ColType.Right:

                //Make sure this line makes sense.
                if (l.Dir == Line.Orientation.Horizontal)
                    throw new ArgumentException();

                //Next, if the bound's left edge isn't touching the line, exit.
                float left = b.center.x - b.extents.x;
                float lineLeft = l.ConstValue;
                if (!StateMachine.WithinError(left, lineLeft, error)) return false;

                //Finally, check to make sure the bounds are within the range of the line.
                return l.LineRange.Touches(new Interval(b.center.y - b.extents.y, b.center.y + b.extents.y, true, 2));

            default: throw new NotImplementedException();
        }
    }

	private StateMachine thisActor;
    private CollisionTracker colTracker { get { return WorldConstants.ColTracker; } }
	
	public float ColLineMarginOfError = 0.0f;
	
	public bool DebugStopOnCol = false;
	
    public Dictionary<ColType, List<Line>> WallSides;
	public List<StateMachine> Actors;
	public List<GameObject> Others;

    private bool disableVerticalColl = false;
    public void DisableVerticalCollisions()
    {
        disableVerticalColl = true;
        WallSides[ColType.Bottom].Clear();
        WallSides[ColType.Top].Clear();
        WorldConstants.Creator.AddTimer(new Timer(WorldConstants.CollObjConsts.PowerupDisableCollisionTime, false, t => disableVerticalColl = false), true);
    }

    void Awake()
    {
        WallSides = new Dictionary<ColType, List<Line>>();
        WallSides.Add(ColType.Bottom, new List<Line>());
        WallSides.Add(ColType.Top, new List<Line>());
        WallSides.Add(ColType.Left, new List<Line>());
        WallSides.Add(ColType.Right, new List<Line>());
		 
		Actors = new List<StateMachine>();
		Others = new List<GameObject>();
		
		thisActor = GetComponent<StateMachine>();
    }

	void CollideWithActor(StateMachine other)
    {
		if (!Actors.Contains (other))
        {
			Actors.Add (other);
			SendMessage("ActorCollision", Actors.Count - 1, SendMessageOptions.DontRequireReceiver);
		}
	}	
	void CollideWithOther(GameObject other)
    {
		if (!Others.Contains (other))
        {
			Others.Add (other);
			SendMessage("OtherCollision", Others.Count - 1, SendMessageOptions.DontRequireReceiver);
		}
	}
    void CollideWithWall(RecBounds bounds)
    {
        if (DebugStopOnCol)
        {
            DebugStopOnCol = DebugStopOnCol;
        }

        StateMachine.DrawBounds(bounds, Color.cyan);

        //Get the side of the wall that was collided with. Ignore collision if necessary.
		ColType c = CollisionType (thisActor.NextActorBounds, thisActor.Velocity, bounds);
        if (disableVerticalColl && (c == ColType.Bottom || c == ColType.Top))
        {
            return;
        }
		
		//Get the closest single wall inside the bound to the player.
		Vector2 wallCenter = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
		RecBounds tempB;
		
		wallCenter = new Vector2(Mathf.Clamp (wallCenter.x, bounds.left + 0.5f, bounds.right - 0.5f),
								 Mathf.Clamp (wallCenter.y, bounds.bottom + 0.5f, bounds.top - 0.5f));
        if (false && !bounds.Inside(wallCenter))
        {
            switch (c)
            {
                case ColType.Bottom:
                    wallCenter.y += 1.0f;
                    break;

                case ColType.Left:
                    wallCenter.x += 1.0f;
                    break;

                case ColType.Top:
                    wallCenter.y -= 1.0f;
                    break;

                case ColType.Right:
                    wallCenter.x -= 1.0f;
                    break;

                default: throw new NotImplementedException();
            }
        }
		
		tempB = new RecBounds(wallCenter, new Vector2(1.0f, 1.0f));
		
		Line l = null;
		
		//Check to see if the line exists in the collision tracker.
		if (!colTracker.Lines.WallExists(tempB))
		{
			//Debug.Log("Wall not valid: wall " + bounds + ", small wall " + tempB + ", side " + CollisionManager.ToString(c));
			
			//Create a substitute line.
			switch (c)
			{
				case ColType.Bottom:
					l = new Line(Line.Orientation.Horizontal, new Interval(tempB.left, tempB.right, true, 2), tempB.bottom);
					break;
				
				case ColType.Left:
					l = new Line(Line.Orientation.Vertical, new Interval(tempB.bottom, tempB.top, true, 2), tempB.left);
					break;
				
				case ColType.Top:
					l = new Line(Line.Orientation.Horizontal, new Interval(tempB.left, tempB.right, true, 2), tempB.top);
					break;
				
				case ColType.Right:
					l = new Line(Line.Orientation.Vertical, new Interval(tempB.bottom, tempB.top, true, 2), tempB.right);
					break;
			}
		}
        else if (!colTracker.Lines.SideExists(tempB, c))
        {
            //Debug.Log("Coll. side not valid: wall " + bounds + ", small wall " + tempB + ", side " + CollisionManager.ToString(c));

            //Create a substitute line.
            switch (c)
            {
                case ColType.Bottom:
                    l = new Line(Line.Orientation.Horizontal, new Interval(tempB.left, tempB.right, true, 2), tempB.bottom);
                    break;

                case ColType.Left:
                    l = new Line(Line.Orientation.Vertical, new Interval(tempB.bottom, tempB.top, true, 2), tempB.left);
                    break;

                case ColType.Top:
                    l = new Line(Line.Orientation.Horizontal, new Interval(tempB.left, tempB.right, true, 2), tempB.top);
                    break;

                case ColType.Right:
                    l = new Line(Line.Orientation.Vertical, new Interval(tempB.bottom, tempB.top, true, 2), tempB.right);
                    break;
            }
        }
        else
        {
            l = colTracker.Lines.GetLine(tempB, c);
        }
		
		l = new Line(l.Dir, new Interval(l.LineRange), l.ConstValue);
		l.LineRange.Range -= ColLineMarginOfError * 2.0f;

        //If the collision was already being tracked, don't change anything.
        if (WallSides[c].Contains(l))
        {
            return;
        }
        //Otherwise, start tracking the line.
        WallSides[c].Add(l);
        SendMessage("WallCollision", new ColIndexPair() { Index = WallSides[c].Count - 1, Type = c }, SendMessageOptions.DontRequireReceiver);
    }

    public void CheckWallBounds()
    {
        //Check for continuous collision with objects/lines.
        RecBounds b = thisActor.ActorBounds;

        //Actors.
        for (int i = 0; i < Actors.Count; ++i)
            if (!Actors[i].ActorBounds.Intersects(b))
                Actors.RemoveAt(i--);

        //Others.
        for (int i = 0; i < Others.Count; ++i)
			if (Others[i] == null ||
				!new RecBounds(Others[i].collider.bounds).Intersects(b))
                Others.RemoveAt(i--);

        //Wall lines.
        List<ColType> keys = WallSides.Keys.ToList();
		List<Line> tempL;
		Line tempLi;
        foreach (ColType c in keys)
            for (int i = 0; i < WallSides[c].Count; ++i)
			{
				tempL = WallSides[c];
				tempLi = tempL[i];
			
				if (colTracker.DrawLines)
					Debug.DrawLine (tempLi.Endpoint1, tempLi.Endpoint2, Color.white);
			
                if (!OnLine(WallSides[c][i], c, b))
                    WallSides[c].RemoveAt(i--);
			}
    }
}

/// <summary>
/// The different possible kinds of collision with a wall.
/// </summary>
public enum ColType
{
    Top,
    Bottom,
    Left,
	Right,
}

/// <summary>
/// Represents the data for a collision with a wall.
/// </summary>
public struct ColIndexPair
{
    public ColType Type;
    public int Index;
}