using DatabaseImporter.Importers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DatabaseImporter.Runtime
{

    public class RuntimeTerrainImporter : MonoBehaviour
    {

        public event Action TerrainLoaded;
        public event System.Action<int, int> LoadedTerrainPart;

        public string TerrainMftFile;
        public string VtMftFile;
        public string BuildingsMftFile;
        public string TreesMftFile;

        private Importers.TerrainImporter terrainImporter;

        public TreeReplacer TreeReplacer;

        public Material MaterialToUse;

        void Start()
        {
            terrainImporter = new Importers.TerrainImporter(
                MftData.FromFile(TerrainMftFile),
                MftData.FromFile(VtMftFile),
                new MftData[] {
                    MftData.FromFile(BuildingsMftFile),
                    MftData.FromFile(TreesMftFile)
                }
            );
            terrainImporter.MaterialToUse = MaterialToUse;
            terrainImporter.PrepareForLoad();
            terrainImporter.LoadedTerrain += terrainImporter_LoadedTerrain;
            terrainImporter.LoadedTerrainPart += terrainImporter_LoadedTerrainPart;
        }

        void terrainImporter_LoadedTerrainPart(int arg1, int arg2)
        {
            LoadedTerrainPart(arg1, arg2);
        }

        void terrainImporter_LoadedTerrain()
        {
            this.TerrainLoaded();
            this.TreeReplacer.ReplaceTrees();
        }

        void Update()
        {
            AddedOnThisFrame = 0;
            //terrainImporter.LoadOneAsync(this);
            terrainImporter.LoadOne();
        }


        public int AddedOnThisFrame { get; set; }
    }

}