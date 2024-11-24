using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Items
{
    public static void SaveItem(int item, int value)
    {
        PlayerPrefs.SetInt($"Item{item}", value);
        PlayerPrefs.Save();
    }

    public static int GetItem(int item) => PlayerPrefs.GetInt($"Item{item}", 0);

    public static void AddItem(int item)
    {
        int itemCount = GetItem(item);
        itemCount++;
        SaveItem(item, itemCount);
    }
}
