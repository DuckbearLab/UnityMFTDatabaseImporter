﻿using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace DatabaseImporter.Importers
{

    public struct MftData
    {

        public struct LevelData
        {
            public int Number { get; set; }
            public int Rows { get; set; }
            public int Columns { get; set; }
        }

        public string Folder { get; private set; }
        public string DatasetName { get; private set; }
        public float TerrainExtentX { get; private set; }
        public float TerrainExtentY { get; private set; }
        public float TerrainOriginX { get; private set; }
        public float TerrainOriginY { get; private set; }
        public float TerrainRefLat { get; private set; }
        public float TerrainRefLon { get; private set; }
        public int NumTileTexels { get; private set; }
        public Dictionary<int, LevelData> LevelsData { get; private set; }

        public static MftData FromFile(string mftFilePath)
        {
            var result = new MftData();

            result.Folder = Path.GetDirectoryName(mftFilePath);

            var xml = new XmlDocument();
            xml.LoadXml(File.ReadAllText(mftFilePath));
            XmlNamespaceManager ns = new XmlNamespaceManager(xml.NameTable);
            ns.AddNamespace("ns", xml.DocumentElement.NamespaceURI);

            result.DatasetName = xml.SelectSingleNode("/ns:MetaFlightRoot/ns:Database", ns).Attributes["name"].Value;
            result.TerrainExtentX = float.Parse(xml.SelectSingleNode("/ns:MetaFlightRoot/ns:Database/ns:Coverage/ns:Extent/ns:x", ns).InnerText);
            result.TerrainExtentY = float.Parse(xml.SelectSingleNode("/ns:MetaFlightRoot/ns:Database/ns:Coverage/ns:Extent/ns:y", ns).InnerText);
            result.TerrainOriginX = float.Parse(xml.SelectSingleNode("/ns:MetaFlightRoot/ns:Database/ns:Coverage/ns:Origin/ns:x", ns).InnerText);
            result.TerrainOriginY = float.Parse(xml.SelectSingleNode("/ns:MetaFlightRoot/ns:Database/ns:Coverage/ns:Origin/ns:y", ns).InnerText);
            result.TerrainRefLat = /*0;*/float.Parse(xml.SelectSingleNode("/ns:MetaFlightRoot/ns:Database/ns:ProjectedCoordSys/ns:FlatEarth/ns:OriginLatitude", ns).InnerText);
            result.TerrainRefLon = /*0;*/float.Parse(xml.SelectSingleNode("/ns:MetaFlightRoot/ns:Database/ns:ProjectedCoordSys/ns:FlatEarth/ns:OriginLongitude", ns).InnerText);
            try
            {
                result.NumTileTexels = int.Parse(xml.SelectSingleNode("/ns:MetaFlightRoot/ns:Database/ns:VirtualTextureDataset/ns:NumTileTexels", ns).InnerText);
            }
            catch (System.Exception e) { }

            result.LevelsData = new Dictionary<int, LevelData>();
            int number = 0;
            foreach (XmlNode gridLevel in xml.SelectNodes("/ns:MetaFlightRoot/ns:Database/ns:GridStructure/ns:GridLevel", ns))
            {
                int rows = 1;
                for (int i = 0; i <= number; i++)
                    rows *= int.Parse(xml.SelectNodes("/ns:MetaFlightRoot/ns:Database/ns:GridStructure/ns:GridLevel/ns:NumRowDivisions", ns)[i].InnerText);

                int columns = 1;
                for (int i = 0; i <= number; i++)
                    columns *= int.Parse(xml.SelectNodes("/ns:MetaFlightRoot/ns:Database/ns:GridStructure/ns:GridLevel/ns:NumColDivisions", ns)[i].InnerText);

                result.LevelsData.Add(number, new LevelData()
                {
                    Number=number++,
                    Rows=rows,
                    Columns=columns
                });
            }

            return result;
        }

    }

}