using UnityEngine;
using UnityEngine.UI;
using System;

public class CreateBoard : MonoBehaviour
{
    private const string _dirtTag = "Dirt";
    private const string _desertTag = "Desert";


    [Space]
    [Header("Objects Settings")]
    [Space]

    [SerializeField] private GameObject[] _tilePrefabs;
    [SerializeField] private GameObject[] _tiles;

    [SerializeField] private GameObject _housePrefab;
    [SerializeField] private GameObject _treePrefab;
    [SerializeField] private Text _scoreText;


    [Space]
    [Header("Points Settings")]
    [Space]

    [SerializeField] int pointsDirt = 10;
    [SerializeField] int pointsDesert = 2;

    private int _maxRows = 8;
    private int _maxCols = 8;

    private long _dirtBB = 0;
    private long _desertBB = 0;
    private long _treeBB = 0;
    private long _playerBB = 0;




    private void Start()
    {
        _tiles = new GameObject[_maxRows * _maxCols];

        for (int row = 0; row < _maxRows; ++row)
        {
            for (int col = 0; col < _maxCols; ++col)
            {
                int randomTileIndex = UnityEngine.Random.Range(0, _tilePrefabs.Length);
                Vector3 position = new Vector3(row, 0, col);

                GameObject tile = Instantiate(_tilePrefabs[randomTileIndex], position, Quaternion.identity, transform);
                tile.name = tile.tag + '_' + row + '_' + col;

                _tiles[row * _maxCols + col] = tile;

                ConfigureCellStates(tile, row, col);
            }
        }
        InvokeRepeating(nameof(PlantTree), 1, 1);
    }

    private void ConfigureCellStates(GameObject tile, int row, int col)
    {
        switch (tile.tag)
        {
            case _dirtTag:
                {
                    _dirtBB = SetCellState(_dirtBB, row, col);
                    break;
                }

            case _desertTag:
                {
                    _desertBB = SetCellState(_desertBB, row, col);
                    break;
                }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                GameObject hitObject = hit.collider.gameObject;
                int row = (int)hitObject.transform.position.x;
                int col = (int)hitObject.transform.position.z;

                long bb = (_dirtBB | _desertBB) & (~_playerBB & ~_treeBB);

                if (GetCellState(bb, row, col))
                {
                    GameObject house = Instantiate(_housePrefab);
                    house.transform.parent = hitObject.transform;
                    house.transform.localPosition = Vector3.zero;

                    _playerBB = SetCellState(_playerBB, row, col);
                    UpdateScore();
                }
            }
        }
    }

    private void PlantTree()
    {
        int randomRow = UnityEngine.Random.Range(0, _maxRows);
        int randomCol = UnityEngine.Random.Range(0, _maxCols);

        if (GetCellState(_dirtBB & (~_playerBB & ~_treeBB), randomRow, randomCol))
        {
            GameObject tree = Instantiate(_treePrefab);
            tree.transform.parent = _tiles[randomRow * _maxCols + randomCol].transform;
            tree.transform.localPosition = Vector3.zero;

            _treeBB = SetCellState(_treeBB, randomRow, randomCol);
        }
    }

    private void PrintBB(string name, long bitboard)
    {
        Debug.Log(name + " : " + Convert.ToString(bitboard, 2).PadLeft(64, '0'));
    }

    private int GetCellsCount(long bitboard)
    {
        int count = 0;
        long bb = bitboard;

        while (bb != 0)
        {
            bb &= bb - 1;
            count++;
        }

        return count;
    }

    private long SetCellState(long bitboard, int row, int col)
    {
        long newBit = 1L << (row * (_maxCols) + col);

        return (bitboard | newBit);
    }

    private bool GetCellState(long bitboard, int row, int col)
    {
        long bitMask = 1L << (row * _maxCols + col);

        return ((bitboard & bitMask) != 0);
    }

    private void UpdateScore()
    {
        int dirtScore = GetCellsCount(_dirtBB & _playerBB) * pointsDirt;
        int desertScore = GetCellsCount(_desertBB & _playerBB) * pointsDesert;
        int generalScore = dirtScore + desertScore;

        _scoreText.text = "Score : " + generalScore.ToString();

        int x = 5;
        int y = 15;

        x ^= y;
        y ^= x;
        x ^= y;

        Debug.Log(x);
        Debug.Log(y);
    }
}
