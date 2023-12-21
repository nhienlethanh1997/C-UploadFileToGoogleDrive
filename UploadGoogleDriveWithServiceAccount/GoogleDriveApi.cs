using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UploadGoogleDriveWithServiceAccount
{
    public class GoogleDriveApi
    {
        static string CredentialPath;
        static DriveService Service;
        string FolderId;

        public GoogleDriveApi()
        {
            CredentialPath = "credentials2.json";
            FolderId = "1fAYZpJMJsKSCKrhhk9w5wTgwqo-uxSRM";
        }

        public string UploadFileToGoogleDrive(string filePath, string folderId)
        {
            // Create the Drive service
            //Service = CreateDriveService();

            // Upload file to Google Drive
            var fileMeta = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(filePath),
                Parents = new List<string> { folderId },
            };

            FilesResource.CreateMediaUpload request;
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                request = Service.Files.Create(fileMeta, stream, "");
                request.Fields = "id";
                request.Upload();
                //request.UploadAsync(CancellationToken.None);
            }

            // Find the file that was just upload
            var uploadedFileId = request.ResponseBody.Id;
            return uploadedFileId;
        }

        public string[] GetFileFromDirectory(string folderToUploadPath, string folderChildId)
        {
            // Get files from folderToUploadPath
            List<string> files = Directory.GetFiles(folderToUploadPath, "*.txt", SearchOption.TopDirectoryOnly).ToList();

            // Get files from folderId that will receive upload files
            List<string> filesOnDrive = GetFilesFromDrive(folderChildId);

            // Remove files already on drive
            foreach (string f in filesOnDrive)
            {
                files.Remove(folderToUploadPath + @"\" + f);
            }

            return files.ToArray();
        }

        private List<string> GetFilesFromDrive(string folderChildId)
        {
            List<string> res = new List<string>();

            var request = Service.Files.List();
            FileList fileList;
            request.Q = folderChildId != null ? $"'{folderChildId}' in parents" : null;
            request.Fields = "nextPageToken, files(id, name)";  // nextPageToken là đối tượng được chia trang với 1000 element/PageToken
            do
            {
                fileList = request.Execute();

                var files = fileList.Files;
                foreach (var f in files)
                    res.Add(f.Name);

                request.PageToken = fileList.NextPageToken;

            } while (!string.IsNullOrEmpty(fileList.NextPageToken));

            return res;
        }

        public void CreateDriveService()
        {
            // Load the Service account credentials and define the scope of its access
            GoogleCredential credentials = GoogleCredential.FromFile(CredentialPath)
                .CreateScoped(new[] { DriveService.Scope.Drive });

            // Create the Drive service
            Service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials,
                ApplicationName = "Google Drive Upload Window Form App"
            });
        }

        public string GetLinkFile(string id)
        {
            return $"https://drive.google.com/file/d/{id}";
        }

        public string CreateFolderIfNotExist(string child)
        {
            var folderRequest = Service.Files.List();
            folderRequest.Q = $"mimeType='application/vnd.google-apps.folder' and name='{child}' and '{FolderId}' in parents and trashed=false";
            var folderResponse = folderRequest.Execute();
            if (folderResponse.Files.Count >= 1)
                return folderResponse.Files[0].Id;

            #region Tạo thư mục
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = child,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new List<string> { FolderId },
            };

            var request = Service.Files.Create(fileMetadata);

            request.Fields = "id";
            var file = request.Execute();
            return file.Id;
            #endregion
        }
    }
}
