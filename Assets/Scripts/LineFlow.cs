using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineFlow : MonoBehaviour
{
    public Material flowMaterial; // 指派带条纹/渐变贴图的材质（支持透明）
    public Vector2 flowDirection = new Vector2(1f, 0f); // 流动方向（如 (1,0) 为向右）
    public float flowDuration = 1f; // 每次位移耗时（越小越快）
    public bool playOnEnable = true;

    LineRenderer lr;
    Material instMat;
    Tween flowTween;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        if (flowMaterial != null)
        {
            instMat = Instantiate(flowMaterial);
            lr.material = instMat;
        }
    }

    void OnEnable()
    {
        if (playOnEnable)
            StartFlow();
    }

    void OnDisable()
    {
        StopFlow();
    }

    public void StartFlow()
    {
        if (instMat == null)
            return;
        flowTween?.Kill();
        Vector2 dir = flowDirection.normalized;
        flowTween = DOTween
            .To(
                () => instMat.mainTextureOffset,
                x => instMat.mainTextureOffset = x,
                instMat.mainTextureOffset + dir,
                flowDuration
            )
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental);
    }

    public void StopFlow()
    {
        flowTween?.Kill();
    }

    void OnDestroy()
    {
        flowTween?.Kill();
        if (instMat != null)
            Destroy(instMat);
    }
}
