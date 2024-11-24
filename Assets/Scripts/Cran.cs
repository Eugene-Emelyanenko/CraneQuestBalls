using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cran : MonoBehaviour
{
    [SerializeField] private Image cranImage;
    public Image selectCircleImage;
    public Button button;
    public float speed = 0;

    private Color purpleColor = new Color(128f / 255f, 0f / 255f, 255f / 255f);
    private Color pinkColor = new Color(255f / 255f, 0f / 255f, 122f / 255f);

    public void SetUp(CranData data)
    {
        speed = data.speed;
        cranImage.sprite = Resources.Load<Sprite>($"Crans/{data.cranIcon}");
        switch (data.cranIcon)
        {
            case "1":
                selectCircleImage.color = Color.yellow;
                break;
            case "2":
                selectCircleImage.color = Color.green;
                break;
            case "3":
                selectCircleImage.color = purpleColor;
                break;
            case "4":
                selectCircleImage.color = Color.red;
                break;
            case "5":
                selectCircleImage.color = Color.blue;
                break;
            case "6":
                selectCircleImage.color = pinkColor;
                break;
        }
    }
}
