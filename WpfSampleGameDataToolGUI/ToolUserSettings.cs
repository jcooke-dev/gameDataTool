
using GameDataTool;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace WpfSampleGameDataToolGUI
{
    internal static class ToolUserSettings
    {
        public static bool bIsMaximized { get; set; }

        public static double dTop { get; set; }
        public static double dLeft { get; set; }
        public static double dHeight { get; set; }
        public static double dWidth { get; set; }

        public static string strLastFolder { get; set; } = "";

        private const short sMaxRecentPKDs = 10;
        public static ObservableCollection<string> qRecentPKDPaths { get; } = new ObservableCollection<string>();


        public static string getDialogFileTypesFilter()
        {
            string strRet = "Asset Files|";

            foreach(string strExt in DataPackage.dictValidAssetExtTypes.Keys)
            {
                strRet += "*" + strExt.ToLower() + ";";
            }

            return strRet;
        }

        public static void LoadToolSettings()
        {

            bIsMaximized = Properties.Settings.Default.bIsMaximized;
            dTop = Properties.Settings.Default.dTop;
            dLeft = Properties.Settings.Default.dLeft;
            dHeight = Properties.Settings.Default.dHeight;
            dWidth = Properties.Settings.Default.dWidth;

            strLastFolder = Properties.Settings.Default.strLastAccessedFolder;

            if (!Directory.Exists(strLastFolder))
            {
                strLastFolder = "";
            }

            qRecentPKDPaths.Clear();
            qRecentPKDPaths.Add("< OR Choose from your recently accessed PKD files: >");
            string[] saPaths = Properties.Settings.Default.strRecentPKDPaths.Split("^");
            foreach(string strPath in saPaths)
            {
                if (File.Exists(strPath))
                {
                    qRecentPKDPaths.Add(strPath);
                }
            }

            return;
        }

        public static void SaveToolSettings()
        {

            Properties.Settings.Default.bIsMaximized = bIsMaximized;
            Properties.Settings.Default.dTop = dTop;
            Properties.Settings.Default.dLeft = dLeft;
            Properties.Settings.Default.dHeight = dHeight;
            Properties.Settings.Default.dWidth = dWidth;

            Properties.Settings.Default.strLastAccessedFolder = strLastFolder;

            StringBuilder sbCombinedPaths = new StringBuilder();
            foreach(string strPath in qRecentPKDPaths)
            {
                if(File.Exists(strPath))
                {
                    sbCombinedPaths.Append(strPath + '^');
                }
            }
            Properties.Settings.Default.strRecentPKDPaths = sbCombinedPaths.ToString();

            Properties.Settings.Default.Save();

            return;
        }

        public static void addToRecentPKDList(string strPath)
        {
            // remove any dupes
            bool bRemoved = false;
            do
            {
                bRemoved = qRecentPKDPaths.Remove(strPath);
            } while (bRemoved);

            qRecentPKDPaths.Insert(1, strPath); // inserting at the 'real' first entry (index 1 since 0 is the placeholder text)

            // limit this to a rolling window max (adding 1 to account for placeholder first text entry)
            while (qRecentPKDPaths.Count > (sMaxRecentPKDs + 1))
            {
                qRecentPKDPaths.RemoveAt(qRecentPKDPaths.Count - 1);
            }

            return;
        }

    }
}
