using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformAligner : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject centerOfPlanet;

    void Start()
    {
        Vector2 newYDir = (transform.position - centerOfPlanet.transform.position).normalized;
        transform.up = newYDir;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
