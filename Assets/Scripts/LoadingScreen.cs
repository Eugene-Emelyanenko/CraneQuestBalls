using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Slider loadingSlider;
    public float loadingTime = 5f;

    private void Start()
    {
        StartCoroutine(AnimateLoading());
    }

    private IEnumerator AnimateLoading()
    {
        float elapsedTime = 0f;

        while (elapsedTime < loadingTime)
        {
            elapsedTime += Time.deltaTime;
            loadingSlider.value = Mathf.Clamp01(elapsedTime / loadingTime);
            yield return null;
        }

        loadingSlider.value = 1f;

        SceneManager.LoadScene("Main");
    }
}
