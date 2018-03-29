using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class GridUI : MonoBehaviour
{
    [Header("References")]
    public GridHolder GridHolder;
    public GameObject StaticCell;
    public GameObject DynamicCell;
    public GameObject FinishScreen;
    public TMPro.TextMeshProUGUI[] ScoreDisplay;

    [Header("Animation")]
    public float ShrinkStretchTime = 0.25f;
    public float MoveSpeed = 30f;

    [Header("Grid Style")]
    public float Spacing = 5f;
    public RectOffset Padding;
    public CellStyle[] Styles;

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
        {
            for (int y = 0; y < GridHolder.GridSize.y; y++)
            {
                DynamicCells[XY(x,y)].Value=GridHolder.Grid[x,y];
            }
        }
    }

    private int _AnimationCount;
    private void Awake()
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


        Invoke(() =>
        {
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
        }, 1);
        Invoke(() => { GridHolder.Initialize(); }, 1f);

    }
#if DEBUG
    private void OnGUI()
    {
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


    #region Timed Actions
    /// <summary>
    /// Invoke a method after a number of frames has rendered
    /// </summary>
    /// <param name="Method"></param>
    /// <param name="framesToWait"></param>
    public static void Invoke(UnityAction Method, int framesToWait)
    {
        if (Method == null) return;
        if (framesToWait == 0)
            Method.Invoke();
        else
            Instance.StartCoroutine(Instance.InvokeAfterFrames(Method, framesToWait));
    }
    /// <summary>
    /// Invoke a method after a number of frames has rendered
    /// </summary>
    /// <param name="Method"></param>
    /// <param name="value"></param>
    /// <param name="framesToWait"></param>
    public static void Invoke(UnityAction<bool> Method, bool value, int framesToWait)
    {
        if (Method == null) return;
        if (framesToWait == 0)
            Method.Invoke(value);
        else
            Instance.StartCoroutine(Instance.InvokeAfterFrames(Method, value, framesToWait));
    }
    /// <summary>
    /// Invoke a method after a specific time
    /// </summary>
    /// <param name="Method"></param>
    /// <param name="secondsToWait"></param>
    public static void Invoke(UnityAction Method, float secondsToWait)
    {
        if (Method == null) return;
        if (secondsToWait <= 0f)
            Method.Invoke();
        else
            Instance.StartCoroutine(Instance.InvokeInSeconds(Method, secondsToWait));
    }
    /// <summary>
    /// Invoke a method after a specific time
    /// </summary>
    /// <param name="Method"></param>
    /// <param name="value"></param>
    /// <param name="secondsToWait"></param>
    public static void Invoke(UnityAction<bool> Method, bool value, float secondsToWait)
    {
        if (Method == null) return;
        if (secondsToWait <= 0f)
            Method.Invoke(value);
        else
            Instance.StartCoroutine(Instance.InvokeInSeconds(Method, value, secondsToWait));
    }

    private IEnumerator InvokeAfterFrames(UnityAction Method, int frames)
    {
        for (int index = 0; index < frames; index++)
            yield return new WaitForEndOfFrame();
        Method?.Invoke();
    }
    private IEnumerator InvokeAfterFrames(UnityAction<bool> Method, bool value, int frames)
    {
        for (int index = 0; index < frames; index++)
            yield return new WaitForEndOfFrame();
        Method?.Invoke(value);
    }
    private IEnumerator InvokeInSeconds(UnityAction Method, float secondsToWait)
    {
        yield return new WaitForSeconds(secondsToWait);
        Method?.Invoke();
    }
    private IEnumerator InvokeInSeconds(UnityAction<bool> Method, bool value, float secondsToWait)
    {
        yield return new WaitForSeconds(secondsToWait);
        Method?.Invoke(value);
    }
    #endregion
    [System.Serializable]
    public class CellStyle
    {
        public Color BackgroundColor = Color.white;
        public Color TextColor = Color.black;
    }
}
