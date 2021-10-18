using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class Choice3Two : MonoBehaviour
{
    public Button choice2btn;

    public AudioSource changeSceneSound;

    // Start is called before the first frame update
    void Start()
    {
        choice2btn = GetComponent<Button>();
        choice2btn.onClick.AddListener(TaskOnClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void TaskOnClick() {
        changeSceneSound.Play();
        // change scene to main board
        PlayerPrefs.SetInt("endlessMode", 3);
        StartCoroutine(LoadEndScene());
    }

    IEnumerator LoadEndScene()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("BoardScene");
    }
}