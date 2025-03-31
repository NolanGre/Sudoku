using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

public class SudokuManager : MonoBehaviour
{
    public GameObject cellPrefab;
    public Transform gridContainer;
    public GameObject winPanel;

    private const int GRID_SIZE = 9;
    private const int BOX_SIZE = 3;

    private Cell[,] cells = new Cell[GRID_SIZE, GRID_SIZE];
    private Cell selectedCell;

    private int[,] currentPuzzle = new int[GRID_SIZE, GRID_SIZE];

    public Difficulties difficulty;  // 0-3
    
    void Start()
    {
        difficulty = DifficultyManager.SelectedDifficulty;
        InitializeGrid();
        GenerateNewPuzzle();
        winPanel.SetActive(false);
    }

    void Update()
    {
        if (selectedCell is null) return;

        for (int i = 1; i < 10; i++)
        {
            if (!Input.GetKeyDown(KeyCode.Alpha1 + i - 1)) continue;
            
            selectedCell.SetValue(i);
            break;
        }
    }
    
    public void RestartGame()
    {
        for (int row = 0; row < GRID_SIZE; row++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                cells[row, col].SetValue(0);
            }
        }

        GenerateNewPuzzle();
        winPanel.SetActive(false);
        SceneManager.LoadScene("Menu");
    }

    private void InitializeGrid()
    {
        for (int row = 0; row < GRID_SIZE; row++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                GameObject cellObj = Instantiate(cellPrefab, gridContainer);
                Cell cell = cellObj.GetComponent<Cell>();

                cells[row, col] = cell;

                cell.Setup(this);

                Button button = cellObj.GetComponent<Button>();
                button.onClick.AddListener(() => SetSelectedCell(cell));
            }
        }
    }

    public void SetSelectedCell(Cell cell)
    {
        selectedCell = cell;
    }

    public void HandleUserInput(string input)
    {
        if (selectedCell == null) return;

        if (!int.TryParse(input, out int value)) return;

        if (value is > 0 and < 10)
        {
            selectedCell.SetValue(value);
        }
    }

    public void ClearSelectedCell()
    {
        if (selectedCell != null && !selectedCell.IsLocked)
        {
            selectedCell.SetValue(0);
        }
    }

    public void GenerateNewPuzzle()
    {
        int[,] puzzle = new int[GRID_SIZE, GRID_SIZE];

        FillPuzzle(puzzle);
        
        currentPuzzle = CreatePuzzleFromSolution(puzzle);
        
        LoadPuzzle(currentPuzzle);
    }   

    private void FillPuzzle(int[,] puzzle)
    {
        FillDiagonalBoxes(puzzle);
        SolvePuzzle(puzzle);
    }

    private void FillDiagonalBoxes(int[,] grid)
    {
        for (int box = 0; box < GRID_SIZE; box += BOX_SIZE)
        {
            FillBox(grid, box, box);
        }
    }

    private void FillBox(int[,] grid, int rowStart, int colStart)
    {
        List<int> numbers = Enumerable.Range(1, GRID_SIZE).ToList();

        ShuffleList(numbers);

        int index = 0;
        for (int row = 0; row < BOX_SIZE; row++)
        {
            for (int col = 0; col < BOX_SIZE; col++)
            {
                grid[rowStart + row, colStart + col] = numbers[index++];
            }
        }
    }

    private static void ShuffleList<T>(List<T> list)
    {
        Random rng = new Random();
        int n = list.Count;

        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
    
    private bool SolvePuzzle(int[,] grid)
    {
        int row = -1, col = -1;
        bool isEmpty = true;
    
        for (int r = 0; r < GRID_SIZE; r++)
        {
            for (int c = 0; c < GRID_SIZE; c++)
            {
                if (grid[r, c] != 0) continue;
                
                row = r;
                col = c;
                isEmpty = false;
                break;
            }
            if (!isEmpty)
            {
                break;
            }
        }
    
        if (isEmpty)
        {
            return true;
        }
    
        List<int> numbers = Enumerable.Range(1, GRID_SIZE).ToList();
        ShuffleList(numbers);
    
        foreach (var num in numbers.Where(num => IsValidPlacement(grid, row, col, num)))
        {
            grid[row, col] = num;
            
            if (SolvePuzzle(grid))
            {
                return true;
            }
            
            grid[row, col] = 0;
        }
    
        return false;
    }
    
    private static bool IsValidPlacement(int[,] grid, int row, int col, int num)
    {
        for (int c = 0; c < GRID_SIZE; c++)
        {
            if (grid[row, c] == num)
            {
                return false;
            }
        }
    
        for (int r = 0; r < GRID_SIZE; r++)
        {
            if (grid[r, col] == num)
            {
                return false;
            }
        }
    
        int boxStartRow = (row / BOX_SIZE) * BOX_SIZE;
        int boxStartCol = (col / BOX_SIZE) * BOX_SIZE;
    
        for (int r = 0; r < BOX_SIZE; r++)
        {
            for (int c = 0; c < BOX_SIZE; c++)
            {
                if (grid[boxStartRow + r, boxStartCol + c] == num)
                {
                    return false;
                }
            }
        }
    
        return true;
    }
    
    private int[,] CreatePuzzleFromSolution(int[,] solution)
    {
        int[,] puzzle = new int[GRID_SIZE, GRID_SIZE];
    
        for (int row = 0; row < GRID_SIZE; row++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                puzzle[row, col] = solution[row, col];
            }
        }
    
        int cellsToRemove = difficulty switch
        {
            Difficulties.Test => 4,
            Difficulties.Easy => 20,
            Difficulties.Normal => 40,
            Difficulties.Hard => 65,
            _ => 40
        };

        List<(int, int)> positions = new List<(int, int)>();
        for (int row = 0; row < GRID_SIZE; row++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                positions.Add((row, col));
            }
        }
    
        ShuffleList(positions);
    
        int removed = 0;
        foreach (var (row, col) in positions)
        {
            if (removed >= cellsToRemove)
            {
                break;
            }
        
            int temp = puzzle[row, col];
            puzzle[row, col] = 0;
        
            removed++;
        }
    
        return puzzle;
    }
    
    private void LoadPuzzle(int[,] puzzle)
    {
        currentPuzzle = puzzle;
    
        for (int row = 0; row < GRID_SIZE; row++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                int value = puzzle[row, col];
            
                cells[row, col].SetValue(value, value > 0);
            }
        }
    }
    
    public void CheckForWin()
    {
        bool allFilled = true;
        
        for (int row = 0; row < GRID_SIZE; row++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                if (cells[row, col].CurrentValue == 0)
                {
                    allFilled = false;
                    break;
                }
            }
            
            if (!allFilled) break;
        }
        
        if (!allFilled) return;
        
        bool isCorrect = true;
        
        for (int row = 0; row < GRID_SIZE; row++)
        {
            if (!CheckRow(row))
            {
                isCorrect = false;
                break;
            }
        }
        
        if (isCorrect)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                if (!CheckColumn(col))
                {
                    isCorrect = false;
                    break;
                }
            }
        }
        
        if (isCorrect)
        {
            for (int boxRow = 0; boxRow < 3; boxRow++)
            {
                for (int boxCol = 0; boxCol < 3; boxCol++)
                {
                    if (!CheckBox(boxRow * 3, boxCol * 3))
                    {
                        isCorrect = false;
                        break;
                    }
                }
                if (!isCorrect) break;
            }
        }
        
        if (isCorrect)
        {
            winPanel.SetActive(true);
        }
    }

    private bool CheckRow(int row)
    {
        bool[] used = new bool[GRID_SIZE + 1];
        
        for (int col = 0; col < GRID_SIZE; col++)
        {
            int num = cells[row, col].CurrentValue;
            if (num == 0 || used[num]) return false;
            used[num] = true;
        }
        
        return true;
    }

    private bool CheckColumn(int col)
    {
        bool[] used = new bool[GRID_SIZE + 1];
        
        for (int row = 0; row < GRID_SIZE; row++)
        {
            int num = cells[row, col].CurrentValue;
            if (num == 0 || used[num]) return false;
            used[num] = true;
        }
        
        return true;
    }

    private bool CheckBox(int startRow, int startCol)
    {
        bool[] used = new bool[GRID_SIZE + 1];
        
        for (int row = 0; row < BOX_SIZE; row++)
        {
            for (int col = 0; col < BOX_SIZE; col++)
            {
                int num = cells[startRow + row, startCol + col].CurrentValue;
                if (num == 0 || used[num]) return false;
                used[num] = true;
            }
        }
        
        return true;
    }
}