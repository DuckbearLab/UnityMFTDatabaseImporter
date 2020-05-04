using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PutTree : MonoBehaviour
{

    public GameObject Tree;

    // Use this for initialization
    void Start()
    {
        StartCoroutine(AddTree());
    }

    private IEnumerator AddTree()
    {
        yield return new WaitForSeconds(Random.value * 5);
        if (Tree != null)
        {
            GameObject gameObj = Instantiate(Tree, transform.position, Quaternion.Euler(0, Random.value * 360f, 0), transform);
            DestroyImmediate(gameObj.GetComponent<Rigidbody>());
        }
    }

}
