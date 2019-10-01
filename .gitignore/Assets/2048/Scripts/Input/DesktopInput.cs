using UnityEngine;

public class DesktopInput : MonoBehaviour, IGameInput
{
    public bool IsSlideToDown()
    {
        return Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);
    }

    public bool IsSlideToLeft()
    {
        return Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
    }

    public bool IsSlideToRight()
    {
        return Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
    }

    public bool IsSlideToUp()
    {
        return Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
    }
}