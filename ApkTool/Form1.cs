using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ApkTool
{
    public partial class Form1 : Form
    {
        private string[] jdk_apps = new string[]
{
            "keytool.exe",
            "jarsigner.exe"
};

        public string[] keystoreExtensions = new string[]
{
            ".jks",
            ".keystore"
};

        private string[] keystoreTypes = new string[]
{
            "PKCS12",
            "JCEKS",
            "JKS"
};

        private string JRE_PATH = "";
        private Ini _ini = new Ini();

        public Form1()
        {
            InitializeComponent();
        }

        private void SetStatus(string str)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    tsslStatus.Text = str;
                }));
            }
            else
                tsslStatus.Text = str;
        }

        private string AddToFileName(string file, string addition)
        {
            string extension = file.Substring(file.LastIndexOf('.'));
            return file.Replace(extension, string.Empty) + addition + extension;
        }

        private bool CheckJDKInstallation(string path)
        {
            try
            {
                string[] directories = Directory.GetDirectories(path);
                for (int j = 0; j < directories.Length; j++)
                {
                    string folder = directories[j];
                    if (folder.Substring(folder.LastIndexOf("\\") + 1) == "bin")
                    {
                        string[] files = Directory.GetFiles(folder);
                        for (int i = 0; i < files.Length; i++)
                        {
                            files[i] = files[i].Substring(files[i].LastIndexOf("\\") + 1);
                        }
                        return files.Except(jdk_apps).Any<string>();
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private int ExecuteCommand(string path, string argument, TextBox console = null)
        {
            if (File.Exists(path))
            {
                Process command = new Process();
                command.StartInfo.UseShellExecute = false;
                command.StartInfo.CreateNoWindow = true;
                command.StartInfo.RedirectStandardOutput = true;
                command.StartInfo.RedirectStandardError = true;
                command.StartInfo.FileName = path;
                command.StartInfo.Arguments = argument;
                command.Start();
                string result = command.StandardOutput.ReadToEnd();
                command.WaitForExit();
                if (console != null)
                {
                    console.AppendText(result);
                }
                int errorCode = command.ExitCode;
                if (errorCode != 0)
                {
                    string error = command.StandardError.ReadToEnd();
                    if (console != null)
                    {
                        console.AppendText(error);
                    }
                }
                command.Close();
                return errorCode;
            }
            throw new Exception("The file \"" + path.Substring(path.LastIndexOf("\\") + 1) + "\" cannot be fonud!");
        }

        private List<string> ListKeyInformation(string path, string keystore, string storepass, TextBox console = null)
        {
            List<string> info = new List<string>();
            if (File.Exists(path))
            {
                Process command = new Process();
                command.StartInfo.UseShellExecute = false;
                command.StartInfo.CreateNoWindow = true;
                command.StartInfo.RedirectStandardOutput = true;
                command.StartInfo.RedirectStandardError = true;
                command.StartInfo.FileName = path;
                command.StartInfo.Arguments = "-v -list -keystore \"" + keystore + "\" -storepass " + storepass;
                command.Start();
                string result = command.StandardOutput.ReadToEnd();
                command.WaitForExit();
                if (console != null)
                {
                    console.AppendText(result);
                }
                if (command.ExitCode == 0)
                {
                    info.AddRange(result.Split(new string[]
                    {
                        "\r\n"
                    }, StringSplitOptions.RemoveEmptyEntries));
                }
                else
                {
                    command.StandardError.ReadToEnd();
                }
                command.Close();
                return info;
            }
            throw new Exception("The application \"" + path.Substring(path.LastIndexOf("\\") + 1) + "\" cannot be found. Please check your JDK / Android Build Tool Pathes!");
        }

        private void SetEnvironmentVariable()
        {
            string jrePath = Path.Combine(Environment.CurrentDirectory, JRE_PATH);
            string pathVar = System.Environment.GetEnvironmentVariable("PATH");

            Environment.SetEnvironmentVariable("JAVA_HOME", jrePath);
            Environment.SetEnvironmentVariable("PATH", pathVar + ";" + jrePath + "\\bin");
        }

        private void ToolOutput(string info, bool append)
        {
            if (append)
                txtToolOutput.AppendText(info + Environment.NewLine);
            else
                txtToolOutput.Text = info + Environment.NewLine;
            txtToolOutput.SelectionStart = txtToolOutput.Text.Length;
            txtToolOutput.ScrollToCaret();
        }

        private void ToolExcute(int flag, object args, bool isshow)
        {
            try
            {
                base.Invoke(new Action(delegate
                {
                    ToolOutput("In processing, please wait...\r\n", false);
                }));
                string fileName = "";
                if (flag == 0)
                {
                    fileName = JRE_PATH + "\\bin\\java.exe";
                }
                else if (flag == 1)
                {
                    fileName = "cmd.exe";
                }

                ProcessStartInfo startInfo = new ProcessStartInfo(fileName, args.ToString())
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                using (Process process = new Process())
                {
                    process.StartInfo = startInfo;
                    process.OutputDataReceived += delegate (object s, DataReceivedEventArgs e)
                    {
                        Invoke(new Action(delegate
                        {
                            if (isshow)
                            {
                                ToolOutput(e.Data, true);
                            }
                        }));
                    };
                    process.ErrorDataReceived += delegate (object s, DataReceivedEventArgs e)
                    {
                        Invoke(new Action(delegate
                        {
                            ToolOutput(e.Data, true);
                        }));
                    };
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                    process.Close();
                    process.Dispose();
                }
                base.Invoke(new Action(delegate
                {
                    ToolOutput("Finish!", true);
                }));
            }
            catch (Exception ex)
            {
                base.Invoke(new Action(delegate
                {
                    ToolOutput(ex.ToString(), true);
                }));
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cboKeyStoreType.Items.AddRange(keystoreTypes);

            JRE_PATH = _ini.Read("config", "JRE_PATH", "");
            txtJREPath.Text = JRE_PATH;

            if(!CheckJDKInstallation(JRE_PATH))
            {
                tabControl1.SelectedIndex = tabControl1.TabCount - 2;
                MessageBox.Show("Invalid JRE path!");
            }
            else
            {
                SetEnvironmentVariable();
            }
        }

        private void btnKeyfileGenerate_Click(object sender, EventArgs e)
        {
            if(txtStorePassword.Text.Length == 0)
            {
                txtStorePassword.Focus();
                return;
            }

            if (txtStoreConfirmPassword.Text.Length == 0)
            {
                txtStoreConfirmPassword.Focus();
                return;
            }

            if (txtKeyAlias.Text.Length == 0)
            {
                txtKeyAlias.Focus();
                return;
            }

            if (txtKeyAliasPassword.Text.Length == 0)
            {
                txtKeyAliasPassword.Focus();
                return;
            }

            if (txtKeyAliasConfirmPassword.Text.Length == 0)
            {
                txtKeyAliasConfirmPassword.Focus();
                return;
            }

            if (cboKeyStoreType.Text.Length == 0)
            {
                cboKeyStoreType.Focus();
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Save your KeyStore file...";
            sfd.Filter = "KeyStore (*.jks)|*.jks|KeyStore (*.keystore)|*.keystore";
            sfd.RestoreDirectory = true;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string strKeyToolExportLocation = sfd.FileName.Substring(0, sfd.FileName.LastIndexOf("\\") + 1);
                string strKeyToolKeyFileName = sfd.FileName.Substring(sfd.FileName.LastIndexOf("\\") + 1);

                StringBuilder arg = new StringBuilder();
                arg.Append("-genkeypair -v -noprompt ");
                arg.Append("-keyalg RSA ");
                arg.Append("-keysize 2048 ");
                arg.Append(string.Concat(new string[]
                {
                    "-keystore \"",
                    strKeyToolExportLocation,
                    "\\",
                    strKeyToolKeyFileName,
                    "\" "
                }));
                arg.Append("-validity " + nudKeyValidity.Value * 365m + " ");
                arg.Append(string.Concat(new string[]
                {
                    "-dname \"CN=",
                    txtKeyFullName.Text,
                    ", OU=",
                    txtKeyOrgUnit.Text,
                    ", O=",
                    txtKeyOrg.Text,
                    ", L=",
                    txtKeyCity.Text,
                    ", S=",
                    txtKeyState.Text,
                    ", C=",
                    txtKeyCountryCode.Text,
                    "\" "
                }));
                arg.Append("-alias " + txtKeyAlias.Text + " ");
                arg.Append("-keypass " + txtKeyAliasPassword.Text + " ");
                arg.Append("-storepass " + txtStorePassword.Text + " ");
                arg.Append("-storetype " + ((cboKeyStoreType.Text == "") ? "PKCS12" : cboKeyStoreType.Text) + " ");

                try
                {
                    if (ExecuteCommand(JRE_PATH + "\\bin\\keytool.exe", arg.ToString(), txtOutput) == 0)
                        MessageBox.Show("Your keyfile \"" + strKeyToolKeyFileName + "\" was created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    else
                        MessageBox.Show("Your keyfile \"" + strKeyToolKeyFileName + "\" failed to be created! Please check your input data for the key!", "Fail", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
        }

        private void btnJREPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = false;
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                JRE_PATH = txtJREPath.Text = PathNetCore.GetRelativePath(Environment.CurrentDirectory, fbd.SelectedPath);
                if (!CheckJDKInstallation(JRE_PATH))
                {
                    MessageBox.Show("Invalid JRE path!");
                }
                else
                {
                    _ini.Write("config", "JRE_PATH", JRE_PATH);
                    SetEnvironmentVariable();
                }
            }
        }

        private void btnSign_Click(object sender, EventArgs e)
        {
            if(tabControl2.SelectedIndex == 0)
            {
                if (txtSignAPKFile.Text.Length == 0 || !File.Exists(txtSignAPKFile.Text))
                {
                    txtSignAPKFile.Focus();
                    return;
                }

                if (txtSignKeyStore.Text.Length == 0 || !File.Exists(txtSignKeyStore.Text))
                {
                    txtSignKeyStore.Focus();
                    return;
                }

                if (txtSignStorePassword.Text.Length == 0)
                {
                    txtSignStorePassword.Focus();
                    return;
                }

                if (cboSignKeyAlias.Text.Length == 0)
                {
                    cboSignKeyAlias.Focus();
                    return;
                }

                if (txtSignAliasPassword.Text.Length == 0)
                {
                    txtSignAliasPassword.Focus();
                    return;
                }
            }
            else
            {
                if (txtSignCertKeyFile.Text.Length == 0 || !File.Exists(txtSignCertKeyFile.Text))
                {
                    txtSignCertKeyFile.Focus();
                    return;
                }

                if (txtSignCertFile.Text.Length == 0 || !File.Exists(txtSignCertFile.Text))
                {
                    txtSignCertFile.Focus();
                    return;
                }

                if (txtSignCertKeyPassword.Text.Length == 0)
                {
                    txtSignCertKeyPassword.Focus();
                    return;
                }
            }
            
            string filename = txtSignAPKFile.Text;

            if (chkSignAlign.Checked)
            {
                filename = AddToFileName(filename, "_ALIGNED");
                StringBuilder argAlign = new StringBuilder();
                argAlign.Append("-f -v 4 ");
                argAlign.Append("\"" + txtSignAPKFile.Text + "\" ");
                argAlign.Append("\"" + filename + "\" ");
                if (ExecuteCommand(GLOBAL.zipalign, argAlign.ToString(), txtOutput) == 0)
                {
                    //if (!sign)
                    //{
                    //MessageBox.Show("Your application has been successfully aligned!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    //}
                }
                else
                {
                    MessageBox.Show("An error occured while aligning your application! Please check your file and try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }

            filename = AddToFileName(filename, "_SIGNED");
            StringBuilder argSign = new StringBuilder();
            argSign.Append("-jar \"" + GLOBAL.apksigner + "\" sign ");

            if (tabControl2.SelectedIndex == 0)
            {
                argSign.Append("--ks \"" + txtSignKeyStore.Text + "\" ");
                argSign.Append("--ks-key-alias " + cboSignKeyAlias.Text + " ");
                argSign.Append("--ks-pass pass:" + txtSignStorePassword.Text + " ");
                argSign.Append("--key-pass pass:" + txtSignAliasPassword.Text + " ");
            }
            else
            {
                argSign.Append("--key \"" + txtSignCertKeyFile.Text + "\" ");
                argSign.Append("--cert \"" + txtSignCertFile.Text + "\" ");
                argSign.Append("--key-pass pass:" + txtSignCertKeyPassword.Text + " ");
            }

            argSign.Append("--verbose ");
            argSign.Append("--out \"" + filename + "\" ");
            argSign.Append("\"" + txtSignAPKFile.Text + "\" ");
            if (ExecuteCommand(JRE_PATH + "\\bin\\java.exe", argSign.ToString(), txtOutput) == 0)
            {
                if (chkSignAlign.Checked)
                {
                    MessageBox.Show("Your application has been successfully aligned and signed!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                else
                {
                    MessageBox.Show("Your application has been successfully signed!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
            else
            {
                MessageBox.Show("An error occured while signing your application! Please check your passwords for your keystore / certificate!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void btnSignAPKBrowser_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Title = "Select your APK file...";
            ofd.Filter = "APK file (*.apk)|*.apk";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtSignAPKFile.Text = ofd.FileName;
            }
        }

        private void btnSignKeyStoreBrowser_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Title = "Select your KeyStore file...";
            ofd.Filter = "KeyStore (*.jks)|*.jks|KeyStore (*.keystore)|*.keystore";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtSignKeyStore.Text = ofd.FileName;
            }
        }

        private void btnSignAlign_Click(object sender, EventArgs e)
        {
            if(txtSignAPKFile.Text.Length == 0 || !File.Exists(txtSignAPKFile.Text))
            {
                txtSignAPKFile.Focus();
                return;
            }

            string filename = txtSignAPKFile.Text;

            filename = AddToFileName(filename, "_ALIGNED");
            StringBuilder argAlign = new StringBuilder();
            argAlign.Append("-f -v ");
            argAlign.Append("4 ");
            argAlign.Append("\"" + txtSignAPKFile.Text + "\" ");
            argAlign.Append("\"" + filename + "\" ");
            if (ExecuteCommand(GLOBAL.zipalign, argAlign.ToString(), txtOutput) == 0)
            {
                MessageBox.Show("Your application has been successfully aligned!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else
            {
                MessageBox.Show("An error occured while aligning your application! Please check your file and try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void txtSignStorePassword_Leave(object sender, EventArgs e)
        {
            cboSignKeyAlias.Items.Clear();
            List<string> info = ListKeyInformation(JRE_PATH + "\\bin\\keytool.exe", txtSignKeyStore.Text, txtSignStorePassword.Text, txtOutput);
            if (info.Count > 0)
            {
                foreach (string str in info)
                {
                    if (str.Contains("Alias name: "))
                    {
                        string name = str.Replace("Alias name: ", string.Empty);
                        cboSignKeyAlias.Items.Add(name);
                    }
                }
            }
            else
            {
                MessageBox.Show("Your key password seems to be invalid. Please try again!", "Invalid password", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void btnSignCertKeyFileBrowser_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select your Private Key file...";
            ofd.Filter = "Private Key (*.pk8)|*.pk8";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtSignCertKeyFile.Text = ofd.FileName;
            }
        }

        private void btnSignCertFileBrowser_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select your certificate file...";
            ofd.Filter = "Cretificate (*.x509.pem)|*.x509.pem";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtSignCertFile.Text = ofd.FileName;
            }
        }

        private void btnToolBrowser_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Support Types|*.apk;*.jar;*.odex;*.dex"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtToolPath.Text = ofd.FileName;
            }
        }

        private void txtToolPath_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = (e.Data.GetDataPresent(DataFormats.FileDrop, false) ? DragDropEffects.Copy : DragDropEffects.None);
        }

        private void txtToolPath_DragDrop(object sender, DragEventArgs e)
        {
            string[] array = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            txtToolPath.Text = array[0];
        }

        private void btnToolDecompile_Click(object sender, EventArgs e)
        {
            string text = txtToolPath.Text;
            if (!File.Exists(text) || Path.GetExtension(text) != ".apk")
            {
                SetStatus("No found apk file!");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Folder|*",
                InitialDirectory = Path.GetDirectoryName(text),
                FileName = Path.GetFileNameWithoutExtension(text)
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string outputFolderName = sfd.FileName.ToString();
                string args;
                if (MessageBox.Show("Ignore the resource file?", "Prompt", MessageBoxButtons.OKCancel) != DialogResult.OK)
                {
                    args = Util.GetDecompilerArg(text, outputFolderName);
                    new Thread(() =>
                    {
                        ToolExcute(0, args, true);
                    }).Start();
                    return;
                }

                args = Util.GetDecompilerArgWithoutRes(text, outputFolderName);
                new Thread(() =>
                {
                    ToolExcute(0, args, true);
                }).Start();
            }
        }

        private void btnToolRecompile_Click(object sender, EventArgs e)
        {
            string text = txtToolPath.Text;
            if (!Directory.Exists(text))
            {
                SetStatus("No found recompile folder!");
                return;
            }
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "APK|*.apk|All|*.*",
                DefaultExt = "apk",
                InitialDirectory = Path.GetDirectoryName(text),
                FileName = Path.GetFileName(text) + "_Mod.apk"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string fileName = sfd.FileName;
                string args = Util.GetBuildArg(text, fileName);
                new Thread(() =>
                {
                    ToolExcute(0, args, true);
                }).Start();
            }
        }

        private void btnToolDex2Jar_Click(object sender, EventArgs e)
        {
            string text = txtToolPath.Text;
            if (!File.Exists(text) || Path.GetExtension(text) != ".dex")
            {
                SetStatus("No found dex file!");
                return;
            }
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "JAR|*.jar|All|*.*",
                InitialDirectory = Path.GetDirectoryName(text),
                FileName = Path.GetFileNameWithoutExtension(text)
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string outputJar = sfd.FileName.ToString();
                string dex2JarArg = Util.GetDex2JarArg(text, outputJar);
                new Thread(() =>
                {
                    ToolExcute(1, dex2JarArg, true);
                }).Start();
            }
        }

        private void btnToolJar2Dex_Click(object sender, EventArgs e)
        {
            string text = txtToolPath.Text;
            if (!File.Exists(text) || Path.GetExtension(text) != ".jar")
            {
                SetStatus("No found jar file!");
                return;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "DEX|*.dex|All|*.*",
                InitialDirectory = Path.GetDirectoryName(text),
                FileName = Path.GetFileNameWithoutExtension(text)
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDialog.FileName;
                string jar2DexArg = Util.GetJar2DexArg(text, fileName);
                new Thread(() =>
                {
                    ToolExcute(1, jar2DexArg, true);
                }).Start();
            }
        }

        private void btnToolDecompileDex_Click(object sender, EventArgs e)
        {
            string text = txtToolPath.Text;
            if (!File.Exists(text) || Path.GetExtension(text) != ".dex")
            {
                SetStatus("No found dex file!");
                return;
            }
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Folder|*",
                InitialDirectory = Path.GetDirectoryName(text),
                FileName = Path.GetFileNameWithoutExtension(text)
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string outputFolderName = sfd.FileName.ToString();
                string decompilerDex = Util.GetDecompilerDex(text, outputFolderName);
                new Thread(() =>
                {
                    ToolExcute(0, decompilerDex, true);
                }).Start();
            }
        }

        private void btnToolRecompileDex_Click(object sender, EventArgs e)
        {
            string text = txtToolPath.Text;
            if (!Directory.Exists(text))
            {
                SetStatus("No found smali folder!");
                return;
            }
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "DEX|*.dex|All|*.*",
                DefaultExt = "dex",
                InitialDirectory = Path.GetDirectoryName(text),
                FileName = Path.GetFileNameWithoutExtension(text) + "_Mod.dex"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string fileName = sfd.FileName;
                string buildDex = Util.GetBuildDex(text, fileName);
                new Thread(() =>
                {
                    ToolExcute(0, buildDex, true);
                }).Start();
            }
        }

        private void btnToolJadxGui_Click(object sender, EventArgs e)
        {
            string text = txtToolPath.Text;
            if (!File.Exists(text) || (Path.GetExtension(text) != ".apk" &&
                Path.GetExtension(text) != ".dex" &&
                Path.GetExtension(text) != ".jar" &&
                Path.GetExtension(text) != ".class" &&
                Path.GetExtension(text) != ".zip" &&
                Path.GetExtension(text) != ".aar" &&
                Path.GetExtension(text) != ".arsc"))
            {
                SetStatus("No found supported file!");
                return;
            }
            new Thread(() =>
            {
                string arg = string.Format("/c {0} \"{1}\"", GLOBAL.jadx, text);
                ToolExcute(1, arg, true);
            }).Start();
        }

        private void btnToolJdGui_Click(object sender, EventArgs e)
        {
            string text = txtToolPath.Text;
            if (!File.Exists(text) || Path.GetExtension(text) != ".jar")
            {
                SetStatus("No found jar file!");
                return;
            }
            new Thread(() =>
            {
                string arg = string.Format("-jar \"{0}\" \"{1}\"", GLOBAL.jd, text);
                ToolExcute(0, arg, true);
            }).Start();
        }

        private void btnToolAppInfo_Click(object sender, EventArgs e)
        {
            string text = txtToolPath.Text;
            if (!File.Exists(text) || Path.GetExtension(text) != ".apk")
            {
                SetStatus("No found apk file!");
                return;
            }
            string arg = Util.GetAppInfo(text);
            new Thread(() =>
            {
                ToolExcute(0, arg, true);
            }).Start();
        }
    }
}