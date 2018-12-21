using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class GridUI : MonoBehaviour
{
    [Header("References")]
    public GridHolder GridHolder;
    public GameObject StaticCell;
    public GameObject DynamicCell;
    public GameObject FinishScreen;
#if DEBUG
    public bool ShowDebugWindow = true;
#endif
    public TMPro.TextMeshProUGUI NextGoalDisplay;
    public TMPro.TextMeshProUGUI BestScoreDisplay;
    public TMPro.TextMeshProUGUI[] ScoreDisplay;
    [Header("Animation")]
    public float ShrinkStretchTime = 0.25f;
    public float MoveSpeed = 30f;

    [Header("Grid Style")]
    public float Spacing = 5f;
    public RectOffset Padding;
    public CellStyle[] Styles;

    private int _BestScore;
    public int BestScore
    {
        get => _BestScore;
        set
        {
            if (BestScore <= value)
            {
                _BestScore = value;
                PlayerPrefs.SetInt("BestScore", BestScore);
                BestScoreDisplay.text = BestScore.ToString();
            }
        }
    }

    private int _NextGoal;
    public int NextGoal
    {
        get => _NextGoal;
        set
        {
            if (NextGoal <= value)
            {
                _NextGoal = value;
                PlayerPrefs.SetInt("NextGoal", NextGoal);
                NextGoalDisplay.text = $"Your next goal it to reach the {NextGoal} tile!";
            }
        }
    }

    private static GridUI Instance;
    private GridLayoutGroup _GridLayout;
    private RectTransform _Rect;
    private RectTransform[] StaticCells;
    private DynamicCellUI[] DynamicCells;
    private DynamicCellUI[] DynamicCells_Animate;
    public int AnimationCount
    {
        get
        {
            return _AnimationCount;
        }
        set
        {
            _AnimationCount = value;
            if (_AnimationCount == 0)
            {
                RefreshUI();
            }
        }
    }

    private void RefreshUI()
    {
        for (int x = 0; x < GridHolder.GridSize.x; x++)
            for (int y = 0; y < GridHolder.GridSize.y; y++)
            {
                DynamicCells[XY(x, y)].Value = GridHolder.Grid[x, y];
            }
    }

    private int _AnimationCount;
    private async void Awake()
    {
        Instance = this;
        _GridLayout = GetComponent<GridLayoutGroup>();
        _Rect = GetComponent<RectTransform>();

        GridHolder.OnNewCell += GridHolder_OnNewCell;
        GridHolder.OnCombineStarted += GridHolder_OnCombineStarted;
        GridHolder.OnMoveStarted += GridHolder_OnMoveStarted;
        GridHolder.OnScoreChanged += GridHolder_OnScoreChanged;
        GridHolder.OnLost += GridHolder_OnLost;

        StaticCells = new RectTransform[GridHolder.GridSize.x * GridHolder.GridSize.y];
        DynamicCells = new DynamicCellUI[StaticCells.Length];
        DynamicCells_Animate = new DynamicCellUI[DynamicCells.Length];
        for (int i = 0; i < StaticCells.Length; i++)
        {
            StaticCells[i] = Instantiate(StaticCell, transform).GetComponent<RectTransform>();
            DynamicCells[i] = Instantiate(DynamicCell, transform).GetComponent<DynamicCellUI>();
            DynamicCells_Animate[i] = Instantiate(DynamicCell, transform).GetComponent<DynamicCellUI>();


            StaticCells[i].hideFlags = HideFlags.HideInHierarchy;
            DynamicCells[i].name = "dynamic_" + i;
            DynamicCells_Animate[i].name = "dynamic_animate_" + i;

            DynamicCells[i].gameObject.SetActive(false);
            DynamicCells_Animate[i].gameObject.SetActive(false);
        }

        float width = (_Rect.sizeDelta.x - Padding.right - Padding.left);
        float height = (_Rect.sizeDelta.y - Padding.top - Padding.bottom);
        Vector2 cellSize = Vector2.zero;
        cellSize.x = (width - ((GridHolder.GridSize.x - 1) * Spacing)) / GridHolder.GridSize.x;
        cellSize.y = (height - ((GridHolder.GridSize.y - 1) * Spacing)) / GridHolder.GridSize.y;

        _GridLayout.spacing = Vector2.one * Spacing;
        _GridLayout.padding = Padding;
        _GridLayout.cellSize = cellSize;

        BestScore = PlayerPrefs.GetInt("BestScore", 0);
        NextGoal = PlayerPrefs.GetInt("NextGoal", GridHolder.LargestTile * 2);

        await Task.Delay(1000);
        for (int i = 0; i < StaticCells.Length; i++)
        {
            DynamicCells[i].Rect.SetAsLastSibling();
            DynamicCells[i].Rect.anchorMin = StaticCells[i].anchorMin;
            DynamicCells[i].Rect.anchorMax = StaticCells[i].anchorMax;
            DynamicCells[i].Rect.anchoredPosition = StaticCells[i].anchoredPosition;

            DynamicCells_Animate[i].Rect.SetAsLastSibling();
            DynamicCells_Animate[i].Rect.anchorMin = StaticCells[i].anchorMin;
            DynamicCells_Animate[i].Rect.anchorMax = StaticCells[i].anchorMax;
            DynamicCells_Animate[i].Rect.anchoredPosition = StaticCells[i].anchoredPosition;
        }
        _GridLayout.enabled = false;
        await Task.Delay(1000);
        GridHolder.Initialize();
    }
#if DEBUG
    private void OnGUI()
    {
        if (!ShowDebugWindow)
            return;
        float nextY = 10;
        for (int y = 0; y < GridHolder.GridSize.y; y++)
        {
            float nextX = 10f;
            for (int x = 0; x < GridHolder.GridSize.x; x++)
            {
                GUI.Button(new Rect(nextX, nextY, 30, 30), GridHolder.Grid[x, y].ToString());
                nextX += 32;
            }
            nextY += 32;
        }
    }
#endif
    private void GridHolder_OnLost()
    {
        FinishScreen.SetActive(true);
        enabled = false;
    }

    private void GridHolder_OnScoreChanged(int oldScore, int newScore)
    {
        BestScore = newScore;
        NextGoal = GridHolder.LargestTile * 2;

        string score = newScore.ToString();
        for (int i = 0; i < ScoreDisplay.Length; i++)
            ScoreDisplay[i].text = score;
    }

    private void GridHolder_OnMoveStarted(int x_to, int y_to, int x_from, int y_from)
    {
        AnimationCount++;
        int moveFrom = XY(x_from, y_from);
        int moveTo = XY(x_to, y_to);

        var cellUI1 = DynamicCells[moveFrom];
        var cellUI2 = DynamicCells[moveTo];

        cellUI2.Value = cellUI1.Value;
        cellUI2.Style = cellUI1.Style;

        cellUI1.gameObject.SetActive(false);
        cellUI2.gameObject.SetActive(true);

        cellUI2.Rect.anchoredPosition = StaticCells[moveFrom].anchoredPosition;
        cellUI2.Rect.sizeDelta = StaticCells[moveFrom].sizeDelta;

        cellUI2.Move(StaticCells[moveTo].anchoredPosition, MoveSpeed, () => { AnimationCount--; });
    }

    private void GridHolder_OnCombineStarted(int x_to, int y_to, int x_from, int y_from)
    {
        AnimationCount++;
        int moveFrom = XY(x_from, y_from);
        int moveTo = XY(x_to, y_to);

        var cellUI2 = DynamicCells[moveTo];
        var cellUI2_animate = DynamicCells_Animate[moveTo];

        int n1 = GridHolder.Grid[x_from, y_from];
        int n2 = GridHolder.Grid[x_to, y_to];


        cellUI2_animate.PasteFrom(DynamicCells[moveFrom]);
        cellUI2_animate.gameObject.SetActive(true);
        DynamicCells[moveFrom].gameObject.SetActive(false);

        cellUI2_animate.Value = n1;

        cellUI2_animate.Move(StaticCells[moveTo].anchoredPosition, MoveSpeed, () =>
        {
            cellUI2_animate.gameObject.SetActive(false);
            cellUI2.Shrink(ShrinkStretchTime * 0.5f, () =>
              {
                  cellUI2.Value = GridHolder.Combine(n1, n2); ;
                  cellUI2.Style = GetStyle(cellUI2.Value);

                  cellUI2.Stretch(StaticCells[moveTo].sizeDelta, ShrinkStretchTime * 0.5f);
                  AnimationCount--;
              });
        });
    }

    private void GridHolder_OnNewCell(int x, int y)
    {
        int i = XY(x, y);
        var cellUI = DynamicCells[i];

        cellUI.Rect.anchoredPosition = StaticCells[i].anchoredPosition;
        cellUI.gameObject.SetActive(true);
        cellUI.Rect.sizeDelta = Vector2.zero;
        cellUI.Stretch(StaticCells[0].sizeDelta, ShrinkStretchTime);
        cellUI.Value = GridHolder.Grid[x, y];
        cellUI.Style = GetStyle(GridHolder.Grid[x, y]);
    }
    private int XY(int x, int y)
    {
        return x + (y * GridHolder.GridSize.y);
    }

    public CellStyle GetStyle(int n)
    {
        return Styles[Mathf.Min((int)System.Math.Log(n, 2), Styles.Length - 1)];
    }

    [Serializable]
    public class CellStyle
    {
        public Color BackgroundColor = Color.white;
        public Color TextColor = Color.black;
    }
}
