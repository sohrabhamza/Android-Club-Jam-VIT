using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlatformController : MonoBehaviour
{
    List<GameObject> myPlats = new List<GameObject>();

    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            myPlats.Add(transform.GetChild(i).gameObject);
        }
    }
    public void StartPlatformAnimation()
    {
        StartCoroutine(PlatStarter());
    }

    IEnumerator PlatStarter()
    {
        foreach (GameObject g in myPlats)
        {
            yield return new WaitForSeconds(0.1f);
            g.GetComponent<PlatfromScaler>().isScale = true;
        }

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartPlatformAnimation();
        }
    }
}
