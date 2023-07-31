using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grafitti : MonoBehaviour
{
    public bool turnOff;
    MeshRenderer mesh;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        mesh = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        if (turnOff)
        {
            Color c = mesh.material.color;
            c.a = Mathf.Lerp(c.a, 0, Time.deltaTime * 4);
            mesh.material.color = c;
        }
    }
}
