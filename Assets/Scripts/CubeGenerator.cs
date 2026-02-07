using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CubeGenerator : MonoBehaviour
{
    public int gridWidth = 8;
    public int gridHeight = 10;
    public float cellSize = 1f;
    public Vector2 gridOrigin = new Vector2(-3.5f, 4.5f);
    public GameObject cubePrefab;

    // 回合结束时调用
    public void OnRoundFinished()
    {
        GenerateNewRow();
        MoveAllCubesDown();
    }

    void GenerateNewRow()
    {
        int count = Random.Range(2, 6); // 2~5个
        HashSet<int> used = new HashSet<int>();
        for (int i = 0; i < count; i++)
        {
            int col;
            do
            {
                col = Random.Range(0, gridWidth);
            } while (used.Contains(col));
            used.Add(col);

            float x = gridOrigin.x + col * cellSize;
            float y = gridOrigin.y + cellSize;
            Vector2 pos = new Vector2(x, y);

            GameObject cube = Instantiate(cubePrefab, pos, cubePrefab.transform.rotation);

            // 设置血量为20~90的10倍数
            int hp = Random.Range(2, 10); // 2~9
            hp = hp * 10; // 20~90
            var ctrl = cube.GetComponent<CubeController>();
            if (ctrl != null)
                ctrl.SetHP(hp);
        }
    }

    void MoveAllCubesDown()
    {
        var cubes = GameObject.FindGameObjectsWithTag("Cube"); // 方块预制体需设置Tag为"Cube"
        foreach (var cube in cubes)
        {
            Vector3 target = cube.transform.position + Vector3.down * cellSize;
            cube.transform.DOMove(target, 0.3f).SetEase(Ease.OutCubic);
            cube.GetComponent<CubeController>()?.CheakPosition();
        }

    }

    void ClearAllCubes()
    {
        var cubes = GameObject.FindGameObjectsWithTag("Cube");
        foreach (var cube in cubes)
        {
            Destroy(cube);
        }
    }

    void OnEnable()
    {
        GameEvents.OnRoundFinished += OnRoundFinished;
        GameEvents.GameOver += ClearAllCubes;
    }

    void OnDisable()
    {
        GameEvents.OnRoundFinished -= OnRoundFinished;
        GameEvents.GameOver -= ClearAllCubes;
    }

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }
}
