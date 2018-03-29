using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(GridHolder))]
public class UserInput : MonoBehaviour
{
    GridHolder GridHolder;

    public KeyCode Up = KeyCode.UpArrow;
    public KeyCode Down = KeyCode.DownArrow;
    public KeyCode Right = KeyCode.RightArrow;
    public KeyCode Left = KeyCode.LeftArrow;
    public float Wait = 0.1f;
    private float _LastKeyTime;

    Direction direction = Direction.None;
    void Awake()
    {
        GridHolder = GetComponent<GridHolder>();
    }
    void Update()
    {
        Profiler.BeginSample("UserInput");
        if (Input.GetKeyDown(Up)) direction = Direction.Up;
        else if (Input.GetKeyDown(Down)) direction = Direction.Down;
        else if (Input.GetKeyDown(Right)) direction = Direction.Right;
        else if (Input.GetKeyDown(Left)) direction = Direction.Left;

        if (direction != Direction.None && Time.time > _LastKeyTime + Wait)
        {
            _LastKeyTime = Time.time;
            GridHolder.Move(direction);
            direction = Direction.None;
        }
        Profiler.EndSample();
    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
