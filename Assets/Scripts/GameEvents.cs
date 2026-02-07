using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents
{
    /// <summary>
    /// 请求发射小球（方向）
    /// </summary>
    public static Action<Vector2> OnShootRequest;

    /// <summary>
    /// 一轮发射结束
    /// </summary>
    public static Action OnRoundFinished;

    public static Action<GameObject> OnFirstBallAnchored;

    public static Action GameOver;
}
