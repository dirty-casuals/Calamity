using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace GameDataEditor
{
	public class GDEDriveHelper
	{
	#region Cert Validation
	    static RemoteCertificateValidationCallback originalValidationCallback;

	    public static bool GDEOAuthValidator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	    {
	        return true;
	    }

	    static void SetCertValidation()
	    {
	        // Set up cert validator
	        originalValidationCallback = ServicePointManager.ServerCertificateValidationCallback;
	        ServicePointManager.ServerCertificateValidationCallback = GDEOAuthValidator;
	    }

	    static void ResetCertValidation()
	    {
	        ServicePointManager.ServerCertificateValidationCallback = originalValidationCallback;
	    }
	#endregion

	    static GDEDriveHelper _helper;
	    public static GDEDriveHelper Instance
	    {
	        get
	        {
	            if (_helper == null)
	                _helper = new GDEDriveHelper();
	            return _helper;
	        }
	    }

	    const string FILE_QUERY = "https://www.googleapis.com/drive/v2/files?fields=items(exportLinks,title,downloadUrl,mimeType,id,labels)";
	    const string ACCESS_TOKEN = "access_token=";
	    float _timeout;
	    const float MAX_TIMEOUT_SEC = 120f;

	    GDEOAuth oauth;

	    List<string> _spreadSheetNames;
	    public string[] SpreadSheetNames
	    {
	        get
	        {
	            if (_spreadSheetNames == null)
	                _spreadSheetNames = new List<string>(){""};

	            return _spreadSheetNames.ToArray();
	        }
	        private set {}
	    }

	    Dictionary<string, Dictionary<string,string>> _spreadSheetLinks;

	    GDEDriveHelper ()
	    {
	        SetCertValidation();

	        oauth = new GDEOAuth();
	        oauth.Init();

	        _spreadSheetLinks = new Dictionary<string, Dictionary<string,string>>();
	        _spreadSheetNames = new List<string>();

	        ResetCertValidation();
	    }

	    public bool HasAuthenticated()
	    {
	        return oauth.HasAuthenticated();
	    }

	    public void SetAccessCode(string code)
	    {
	        SetCertValidation();

	        oauth.SetAccessCode(code);

	        ResetCertValidation();
	    }

	    public void RequestAuthFromUser()
	    {
	        SetCertValidation();

	        string authURL = oauth.GetAuthURL();

	        ResetCertValidation();

	        Application.OpenURL(authURL);
	    }

	    public string DownloadSpreadSheet(string fileName, string localName)
	    {
	        string localPath = FileUtil.GetUniqueTempPathInProject() + localName;

	        GetSpreadsheetList();

	        string fileUrl;
	        Dictionary<string, string> metadata;
	        if(_spreadSheetLinks.TryGetValue(fileName, out metadata))
	        {
	            metadata.TryGetValue("url", out fileUrl);
	            DoDownload(fileUrl, localPath);
	        }
	        else
	        {
	            Debug.LogError(GDEConstants.ErrorDownloadingSheet+fileName);
	            localPath = string.Empty;
	        }

	        return localPath;
	    }

	    public string GetFileId(string fileName)
	    {
	        Dictionary<string, string> metadata;
	        _spreadSheetLinks.TryGetValue(fileName, out metadata);
	        return metadata["id"];
	    }

	    public void GetSpreadsheetList()
	    {
	        SetCertValidation();

	        oauth.Init();

	        string url = FILE_QUERY + "&" + ACCESS_TOKEN + oauth.AccessToken;

	        WWW req = new WWW(url);
	        while(!req.isDone);

	        ResetCertValidation();

	        if (string.IsNullOrEmpty(req.error))
	        {
	            Dictionary<string, object> response = Json.Deserialize(req.text) as Dictionary<string, object>;
	            List<object> items = response["items"] as List<object>;

	            _spreadSheetLinks.Clear();
	            _spreadSheetNames.Clear();

	            foreach(var item in items)
	            {
	                Dictionary<string, object> itemData = item as Dictionary<string, object>;

	                Dictionary<string, object> labels = itemData["labels"] as Dictionary<string, object>;
	                string mimeType = itemData["mimeType"].ToString();
	                string fileName = itemData["title"].ToString();
	                string fileUrl = string.Empty;
	                string fileId = itemData["id"].ToString();

	                if (!mimeType.Contains("spreadsheet") ||
	                    Convert.ToBoolean(labels["trashed"]))
	                    continue;

	                if (itemData.ContainsKey("exportLinks"))
	                {
	                    Dictionary<string, object> exportLinks = itemData["exportLinks"] as Dictionary<string, object>;
	                    foreach(var pair in exportLinks)
	                    {
	                        if(pair.Value.ToString().Contains("xlsx"))
	                            fileUrl = pair.Value.ToString();
	                    }
	                }
	                else if(itemData.ContainsKey("downloadUrl"))
	                    fileUrl = itemData["downloadUrl"].ToString();
	                else
	                {
	                    fileUrl = string.Empty;
	                }

	                try
	                {
	                    if (!_spreadSheetNames.Contains(fileName))
	                    {
	                        _spreadSheetNames.Add(fileName);
	                        var metadata = new Dictionary<string, string>();
	                        metadata.Add("url", fileUrl);
	                        metadata.Add("id", fileId);
	                        _spreadSheetLinks.Add(fileName, metadata);
	                    }
	                }
	                catch(Exception ex)
	                {
	                    Debug.LogWarning(ex.ToString());
	                }
	            }
	        }
	        else
	        {
	            Debug.Log(req.error);
	        }
	    }

	    void DoDownload(string fileUrl, string downloadPath)
	    {
	        SetCertValidation();

	        oauth.Init();

	        fileUrl = fileUrl + "&" + ACCESS_TOKEN + oauth.AccessToken;

	        WWW req = new WWW(fileUrl);
	        while(!req.isDone);

	        ResetCertValidation();

	        if (string.IsNullOrEmpty(req.error))
	            File.WriteAllBytes(downloadPath, req.bytes);
	        else
	                Debug.Log(req.error);
	    }

	    public void UploadToExistingSheet(string googleSheetImportName, string dummyFilePath)
	    {
	        try
	        {
	            var meta = _spreadSheetLinks[googleSheetImportName];

	            oauth.Init();
	            string url = "https://www.googleapis.com/upload/drive/v2/files/" + meta["id"] + "?uploadType=resumable" + "&" + ACCESS_TOKEN + oauth.AccessToken;

	            byte[] bytes = File.ReadAllBytes(dummyFilePath);
	            Hashtable metadata = new Hashtable();

	            // supply any metadata for the uploaded file here
	            // we need the Content-Type & Content-Length headers otherwise the we get a 400 error invalid URI
	            byte[] metadata_bytes = Encoding.UTF8.GetBytes(Json.Serialize(metadata));

	            Dictionary<string, string> headers = new Dictionary<string, string>();
	            headers["X-Upload-Content-Type"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
	            headers["X-Upload-Content-Length"] = bytes.Length.ToString();
	            headers["Content-Type"] = "application/json";
	            headers["Content-Length"] = metadata_bytes.Length.ToString();
	            // www class sends a POST; tell google to process it as a PUT
	            headers["X-HTTP-Method-Override"] = "PUT";

	            start_timer();
	            var www = new WWW(url, metadata_bytes, headers);

	            // spin until the web request is done or the timeout is reached
	            while (!www.isDone && elapsed_time() < MAX_TIMEOUT_SEC);

	            // check for timeout
	            if (elapsed_time() >= MAX_TIMEOUT_SEC)
	            {
	                Debug.Log(string.Format("url: {0}", url));
	                Debug.Log(string.Format("Web request timed out. Elapsed time: {0}", elapsed_time()));
	                return;
	            }

	            // SUCCESS 200
	            if (www.error == null)
	            {
	                string session_uri = www.responseHeaders["LOCATION"];
	                url = session_uri + "&" + ACCESS_TOKEN + oauth.AccessToken;

	                Dictionary<string, string> headers2 = new Dictionary<string, string>();
	                headers2["Content-Length"] = bytes.Length.ToString();

	                var www2 = new WWW(url, bytes, headers);

	                // spin until the web request is done or the timeout is reached
	                while (!www2.isDone && elapsed_time() < MAX_TIMEOUT_SEC);

	                // check for timeout
	                if (elapsed_time() >= MAX_TIMEOUT_SEC)
	                {
	                    Debug.Log(string.Format("url: {0}", url));
	                    Debug.Log(string.Format("Upload failed. Web request timed out. Elapsed time: {0}", elapsed_time()));
	                    return;
	                }

	                // SUCCESS 200
	                if (www2.error == null)
	                {
	                    Debug.Log("Upload Success!");
	                }
	                else
	                {
	                    Debug.Log(string.Format("Error uploading. Upload Failed: {0}", www2.error));
	                }
	            }
	            else
	            {
	                Debug.Log(string.Format("url: {0}", url));
	                Debug.Log(string.Format("Error uploading. Upload Failed: {0}", www.error));
	            }
	        }
	        catch (Exception ex)
	        {
	            Debug.LogException(ex);
	        }
	    }

	    public void UploadNewSheet(string localFilePath, string title)
	    {
	        try
	        {
	            oauth.Init();
	            string url = "https://www.googleapis.com/upload/drive/v2/files" + "?uploadType=resumable" + "&" + ACCESS_TOKEN + oauth.AccessToken;

	            byte[] bytes = File.ReadAllBytes(localFilePath);

	            Hashtable metadata = new Hashtable();
	            metadata.Add("title", title);
	            metadata.Add("mimeType", "application/vnd.google-apps.spreadsheet");
	            byte[] metadata_bytes = Encoding.UTF8.GetBytes(Json.Serialize(metadata));

	            Dictionary<string, string> headers = new Dictionary<string, string>();
	            headers["X-Upload-Content-Type"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
	            headers["X-Upload-Content-Length"] = bytes.Length.ToString();
	            headers["Content-Type"] = "application/json";
	            headers["Content-Length"] = metadata_bytes.Length.ToString();

	            start_timer();
	            var www = new WWW(url, metadata_bytes, headers);

	            // spin until the web request is done or the timeout is reached
	            while (!www.isDone && elapsed_time() < MAX_TIMEOUT_SEC);

	            // check for timeout
	            if (elapsed_time() >= MAX_TIMEOUT_SEC)
	            {
	                Debug.Log(string.Format("url: {0}", url));
	                Debug.Log(string.Format("Web request timed out. Elapsed time: {0}", elapsed_time()));
	                return;
	            }

	            // SUCCESS 200
	            if (www.error == null)
	            {
	                string session_uri = www.responseHeaders["LOCATION"];
	                url = session_uri + "&" + ACCESS_TOKEN + oauth.AccessToken;

	                Dictionary<string, string> headers2 = new Dictionary<string, string>();
	                headers2["Content-Length"] = bytes.Length.ToString();

	                var www2 = new WWW(url, bytes, headers);

	                // spin until the web request is done or the timeout is reached
	                while (!www2.isDone && elapsed_time() < MAX_TIMEOUT_SEC);

	                // check for timeout
	                if (elapsed_time() >= MAX_TIMEOUT_SEC)
	                {
	                    Debug.Log(string.Format("url: {0}", url));
	                    Debug.Log(string.Format("Upload failed. Web request timed out. Elapsed time: {0}", elapsed_time()));
	                    return;
	                }

	                // SUCCESS 200
	                if (www2.error == null)
	                {
	                    Debug.Log("Upload Success!");
	                }
	                else
	                {
	                    Debug.Log(string.Format("Error uploading. Upload Failed: {0}", www2.error));
	                }
	            }
	            else
	            {
	                Debug.Log(string.Format("Error uploading. Upload Failed: {0}", www.error));
	            }

	        }
	        catch (Exception ex)
	        {
	            Debug.LogException(ex);
	        }
	    }

	    private void start_timer()
	    {
	        _timeout = Time.realtimeSinceStartup;
	    }
	    private float elapsed_time()
	    {
	        return Time.realtimeSinceStartup - _timeout;
	    }
	}
}
