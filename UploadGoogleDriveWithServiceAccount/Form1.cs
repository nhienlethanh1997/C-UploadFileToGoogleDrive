using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UploadGoogleDriveWithServiceAccount
{
    public partial class Form1 : Form
    {
        GoogleDriveApi _api = new GoogleDriveApi();

        public Form1()
        {
            InitializeComponent();
        }

        private void btnTaiLen_Click(object sender, EventArgs e)
        {
            _api.CreateDriveService();
            string folderChildId = _api.CreateFolderIfNotExist(DateTime.Today.ToString("yyyyMMdd"));
            string[] filesToUpload = _api.GetFileFromDirectory(txtDuongDanTep.Text, folderChildId);
            foreach (string file in filesToUpload)
            {
                string uploadedFileId = _api.UploadFileToGoogleDrive(file, folderChildId);
                string linkFile = _api.GetLinkFile(uploadedFileId);
                txtNhatKy.AppendText($"Tải lên thành công, đường dẫn tệp {Path.GetFileName(file)}:\r\n{linkFile}\r\n");
            }
        }

        private void btnChonTep_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog open = new FolderBrowserDialog();
            if (open.ShowDialog() == DialogResult.OK)
                txtDuongDanTep.Text = open.SelectedPath;
        }
    }
}
