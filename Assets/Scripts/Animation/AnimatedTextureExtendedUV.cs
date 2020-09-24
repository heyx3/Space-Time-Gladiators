using UnityEngine;
using System.Collections;

public class AnimatedTextureExtendedUV : MonoBehaviour
{
    //Sheet data.
    public int Columns;
    public int Rows;

    //Animation data. They are zero-indexed.
    public int RowPos;
    public int ColumnPos;
    public int Frames;

    //Blinking.

    public bool Blink = false;
    public float BlinksPerSecond = 10;

    private float elapsedSinceBlink = 0.0f;
    private bool currentlyBlinked = false;
    private float blinkInterval { get { return 1.0f / BlinksPerSecond; } }

    //Frames.
    public int NonBlinkIndex;
    public int Index
    {
        get
        {
            if (!Blink || !currentlyBlinked)
            {
                return NonBlinkIndex;
            }
            else
            {
                return Columns * Rows;
            }
        }
    }
    public int Fps;

    //Timing.
    public double AnimationDoneTime;
    public double ElapsedTime;
    public bool IsAnimating = true;

    public Vector2 Offset;

    public float FrameWidth { get { return 1.0f / Columns; } }
    public float FrameHeight { get { return 1.0f / Rows; } }

    public void Update()
    {
        if (Columns == 0 || Frames == 0) return;

        UpdateBlinking();

        if (IsAnimating)
        {
            NonBlinkIndex = (int)(ElapsedTime * Fps);
            NonBlinkIndex = NonBlinkIndex % Frames;

            if (ElapsedTime >= AnimationDoneTime)
            {
                ElapsedTime = 0.0;
                SendMessage("AnimationDone", SendMessageOptions.DontRequireReceiver);
            }
            ElapsedTime += Time.deltaTime;
        }

        UpdateMesh();
    }

    /// <summary>
    /// Adds the given number of frames to the mesh's current frame.
    /// </summary>
    public void ChangeFrame(int deltaFrames)
    {
        NonBlinkIndex += deltaFrames;
        NonBlinkIndex = NonBlinkIndex % Frames;

        UpdateMesh();
    }

    /// <summary>
    /// Updates the mesh to reflect the current frame.
    /// </summary>
    private void UpdateMesh()
    {
        int uIndex = Index % Columns;
        int vIndex = Index / Columns;

        Offset = new Vector2(((float)uIndex + ColumnPos) / Columns,
                             (1.0f - (1.0f / Rows)) - ((vIndex + RowPos) * (1.0f / Rows)));

        renderer.material.SetTextureOffset("_MainTex", Offset);
    }

    /// <summary>
    /// Updates blinking behavior.
    /// </summary>
    private void UpdateBlinking()
    {
        elapsedSinceBlink += Time.deltaTime;
        if (elapsedSinceBlink > blinkInterval)
        {
            elapsedSinceBlink = 0.0f;
            currentlyBlinked = !currentlyBlinked;
        }
    }

    public void SetSpriteAnimation(int colCount, int rowCount, int colNumber, int rowNumber, int totalCells, int fps)
    {
        this.Columns = colCount;
        this.Rows = rowCount;
        this.RowPos = rowNumber;
        this.ColumnPos = colNumber;
        this.Frames = totalCells;
        this.Fps = fps;

        // Size of every cell
        float sizeX = 1.0f / colCount;
        float sizeY = 1.0f / rowCount;
        Vector2 size = new Vector2(sizeX, sizeY);
        renderer.material.SetTextureScale("_MainTex", size);

        //Timing.
        double d1 = totalCells;
        double d2 = fps;
        AnimationDoneTime = d1 / d2;
        ElapsedTime = -Time.deltaTime;

        Update();
    }
}