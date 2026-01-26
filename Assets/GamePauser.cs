using UnityEngine;

public class GamePauser : MonoBehaviour
{
    private float timeScale;

    public void PauseGame()
    {
        timeScale = Time.timeScale;
        Time.timeScale = 0;
    }

    public void UnpauseGame()
    {
        Time.timeScale = timeScale;
    }
}
