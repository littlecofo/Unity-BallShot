using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fakeBall : MonoBehaviour
{
    public LayerMask wallLayerMask; // 在 Inspector 选择墙所在 Layer
    private LineRenderer lineRenderer;

    public void RenewLine(Vector2 inDir, RaycastHit2D hit)
    {
        Vector2 reflectedDir = Vector2.Reflect(inDir, hit.normal);
        RaycastHit2D[] hit2 = Physics2D.RaycastAll(
            (Vector2)this.transform.position,
            reflectedDir,
            20f,
            wallLayerMask
        );
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, new Vector3(hit.point.x, hit.point.y, 0));
        foreach (var h in hit2)
        {
            if (h.collider != hit.collider)
            {
                lineRenderer.SetPosition(1, new Vector3(h.point.x, h.point.y, 0));
                return;
            }
        }
    }

    void Start()
    {
        lineRenderer = this.GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update() { }
}
