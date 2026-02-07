using DG.Tweening;
using TMPro;
using UnityEngine;

/*
每一个方块的控制脚本，负责：
- 维护血量显示
- 处理被小球击中时的血量减少、击中特效、销毁逻辑
- 在销毁时生成碎片和可选的方框掉落

使用方法：将此脚本挂载在方块预制体上。必须包含一个 TextMeshProUGUI 组件用于显示血量（可以在子对象中）。方块预制体需要设置 Tag 为 "Cube"。
*/

public class CubeController : MonoBehaviour
{
    [Header("配置")]
    public string ballTag = "Ball";
    public int initialHP = 1;

    [Header("击中特效（保留）")]
    public GameObject hitEffectPrefab;
    public float hitEffectScale = 1.5f;
    public float hitEffectDuration = 0.45f;

    [Header("碎片（销毁时生成）")]
    public GameObject debrisPrefab; // 小正方形碎片预制体（推荐 SpriteRenderer，大小 1 单位）
    public int debrisCountMin = 3;
    public int debrisCountMax = 6;
    public float debrisSpawnRadius = 0.3f; // 初始生成位置随机偏移
    public float debrisInitialImpulse = 1.5f; // 初始向下/随机速度幅度
    public float debrisGravity = 1.0f; // Rigidbody2D gravityScale
    public float debrisShrinkDuration = 0.8f; // 缩小时间
    public float debrisDelayMax = 0.12f; // 每个碎片缩小/销毁的随机延迟

    [Header("方框碎片（可选）")]
    public GameObject boxFallPrefab; // 在销毁时生成的方框预制体（会向下掉落并缩小）
    public float boxFallScale = 1f;
    public float boxFallDuration = 1f;

    TextMeshProUGUI hpText;
    public int initHP;
    int hp;
    /*
    为血量进行赋值，如果在子对象中找到了 TextMeshProUGUI 组件，则尝试从文本中解析初始血量，否则使用 initialHP 的值，并在控制台输出警告。
    */
    void Awake()
    {
        hpText = GetComponentInChildren<TextMeshProUGUI>(true);
        if (hpText != null)
        {
            if (!int.TryParse(hpText.text, out hp))
            {
                hp = initialHP;
                hpText.text = hp.ToString();
            }
        }
        else
        {
            hp = initialHP;
            Debug.LogWarning(
                $"CubeController ({name}): 未找到子对象中的 TextMeshProUGUI，使用 initialHP={initialHP}。"
            );
        }
    }
    /*
    当方块被小球击中时，减少血量并更新显示。如果血量降到 0，则触发销毁逻辑，包括生成碎片和可选的方框掉落。
    */
    void ApplyHit()
    {
        if (hp <= 0)
            return;

        hp = Mathf.Max(0, hp - 1);
        if (hpText != null)
            hpText.text = hp.ToString();

        // 击中特效
        PlayHitEffect();

        if (hp == 0)
        {
            OnDestroyed();
            Destroy(gameObject);
        }
    }

    void PlayHitEffect()
    {
        if (hitEffectPrefab == null)
            return;

        GameObject ef = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        var efSR = ef.GetComponent<SpriteRenderer>();
        // 固定为橙色
        if (efSR != null)
            efSR.color = new Color32(255, 165, 0, 255);

        // 初始 scale 由方块大小决定（尝试用 sprite bounds 或 transform）
        Vector3 startScale = Vector3.one;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            // sprite.bounds 是以 sprite 单位为准，乘以 transform.localScale 得到大致世界尺寸
            var b = sr.sprite.bounds;
            Vector3 worldSize = new Vector3(
                b.size.x * transform.localScale.x,
                b.size.y * transform.localScale.y,
                1f
            );
            startScale = worldSize;
        }
        ef.transform.localScale = Vector3.zero;

        Sequence seq = DOTween.Sequence();
        seq.Append(
            ef.transform.DOScale(startScale * hitEffectScale, hitEffectDuration)
                .SetEase(Ease.OutExpo)
        );
        if (efSR != null)
            seq.Join(efSR.DOFade(0f, hitEffectDuration));
        seq.OnComplete(() => Destroy(ef));
    }

    void OnDestroyed()
    {
        SpawnDebrisPieces();
        SpawnBoxFall();
    }

    void SpawnDebrisPieces()
    {
        if (debrisPrefab == null)
            return;

        int count = Random.Range(debrisCountMin, debrisCountMax + 1);
        for (int i = 0; i < count; i++)
        {
            // 随机圆形分布在方块周围
            Vector2 offset = Random.insideUnitCircle * debrisSpawnRadius;
            Vector3 spawnPos = transform.position + (Vector3)offset;

            GameObject d = Instantiate(debrisPrefab, spawnPos, Quaternion.identity);
            d.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

            Rigidbody2D rb = d.GetComponent<Rigidbody2D>();
            if (rb == null)
                rb = d.AddComponent<Rigidbody2D>();
            rb.gravityScale = debrisGravity;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // 横向（x）分量根据 offset.x 的方向和大小，产生“爆开”向外效果
            Vector2 dir = offset.normalized;
            float horStrength = Random.Range(
                debrisInitialImpulse * 0.6f,
                debrisInitialImpulse * 1.2f
            );
            float vertStrength = Random.Range(
                debrisInitialImpulse * 0.6f,
                debrisInitialImpulse * 1.1f
            );

            float hor = dir.x * horStrength; // 向外的横向速度
            float vert = -Mathf.Abs(Random.Range(vertStrength * 0.8f, vertStrength)); // 向下（负 y），保证向下方向

            // 如果 offset 非常小（几乎在中心），随机给一个左右方向，避免全部垂直下落
            if (dir.magnitude < 0.1f)
                hor = Mathf.Sign(Random.Range(-1f, 1f)) * horStrength * 0.6f;

            Vector2 impulse = new Vector2(hor, vert);
            rb.velocity = impulse;

            // 碎片使用预制体原始大小，不修改 localScale
            float delay = Random.Range(0f, debrisDelayMax);
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(delay);
            seq.Append(
                d.transform.DOScale(Vector3.zero, debrisShrinkDuration).SetEase(Ease.InQuad)
            );
            seq.OnComplete(() =>
            {
                if (d != null)
                    Destroy(d);
            });
        }
    }

    void SpawnBoxFall()
    {
        if (boxFallPrefab == null)
            return;

        GameObject box = Instantiate(boxFallPrefab, transform.position, Quaternion.identity);
        box.transform.localScale = Vector3.one * boxFallScale;

        Rigidbody2D rb = box.GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = box.AddComponent<Rigidbody2D>();
        rb.gravityScale = debrisGravity * 0.8f;
        rb.velocity = new Vector2(Random.Range(-0.4f, 0.4f), -Random.Range(0.6f, 1.2f));

        // 缩小并销毁
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(0.1f);
        seq.Append(box.transform.DOScale(Vector3.zero, boxFallDuration).SetEase(Ease.InQuad));
        seq.OnComplete(() =>
        {
            if (box != null)
                Destroy(box);
        });
    }

    // 2D 碰撞检测
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(ballTag))
            ApplyHit();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(ballTag))
            ApplyHit();
    }

    public void CheakPosition()
    {
        if (transform.position.y <= -3.4f)
        {
            GameEvents.GameOver?.Invoke();
        }
    }

    // 外部调用：设置血量
    public void SetHP(int value)
    {
        initialHP = value;
        hp = value;
        if (hpText != null)
            hpText.text = hp.ToString();
    }

    void Start()
    {
        SetHP(initialHP);
    }
}
