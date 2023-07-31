using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EraseGrafitti : MonoBehaviour
{
    [SerializeField] LayerMask layer;
    [SerializeField] Camera main;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Ray ray = main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 3.5f, layer))
            {
                if (hit.transform.TryGetComponent<Grafitti>(out Grafitti myG))
                {
                    myG.turnOff = true;
                }
            }
        }
    }
}
