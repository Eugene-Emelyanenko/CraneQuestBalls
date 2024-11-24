using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SuperGame : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform puzzleField;
    [SerializeField] private GridLayoutGroup fieldGrid;
    [SerializeField] private GameObject puzzlePrefab;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Variables")]
    [SerializeField] private Vector2 easyPuzzleSize;
    [SerializeField] private Vector2 easySpace;
    [SerializeField] private Vector2 mediumPuzzleSize;
    [SerializeField] private Vector2 mediumSpace;
    [SerializeField] private Vector2 hardPuzzleSize;
    [SerializeField] private Vector2 hardSpace;
    [SerializeField] private float timeToCheck;

    [Header("Timer")]
    [SerializeField] private float time = 40f;

    [Header("Sprites")]
    [SerializeField] private Sprite easyBgSprite;
    [SerializeField] private Sprite mediumBgSprite;
    [SerializeField] private Sprite hardBgSprite;
    [SerializeField] private Sprite[] puzzles;
    [SerializeField] private Sprite bombSprite;

    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Image gameOverImage;
    [SerializeField] private Sprite winFrame;
    [SerializeField] private Sprite looseFrame;

    [Header("Lists")]
    [SerializeField] private List<Sprite> gamePuzzles = new List<Sprite>();
    [SerializeField] private List<Button> btns = new List<Button>();

    private int selectedLevel = 0;
    private int columnCount = 3;
    private Vector2 puzzleSize;
    private Vector2 puzzleSpace;
    private Sprite currentBgSprite;

    private bool firstGuess, secondGuess;

    private int countGuesses;
    private int countCorrectGuesses;
    private int gameGuesses;

    private int firstGuessIndex, secondGuessIndex;

    private string firstGuessPuzzle, secondGuessPuzzle;

    private bool isGameOver = false;

    private float remainingTime;

    private void Awake()
    {
        selectedLevel = PlayerPrefs.GetInt("SuperGameSelectedLevel", 0);
        if(selectedLevel == 0) //easy
        {
            columnCount = 3;
            puzzleSize = easyPuzzleSize;
            puzzleSpace = easySpace;
            currentBgSprite = easyBgSprite;
        }
        else if(selectedLevel == 1) //medium
        {
            columnCount = 4;
            puzzleSize = mediumPuzzleSize;
            puzzleSpace = mediumSpace;
            currentBgSprite = mediumBgSprite;
        }
        else //hard
        {
            columnCount = 6;
            puzzleSize = hardPuzzleSize;
            puzzleSpace = hardSpace;
            currentBgSprite = hardBgSprite;
        }
    }

    private void Start()
    {
        AddButtons();
        AddGamePuzzles();
        UpdateCoinsText();
        Shuffle(gamePuzzles);
        StartTimer(time);

        gameGuesses = (gamePuzzles.Count) / 2;
        Debug.Log("Game Guessed: " + gameGuesses);
    }

    private void StartTimer(float duration)
    {
        remainingTime = duration;
        StartCoroutine(UpdateTimer());
    }

    private IEnumerator UpdateTimer()
    {
        while (remainingTime > 0 && !isGameOver)
        {
            yield return new WaitForSeconds(1f);
            remainingTime--;
            UpdateTimerText();

            if (remainingTime <= 0)
            {
                GameOver(false);
            }
        }
    }

    private void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void AddButtons()
    {
        fieldGrid.constraintCount = columnCount;
        fieldGrid.cellSize = puzzleSize;
        fieldGrid.spacing = puzzleSpace;

        int size = columnCount * columnCount;
        bool isSpaceAdded = false;
        
        for (int i = 0; i < size; i++)
        {
            if (selectedLevel == 0 && i == 4 && !isSpaceAdded)
            {
                GameObject spaceObject = Instantiate(new GameObject("Space"), puzzleField);
                spaceObject.AddComponent<RectTransform>();
                size--;
                i--;
                isSpaceAdded = true;
                continue;
            }               
            GameObject puzzleObject = Instantiate(puzzlePrefab, puzzleField);
            puzzleObject.name = $"{i}";
            Button puzzleButton = puzzleObject.GetComponent<Button>();
            puzzleButton.image.sprite = currentBgSprite;
            puzzleButton.onClick.AddListener(PizkPuzzle);
            btns.Add(puzzleButton);
        }
    }

    private void PizkPuzzle()
    {
        if (isGameOver)
            return;

        string name = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;
        Debug.Log($"Selected Puzzle_{name}");

        if(!firstGuess)
        {
            firstGuess = true;

            firstGuessIndex = int.Parse(name);

            firstGuessPuzzle = gamePuzzles[firstGuessIndex].name;

            btns[firstGuessIndex].image.sprite = gamePuzzles[firstGuessIndex];

            /*
            if (firstGuessPuzzle == bombSprite.name)
            {
                GameOver(false);
                return;
            }
            */         
        }
        else if(!secondGuess)
        {
            secondGuess = true;

            secondGuessIndex = int.Parse(name);

            secondGuessPuzzle = gamePuzzles[secondGuessIndex].name;

            btns[secondGuessIndex].image.sprite = gamePuzzles[secondGuessIndex];

            countGuesses++;

            /*
            if (secondGuessPuzzle == bombSprite.name)
            {
                GameOver(false);
                return;
            }
            */

            StartCoroutine(CheckPuzzles());
        }
    }

    IEnumerator CheckPuzzles()
    {
        yield return new WaitForSeconds(timeToCheck);

        if(firstGuessPuzzle == secondGuessPuzzle)
        {
            yield return new WaitForSeconds(0.5f);

            btns[firstGuessIndex].interactable = false;
            btns[secondGuessIndex].interactable = false;

            countCorrectGuesses++;

            if (countCorrectGuesses == gameGuesses)
                GameOver(true);
            else
            {
                SoundManager.Instance.PlayClip("Score");
            }
        }
        else
        {
            btns[firstGuessIndex].image.sprite = currentBgSprite;
            btns[secondGuessIndex].image.sprite = currentBgSprite;
        }

        yield return new WaitForSeconds(0.5f);

        firstGuess = secondGuess = false;
    }

    private void GameOver(bool isWin)
    {
        if (isGameOver)
            return;

        isGameOver = true;

        gameOverPanel.SetActive(true);

        if (isWin)
        {
            gameOverImage.sprite = winFrame;
            Coins.SaveCoins(Coins.GetCoins() + 20);
            UpdateCoinsText();
            SoundManager.Instance.PlayClip("Win");
        }
        else
        {
            gameOverImage.sprite = looseFrame;
            SoundManager.Instance.PlayClip("GameOver");
        }
    }

    private void UpdateCoinsText()
    {
        coinsText.text = Coins.GetCoins().ToString();
    }

    private void AddGamePuzzles()
    {
        int looper = btns.Count;
        int index = 0;

        for (int i = 0; i < looper; i += 2)
        {
            if (index >= puzzles.Length)
                index = 0;

            gamePuzzles.Add(puzzles[index]);
            if (i + 1 < looper)
            {
                gamePuzzles.Add(puzzles[index]);
            }

            index++;
        }

        /*
        gamePuzzles.RemoveAt(gamePuzzles.Count - 1);
        gamePuzzles.Add(bombSprite);

        gameGuesses = (gamePuzzles.Count - 1) / 2;

        if (selectedLevel != 0)
        {
            gamePuzzles.RemoveAt(gamePuzzles.Count - 2);
            gamePuzzles.Add(bombSprite);           
        }
        */
    }

    private void Shuffle(List<Sprite> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Sprite temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

}
