using System.Collections.ObjectModel;
using System.Diagnostics;

namespace GameDataTool
{
    public class DataPackage
    {

        public static Dictionary<string, eAssetType> dictValidAssetExtTypes { get; } =
            new Dictionary<string, eAssetType>
            {
                // must store each extension key as UPPERCASE (for reliable later mapping)
                { ".PNG", eAssetType.IMAGE },
                { ".JPG", eAssetType.IMAGE },
                { ".JPEG", eAssetType.IMAGE },
                { ".BMP", eAssetType.IMAGE },
                { ".MAT", eAssetType.TEXTURE },
                { ".DDS", eAssetType.TEXTURE },
                { ".MP3", eAssetType.SOUND },
                { ".WAV", eAssetType.SOUND },
                { ".OBJ", eAssetType.MESH },
                { ".FBX", eAssetType.MESH },
                { ".PY", eAssetType.SCRIPT },
                { ".PS1", eAssetType.SCRIPT },
                { ".XML", eAssetType.DATA },
                { ".JSON", eAssetType.DATA }
            };

        public ObservableCollection<AssetFile> listAssets { get; set; }

        public string strFileExt { get; set; }
        public string strFileName { get; set; }
        public string strFullPath { get; set; }
        public int nChecksum { get; set; }


        public DataPackage()
        {
            listAssets = new ObservableCollection<AssetFile>();
            strFileExt = ".PKD";
            nChecksum = 0;

            return;
        }

        public void clear()
        {
            listAssets.Clear();
            nChecksum = 0;
        }

        public bool generatePKDFile(string strFolder, string strBaseName, bool bStamp = true)
        {
            bool bRet = true;

            try
            {
                if (listAssets.Count >= 1)
                {
                    strFileName = strBaseName + ((bStamp) ? ("_" + DateTime.Now.ToString("yyyyMMdd-HHmmssff")) : (""));
                    strFullPath = Path.Combine(strFolder, strFileName + strFileExt);

                    using (FileStream fsOut = new FileStream(strFullPath, FileMode.OpenOrCreate))
                    using (BinaryWriter brOut = new BinaryWriter(fsOut))
                    {
                        writeHeaderToFile(brOut);

                        foreach (AssetFile af in listAssets)
                        {
                            if( !writeAssetToFile(brOut, af) )
                            {
                                bRet = false;
                                break;
                            }
                        }
                    }

                }
                else
                {
                    bRet = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                bRet = false;
            }

            return bRet;
        }

        public bool loadFromPKDFile(string strFilePath)
        {
            return readFromFile(strFilePath);
        }

        private int getChecksumIncrement(byte[] baBytes)
        {
            // calculate and return the checksum change
            int nCheckInc = 0;
            short nNumBytes = 4;

            for (int nIndex = 0; nIndex <= (baBytes.Length - nNumBytes); nIndex += nNumBytes)
            {
                // note we don't care about endianness, since only calculating a checksum (no interest in actual int value here)
                nCheckInc += BitConverter.ToInt32(baBytes, nIndex);
            }

            return nCheckInc;
        }

        public bool addFromFolder(string strFolderPath)
        {
            bool bRet = true;

            try
            {
                foreach (string strFilePath in Directory.GetFiles(strFolderPath))
                {
                    if (dictValidAssetExtTypes.Keys.Contains(Path.GetExtension(strFilePath), StringComparer.OrdinalIgnoreCase))
                    {
                        listAssets.Add(new AssetFile(strFilePath));
                        // add to the checksum
                        nChecksum += getChecksumIncrement(listAssets[listAssets.Count - 1].baContents);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                bRet = false;
            }

            return bRet;
        }
        
        public bool addFile(string strFilePath)
        {
            bool bRet = true;

            try
            {
                if (dictValidAssetExtTypes.Keys.Contains(Path.GetExtension(strFilePath), StringComparer.OrdinalIgnoreCase))
                {
                    listAssets.Add(new AssetFile(strFilePath));
                    // add to the checksum
                    nChecksum += getChecksumIncrement(listAssets[listAssets.Count - 1].baContents);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                bRet = false;
            }

            return bRet;
        }

        private bool writeHeaderToFile(BinaryWriter bw)
        {
            bool bRet = true;

            try
            {
                bw.Write(listAssets.Count);
                bw.Write(nChecksum);

                foreach(AssetFile af in listAssets)
                {
                    bw.Write((int)af.atAssetType);
                    bw.Write(af.strFolder);
                    bw.Write(af.strName);
                    bw.Write(af.strExtension);
                    bw.Write(af.lSize);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                bRet = false;
            }

            return bRet;
        }

        private bool readFromFile(string strPath)
        {
            bool bRet = true;

            try
            {
                using (FileStream fsIn = new FileStream(strPath, FileMode.Open))
                using (BinaryReader brIn = new BinaryReader(fsIn))
                {

                    List<AssetFile> listAssetFiles = new List<AssetFile>();

                    int nNumAssets = brIn.ReadInt32();
                    int nCheckFromFile = brIn.ReadInt32();
                    int nCheckCalculated = 0;

                    for (int nIndex = 0; nIndex < nNumAssets; nIndex++)
                    {
                        eAssetType aType = (eAssetType)brIn.ReadInt32();

                        string strFolder = brIn.ReadString();
                        string strName = brIn.ReadString();
                        string strExt = brIn.ReadString();

                        long lSize = brIn.ReadInt64();

                        listAssetFiles.Add(new AssetFile(null, aType, strFolder, strExt, strName, lSize));
                    }

                    // now, read the sequence of stored file bytes and set them in the asset list
                    foreach(AssetFile afFile in listAssetFiles)
                    {
                        // we can read a single asset file up to ~2GB, so that's fine
                        afFile.baContents = brIn.ReadBytes((int)afFile.lSize);
                        // add to the locally read checksum
                        nCheckCalculated += getChecksumIncrement(afFile.baContents);
                    }

                    // now, add all of these loaded assets to the base DataPackage assets list, if checksum indicates safe
                    if (nCheckFromFile == nCheckCalculated)
                    {
                        foreach (AssetFile afFile in listAssetFiles)
                        {
                            listAssets.Add(afFile);
                        }

                        nChecksum += nCheckFromFile;
                    }
                    else
                    {
                        bRet = false;
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                bRet = false;
            }

            return bRet;
        }

        private bool writeAssetToFile(BinaryWriter bw, AssetFile aFile)
        {
            bool bRet = true;

            try
            {
                bw.Write(aFile.baContents);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                bRet = false;
            }

            return bRet;
        }

        // added this only as a functional test to extract the individual files from a PKD and confirm they're identical to the source files
        public bool extractPKDFilesToPath(string strFolder)
        {
            bool bRet = true;

            try
            {
                if (listAssets.Count >= 1)
                {
                    foreach (AssetFile af in listAssets)
                    {
                        string strFullPath = Path.Combine(strFolder, af.strName + af.strExtension);
                        short sIncr = 1;
                        while (File.Exists(strFullPath))
                        {
                            strFullPath = Path.Combine(strFolder, af.strName + "_" + sIncr++ + af.strExtension);
                        }

                        using (FileStream fsOut = new FileStream(strFullPath, FileMode.CreateNew))
                        using (BinaryWriter brOut = new BinaryWriter(fsOut))
                        {
                            if (!writeAssetToFile(brOut, af))
                            {
                                bRet = false;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    bRet = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                bRet = false;
            }

            return bRet;
        }

    }
}
