using UnityEngine;

public class FrameRate : MonoBehaviour {

    public int desirableFrameRate = 60;

    private void Start()
    {
        Application.targetFrameRate = desirableFrameRate;
    }
}
