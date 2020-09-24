using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using Vector2 = UnityEngine.Vector2;

public static class MathF
{
    public static System.Random R = new System.Random();

    private static readonly float Pi = (float)Math.PI;
    private static readonly float TwoPi = (float)Math.PI * 2.0f;
    private static readonly Interval ASineInterval = new Interval(-1, 1, true, 4);

    /// <summary>
    /// Gets a sine function with the given properties.
    /// </summary>
    /// <param name="ValueWhenXIsZero">The value that this sine function should give when X is zero.</param>
    static public Func<float, float> GetSine(float centerY, float amplitude, float xMultiplier, float ValueWhenXIsZero)
    {
        return x => centerY + (amplitude * (float)System.Math.Sin((xMultiplier * x) +
                                                                  System.Math.Asin(ASineInterval.Wrap((ValueWhenXIsZero - centerY) / amplitude))));
    }
    /// <summary>
    /// Gets a sine function with the given properties.
    /// </summary>
    /// <param name="offset">The X and Y offsets (Y offset is the middle value of the sine wave. X offset is the value when X is zero).</param>
    /// <param name="periodAndAmplitude">A Vector2 with the X component equal to the period, and the Y component equal to the amplitude.</param>
    static public Func<float, float> GetSine(float[] offsetXY, float[] periodAndAmplitude)
    {
        return GetSine(offsetXY[1], periodAndAmplitude[1], TwoPi / periodAndAmplitude[0], offsetXY[0]);
    }
    /// <summary>
    /// Gets a sine function with the given properties.
    /// </summary>
    /// <param name="oscillationRange">The range of values the sine function oscillates through.</param>
    /// <param name="period">The x distance between two peaks or two valleys.</param>
    /// <param name="valueWhenXIsZero">The value of this sine wave when X is zero.</param>
    static public Func<float, float> GetSine(Interval oscillationRange, float period, float valueWhenXIsZero)
    {
        return GetSine(oscillationRange.Center, oscillationRange.Range * 0.5f, TwoPi / period, valueWhenXIsZero);
    }

    public enum RectCorners
    {
        TopLeft,
        BottomLeft,
        TopRight,
        BottomRight,
    }
    /// <summary>
    /// Creates a logarithmic function.
    /// </summary>
    /// <param name="values">A rectangle representing two points in the logarithm: one corner is the logarithmic origin point (on the simple function "log(x, b)" this would be (1, 0)), and the opposite corner is any arbitrary point on the function.</param>
    /// <param name="logBase">The logarithm base. Affects the slope of the function.</param>
    /// <param name="originPoint">Which corner of "values" is the logarithmic origin.</param>
    /// <returns>A logarithm function with the given properties.</returns>
    static public Func<float, float> GetLog(float[] valueBoundsTopLeft, float[] valueBoundsBottomRight, RectCorners originPoint)
    {
        float[] sign = new float[2] { 1, 1 };
        if (originPoint == RectCorners.TopLeft || originPoint == RectCorners.TopRight)
            sign[1] = -1;
        if (originPoint == RectCorners.TopRight || originPoint == RectCorners.BottomRight)
            sign[0] = -1;

        float[] origin = new float[2] { 0, 0 };
        switch (originPoint)
        {
            case RectCorners.BottomLeft:
                origin = new float[2] { valueBoundsTopLeft[0], valueBoundsBottomRight[1] };
                break;
            case RectCorners.BottomRight:
                origin = valueBoundsBottomRight;
                break;
            case RectCorners.TopLeft:
                origin = valueBoundsTopLeft;
                break;
            case RectCorners.TopRight:
                origin = new float[2] { valueBoundsBottomRight[0], valueBoundsTopLeft[1] };
                break;
        }

        float a = sign[1] * (valueBoundsTopLeft[1] - valueBoundsBottomRight[1]) / (float)System.Math.Log(valueBoundsBottomRight[0] - valueBoundsTopLeft[0], System.Math.E);
        return f => origin[1] + (a * (float)System.Math.Log(sign[0] * (f - origin[0])));
    }
    /// <summary>
    /// Creates a logarithmic function.
    /// </summary>
    /// <param name="originOfFuncXY">The origin point (on the simple function "log(x, b)" this would be (1, 0))</param>
    /// <param name="logBase">The logarithm base. Affects the slope of the function.</param>
    /// <param name="fartherPointXY">Any other arbitrary point on the function.</param>
    /// <returns>A logarithm function with the given properties.</returns>
    static public Func<float, float> GetLog(float[] originOfFuncXY, float[] fartherPointXY)
    {
        float top = System.Math.Min(originOfFuncXY[1], fartherPointXY[1]);
        float bottom = System.Math.Max(originOfFuncXY[1], fartherPointXY[1]);
        float left = System.Math.Min(originOfFuncXY[0], fartherPointXY[0]);
        float right = System.Math.Max(originOfFuncXY[0], fartherPointXY[0]);

        RectCorners corner = RectCorners.TopLeft;
        if (originOfFuncXY[0] < fartherPointXY[0] && originOfFuncXY[1] > fartherPointXY[1])
            corner = RectCorners.BottomLeft;
        if (originOfFuncXY[0] > fartherPointXY[0] && originOfFuncXY[1] < fartherPointXY[1])
            corner = RectCorners.TopRight;
        if (originOfFuncXY[0] > fartherPointXY[0] && originOfFuncXY[1] > fartherPointXY[1])
            corner = RectCorners.BottomRight;

        return GetLog(new float[2] { left, top }, new float[2] { right, bottom }, corner);
    }


    /// <summary>
    /// Finds the angle that points from "currentVector" to "nextVector."
    /// </summary>
    static public float FindRotation(Vector2 currentVector, Vector2 nextVector)
    {
        //Declare starting variables.
        float x = nextVector.x - currentVector.x;
        float y = nextVector.y - currentVector.y;

        //Use trig to find the desired Angle.
        float desiredAngle = (float)System.Math.Atan2(y, x);
        return desiredAngle;
    }
    ///<summary>
    ///Finds the rotation to go from the origin to the given Vector2.
    ///</summary>
    static public float FindRotation(Vector2 v)
    {
        return FindRotation(Vector2.zero, v);
    }

    /// <summary>
    /// Finds the direction to get from currentVector to nextVector.
    /// </summary>
    /// <param name="currentVector">The current position.</param>
    /// <param name="nextVector">The position to get to.</param>
    /// <param name="normalized">Whether or not the return result should be normalized.</param>
    /// <returns>The normalized vector2 pointing where to go to get to nextVector from currentVector, or "Vector2.zero" if the two points are equal..</returns>
    static public Vector2 FindDirection(Vector2 currentVector, Vector2 nextVector)
    {
        if (currentVector == nextVector)
            return Vector2.zero;

        Vector2 returnVector = nextVector - currentVector;
        if (true && returnVector != Vector2.zero) returnVector.Normalize();
        return returnVector;
    }
    /// <summary>
    /// Finds the normalized direction to point in given a rotation.
    /// </summary>
    /// <returns>The direction to point in given a rotation (normalized).</returns>
    static public Vector2 FindDirection(float rotation)
    {
        return new Vector2((float)System.Math.Cos(rotation), (float)System.Math.Sin(rotation));
    }

    /// <summary>
    /// Rotates a Vector2 with the given rotation and length around the 'origin' vector.
    /// </summary>
    /// <param name="currentRad">The current rotation of the Vector2 to rotate.</param>
    /// <param name="radChange">The amount to rotate the Vector2.</param>
    /// <param name="length">The length of the Vector2 to rotate.</param>
    /// <param name="origin">The Vector2 to rotate the image around.</param>
    /// <returns>A Vector2 equal to the Vector2 with the given information rotated around "origin."</returns>
    static public Vector2 Rotate(float currentRad, float radChange, float length, Vector2 origin)
    {		
        if (length == 0) return Vector2.zero;
        else if (radChange == 0)
            return FindDirection(currentRad) * length;

        //First find the target rotation.
        float final = currentRad + radChange;
        while (final < 0)
            final += TwoPi;
        while (final >= TwoPi)
            final -= TwoPi;
		
		//Quaternion rot = Quaternion.AngleAxis(radChange, new Vector3(0, 0, 1));
		//return origin + (Vector2)(
		
		
		
        //Now, see if the target rotation is +/- 90/180 degrees.
        if (final == 0)
            return new Vector2(origin.x + length, origin.y);
        else if (final == Pi * 0.5f)
            return new Vector2(origin.x, origin.y + length);
        else if (final == -Pi * 0.5f)
            return new Vector2(origin.x, origin.y - length);
        else if (System.Math.Abs(final) == Pi)
            return new Vector2(origin.x - length, origin.y);

        //Otherwise use trig to get the answer.
        else if ((final < Pi && final > Pi * 0.5f) ||
                 (final > -Pi & final < -Pi * 0.5f))
        {
            final -= Pi * 0.5f;
            return origin - new Vector2(length * (float)System.Math.Sin(final), -length * (float)System.Math.Cos(final));
        }

        return origin + new Vector2(length * (float)System.Math.Cos(final), length * (float)System.Math.Sin(final));
    }
}
