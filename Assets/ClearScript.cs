﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearScript : MonoBehaviour
{
    //現在のシーンが何番目にあるか
    private int _setsceneIndex = default;
    void Update()
    {
        //Spaceキーを入力したらリセット
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _setsceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(_setsceneIndex);
        }
        //Enterキーを入力したら次のステージへ
        if (Input.GetKeyDown(KeyCode.Return))
        {
            _setsceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(_setsceneIndex + 1);
        }
    }
}
