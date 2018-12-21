using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum Direction
{
    None = -1,
    Up = 0,
    Down = 1,
    Right = 2,
    Left = 3,
}
public class GridHolder : MonoBehaviour
{
    public event UnityAction OnLost;
    public event UnityAction<int, int, int, int> OnCombineStarted;
    public event UnityAction<int, int, int, int> OnMoveStarted;
    public event UnityAction<int, int> OnNewCell;
    public event UnityAction<int, int> OnScoreChanged;

    public int[] InitializeValues = new int[] { 2, 4 };

    public int Score
    {
        get
        {
            return _Score;
        }
        set
        {
            if (value != _Score)
            {
                OnScoreChanged?.Invoke(_Score, value);
                _Score = value;
            }
        }
    }
    private int _Score;

    public int[,] Grid
    {
        get
        {
            if (_Grid == null)
            {
                _Grid = new int[GridSize.x, GridSize.y];
            }
            return _Grid;
        }
        private set
        {
            _Grid = value;
        }
    }
    private int[,] _Grid;

    public Vector2Int GridSize = new Vector2Int(4, 4);

    protected void OnValidate()
    {
        GridSize = new Vector2Int(Mathf.Max(2, GridSize.x), Mathf.Max(2, GridSize.y));
    }
    protected bool MoveDown()
    {
        bool didMove = false;
        bool didCombineAny = false;
        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = GridSize.y - 1; y > 0; y--)
            {
                for (int j = y - 1; j >= 0; j--)
                {
                    if (Grid[x, j] == 0)
                        continue;

                    if (Grid[x, y] != 0)
                    {
                        if (CanCombine(Grid[x, y], Grid[x, j]))
                        {
                            Combine(x, y, x, j);
                            Grid[x, j] = 0;
                            didCombineAny = true;
                        }
                        else
                            break;
                    }
                    else
                    {
                        OnMoveStarted?.Invoke(x, y, x, j);
                        Grid[x, y] = Grid[x, j];
                        Grid[x, j] = 0;
                        didMove = true;
                    }
                }
            }
        }
        return didMove || didCombineAny;
    }
    protected bool MoveUp()
    {
        bool didMove = false;
        bool didCombineAny = false;
        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {

                for (int j = y + 1; j < GridSize.y; j++)
                {
                    if (Grid[x, j] == 0)
                        continue;

                    if (Grid[x, y] != 0)
                    {
                        if (CanCombine(Grid[x, y], Grid[x, j]))
                        {
                            Combine(x, y, x, j);
                            didCombineAny = true;
                        }
                        else
                            break;
                    }
                    else
                    {
                        OnMoveStarted?.Invoke(x, y, x, j);
                        Grid[x, y] = Grid[x, j];
                        Grid[x, j] = 0;
                        didMove = true;
                    }
                }
            }
        }
        return didMove || didCombineAny;
    }
    protected bool MoveLeft()
    {
        bool didMove = false;
        bool didCombineAny = false;
        for (int y = 0; y < GridSize.y; y++)
        {
            for (int x = 0; x < GridSize.x; x++)
            {

                for (int j = x + 1; j < GridSize.x; j++)
                {
                    if (Grid[j, y] == 0)
                        continue;

                    if (Grid[x, y] != 0)
                    {
                        if (CanCombine(Grid[x, y], Grid[j, y]))
                        {
                            Combine(x, y, j, y);
                            didCombineAny = true;
                        }
                        else
                            break;
                    }
                    else
                    {
                        OnMoveStarted?.Invoke(x, y, j, y);
                        Grid[x, y] = Grid[j, y];
                        Grid[j, y] = 0;
                        didMove = true;
                    }
                }
            }
        }
        return didMove || didCombineAny;
    }
    protected bool MoveRight()
    {
        bool didMove = false;
        bool didCombineAny = false;
        for (int y = 0; y < GridSize.y; y++)
        {
            for (int x = GridSize.x - 1; x > 0; x--)
            {
                for (int j = x - 1; j >= 0; j--)
                {
                    if (Grid[j, y] == 0)
                        continue;

                    if (Grid[x, y] != 0)
                    {
                        if (CanCombine(Grid[x, y], Grid[j, y]))
                        {
                            Combine(x, y, j, y);
                            Grid[j, y] = 0;
                            didCombineAny = true;
                        }
                        else
                            break;
                    }
                    else
                    {
                        OnMoveStarted?.Invoke(x, y, j, y);
                        Grid[x, y] = Grid[j, y];
                        Grid[j, y] = 0;
                        didMove = true;
                    }
                }
            }
        }
        return didMove || didCombineAny;
    }
    protected bool CanCombine(Vector2Int from, Vector2Int to)
    {
        return CanCombine(from.x, from.y, to.x, to.y);
    }
    protected bool CanCombine(int x1, int y1, int x2, int y2)
    {
        return CanCombine(Grid[x1, y1], Grid[x2, y2]);
    }
    protected bool CanCombine(int n1, int n2)
    {
        return n1 == n2;
    }
    protected void Combine(int x1, int y1, int x2, int y2)
    {
        OnCombineStarted?.Invoke(x1, y1, x2, y2);
        Grid[x1, y1] = Combine(Grid[x1, y1], Grid[x2, y2]);
        Grid[x2, y2] = 0;
        Score += Grid[x1, y1];
    }
    public int Combine(int n1, int n2)
    {
        return n1 + n2;
    }
    public void Initialize()
    {
        Grid = new int[GridSize.x, GridSize.y];
        AddRandom();
    }
    public void AddRandom(bool corners = true, bool stop = false)
    {
        int count = 0;
        int x = 0;
        int y = 0;
        do
        {
            count++;
            x = corners ? (Random.value > 0.49f ? GridSize.x - 1 : 0) : Random.Range(0, GridSize.x);
            y = corners ? (Random.value > 0.49f ? GridSize.y - 1 : 0) : Random.Range(0, GridSize.y);
        } while (Grid[x, y] != 0 && count < Grid.Length);
        if (count < Grid.Length)
        {
            Grid[x, y] = InitializeValues[Random.Range(0, InitializeValues.Length)];
            OnNewCell?.Invoke(x, y);
        }
        else if (corners && !stop)
        {
            AddRandom(false, true);
        }
        else
        {
            IsGameLost();
        }
    }
    public bool Move(Direction direction)
    {
        bool didMove = false;

        switch (direction)
        {
            case Direction.Down:
                didMove = MoveDown();
                break;
            case Direction.Up:
                didMove = MoveUp();
                break;
            case Direction.Right:
                didMove = MoveRight();
                break;
            case Direction.Left:
                didMove = MoveLeft();
                break;
        }
        if (IsGameLost())
            OnLost?.Invoke();
        else if (didMove)
            AddRandom();

        return didMove;
    }
    public bool IsGameLost()
    {
        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y; y++)
            {
                if (x > 0)
                    if (Grid[x - 1, y] == Grid[x, y] || Grid[x - 1, y] == 0)
                        return false;
                if (x < GridSize.x - 1)
                    if (Grid[x + 1, y] == Grid[x, y] || Grid[x + 1, y] == 0)
                        return false;
                if (y > 0)
                    if (Grid[x, y - 1] == Grid[x, y] || Grid[x, y - 1] == 0)
                        return false;
                if (y < GridSize.y - 1)
                    if (Grid[x, y + 1] == Grid[x, y] || Grid[x, y + 1] == 0)
                        return false;
            }
        return true;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GridHolder))]
public class GridHolderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var obj = (GridHolder)target;
        if (GUILayout.Button("Start"))
        {
            obj.Initialize();
        }
        else if (GUILayout.Button("AddRandomCorner"))
        {
            obj.AddRandom();
        }
        else if (GUILayout.Button("AddRandom"))
        {
            obj.AddRandom(false);
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        if (GUILayout.Button("Up"))
            obj.Move(Direction.Up);
        EditorGUILayout.Space();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Left"))
            obj.Move(Direction.Left);
        EditorGUILayout.Space();
        if (GUILayout.Button("Right"))
            obj.Move(Direction.Right);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        if (GUILayout.Button("Down"))
            obj.Move(Direction.Down);
        EditorGUILayout.Space();
        EditorGUILayout.EndHorizontal();

        if (obj.Grid.Length != (obj.GridSize.x * obj.GridSize.y))
            return;

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        for (int y = 0; y < obj.GridSize.y; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < obj.GridSize.x; x++)
            {
                obj.Grid[x, y] = EditorGUILayout.IntField(obj.Grid[x, y]);
                if (x != obj.GridSize.x - 1) EditorGUILayout.Space();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }
}
#endif
