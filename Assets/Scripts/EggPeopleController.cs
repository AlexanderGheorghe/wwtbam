using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggPeopleController : MonoBehaviour
{
    public List<Material> Materials;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Renderer>().material = Materials[Random.Range(0, Materials.Count)];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
