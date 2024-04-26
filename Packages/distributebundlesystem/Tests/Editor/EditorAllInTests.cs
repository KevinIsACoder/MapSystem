using System.IO;
using NUnit.Framework;
using UnityEditor.AdvancedBundleSystem.GUI;
using UnityEditor.AdvancedBundleSystem.Settings;
using UnityEngine;

namespace UnityEditor.AdvancedBundleSystem.Tests
{
    public class EditorAllInTests
    {
        private AdvancedBundleSystemSettings m_Settings;
        
        private string assetSetNameAdd = "assetSetNew";
        private string assetSetNameRemove = "assetSetRemove";
        
        [OneTimeSetUp]
        public void Init()
        {
            AdvancedBundleSystemSettingsDefaultObject.Settings = AdvancedBundleSystemSettings.Create(AdvancedBundleSystemSettingsDefaultObject.c_DefaultConfigFolder, AdvancedBundleSystemSettingsDefaultObject.c_DefaultConfigAssetName);
            m_Settings = AdvancedBundleSystemSettingsDefaultObject.Settings;
        }
        
        [Test, Order(1)]
        public void CreateRootAssetGroup()
        {
            m_Settings.CreateAssetGroup(0,  true, null);
        }

        [Test, Order(2)]
        public void CreateSubAssetGroups()
        {
            AssetGroup rootAssetGroup = m_Settings.FindAssetGroup(0);
            AssetGroup group10001 = m_Settings.CreateAssetGroup(10001, false, rootAssetGroup);
            AssetGroup group10002 = m_Settings.CreateAssetGroup(10002, false, rootAssetGroup);
            AssetGroup group10003 = m_Settings.CreateAssetGroup(10003, false, rootAssetGroup);
            AssetGroup group10004 = m_Settings.CreateAssetGroup(10004, false, group10003);
        }

        
        [Test, Order(3)]
        public void CreateAndRemoveAssetSet()
        {
            var created = m_Settings.CreateAssetSet(assetSetNameRemove, m_Settings.RootAssetGroup, BundleCompression.LZMA, PackingMode.Separate, true, true);
            Assert.AreEqual(created.AssetSetName, assetSetNameRemove);
            m_Settings.RemoveAssetSet(m_Settings.FindAssetSet(assetSetNameRemove));
            Assert.IsNull(m_Settings.FindAssetSet(assetSetNameRemove));
        }
        
        [Test, Order(4)]
        public void CreateAndRemoveAssetEntry()
        {
            var assetGroup10004 = m_Settings.FindAssetGroup(10004);
            AssetSet createdAssetSet = m_Settings.CreateAssetSet("set8", assetGroup10004, BundleCompression.LZMA, PackingMode.Separate, false, true);
            AssetDatabase.CreateFolder("Assets", "ABS_UnitTestData");
            AssetDatabase.CreateFolder("Assets/ABS_UnitTestData", "Material1");
            string path = "Assets/ABS_UnitTestData/Material1/Material_1.mat";
            AssetDatabase.CreateAsset(new Material(Shader.Find("Specular")), path);
            AssetEntry entry = m_Settings.CreateAssetEntry(path, createdAssetSet);
            Assert.IsTrue(m_Settings.IsAssetEntryExisting(AssetDatabase.AssetPathToGUID(path), createdAssetSet));
            m_Settings.RemoveAssetEntry(entry, createdAssetSet);
            Assert.IsFalse(m_Settings.IsAssetEntryExisting(AssetDatabase.AssetPathToGUID(path), createdAssetSet));
            m_Settings.RemoveAssetSet(createdAssetSet);
            AssetDatabase.DeleteAsset("Assets/ABS_UnitTestData");
        }



        private void CreateFakeGame(string gameName, int soundCount, int soundSize, int effectCount, int effectSize, AssetGroup assetGroup, BundleCompression compressionMethod, PackingMode packingMode, bool inPackage)
        {
            void CreateFakeData(string assetPath, int size)
            {
                string absolutePath = Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - 6), assetPath);
                using (FileStream fs = File.Open(absolutePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    byte[] info = new byte[1024 * 1024 * size];
                    fs.Write(info, 0, info.Length);
                }
            }
            
            AssetDatabase.CreateFolder("Assets/ABS_UnitTestData", gameName);
            AssetDatabase.CreateFolder($"Assets/ABS_UnitTestData/{gameName}", "Sound");
            AssetDatabase.CreateFolder($"Assets/ABS_UnitTestData/{gameName}", "Effect");
            
            var assetSet = m_Settings.CreateAssetSet(gameName, assetGroup, compressionMethod, packingMode, false, inPackage);
            for (int i = 1; i <= soundCount; i++)
            {
                string soundPath = $"Assets/ABS_UnitTestData/{gameName}/Sound/sound{i}.txt";
                CreateFakeData(soundPath, soundSize);
                AssetDatabase.Refresh();
                m_Settings.CreateAssetEntry(soundPath, assetSet);
            }
            
            for (int i = 1; i <= effectCount; i++)
            {
                CreateFakeData($"Assets/ABS_UnitTestData/{gameName}/Effect/effect{i}.txt", effectSize);
            }
            AssetDatabase.Refresh();
            m_Settings.CreateAssetEntry($"Assets/ABS_UnitTestData/{gameName}/Effect", assetSet);
        }
        
        [Test, Order(5)]
        public void CreateFakeDataForRuntimeTest()
        {
            AssetDatabase.CreateFolder("Assets", "ABS_UnitTestData");
            CreateFakeGame("MainGame", 1, 20, 10, 4, m_Settings.RootAssetGroup, BundleCompression.LZ4, PackingMode.Together, true);
            CreateFakeGame("MiniGame1", 1, 20, 10, 4, m_Settings.FindAssetGroup(10001), BundleCompression.LZ4, PackingMode.Separate, true);
            CreateFakeGame("MiniGame2", 1, 20, 10, 4, m_Settings.FindAssetGroup(10002), BundleCompression.LZMA, PackingMode.Separate, false);
            CreateFakeGame("MiniGame3", 1, 20, 10, 4, m_Settings.FindAssetGroup(10003), BundleCompression.Uncompressed, PackingMode.Together, true);
            CreateFakeGame("MiniGame4", 1, 20, 10, 4, m_Settings.FindAssetGroup(10004), BundleCompression.LZMA, PackingMode.Separate, false);
            m_Settings.MarkDirtyForAll();
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        [OneTimeTearDown]
        public void ClearUp()
        {
            m_Settings = null;
            assetSetNameAdd = null;
            assetSetNameRemove = null;
        }

    }
}
