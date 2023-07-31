using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PodiumStart : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.E))
                FindObjectOfType<PlatformController>().StartPlatformAnimation();
        }
    }
}
