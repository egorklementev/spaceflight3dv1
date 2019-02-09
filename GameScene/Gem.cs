using System.Collections;
using UnityEngine;

public class Gem {

    public GameObject GameObj { get; set; }
    public GemColor Color { get; set; }
    public bool IsActing { get; set; }
    public Vector3 LocalScale { get; set; }
    public int ColorIndex { get; set; }
    public Vector2 GridPos { get; set; }

    public Gem(GameObject gameObject, GemColor color)
    {
        GameObj = gameObject;
        Color = color;
        IsActing = false;
        LocalScale = GameObj.transform.localScale;
        ColorIndex = GetColorIndex(Color);
    }

    public IEnumerator Scale(float sizeParam, float speed)
    {
        Vector3 scale = GameObj.transform.localScale;
        for (float t = 0; t < 1f; t += Time.deltaTime * speed)
        {
            if (GameObj != null)
            {
                GameObj.transform.localScale = Vector3.Lerp(scale, scale * sizeParam, t);
                yield return null;
            }
            else
            {
                break;
            }
        }
    }

    // Scales the gem to it's original local scale
    public IEnumerator ScaleToLocal(float speed)
    {
        Vector3 scale = GameObj.transform.localScale;
        for (float t = 0; t < 1f; t += Time.deltaTime * speed)
        {
            if (GameObj != null)
            {
                GameObj.transform.localScale = Vector3.Lerp(scale, LocalScale, t);
                yield return null;
            }
            else
            {
                break;
            }
        }
    }

    public IEnumerator Move(Vector2 destination, Vector3 origin, float gemOffset, float time)
    {
        Vector3 velocity = Vector3.zero; velocity.x += .1f;
        Vector3 newPosition = new Vector3(
            destination.x * gemOffset + origin.x,
            destination.y * gemOffset + origin.y,
            GameObj.transform.position.z
            );
        while (velocity.sqrMagnitude > .001f)
        {
            if (GameObj != null)
            {
                IsActing = true;
                GameObj.transform.position = Vector3.SmoothDamp(GameObj.transform.position, newPosition, ref velocity, time);
                yield return null;
            }
            else
            {
                break;
            }
        }
        IsActing = false;
    }
    
    public enum GemColor
    {
        RED, BLUE, GREEN, YELLOW, MAGENTA, WHITE, ORANGE, DIAMOND
    }

    private int GetColorIndex(GemColor color)
    {
        if (color.Equals(GemColor.BLUE)) return 0;
        if (color.Equals(GemColor.DIAMOND)) return 1;
        if (color.Equals(GemColor.GREEN)) return 2;
        if (color.Equals(GemColor.MAGENTA)) return 3;
        if (color.Equals(GemColor.ORANGE)) return 4;
        if (color.Equals(GemColor.RED)) return 5;
        if (color.Equals(GemColor.WHITE)) return 6;
        if (color.Equals(GemColor.YELLOW)) return 7;
        return -1;
    }

}
