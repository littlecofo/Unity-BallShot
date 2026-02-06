using System.Collections;
using UnityEngine;

public class BallBehavior : MonoBehaviour
{
    private Rigidbody2D rb;
    private static BallBehavior anchorBall;
    private static Vector2 anchorPos;
    private bool isMovingToAnchor = false;
    public float moveToAnchorSpeed = 8f;

    // 新增：统计已销毁的非锚点球数量
    private static int destroyedCount = 0;
    private static int expectedToDestroy = 0;

    public static void SetExpectedToDestroy(int count)
    {
        expectedToDestroy = count;
        destroyedCount = 0;
        anchorBall = null;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bottom"))
        {
            if (anchorBall == null)
            {
                // 第一个触底球，成为锚点
                anchorBall = this;
                anchorPos = transform.position;
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
                GameEvents.OnFirstBallAnchored?.Invoke(gameObject);
            }
            else
            {
                // 其他球，停止并移动到锚点
                if (!isMovingToAnchor)
                {
                    rb.velocity = Vector2.zero;
                    rb.isKinematic = true;
                    StartCoroutine(MoveToAnchor());
                }
            }
        }
    }

    IEnumerator MoveToAnchor()
    {
        isMovingToAnchor = true;
        while (Vector2.Distance(transform.position, anchorPos) > 0.05f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                anchorPos,
                moveToAnchorSpeed * Time.deltaTime
            );
            yield return null;
        }
        transform.position = anchorPos;
        // 销毁前统计
        destroyedCount++;
        if (destroyedCount >= expectedToDestroy)
        {
            GameEvents.OnRoundFinished?.Invoke();
        }
        Destroy(gameObject);
    }
}
