using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    
    public void SetDifficulty(int level)
    {
        DifficultyManager.SelectedDifficulty = (Difficulties)level;
        SceneManager.LoadScene("Sudoku");
    }
}