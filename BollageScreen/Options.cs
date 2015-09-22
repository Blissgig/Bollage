using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace BollageScreenSaver
{
    public partial class Options : Form
    {
        public Options()
        {
            InitializeComponent();
        }

        private void Options_Load(object sender, EventArgs e)
        {
            try
            {
                string sFileName = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\" + Application.ProductName + "\\BollageFolderList.txt";

                if (File.Exists(sFileName))
                {
                    StreamReader rdrFolders = File.OpenText(@sFileName);

                    while (!rdrFolders.EndOfStream)
                    {
                        string sLine = rdrFolders.ReadLine();
                        this.lstFolders.Items.Add(sLine);                      
                    }
                    rdrFolders.Close();
                    rdrFolders = null;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void cmdClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cmdAdd_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlgFolder = new FolderBrowserDialog();

            if (dlgFolder.ShowDialog() == DialogResult.OK)
            {
                this.lstFolders.Items.Add(dlgFolder.SelectedPath);
                SaveFolders();
            }

        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = this.lstFolders.SelectedIndices.Count - 1; i >= 0; i--)
                {
                    this.lstFolders.Items.RemoveAt(this.lstFolders.SelectedIndices[i]);
                }
                SaveFolders();
            }
            catch (Exception ex)
            {
            }
        }

        private void SaveFolders()
        {
            try
            {
                string sAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\" + Application.ProductName;
                string sFolders = sAppDataFolder + "\\BollageFolderList.txt";


                if (Directory.Exists(sAppDataFolder) == false)
                {
                    //In case this is the first time the app is run, we need the app folder
                    Directory.CreateDirectory(sAppDataFolder);
                }

                if (File.Exists(sFolders))
                {
                    //Delete the file, we're gonna create a new one.  Bit of a hack, but it is easiest AND surest way.
                    File.Delete(sFolders);
                }

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@sFolders))
                {
                    foreach (string sFolder in this.lstFolders.Items)
                    {
                        file.WriteLine(sFolder);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        
    }
}
