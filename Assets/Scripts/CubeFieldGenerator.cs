using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeFieldGenerator : MonoBehaviour {
    List<GameObject> cubes = new List<GameObject>();

    void Start() {
        const int range = 150;
        for (int i = 0; i < 400; i++) {
            cubes.Add(GameObject.CreatePrimitive(PrimitiveType.Cube));
            cubes[^1].transform.localScale = new Vector3(Random.Range(3, 7), Random.Range(3, 7), Random.Range(3, 7));
            cubes[^1].transform.parent = gameObject.transform;
            cubes[^1].transform.localPosition = new Vector3(Random.Range(-range / 2, range / 2), Random.Range(0, range), Random.Range(-range / 2, range / 2));
            cubes[^1].name = "Cube" + i;
        }
        
    }
}
