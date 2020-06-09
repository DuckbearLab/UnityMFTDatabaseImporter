using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
//using UnityEditor;

public class PutCopy : MonoBehaviour
{

    [HideInInspector]
    public List<Details> Copies = new List<Details>();

    public List<TextureSwitch> TextureSwitches = new List<TextureSwitch>();

    [System.Serializable]
    public struct Details
    {
        public GameObject ToCopy;
        public Vector3 Position;
        public Quaternion Rotation;
    }

    [System.Serializable]
    public struct TextureSwitch
    {
        public int Group;
        public Material Material;
    }

    private Dictionary<Material, List<Material>> textureSwitchesCache;

    private struct Point
    {
        public int x;
        public int y;

        public override int GetHashCode()
        {
            return x << 10 + y;
        }

        public override bool Equals(object obj)
        {
            if (obj is Point)
            {
                Point other = (Point)obj;
                return this.x == other.x && this.y == other.y;
            }
            return base.Equals(obj);
        }
    }

    private Dictionary<Point, List<Details>> orderedCopies;
    private const int OrganizedChunkSize = 50;
    private const int MaxInstantiationsPerFrame = 1;
    private const float InstantiationRadius = 999000;

    public event Action Done;

    // Use this for initialization
    void Start()
    {
        PrepareTextureSwitchesCaches();
        Go();
    }

    private void OrganizeCopies()
    {
        orderedCopies = new Dictionary<Point, List<Details>>();

        foreach (var copy in Copies)
        {
            var p = new Point()
            {
                x = (int)(copy.Position.x / OrganizedChunkSize),
                y = (int)(copy.Position.y / OrganizedChunkSize)
            };
            List<Details> details;
            if (orderedCopies.TryGetValue(p, out details))
                details.Add(copy);
            else
                orderedCopies[p] = new List<Details>(new Details[] { copy });
        }
    }

    private void PrepareTextureSwitchesCaches()
    {
        textureSwitchesCache = new Dictionary<Material, List<Material>>();

        Dictionary<int, List<Material>> groups = new Dictionary<int, List<Material>>();
        foreach (var textureSwitch in TextureSwitches)
        {
            if (!groups.ContainsKey(textureSwitch.Group))
                groups[textureSwitch.Group] = new List<Material>();

            groups[textureSwitch.Group].Add(textureSwitch.Material);
            textureSwitchesCache[textureSwitch.Material] = groups[textureSwitch.Group];
        }
    }

    /*[MenuItem("AAA/BBB")]
    private static void Goga()
    {
        FindObjectOfType<PutCopy>().Go();
    }

    [MenuItem("AAA/CCC")]
    private static void Gogaaaa()
    {
        StaticBatchingUtility.Combine(GameObject.Find("terrain"));
    }*/

    public void Go()
    {
        PrepareTextureSwitchesCaches();
        var copies = new GameObject("copies");
        copies.transform.parent = GameObject.Find("terrain").transform;
        foreach(var copy in Copies)
        {
            /*if (copy.ToCopy.activeSelf)
                copy.ToCopy.SetActive(false);*/

            if (!copy.ToCopy)
                continue;

            foreach (Rigidbody rb in copy.ToCopy.GetComponentsInChildren<Rigidbody>())
                Destroy(rb);

            GameObject gameObj = Instantiate(copy.ToCopy, copy.Position, copy.Rotation, copies.transform);
            gameObj.name = copy.ToCopy.name;
            ApplyTextureSwitches(gameObj);
        }
        Done?.Invoke();
    }


    private void ApplyTextureSwitches(GameObject gameObj)
    {
        foreach (var renderer in gameObj.GetComponentsInChildren<MeshRenderer>())
        {
            var materials = renderer.sharedMaterials;
            bool materialChanged = false;
            for (int i = 0; i < materials.Length; i++)
            {

                List<Material> replaceWith;
                if (textureSwitchesCache.TryGetValue(materials[i], out replaceWith))
                {
                    materials[i] = ChooseRandomTexture(replaceWith, gameObj);
                    materialChanged = true;
                }
            }
            if (materialChanged)
                renderer.sharedMaterials = materials;
        }
    }

    private Material ChooseRandomTexture(List<Material> replaceWith, GameObject gameObj)
    {
        return replaceWith[HashLocation(gameObj.transform.position, replaceWith.Count)];
    }

    private int HashLocation(Vector3 location, int mod)
    {
        int x = Mathf.FloorToInt(location.x);
        int y = Mathf.FloorToInt(location.y);
        int z = Mathf.FloorToInt(location.z);
        return Mathf.Abs(x ^ y ^ z) % mod;
    }

}
