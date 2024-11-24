using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    [SerializeField] private Image ballImage;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemCountText;

    public void SetUp(int itemId, int itemCount)
    {
        itemIcon.sprite = Resources.Load<Sprite>($"Items/{itemId}");
        ballImage.sprite = Resources.Load<Sprite>($"Balls/{itemId}");
        itemCountText.text = $"{itemCount} -";
    }
}
