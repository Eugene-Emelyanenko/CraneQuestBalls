using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class CranData
{
    public string cranIcon;
    public int price;
    public int item;
    public int speed;
    public bool isUnlocked;
    public bool isSelected;

    public CranData(string cranIcon, int price, int item, int speed, bool isUnlocked, bool isSelected)
    {
        this.cranIcon = cranIcon;
        this.price = price;
        this.item = item;
        this.speed = speed;
        this.isUnlocked = isUnlocked;
        this.isSelected = isSelected;
    }
}

public static class CranDataManager
{
    public readonly static string cranDataKey = "CranData";

    public static List<CranData> LoadCranData()
    {
        string json = PlayerPrefs.GetString(cranDataKey, string.Empty);
        if (!string.IsNullOrEmpty(json))
        {
            CranDataListWrapper wrapper = JsonUtility.FromJson<CranDataListWrapper>(json);
            return wrapper.cranDataList;
        }
        return new List<CranData>();
    }

    public static void SaveCranData(List<CranData> cranDataList)
    {
        CranDataListWrapper wrapper = new CranDataListWrapper(cranDataList);
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(cranDataKey, json);
        PlayerPrefs.Save();
    }
}

[Serializable]
public class CranDataListWrapper
{
    public List<CranData> cranDataList;

    public CranDataListWrapper(List<CranData> cranDataList)
    {
        this.cranDataList = cranDataList;
    }
}

public class Shop : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] RectTransform coinsTransform;
    [SerializeField] private float xDefaultCoinsPos;
    [SerializeField] private float xShopCoinsPos;
    [SerializeField] private TextMeshProUGUI coinsText;

    [SerializeField] private GameObject marketCranPrefab;
    [SerializeField] private Transform cranContainer;

    [SerializeField] private int easySuperGamePrice = 30;
    [SerializeField] private int mediumSuperGamePrice = 50;
    [SerializeField] private int hardSuperGamePrice = 70;

    private List<CranData> cranDataList = new List<CranData>();

    private void Start()
    {
        EnableShopPanel(false);
    }

    public void EnableShopPanel(bool isOpen)
    {
        shopPanel.SetActive(isOpen);

        cranDataList = CranDataManager.LoadCranData();
        if (cranDataList.Count == 0)
            CreateDefaultCranData();

        if (isOpen)
        {
            coinsTransform.anchorMin = new Vector2(1, 1);
            coinsTransform.anchorMax = new Vector2(1, 1);
            coinsTransform.anchoredPosition = new Vector2(xShopCoinsPos, coinsTransform.anchoredPosition.y);

            DisplayCrans();
        }
        else
        {
            coinsTransform.anchorMin = new Vector2(0, 1);
            coinsTransform.anchorMax = new Vector2(0, 1);
            coinsTransform.anchoredPosition = new Vector2(xDefaultCoinsPos, coinsTransform.anchoredPosition.y);
        }

        UpdateCoinsText();
    }

    private void CreateDefaultCranData()
    {
        Debug.Log("Creating default cran data");

        cranDataList = new List<CranData>();
        int basePrice = 10;
        int baseSpeed = 10;
        int priceAmount = 10;
        int speedAmount = 5;

        for (int i = 0; i < 6; i++)
        {
            int price = basePrice + (i * priceAmount);
            int speed = baseSpeed + (i * speedAmount);

            cranDataList.Add(new CranData(
                cranIcon: $"{i + 1}",
                price: price,
                item: i + 1,
                speed: speed,
                isUnlocked: i == 0,
                isSelected: i == 0
            ));
        }

        CranDataManager.SaveCranData(cranDataList);
    }

    private void DisplayCrans()
    {
        foreach (Transform item in cranContainer)
        {
            Destroy(item.gameObject);
        }

        foreach (CranData data in cranDataList)
        {
            GameObject cranObject = Instantiate(marketCranPrefab, cranContainer);
            CranUI cranUI = cranObject.GetComponent<CranUI>();
            cranUI.SetUp(data);
            cranUI.button.onClick.RemoveAllListeners();
            cranUI.button.onClick.AddListener(() =>
            {
                if(data.isSelected)
                {
                    Debug.Log($"Cran_{data.cranIcon} already selected. Returned");
                    return;
                }
                else if(data.isUnlocked)
                {
                    Debug.Log($"Selected Cran_{data.cranIcon}");
                    foreach (CranData cranData in cranDataList)
                    {
                        cranData.isSelected = false;
                    }

                    data.isSelected = true;
                    CranDataManager.SaveCranData(cranDataList);
                    DisplayCrans();
                }
                else
                {
                    Debug.Log($"Trying to unlock Cran_{data.cranIcon}. Current balance {Coins.GetCoins()}. Price {data.price} coins and Item_{data.item}");
                    int coins = Coins.GetCoins();
                    int items = Items.GetItem(data.item);
                    if(coins >= data.price && items >= 1)
                    {
                        coins -= data.price;
                        Coins.SaveCoins(coins);
                        UpdateCoinsText();
                        items -= 1;
                        Items.SaveItem(data.item, items);

                        data.isUnlocked = true;
                        CranDataManager.SaveCranData(cranDataList);


                        SoundManager.Instance.PlayClip("Buy");
                        DisplayCrans();
                    }
                    else
                    {
                        if(coins < data.price)
                        {
                            Debug.Log($"Not enought coins!");
                        }
                        else if(items < 1)
                        {
                            Debug.Log($"Not enought items!");
                        }
                        else
                        {
                            Debug.Log($"Not enought coins and items!");
                        }
                    }
                }
            });
        }
    }

    private void UpdateCoinsText()
    {
        coinsText.text = Coins.GetCoins().ToString();
    }

    public void LoadSuperGame(int level)
    {
        int[] superGamePrices = { easySuperGamePrice, mediumSuperGamePrice, hardSuperGamePrice };

        if (level < 0 || level >= superGamePrices.Length)
        {
            Debug.LogError($"Invalid level: {level}");
            return;
        }

        int requiredCoins = superGamePrices[level];
        int coins = Coins.GetCoins();

        if (coins >= requiredCoins)
        {
            coins -= requiredCoins;
            Coins.SaveCoins(coins);
            UpdateCoinsText();
            SoundManager.Instance.PlayClip("Buy");
            PlayerPrefs.SetInt("SuperGameSelectedLevel", level);
            SceneManager.LoadScene("SuperGame");
        }
        else
        {
            Debug.Log($"Not enough coins. Need {requiredCoins} or higher.");
        }
    }

    public void LoadCranGame(int level)
    {
        PlayerPrefs.SetInt("CranGameSelectedLevel", level);
        SceneManager.LoadScene("CranGame");
    }
}
