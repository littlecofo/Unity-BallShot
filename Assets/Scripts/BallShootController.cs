using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallShootController : MonoBehaviour
{
    [Header("Move Settings")]
    public float speed = 5f;
    public int totalBalls = 60;
    public float spawnInterval = 0.08f;

    private Vector2 shootPoint;
    public GameObject sliderUI; // 滑动条的父物体或自身
    public GameObject fireButton; // 发射按钮

    private Vector2 startDragPos;
    public GameObject ballPrefab;
    public GameObject mainBall;
    public SliderController sliderController;

    bool isShooting = false;
    Coroutine spawnRoutine;

    // Start is called before the first frame update
    void Awake() { }

    void OnEnable()
    {
        GameEvents.OnShootRequest += OnShoot;
        GameEvents.OnFirstBallAnchored += OnFirstBallAnchored;
        GameEvents.OnRoundFinished += OnRoundFinished;
    }

    void OnDisable()
    {
        GameEvents.OnShootRequest -= OnShoot;
        GameEvents.OnFirstBallAnchored -= OnFirstBallAnchored;
        GameEvents.OnRoundFinished -= OnRoundFinished;
    }

    void OnRoundFinished()
    {
        if (sliderUI != null)
            sliderUI.SetActive(true);
        if (fireButton != null)
            fireButton.SetActive(true);
        shootPoint = mainBall.transform.position;
        mainBall.GetComponent<LineRenderer>().enabled = true;
        sliderController.UpdatePreviewAnimated();
    }

    void OnFirstBallAnchored(GameObject newMainBall)
    {
        mainBall = newMainBall;
    }

    void Start()
    {
        shootPoint = mainBall.transform.position;
    }

    // Update is called once per frame
    void Update() { }

    public void OnShoot(Vector2 dir)
    {
        if (isShooting)
            return;
        isShooting = true;
        // 设置本轮要销毁的小球数（总球数-1，主球不销毁）
        BallBehavior.SetExpectedToDestroy(totalBalls - 1);
        // 隐藏滑动条和按钮
        if (sliderUI != null)
            sliderUI.SetActive(false);
        if (fireButton != null)
            fireButton.SetActive(false);

        // 隐藏所有小球的 LineRenderer
        foreach (var ball in GameObject.FindGameObjectsWithTag("Ball"))
        {
            var lr = ball.GetComponent<LineRenderer>();
            if (lr != null)
                lr.enabled = false;
        }

        Vector2 normDir = dir.normalized;

        // 发射主球
        if (mainBall != null)
        {
            var rb = mainBall.GetComponent<Rigidbody2D>();
            if (rb == null)
                rb = mainBall.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.isKinematic = false;
            rb.velocity = normDir * speed;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.sharedMaterial = rb.sharedMaterial;
        }

        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);
        spawnRoutine = StartCoroutine(SpawnRemaining(normDir));
    }

    IEnumerator SpawnRemaining(Vector2 dir)
    {
        int toSpawn = Mathf.Max(0, totalBalls - 1);
        for (int i = 0; i < toSpawn; i++)
        {
            if (ballPrefab != null)
            {
                var go = GameObject.Instantiate(
                    ballPrefab,
                    (Vector3)shootPoint,
                    Quaternion.identity
                );
                var lr = go.GetComponent<LineRenderer>();
                if (lr != null)
                    lr.enabled = false;
                var rb = go.GetComponent<Rigidbody2D>();
                if (rb == null)
                    rb = go.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.velocity = dir * speed;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
            yield return new WaitForSeconds(spawnInterval);
        }

        isShooting = false;
        spawnRoutine = null;
    }
}
