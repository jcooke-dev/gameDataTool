using GameDataTool;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;


namespace WpfSampleGameDataToolGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DataPackage dpPackage;

        protected double dWindowHeightOld;
        protected double dLBAssetsHeight;
        protected DispatcherTimer timerPopup;
        private string strTextRed = "#FFFFB3B3";
        private string strTextYellow = "#FFFFF500";


        public MainWindow()
        {
            InitializeComponent();

            return;
        }

        private void Window_Initialized(object sender, EventArgs e)
        {

            ToolUserSettings.LoadToolSettings();
            setWindowState();

            dpPackage = new DataPackage();

            lbAssets.ItemsSource = dpPackage.listAssets;
            dWindowHeightOld = this.ActualHeight;
            dLBAssetsHeight = lbAssets.ActualHeight;

            imgView1.Visibility = Visibility.Hidden;
            lblNonImage.Visibility = Visibility.Hidden;

            cmbRecentPKDs.ItemsSource = ToolUserSettings.qRecentPKDPaths;
            cmbRecentPKDs.SelectedIndex = 0;

            blurGrid.Radius = 0;

            updateUIButtons();
            
            return;
        }

        void displayPopupMessage( string strMessage, short sSecs, string strColor )
        {
            timerPopup = new DispatcherTimer();
            timerPopup.Interval = TimeSpan.FromSeconds(sSecs);
            timerPopup.Tick += TimerPopup_Tick;

            this.IsHitTestVisible = false;
            lblPopMsg.Content = strMessage;
            BrushConverter converter = new BrushConverter();
            Brush brColor = (Brush)converter.ConvertFromString( strColor );
            lblPopMsg.Foreground = brColor;
            popMessage.IsOpen = true;

            blurGrid.Radius = 15;

            timerPopup.Start();
        }

        private void TimerPopup_Tick(object? sender, EventArgs e)
        {
            this.IsHitTestVisible = true;
            popMessage.IsOpen = false;
            blurGrid.Radius = 0;
            timerPopup.Stop();
        }

        private void setWindowState()
        {
            if (ToolUserSettings.bIsMaximized)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;

                this.Top = ToolUserSettings.dTop;
                this.Left = ToolUserSettings.dLeft;
                this.Height = ToolUserSettings.dHeight;
                this.Width = ToolUserSettings.dWidth;
            }

            return;
        }

        private void recordWindowState()
        {
            ToolUserSettings.bIsMaximized = (this.WindowState == WindowState.Maximized);

            ToolUserSettings.dTop = this.Top;
            ToolUserSettings.dLeft = this.Left;
            ToolUserSettings.dHeight = this.Height;
            ToolUserSettings.dWidth = this.Width;

            return;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            recordWindowState();

            return;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            recordWindowState();

            double dHeightChange = this.ActualHeight - dWindowHeightOld;
            dWindowHeightOld = this.ActualHeight;

            return;
        }
        
        private void Window_LocationChanged(object sender, EventArgs e)
        {
            recordWindowState();

            return;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ToolUserSettings.SaveToolSettings();

            return;
        }

        private void updateUIButtons()
        {
            if(dpPackage.listAssets.Count > 0)
            {
                btnClearPackage.IsEnabled = true;
                btnGeneratePKD.IsEnabled = true;
                btnExtractFromPKD.IsEnabled = true;
            }
            else
            {
                btnClearPackage.IsEnabled = false;
                btnGeneratePKD.IsEnabled = false;
                btnExtractFromPKD.IsEnabled = false;
            }

            return;
        }

        private void btnClearPackage_Click(object sender, RoutedEventArgs e)
        {
            clearPackage();
            updateUIButtons();

            return;
        }

        private void clearPackage()
        {
            dpPackage.clear();
            imgView1.Source = null;
            imgView1.Visibility = Visibility.Hidden;
            lblNonImage.Visibility = Visibility.Hidden;
            lblAssetsHeader.Text = "";
            lblAssetDetails.Text = "";

            return;
        }

        private void btnAddFromPKD_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = ToolUserSettings.strLastFolder;
            dlg.Filter = "Packaged Data Files | *.pkd;";

            bool? bRes = dlg.ShowDialog();

            if (bRes == true)
            {
                string strFileName = dlg.FileName;

                if(Path.GetExtension(strFileName).ToUpper() == ".PKD")
                {
                    if (dpPackage.loadFromPKDFile(strFileName))
                    {

                        if (lbAssets.Items.Count > 0)
                        {
                            lbAssets.SelectedIndex = lbAssets.Items.Count - 1;
                        }

                        displayPopupMessage("Files from PKD added to package", 2, strTextYellow);

                        // store in recent PKDs queue
                        ToolUserSettings.addToRecentPKDList(strFileName);
                    }
                    else
                    {
                        displayPopupMessage("There was an error loading from the PKD", 3, strTextRed);
                    }

                }

                ToolUserSettings.strLastFolder = System.IO.Path.GetDirectoryName(strFileName);
            }

            updateUIButtons();

            return;
        }

        private void btnAddFromFolder_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFolderDialog dlg = new Microsoft.Win32.OpenFolderDialog();
            dlg.InitialDirectory = ToolUserSettings.strLastFolder;
            
            bool? bRes = dlg.ShowDialog();

            if (bRes == true)
            {
                string strPath = dlg.FolderName;

                dpPackage.addFromFolder(strPath);

                if (lbAssets.Items.Count > 0)
                {
                    lbAssets.SelectedIndex = lbAssets.Items.Count - 1;
                }

                ToolUserSettings.strLastFolder = strPath;
            }

            updateUIButtons();

            return;
        }

        private void btnAddFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = ToolUserSettings.strLastFolder;
            dlg.Filter = ToolUserSettings.getDialogFileTypesFilter();

            bool? bRes = dlg.ShowDialog();

            if (bRes == true)
            {
                string strFileName = dlg.FileName;

                dpPackage.addFile(strFileName);

                if (lbAssets.Items.Count > 0)
                {
                    lbAssets.SelectedIndex = lbAssets.Items.Count - 1;
                }

                ToolUserSettings.strLastFolder = Path.GetDirectoryName(strFileName);
            }

            updateUIButtons();

            return;
        }

        private void btnGeneratePKD_Click(object sender, RoutedEventArgs e)
        {

            if (dpPackage.listAssets.Count >= 1)
            {
                dpPackage.generatePKDFile(dpPackage.listAssets[0].strFolder, dpPackage.listAssets[0].strName, true);

                displayPopupMessage("PKD file generated", 2, strTextYellow);
            }

            return;
        }

        private void lbAssets_ItemSelected(object sender, SelectionChangedEventArgs e)
        {
            if (lbAssets.SelectedItem != null)
            {
                AssetFile af = (AssetFile)lbAssets.SelectedItem;

                if (af.atAssetType == eAssetType.IMAGE)
                {
                    BitmapImage bmpImage = new BitmapImage();
                    bmpImage.BeginInit();
                    bmpImage.StreamSource = new MemoryStream(af.baContents);
                    bmpImage.EndInit();
                                        
                    imgView1.Source = bmpImage;
                    
                    imgView1.Visibility = Visibility.Visible;
                    lblNonImage.Visibility = Visibility.Hidden;
                }
                else
                {
                    imgView1.Source = null;
                    imgView1.Visibility = Visibility.Hidden;
                    lblNonImage.Visibility = Visibility.Visible;
                }

                lblAssetsHeader.Text = $"AssetType:\t\nFileName:\t\nSourcePath:\t";
                lblAssetDetails.Text = $"{af.atAssetType}\n{af.strName}{af.strExtension}\n{(af.bIsBackedByFile ? af.strPath : "** Imported from PKD File **")}";
            }

            return;
        }

        private void cmbRecentPKDs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbRecentPKDs.SelectedIndex > 0)
            {
                string strPath = cmbRecentPKDs.SelectedItem.ToString();

                if (Path.GetExtension(strPath).ToUpper() == ".PKD")
                {
                    if(dpPackage.loadFromPKDFile(strPath))
                    {
                        if (lbAssets.Items.Count > 0)
                        {
                            lbAssets.SelectedIndex = lbAssets.Items.Count - 1;
                        }

                        displayPopupMessage("Files from PKD added to package", 2, strTextYellow);

                        // store in recent PKDs queue
                        ToolUserSettings.addToRecentPKDList(strPath);
                    }
                    else
                    {
                        displayPopupMessage("There was an error loading from the PKD", 4, strTextRed);
                    }
                }

                ToolUserSettings.strLastFolder = Path.GetDirectoryName(strPath);

                cmbRecentPKDs.SelectedIndex = 0;

                updateUIButtons();
            }

            return;
        }

        private void btnExtractFromPKD_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFolderDialog dlg = new Microsoft.Win32.OpenFolderDialog();
            dlg.InitialDirectory = ToolUserSettings.strLastFolder;

            bool? bRes = dlg.ShowDialog();

            if (bRes == true)
            {
                string strPath = dlg.FolderName;

                dpPackage.extractPKDFilesToPath(strPath);

                displayPopupMessage("Files extracted into specified folder", 2, strTextYellow);

                ToolUserSettings.strLastFolder = strPath;
            }

            return;
        }
    }
}