using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    private Button _cellButton;
    private TMP_Text _cellText;
    private int _currentValue;
    private bool _isLocked;

    private SudokuManager _manager;

    private void Awake()
    {
        _cellButton = GetComponent<Button>();
        _cellText = GetComponentInChildren<TMP_Text>();
        _cellButton.onClick.AddListener(OnCellClick);
    }

    public void Setup(SudokuManager manager)
    {
        _manager = manager;
        UpdateDisplay();
    }

    public void SetValue(int value, bool locked = false)
    {
        _currentValue = value;
        _isLocked = locked;

        ColorBlock colorBlock = _cellButton.colors;

        if (_isLocked)
        {
            colorBlock.disabledColor = new Color(0.56f, 0.94f, 0.63f);
        }
        else
        {
            colorBlock.normalColor = new Color(0.76f, 0.94f, 0.73f);
        }

        _cellButton.colors = colorBlock;

        _cellButton.interactable = !_isLocked;

        UpdateDisplay();
        _manager.CheckForWin();
    }

    private void UpdateDisplay()
    {
        _cellText.text = _currentValue is > 0 and < 10 ? _currentValue.ToString() : "";
    }

    private void OnCellClick()
    {
        if (!_isLocked)
        {
            _manager.SetSelectedCell(this);
        }
    }

    public void SetLocked(bool value)
    {
        _isLocked = value;
    }

    public bool IsLocked => _isLocked;
    
    public int CurrentValue => _currentValue;
}