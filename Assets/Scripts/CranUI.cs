using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CranUI : MonoBehaviour
{
    [SerializeField] private Image cranIcon;
    [SerializeField] private GameObject price;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Image item;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite selectedSprite;
    public Button button;
    public CranData cranData;
    public void SetUp(CranData data)
    {
        cranData = data;
        gameObject.name = $"Cran_{data.cranIcon}";
        cranIcon.sprite = Resources.Load<Sprite>($"Crans/{data.cranIcon}");
        priceText.text = data.price.ToString();
        item.sprite = Resources.Load<Sprite>($"Items/{data.item}");
        speedText.text = $"{data.speed}%";
        button.image.sprite = defaultSprite;
        if(data.isUnlocked)
        {
            price.SetActive(false);
            if(data.isSelected)
            {
                button.image.sprite = selectedSprite;
            }
        }
        else
        {
            price.SetActive(true);
        }
    }
}
