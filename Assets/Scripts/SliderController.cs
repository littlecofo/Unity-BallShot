using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public Slider uiSlider;
    public GameObject fakeBall;
    public Transform origin;
    public LineRenderer lineRenderer;
    public float previewLength = 10f;
    public float halfAngle = 60f;
    public LayerMask wallLayerMask; // 在 Inspector 选择墙所在 Layer

    [Header("动画设置")]
    public float tweenDuration = 0.25f;
    public Ease tweenEase = Ease.OutCubic;
    public bool enableWidthPulse = true;
    public float pulseScale = 1.6f;
    public float pulseDuration = 0.15f;

    Tween endTween;
    Tween widthTween;

    void Reset()
    {
        if (uiSlider == null)
            uiSlider = GetComponent<Slider>();
    }

    void OnEnable()
    {
        if (uiSlider != null)
            uiSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnDisable()
    {
        if (uiSlider != null)
            uiSlider.onValueChanged.RemoveListener(OnSliderChanged);
        endTween?.Kill();
        widthTween?.Kill();
    }

    void OnFirstBallAnchored(GameObject newMainBall)
    {
        // 当第一个球锚定时，更新 origin 与 lineRenderer 引用
        origin = newMainBall != null ? newMainBall.transform : null;
        lineRenderer = newMainBall != null ? newMainBall.GetComponent<LineRenderer>() : null;
        UpdatePreviewInstant();
    }

    void Start()
    {
        if (lineRenderer != null)
        {
            if (lineRenderer.positionCount < 2)
                lineRenderer.positionCount = 2;
        }
        UpdatePreviewInstant();
        GameEvents.OnFirstBallAnchored += OnFirstBallAnchored;
    }

    void OnValidate()
    {
        // 编辑器模式下也尽量安全地刷新（本类不再实例化任何对象）
        UpdatePreviewInstant();
    }

    void OnSliderChanged(float _) => UpdatePreviewAnimated();

    Vector3 ComputeDirection()
    {
        if (uiSlider == null)
            return Vector3.up;
        float range = Mathf.Max(0.0001f, uiSlider.maxValue - uiSlider.minValue);
        float t = (uiSlider.value - uiSlider.minValue) / range;
        float angle = Mathf.Lerp(halfAngle, -halfAngle, t);

        return Quaternion.Euler(0f, 0f, angle) * Vector3.up;
    }

    public void UpdatePreviewInstant()
    {
        if (origin == null || uiSlider == null)
            return;

        Vector3 start = origin.position;
        Vector3 dir3 = ComputeDirection().normalized;
        Vector3 end = start + dir3 * previewLength;

        RaycastHit2D hit2 = Physics2D.Raycast(
            (Vector2)start,
            new Vector2(dir3.x, dir3.y),
            previewLength,
            wallLayerMask
        );
        if (hit2.collider != null)
            end = hit2.point;

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }
        else
        {
            Debug.DrawLine(start, end, Color.green);
        }
    }

    //更新瞄准线
    public void UpdatePreviewAnimated()
    {
        if (origin == null || uiSlider == null)
            return;

        Vector3 start = origin.position;
        Vector3 dir3 = ComputeDirection().normalized;
        Vector3 end = start + dir3 * previewLength;

        RaycastHit2D hit2 = Physics2D.Raycast(
            (Vector2)start,
            (Vector2)dir3,
            previewLength,
            wallLayerMask
        );

        if (hit2.collider != null)
        {
            end = hit2.point;

            if (fakeBall != null)
            {
                fakeBall.transform.position = new Vector3(end.x, end.y, -0.1f);

                fakeBall.GetComponent<fakeBall>()?.RenewLine((Vector2)dir3, hit2);
            }
        }

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }
        else
        {
            Debug.DrawLine(start, end, Color.green);
        }
    }

    public Vector3 GetAimDirectionWorld()
    {
        return ComputeDirection();
    }

    // 由 UI Button 的 OnClick 调用：触发发射事件
    public void Fire()
    {
        Vector3 dir3 = GetAimDirectionWorld();
        Vector2 dir2 = new Vector2(dir3.x, dir3.y).normalized;
        GameEvents.OnShootRequest?.Invoke(dir2);
    }
}
