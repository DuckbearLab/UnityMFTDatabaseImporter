using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeReplacer : MonoBehaviour {

    [System.Serializable]
    public struct TreeNameGameObj
    {
        public string TreeName;
        public GameObject GameObj;
    }

    public TreeNameGameObj[] Trees;

    public void ReplaceTrees()
    {
        foreach (var gameObj in GameObject.FindObjectsOfType<GameObject>())
        {
            foreach (var treeNameGameObj in Trees)
            {
                if (gameObj.name == treeNameGameObj.TreeName)
                    Instantiate(treeNameGameObj.GameObj, gameObj.transform.position, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
            }
        }
    }
}
