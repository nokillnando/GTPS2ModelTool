using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GTPS2ModelToolGUI
{
    public partial class Form1 : Form
    {
        private ListBox lstFiles;
        private TextBox txtSpecDbPath;
        private TextBox txtLog;
        private Button btnAddFiles;
        private Button btnClear;
        private Button btnBrowseSpecDb;
        private Button btnExtract;
        private Label lblSpecDb;
        private ProgressBar progressBar;

        public Form1()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "GTPS2ModelTool Batch Extractor";
            this.Size = new Size(800, 600);
            this.MinimumSize = new Size(600, 450);

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(10)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40)); // Listbox
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));  // SpecDB logic
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));  // Action Buttons
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60)); // Log

            // --- Panel 1: File List ---
            var pnlFiles = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            pnlFiles.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pnlFiles.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));

            lstFiles = new ListBox { Dock = DockStyle.Fill, SelectionMode = SelectionMode.MultiExtended };

            var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown };
            btnAddFiles = new Button { Text = "Add Files...", Width = 90 };
            btnAddFiles.Click += BtnAddFiles_Click;
            btnClear = new Button { Text = "Clear List", Width = 90 };
            btnClear.Click += BtnClear_Click;
            btnPanel.Controls.Add(btnAddFiles);
            btnPanel.Controls.Add(btnClear);

            pnlFiles.Controls.Add(lstFiles, 0, 0);
            pnlFiles.Controls.Add(btnPanel, 1, 0);

            // --- Panel 2: Spec DB ---
            var pnlSpecDb = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
            pnlSpecDb.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            pnlSpecDb.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pnlSpecDb.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));

            lblSpecDb = new Label { Text = "SpecDB Path:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
            txtSpecDbPath = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 5, 0, 0) };
            
            // Default to known user DB path if possible, but leave it empty otherwise
            txtSpecDbPath.Text = @"C:\Gt4\tools\gt4fs\extracted\specdb\GT4_US2560";

            btnBrowseSpecDb = new Button { Text = "Browse...", Width = 90, Margin = new Padding(3,3,0,0) };
            btnBrowseSpecDb.Click += BtnBrowseSpecDb_Click;

            pnlSpecDb.Controls.Add(lblSpecDb, 0, 0);
            pnlSpecDb.Controls.Add(txtSpecDbPath, 1, 0);
            pnlSpecDb.Controls.Add(btnBrowseSpecDb, 2, 0);

            // --- Panel 3: Action Buttons ---
            var pnlActions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            pnlActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pnlActions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));

            progressBar = new ProgressBar { Dock = DockStyle.Fill, Margin = new Padding(0, 5, 10, 5), Visible = false };
            btnExtract = new Button { Text = "Start Extraction", Dock = DockStyle.Fill, Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold) };
            btnExtract.Click += BtnExtract_Click;

            pnlActions.Controls.Add(progressBar, 0, 0);
            pnlActions.Controls.Add(btnExtract, 1, 0);

            // --- Panel 4: Log Output ---
            txtLog = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.LightGray,
                Font = new Font("Consolas", 9)
            };

            mainLayout.Controls.Add(pnlFiles, 0, 0);
            mainLayout.Controls.Add(pnlSpecDb, 0, 1);
            mainLayout.Controls.Add(pnlActions, 0, 2);
            mainLayout.Controls.Add(txtLog, 0, 3);

            this.Controls.Add(mainLayout);
        }

        private void BtnAddFiles_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Multiselect = true;
                ofd.Filter = "All Files (*.*)|*.*|Model Files (*.mdls;*.bin)|*.mdls;*.bin";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (string file in ofd.FileNames)
                    {
                        if (!lstFiles.Items.Contains(file))
                            lstFiles.Items.Add(file);
                    }
                }
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            lstFiles.Items.Clear();
        }

        private void BtnBrowseSpecDb_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (Directory.Exists(txtSpecDbPath.Text))
                    fbd.SelectedPath = txtSpecDbPath.Text;

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtSpecDbPath.Text = fbd.SelectedPath;
                }
            }
        }

        private void AppendLog(string text)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => AppendLog(text)));
                return;
            }
            txtLog.AppendText(text + Environment.NewLine);
        }

        private async void BtnExtract_Click(object sender, EventArgs e)
        {
            if (lstFiles.Items.Count == 0)
            {
                MessageBox.Show("Please add files to extract.", "No Files", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSpecDbPath.Text) || !Directory.Exists(txtSpecDbPath.Text))
            {
                MessageBox.Show("Please select a valid SpecDB path.", "Invalid Path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string toolExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "GTPS2ModelTool", "bin", "Release", "net8.0", "GTPS2ModelTool.exe");
            
            // fallback lookup
            if (!File.Exists(toolExePath))
                toolExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GTPS2ModelTool.exe");

            if (!File.Exists(toolExePath))
            {
                MessageBox.Show($"Cannot find GTPS2ModelTool.exe!\nLooked in: {toolExePath}\nPlease ensure you compile the core tool and this GUI is in the correct directory.", "Missing Executable", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SetUIState(false);
            txtLog.Clear();
            progressBar.Value = 0;
            progressBar.Maximum = lstFiles.Items.Count;
            progressBar.Visible = true;

            int successCount = 0;

            foreach (string file in lstFiles.Items)
            {
                AppendLog($"======================================================");
                AppendLog($"Running Extraction for: {Path.GetFileName(file)}");
                
                bool result = await RunProcessAsync(toolExePath, $"dump -i \"{file}\" -s \"{txtSpecDbPath.Text}\"");
                if (result) successCount++;
                
                progressBar.Value += 1;
            }
            
            AppendLog($"======================================================");
            AppendLog($"Batch Complete! {successCount}/{lstFiles.Items.Count} extracted successfully.");
            SetUIState(true);
            progressBar.Visible = false;
        }

        private void SetUIState(bool enabled)
        {
            btnAddFiles.Enabled = enabled;
            btnClear.Enabled = enabled;
            btnExtract.Enabled = enabled;
            btnBrowseSpecDb.Enabled = enabled;
            txtSpecDbPath.Enabled = enabled;
            lstFiles.Enabled = enabled;
        }

        private Task<bool> RunProcessAsync(string filename, string arguments)
        {
            var tcs = new TaskCompletionSource<bool>();

            Process process = new Process();
            process.StartInfo.FileName = filename;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true; // hides console

            process.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    AppendLog(e.Data);
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    AppendLog($"ERROR: {e.Data}");
            };

            process.EnableRaisingEvents = true;
            process.Exited += (s, e) =>
            {
                tcs.SetResult(process.ExitCode == 0);
                process.Dispose();
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                AppendLog($"Failed to start process: {ex.Message}");
                tcs.SetResult(false);
            }

            return tcs.Task;
        }

        // Auto-generated partial class properties
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Text = "Form1";
        }
        #endregion
    }
}
