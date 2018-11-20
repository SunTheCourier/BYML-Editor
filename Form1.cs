using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using The4Dimension;
using Yaz0Enc;

namespace BYML_Editor
{

    public partial class Editor : Form
    {
        static DirectoryInfo TempPath = new DirectoryInfo($"{Path.GetTempPath()}/BYML-Editor");
        static FileInfo YamlPath = new FileInfo($"{Path.GetTempPath()}/BYML-Editor/temp.yaml");
        private bool IsXML;

        public Editor()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
        }

        private void OpenYAMLDisplayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConvertBYML(false);
        }

        private void OpenXMLDisplayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConvertBYML(true);
        }

        private void CreateYAMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.Text = "";
            textBox.ReadOnly = true;
            IsXML = false;
        }

        private void CreateXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.Text = "";
            textBox.ReadOnly = false;
            IsXML = true;
        }

        private void SaveLittleEndianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save(false);
        }

        private void SaveBigEndianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save(true);
        }

        private void ClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.Text = "";
            textBox.ReadOnly = true;
        }

        private void Yaz0CompressLittleEndianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openymlFileDialog.ShowDialog() == DialogResult.OK)
            {
                FileInfo file = new FileInfo(openymlFileDialog.FileName);
                
                saveyaz0FileDialog.ShowDialog();
                if (saveyaz0FileDialog.FileName != "")
                { 
                    FileInfo selected = new FileInfo(saveFileDialog.FileName);

                    FileStream byml = file.OpenRead();
                    File.WriteAllBytes(selected.FullName, Yaz0.Encode(byml));
                    byml.Close();
                }
            }
        }

        private void ConvertBYML(bool wantXML)
        {
            Directory.CreateDirectory(TempPath.FullName);

            OpenFileDialog bymlselect;
            if (wantXML == true) bymlselect = openxmlFileDialog;
            else bymlselect = openymlFileDialog;

            if (bymlselect.ShowDialog() == DialogResult.OK)
            {
                FileInfo selected = new FileInfo(bymlselect.FileName);

                if (wantXML == false)
                {
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                        FileName = "cmd.exe",
                        Arguments = $"/C byml_to_yml.exe \"{selected.FullName}\" \"{YamlPath.FullName}\""
                    };
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();

                    textBox.Text = File.ReadAllText(YamlPath.FullName);
                    IsXML = false;
                }
                else
                {
                    textBox.Text = BymlConverter.GetXml(selected.FullName);
                    IsXML = true;
                }
                bymlselect.FileName = "";
                textBox.ReadOnly = false;
            } 
        }

        private void Save(bool isBigEndian)
        {
            saveFileDialog.ShowDialog();
            if (saveFileDialog.FileName != "")
            {
                FileInfo savePath = new FileInfo(saveFileDialog.FileName);
                if (IsXML == true)
                {
                    if (isBigEndian == true) textBox.Text.Replace($"isBigEndian Value=\"False\"", $"isBigEndian Value=\"True\"");
                    else textBox.Text.Replace($"isBigEndian Value=\"True\"", $"isBigEndian Value=\"False\"");
                    File.WriteAllBytes(savePath.FullName, BymlConverter.GetByml(textBox.Text));
                }
                else
                {
                    File.WriteAllText(YamlPath.FullName, textBox.Text);
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                        FileName = "cmd.exe"
                    };
                    if (isBigEndian == true) startInfo.Arguments = $"/C yml_to_byml.exe \"{YamlPath.FullName}\" \"{savePath.FullName}\" -b";
                    else startInfo.Arguments = $"/C yml_to_byml.exe \"{YamlPath.FullName}\" \"{savePath.FullName}\"";
                    process.StartInfo = startInfo;
                    //todo: catch errors somehow
                    process.Start();
                    process.WaitForExit();
                }
                saveFileDialog.FileName = "";
            }
        }

        private void DecryptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openxmlFileDialog.ShowDialog() == DialogResult.OK)
            {
                DialogResult result = gamefolderBrowserDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string relativefile = openxmlFileDialog.FileName.Replace(gamefolderBrowserDialog.SelectedPath, "").Replace('\\', '/').Substring(1);
                    List<string> args = new List<string>(new string[] { relativefile, openxmlFileDialog.FileName });
                    NisasystSharp.NisasystSharp.Decrypt(args.ToArray());
                }
            }
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            if (TempPath.Exists) TempPath.Delete(true);
        }
    }
}
