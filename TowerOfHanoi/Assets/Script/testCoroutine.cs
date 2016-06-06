using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class testCoroutine : MonoBehaviour
{

    [SerializeField]
    Text text;


    IEnumerator coroutineMethod;


    void Start()
    {
        coroutineMethod = UpdateText();
    }


    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            StartCoroutine(coroutineMethod);
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopCoroutine(coroutineMethod);
        }
    }


    IEnumerator UpdateText()
    {
        float clickTime = 0;

        while (clickTime < 3)
        {
            clickTime += Time.deltaTime;
            text.text = clickTime.ToString("0.000");
            yield return null;
        }
        text.text = "finished";
    }


}
