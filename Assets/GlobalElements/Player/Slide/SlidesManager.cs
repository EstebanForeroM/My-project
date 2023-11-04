using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidesManager : MonoBehaviour, Observer
{
    [SerializeField] private SlideOptimizer[] slides;

    private GameObject[] previousSlides;

    private int currentSlideIndex;

    public void UpdatePlayerState(int index)
    {
        if (index == currentSlideIndex) return;
        currentSlideIndex = index;
        if (index <= 1) return;

        if (index >= slides.Length - 2)
        {
            slides[index - 2].SetActive(false);
            slides[index - 1].SetActive(true);
            if (index == slides.Length - 1) return;
            slides[index + 1].SetActive(true);
            return;
        }

        slides[index - 2].SetActive(false);
        slides[index + 2].SetActive(false);
        slides[index - 1].SetActive(true);
        slides[index + 1].SetActive(true);
    }

    // Start is called before the first frame update
    void Awake()
    {
        for (int i = 0; i < slides.Length; i++)
        {
            slides[i].index = i;
            slides[i].observer = this;
            slides[i].SetActive(false);
        }
        currentSlideIndex = 0;
        slides[0].SetActive(true);
        slides[1].SetActive(true);
        slides[2].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {

    }
}

public interface Observer
{
    void UpdatePlayerState(int index);
}
