using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CranGame : MonoBehaviour
{
    [SerializeField] private Transform cranContainer;
    [SerializeField] private GameObject cranPrefab;

    [SerializeField] private TextMeshProUGUI coinsText;

    [SerializeField] private Transform heartContainer;

    [SerializeField] private Transform items;

    [SerializeField] private Button pullButton;
    [SerializeField] private Sprite greenButton;
    [SerializeField] private Sprite redButton;

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject itemPrefab;

    private int selectedLevel;
    private CranData cranData;
    private int cranCount;
    private List<Cran> crans = new List<Cran>();
    private Cran selectedCran;
    private int hearts = 3;
    private List<GameObject> bombs = new List<GameObject>();
    private bool isPulling = false;
    public float cranSpeed = 2f;
    private Dictionary<int, int> collectedItemCounts = new Dictionary<int, int>();

    void Start()
    {
        selectedLevel = PlayerPrefs.GetInt("CranGameSelectedLevel", 0);
        List<CranData> cranDataList = CranDataManager.LoadCranData();
        cranData = cranDataList.Find(data => data.isSelected);

        if (selectedLevel == 0)
            cranCount = 2;
        else if (selectedLevel == 1)
            cranCount = 3;
        else
            cranCount = 4;

        UpdateHearts();

        UpdateCoinsText();

        DisplayBombs();

        DisplayCrans();

        pullButton.onClick.RemoveAllListeners();
        pullButton.onClick.AddListener(Pull);
        UpdatePullButton();

        cranSpeed = selectedCran.speed / 5f;
        gameOverPanel.SetActive(false);
    }

    private void DisplayBombs()
    {
        foreach (Transform t in items)
        {
            if(t.gameObject.CompareTag("Bomb"))
            {
                bombs.Add(t.gameObject);
            }
        }

        ShuffleList(bombs);

        for (int i = 3; i < bombs.Count; i++)
        {
            Destroy(bombs[i]);
        }

        bombs.RemoveRange(3, bombs.Count - 3);
    }

    // Функция для перемешивания списка
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private void DisplayCrans()
    {
        foreach (Transform item in cranContainer)
        {
            Destroy(item.gameObject);
        }

        for (int i = 0; i < cranCount; i++)
        {
            GameObject cranObject = Instantiate(cranPrefab, cranContainer);
            Cran cran = cranObject.GetComponent<Cran>();
            crans.Add(cran);
            cran.SetUp(cranData);
            cran.button.onClick.RemoveAllListeners();
            cran.button.onClick.AddListener(() =>
            {
                SelectCran(cran);
            });               
        }

        SelectCran(crans[0]);
    }

    private void SelectCran(Cran selectedCran)
    {
        if (isPulling)
            return;

        foreach (Cran cran in crans)
        {
            if(selectedCran == cran)
                cran.selectCircleImage.gameObject.SetActive(true);
            else
                cran.selectCircleImage.gameObject.SetActive(false);
        }

        this.selectedCran = selectedCran;
    }

    private void UpdateCoinsText()
    {
        coinsText.text = Coins.GetCoins().ToString();
    }

    private void UpdateHearts()
    {
        foreach (Transform heart in heartContainer)
        {
            heart.gameObject.SetActive(false);
        }

        for (int i = 0; i < hearts; i++)
        {
            heartContainer.GetChild(i).gameObject.SetActive(true);
        }
    }

    public void TakeDamage()
    {
        hearts -= 1;
        if(hearts <= 0)
        {
            hearts = 0;
            GameOver();
        }
        else
        {
            SoundManager.Instance.PlayClip("Damage");
        }

        UpdateHearts();
    }

    private void GameOver()
    {
        SoundManager.Instance.PlayClip("Win");
        int coins = Coins.GetCoins();
        coins += 10;
        Coins.SaveCoins(coins);
        UpdateCoinsText();
        gameOverPanel.SetActive(true);
        DisplayCollectedItems();
    }

    private void DisplayCollectedItems()
    {
        foreach (Transform t in itemContainer)
        {
            Destroy(t.gameObject);
        }

        foreach (var kvp in collectedItemCounts)
        {
            int itemId = kvp.Key;
            int itemCount = kvp.Value;

            GameObject itemObject = Instantiate(itemPrefab, itemContainer);
            Item item = itemObject.GetComponent<Item>();
            item.SetUp(itemId, itemCount);
        }
    }

    private void UpdatePullButton()
    {
        pullButton.image.sprite = isPulling ? redButton : greenButton;
    }

    private void Pull()
    {
        if (selectedCran == null || isPulling)
            return;

        isPulling = true;
        UpdatePullButton();

        Transform closestItem = FindClosestItem();
        if (closestItem != null)
        {
            StartCoroutine(MoveCranToItemAndBack(closestItem));
        }
        else
        {
            GameOver();
            isPulling = false;
            UpdatePullButton();
        }
    }

    private Transform FindClosestItem()
    {
        Transform closestItem = null;
        float closestDistance = Mathf.Infinity;

        foreach (Transform item in items)
        {
            float distance = Vector3.Distance(selectedCran.transform.position, item.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestItem = item;
            }
        }

        return closestItem;
    }

    private IEnumerator MoveCranToItemAndBack(Transform item)
    {
        Vector3 startPosition = selectedCran.transform.position;
        Vector3 itemPosition = item.position;

        // Перемещение по оси X к предмету
        yield return StartCoroutine(MoveCran(new Vector3(itemPosition.x, startPosition.y, startPosition.z)));

        // Перемещение вниз по оси Y для захвата предмета
        yield return StartCoroutine(MoveCran(new Vector3(itemPosition.x, itemPosition.y, startPosition.z)));

        // Поднять предмет
        item.SetParent(selectedCran.transform);
        item.localPosition = new Vector2(item.localPosition.x, item.localPosition.y - 100);

        // Перемещение вверх по оси Y к исходной позиции по высоте
        yield return StartCoroutine(MoveCran(new Vector3(itemPosition.x, startPosition.y, startPosition.z)));

        // Перемещение по оси X обратно в исходное положение
        yield return StartCoroutine(MoveCran(startPosition));

        // Проверка тега предмета
        CheckItemTag(item);

        isPulling = false;
        UpdatePullButton();

        if (items.childCount == 0)
            GameOver();
    }

    private IEnumerator MoveCran(Vector3 targetPosition)
    {
        while (Vector3.Distance(selectedCran.transform.position, targetPosition) > 0.01f)
        {
            selectedCran.transform.position = Vector3.MoveTowards(selectedCran.transform.position, targetPosition, cranSpeed * Time.deltaTime);
            yield return null;
        }
        selectedCran.transform.position = targetPosition;
    }

    private void CheckItemTag(Transform item)
    {
        if (item.CompareTag("Bomb"))
        {
            TakeDamage();           
        }
        else if (item.CompareTag("Ball"))
        {
            Ball ball = item.gameObject.GetComponent<Ball>();
            Items.AddItem(ball.item);
            SoundManager.Instance.PlayClip("Score");

            if (collectedItemCounts.ContainsKey(ball.item))
            {
                collectedItemCounts[ball.item]++;
            }
            else
            {
                collectedItemCounts[ball.item] = 1;
            }
        }

        item.SetParent(null);
        Destroy(item.gameObject);
    }
}
