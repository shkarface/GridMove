using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class DynamicCellUI : MonoBehaviour
{
    public RectTransform Rect
    {
        get
        {
            if (_Rect == null) _Rect = GetComponent<RectTransform>();
            return _Rect;
        }
    }
    public Image Image
    {
        get
        {
            if (_Image == null) _Image = GetComponent<Image>();
            return _Image;
        }
    }
    public int Value
    {
        get
        {
            return _Value;
        }
        set
        {
            _Value = value;
            Text.text = _Value.ToString();
        }
    }
    public GridUI.CellStyle Style
    {
        get
        {
            return _Style;
        }
        set
        {
            _Style = value;
            Image.color = _Style.BackgroundColor;
            Text.color = _Style.TextColor;
        }
    }

    public TextMeshProUGUI Text;

    private RectTransform _Rect;
    private Image _Image;
    [System.NonSerialized]
    private int _Value;
    [System.NonSerialized]
    private GridUI.CellStyle _Style;

    public void PasteFrom(DynamicCellUI other)
    {
        Value = other.Value;
        Style = other.Style;

        Rect.anchoredPosition = other.Rect.anchoredPosition;
        Rect.sizeDelta = other.Rect.sizeDelta;
    }
    public void Move(Vector2 pos, float speed, UnityAction callback = null)
    {
        StartCoroutine(IMove(pos, speed, callback));
    }
    public void Shrink(float time, UnityAction callback = null)
    {
        StartCoroutine(IAnimate(Vector2.zero, time,callback));
    }
    public void Stretch(Vector2 size, float time)
    {
        StartCoroutine(IAnimate(size, time,null));
    }

    private IEnumerator IMove(Vector2 newPos, float speed, UnityAction callback)
    {
        Vector2 startPos = Rect.anchoredPosition;

        
        while (Rect.anchoredPosition != newPos)
        {
            Rect.anchoredPosition = Vector2.MoveTowards(Rect.anchoredPosition, newPos, speed);
            yield return null;
        }
        callback?.Invoke();
    }
    private IEnumerator IAnimate(Vector2 size, float time, UnityAction callback)
    {
        Vector2 startSize = Rect.sizeDelta;
        float rate = 1f / time;
        float i = 0f;
        while (i < 1f)
        {
            i += Time.deltaTime * rate;
            Rect.sizeDelta = Vector2.Lerp(startSize, size, i);
            yield return null;
        }
        callback?.Invoke();
    }
}
