using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UFLT.Records;
using UnityEngine;

namespace UFLT.Utils
{
    public class ExternalReferencesBank
    {

        private PutCopy putCopyComponent;
        private Dictionary<string, ExternalReference> bank;

        public ExternalReferencesBank(PutCopy putCopyComponent)
        {
            bank = new Dictionary<string, ExternalReference>();
            this.putCopyComponent = putCopyComponent;
        }

        public bool Contains(string path)
        {
            return bank.ContainsKey(path);
        }

        public void Add(string Path, ExternalReference externalReference)
        {
            bank.Add(Path, externalReference);
        }

        public bool ContainsMe(string path, ExternalReference me)
        {
            ExternalReference result;
            if(bank.TryGetValue(path, out result))
                return result == me;
            return false;
        }

        public ExternalReference Get(string path)
        {
            return bank[path];
        }

        public void AddCopy(GameObject tCopy, Vector3 position, Quaternion rotation)
        {
            if (putCopyComponent != null)
            {
                putCopyComponent.Copies.Add(new PutCopy.Details()
                {
                    ToCopy = tCopy,
                    Position = position,
                    Rotation = rotation
                });
            }
        }

    }
}
