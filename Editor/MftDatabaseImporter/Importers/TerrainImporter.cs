using DatabaseImporter.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DatabaseImporter.Importers
{

    public class TerrainImporter
    {
        public event System.Action<int, int> LoadedTerrainPart;
        public event System.Action LoadedTerrain;

        public Material MaterialToUse { get; set; }
        public Transform ParentTransform = null;

        private MftData terrainMft;
        private MftData vtMft;
        private MftData[] otherMfts;

        private UFLT.Utils.MaterialBank materialBank;
        private UFLT.Utils.ExternalReferencesBank externalReferencesBank;
        private UFLT.ImportSettings importSettings;

        private bool done;

        private delegate void PostProcessingFunc(GameObject result);

        private int curPartLevel;
        private int curPartCol;
        private int curPartRow;

        public TerrainImporter(MftData terrainMft, MftData vtMft, MftData[] otherMfts)
        {
            this.terrainMft = terrainMft;
            this.vtMft = vtMft;
            this.otherMfts = otherMfts;
            this.done = false;
        }

        private struct PartToLoad
        {
            public int num;
            public int col;
            public int row;
        }

        private int totalPartsToLoad;
        private Queue<PartToLoad> partsToLoad;

        public void PrepareForLoad()
        {
            this.materialBank = new UFLT.Utils.MaterialBank();
            this.externalReferencesBank = new UFLT.Utils.ExternalReferencesBank(this.ParentTransform.gameObject.AddComponent<PutCopy>());

            this.importSettings = new UFLT.ImportSettings();
            this.importSettings.generateColliders = true;
            this.importSettings.additionalSearchDirectories.Add(Path.Combine(Directory.GetParent(terrainMft.Folder).FullName, "lel.txt"));

            partsToLoad = new Queue<PartToLoad>();
            foreach (var level in this.terrainMft.LevelsData.Values)
            {
                for (int col = 0 * Mathf.RoundToInt(Mathf.Pow(2, level.Number)); col < level.Columns/* && col < 3 * Mathf.Pow(2, level.Number)*/; col++)
                    for (int row = 0 * Mathf.RoundToInt(Mathf.Pow(2, level.Number)); row < level.Rows/* && row < 3 * Mathf.Pow(2, level.Number)*/; row++)
                        partsToLoad.Enqueue(new PartToLoad() { num = level.Number, col = col, row = row });
            }
            totalPartsToLoad = partsToLoad.Count;

            AddTerrainRefLatLon();
            AddTerrainMinimap();
        }

        private void AddTerrainRefLatLon()
        {
            var refLatLon = ParentTransform.gameObject.AddComponent<TerrainRefLatLon>();

            refLatLon.RefLat = terrainMft.TerrainRefLat;
            refLatLon.RefLon = terrainMft.TerrainRefLon;

            refLatLon.OriginX = terrainMft.TerrainOriginX;
            refLatLon.OriginY = terrainMft.TerrainOriginY;
        }

        private void AddTerrainMinimap()
        {
            foreach(var level in terrainMft.LevelsData.Values)
            {
                if(level.Columns > 4 && level.Columns > 4)
                {
                    var minimap = ParentTransform.gameObject.AddComponent<TerrainMinimap>();
                    minimap.MapMin = new Vector2(terrainMft.TerrainOriginX, terrainMft.TerrainOriginY);
                    minimap.MapMax = new Vector2(terrainMft.TerrainOriginX + terrainMft.TerrainExtentX, terrainMft.TerrainOriginY + terrainMft.TerrainExtentY);

                    minimap.MapTextureParts = new Texture2D[level.Columns * level.Rows];
                    minimap.MapTexturePartsColumns = level.Columns;
                    minimap.MapTexturePartsRows = level.Rows;

                    int i = 0;
                    for (int y = 0; y < level.Rows; y++)
                        for (int x = 0; x < level.Columns; x++)
                            minimap.MapTextureParts[i++] = LoadTexture(GetVtPath(level.Number, x, y)); ;

                    return;
                }
            }
        }

        Texture2D LoadTexture(string path)
        {
            var bytes = File.ReadAllBytes(path);
            int height = bytes[13] * 256 + bytes[12];
            int width = bytes[17] * 256 + bytes[16];

            byte[] dxt = new byte[bytes.Length - 128];
            System.Buffer.BlockCopy(bytes, 128, dxt, 0, bytes.Length - 128);

            Texture2D tex = new Texture2D(width, height, TextureFormat.DXT1, false);
            tex.LoadRawTextureData(dxt);
            tex.Apply();

            return tex;
        }

        public void LoadAll()
        {
            while(partsToLoad.Count > 0)
                LoadOne();
        }

        public void LoadOne()
        {
            if (partsToLoad.Count > 0)
            {
                var partToLoad = partsToLoad.Dequeue();

                curPartLevel = partToLoad.num;
                curPartCol = partToLoad.col;
                curPartRow = partToLoad.row;

                if(!NextLevelTerrainPartExists())
                    LoadLayerPart(terrainMft, TerrainPostProcessing);
                for (int i = 0; i < otherMfts.Length; i++)
                    LoadLayerPart(otherMfts[i]);

                LoadedTerrainPart(partsToLoad.Count, totalPartsToLoad);
            }
            else
            {
                if(!done)
                {
                    done = true;
                    LoadedTerrain();
                }
            }
        }

        private void LoadLayerPart(MftData layerMft, PostProcessingFunc postProcessing = null)
        {
            string fltFile = GetFltPath(layerMft);
            if (File.Exists(fltFile))
            {
                var result = LoadFltToGameObject(fltFile);

                result.name = Path.GetFileName(fltFile);
                PositionTerrainPart(result);

                if(postProcessing != null)
                    postProcessing(result);

                AddColliders(result);
            }
        }

        private void TerrainPostProcessing(GameObject result)
        {
            string vtTexture = GetVtPath(curPartLevel, curPartCol, curPartRow);

            var bytes = File.ReadAllBytes(vtTexture);
            int height = bytes[13] * 256 + bytes[12];
            int width = bytes[17] * 256 + bytes[16];

            byte[] dxt = new byte[bytes.Length - 128];
            System.Buffer.BlockCopy(bytes, 128, dxt, 0, bytes.Length - 128);

            Texture2D tex = new Texture2D(width, height, TextureFormat.DXT1, false);
            tex.LoadRawTextureData(dxt);
            tex.Apply();
            tex.name = Path.GetFullPath(vtTexture);

            foreach (var mRenderer in result.GetComponentsInChildren<MeshRenderer>())
            {
                var newMat = new Material(MaterialToUse);

                var sharedMaterials = mRenderer.sharedMaterials;

                float scale = Mathf.Pow(2, GetVtLevel(curPartLevel)) / vtMft.NumTileTexels;
                var textureScale = new Vector2(scale, scale);

                newMat.SetTexture("_BaseMap", tex);
                newMat.SetTextureScale("_BaseMap", textureScale);
                newMat.name = tex.name + textureScale.ToString();

                for (int i = 0; i < mRenderer.sharedMaterials.Length; i++)
                    sharedMaterials[i] = newMat;

                mRenderer.sharedMaterials = sharedMaterials;
            }
        }

        private void AddColliders(GameObject result)
        {
            foreach(var mRenderer in result.GetComponentsInChildren<MeshRenderer>())
            {
                if (mRenderer.GetComponent<MeshCollider>() == null)
                    mRenderer.gameObject.AddComponent<MeshCollider>();
            }
        }

        private UFLT.Records.Database LoadFlt(string fltFile)
        {
            UFLT.Records.Database db = new UFLT.Records.Database(fltFile, null, this.importSettings);
            db.MaterialBank = this.materialBank;
            db.ExternalReferencesBank = this.externalReferencesBank;
            db.Parse();
            db.PrepareForImport();

            return db;
        }

        private GameObject LoadFltToGameObject(string fltFile)
        {
            UFLT.Records.Database db = new UFLT.Records.Database(fltFile, null, this.importSettings);
            db.MaterialBank = this.materialBank;
            db.ExternalReferencesBank = this.externalReferencesBank;
            db.Parse();
            db.PrepareForImport();
            db.ImportIntoScene();

            db.UnityGameObject.transform.parent = ParentTransform;

            return db.UnityGameObject;
        }

        private void PositionTerrainPart(GameObject terrainPart)
        {
            int groupingAmount = Mathf.RoundToInt(Mathf.Pow(2, curPartLevel));
            Vector3 halfPartOffset = new Vector3(
                terrainMft.TerrainExtentX / terrainMft.LevelsData[0].Columns / 2,
                0,
                terrainMft.TerrainExtentY / terrainMft.LevelsData[0].Rows / 2
                );
            Vector3 partPosition = new Vector3(
                (curPartCol / groupingAmount * groupingAmount) * terrainMft.TerrainExtentX / terrainMft.LevelsData[curPartLevel].Columns,
                0,
                (curPartRow / groupingAmount * groupingAmount) * terrainMft.TerrainExtentY / terrainMft.LevelsData[curPartLevel].Rows);
            //Vector3 terrainOffset = new Vector3(terrainMft.TerrainOriginX, 0, terrainMft.TerrainOriginY);
            terrainPart.transform.position = halfPartOffset + partPosition/* + terrainOffset*/;
        }

        private string GetFltPath(MftData mft)
        {
            return Path.Combine(mft.Folder, string.Format(@"v{1}\r{3}\{0}-v{1}-c{2}-r{3}.flt", mft.DatasetName, PadNumber(curPartLevel, 2), PadNumber(curPartCol, 3), PadNumber(curPartRow, 3)));
        }

        private string GetVtPath(int terrainLevelNumber, int col, int row)
        {
            var levelDirNum = Mathf.RoundToInt(Mathf.Pow(2, GetVtLevel(terrainLevelNumber)));
            return Path.Combine(this.vtMft.Folder, string.Format(@"level-{1}\r{3}\{0}-l{1}-c{2}-r{3}.dds", this.vtMft.DatasetName, PadNumber(levelDirNum, 8), PadNumber(col, 3), PadNumber(row, 3)));
        }

        private int GetVtLevel(int terrainLevelNumber)
        {
            foreach (var vtLevel in vtMft.LevelsData.Values)
                if (vtLevel.Rows >= terrainMft.LevelsData[terrainLevelNumber].Rows && vtLevel.Columns >= terrainMft.LevelsData[terrainLevelNumber].Columns)
                    return vtLevel.Number;
            throw new Exception("Vt not found :(");
        }

        private bool NextLevelTerrainPartExists()
        {
            if (terrainMft.LevelsData.Count > curPartLevel + 1)
            {
                int colMult = terrainMft.LevelsData[curPartLevel + 1].Columns / terrainMft.LevelsData[curPartLevel].Columns;
                int rowMult = terrainMft.LevelsData[curPartLevel + 1].Rows / terrainMft.LevelsData[curPartLevel].Rows;
                return File.Exists(Path.Combine(terrainMft.Folder, string.Format(@"v{1}\r{3}\{0}-v{1}-c{2}-r{3}.flt", terrainMft.DatasetName, PadNumber(curPartLevel + 1, 2), PadNumber(curPartCol * colMult, 3), PadNumber(curPartRow * rowMult, 3))));
            }
            return false;
        }

        private string PadNumber(int number, int desiredDigitd)
        {
            string result = number.ToString();
            while (result.Length < desiredDigitd)
                result = "0" + result;
            return result;
        }

    }

}