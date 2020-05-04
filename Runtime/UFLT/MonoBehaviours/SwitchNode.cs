using UnityEngine;
using System.Collections;
using UFLT.Records;
//using UFLT.Records;

namespace UFLT.MonoBehaviours
{
    //[ExecuteInEditMode]
    public class SwitchNode : MonoBehaviour
    {
        #region Properties

        /// <summary>
        /// How often the switch should be checked.
        /// </summary>
        public static float switchUpdateRate = 0.1f;

        /// <summary>
        /// The index of the current Mask
        /// </summary>
        public int index;

        /// <summary>
        /// Node masks
        /// </summary>
        public int[] masks;
        #endregion

        void Awake()
        {
            index = HashLocation(transform.position, masks.Length);
            int mask = masks[index];
            for (int i = 0; i < transform.childCount; i++)
            {
                int mask_bit = 1 << (i % 32);
                transform.GetChild(i).gameObject.SetActive((mask & mask_bit) != 0);
            }
        }

        private int HashLocation(Vector3 location, int mod)
        {
            int x = Mathf.FloorToInt(location.x);
            int y = Mathf.FloorToInt(location.y);
            int z = Mathf.FloorToInt(location.z);
            return Mathf.Abs(x ^ y ^ z) % mod;
        }

        /// <summary>
        /// Called by the Switch class when creating an OpenFlight Switch node from file.
        /// </summary>
        /// <param name="switchData"></param>
        public virtual void OnSwitchNode(Switch switchData)
        {
            index = switchData.Index;
            masks = switchData.Masks;
        }


    }
}