using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ball : MonoBehaviour
{
    public Image ballImage;
    public Image itemIcon;

    public int item { get; private set; }
    private int ballSprite;

    private void Start()
    {
        item = Random.Range(1, 7);
        ballImage.sprite = Resources.Load<Sprite>($"Balls/{item}");
        itemIcon.sprite = Resources.Load<Sprite>($"Items/{item}");
    }
}
