using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    public void PlayClickSound()
    {
        SoundManager.Instance.PlayClip("Click");
    }
}
