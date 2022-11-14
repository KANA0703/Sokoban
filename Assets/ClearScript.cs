using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearScript : MonoBehaviour
{
    //現在のシーンが何番目にあるか
    private int SetsceneIndex;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Spaceキーを入力したらリセット
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetsceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(SetsceneIndex);
        }
        //Enterキーを入力したら次のステージへ
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SetsceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(SetsceneIndex + 1);
        }
    }
}
