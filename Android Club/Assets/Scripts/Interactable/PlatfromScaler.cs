using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatfromScaler : MonoBehaviour
{
    public bool isScale;
    float origYScale;
    float origYPos;

    private void Start()
    {
        origYScale = transform.localScale.y;
        origYPos = transform.position.y;
        transform.localScale = new Vector3(transform.localScale.x, 0, transform.localScale.z);
        transform.position = new Vector3(transform.position.x, -0.1f, transform.position.z);
    }

    private void Update()
    {
        if (isScale)
        {
            float newScale = Mathf.Lerp(transform.localScale.y, origYScale, Time.deltaTime * 3);
            transform.localScale = new Vector3(transform.localScale.x, newScale, transform.localScale.z);
            transform.position = new Vector3(transform.position.x, Mathf.Lerp(transform.position.y, origYPos, Time.deltaTime * 3), transform.position.z);
        }
    }
}


