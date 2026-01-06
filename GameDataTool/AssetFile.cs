namespace GameDataTool
{
    public enum eAssetType
    {
        IMAGE,
        TEXTURE,
        MESH,
        SOUND,
        SCRIPT,
        DATA
    }

    
    public class AssetFile
    {
        public eAssetType atAssetType { get; }
        public string strFolder { get; }
        public string strExtension { get; }
        public string strName { get; }
        public string strPath { get; }
        public long lSize { get; }
        public byte[]? baContents { get; set; }
        public bool bIsBackedByFile { get; }


        public AssetFile(eAssetType atType, string strFileFolder, string strFileExt, string strFileName, string strFilePath, long lFileSize, byte[] baContent, bool bIsFromFile)
        {
            atAssetType = atType;
            strFolder = strFileFolder;
            strExtension = strFileExt;
            strName = strFileName;
            strPath = strFilePath;
            lSize = lFileSize;
            baContents = baContent;
            bIsBackedByFile = bIsFromFile;

            return;
        }

        public AssetFile(string strFilePath)
        {
            bIsBackedByFile = true;

            strFolder = System.IO.Path.GetDirectoryName(strFilePath);
            strName = System.IO.Path.GetFileNameWithoutExtension(strFilePath);
            strExtension = System.IO.Path.GetExtension(strFilePath);
            
            strPath = strFilePath;

            try
            {
                atAssetType = DataPackage.dictValidAssetExtTypes[strExtension.ToUpper()];
            }
            catch (KeyNotFoundException)
            {
                atAssetType = eAssetType.DATA;
            }

            lSize = new FileInfo(strFilePath).Length;

            baContents = File.ReadAllBytes(strPath);

            return;
        }

        public AssetFile(byte[]? baContent, eAssetType atType, string strFileFolder, string strFileExt, string strFileName, long lFileSize)
        {
            bIsBackedByFile = false;

            strFolder = strFileFolder;
            strName = strFileName;
            strExtension = strFileExt;
            strPath = "";
                        
            atAssetType = atType;
            lSize = lFileSize;

            baContents = baContent;

            return;
        }
    }
}
