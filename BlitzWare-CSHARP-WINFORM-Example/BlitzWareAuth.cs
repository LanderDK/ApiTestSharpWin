using System;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace BlitzWare
{
    class Security
    {
        public static string CalculateResponseHash(string data)
        {
            SHA256 sha256Hash = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            byte[] hashBytes = sha256Hash.ComputeHash(bytes);
            string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            return hash;
        }
        public static string CalculateFileHash(string filename)
        {
            SHA256 sha256 = SHA256.Create();
            FileStream fileStream = File.OpenRead(filename);
            byte[] hashBytes = sha256.ComputeHash(fileStream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
    class Utilities
    {
        public static string HWID()
        {
            string hwid = string.Empty;

            try
            {
                ProcessStartInfo processStartInfo = new()
                {
                    FileName = "wmic",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Arguments = "diskdrive get serialnumber"
                };

                Process process = new() { StartInfo = processStartInfo };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(output))
                {
                    string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length > 1)
                    {
                        hwid = lines[1].Trim().TrimEnd('.');
                    }
                }
            }
            catch (Exception ex)
            {
                hwid = "Error hwid: " + ex.Message;
            }

            return hwid;
        }
        public static string IP()
        {
            string externalIpString = new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
            return externalIpString;
        }
    }
    class API
    {
        public readonly string ApiUrl;
        public readonly string AppName;
        public readonly string AppSecret;
        public readonly string AppVersion;
        public bool Initialized;

        public class ErrorData
        {
            public string Code { get; set; }
            public string Message { get; set; }
        }

        public ApplicationData appData = new();
        public class ApplicationData
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public int Status { get; set; }
            public int HwidCheck { get; set; }
            public int DeveloperMode { get; set; }
            public int IntegrityCheck { get; set; }
            public int FreeMode { get; set; }
            public int TwoFactorAuth { get; set; }
            public string ProgramHash { get; set; }
            public string Version { get; set; }
            public string DownloadLink { get; set; }
        }

        public UserData userData = new();
        public class UserData
        {
            public string Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string ExpiryDate { get; set; }
            public string LastLogin { get; set; }
            public string LastIP { get; set; }
            public string HWID { get; set; }
            public string Token { get; set; }
        }

        public API(string apiUrl, string appName, string appSecret, string appVersion)
        {
            ApiUrl = apiUrl;
            AppName = appName;
            AppSecret = appSecret;
            AppVersion = appVersion;
            Initialized = false;
        }

        public void Initialize()
        {
            if (Initialized)
            {
                MessageBox.Show("Application is already initialized!", AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(0);
            }
            try
            {
                HttpClient client = new();
                string url = ApiUrl + "/applications/initialize";

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                string jsonData = $"{{\"name\":\"{AppName}\",\"secret\":\"{AppSecret}\",\"version\":\"{AppVersion}\"}}";
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(url, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string responseHash = response.Headers.GetValues("X-Response-Hash").FirstOrDefault();
                    string recalculatedHash = Security.CalculateResponseHash(response.Content.ReadAsStringAsync().Result);
                    if (responseHash != recalculatedHash)
                    {
                        MessageBox.Show("Possible malicious activity detected!", AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(0);
                    }

                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    var serializer = new JavaScriptSerializer();
                    appData = (ApplicationData)serializer.Deserialize(responseContent, typeof(ApplicationData));
                    Initialized = true;

                    if (appData.Status == 0)
                    {
                        MessageBox.Show("Looks like this application is offline, please try again later!", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(0);
                    }

                    if (appData.FreeMode == 1)
                        MessageBox.Show("Application is in Free Mode!", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);

                    if (appData.DeveloperMode == 1)
                    {
                        MessageBox.Show("Application is in Developer Mode, bypassing integrity and update check!", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        File.Create(Environment.CurrentDirectory + "/integrity.txt").Close();
                        string hash = Security.CalculateFileHash(Process.GetCurrentProcess().MainModule.FileName);
                        File.WriteAllText(Environment.CurrentDirectory + "/integrity.txt", hash);
                        MessageBox.Show("Your applications hash has been saved to integrity.txt, please refer to this when your application is ready for release!", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        if (appData.Version != AppVersion)
                        {
                            MessageBox.Show($"Update {appData.Version} available, redirecting to update!", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            Process.Start(appData.DownloadLink);
                            Environment.Exit(0);
                        }
                        if (appData.IntegrityCheck == 1)
                        {
                            if (appData.ProgramHash != Security.CalculateFileHash(Process.GetCurrentProcess().MainModule.FileName))
                            {
                                MessageBox.Show("File has been tampered with, couldn't verify integrity!", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                Environment.Exit(0);
                            }
                        }
                    }
                }
                else
                {
                    string errorContent = response.Content.ReadAsStringAsync().Result;
                    var serializer = new JavaScriptSerializer();
                    ErrorData errorData = (ErrorData)serializer.Deserialize(errorContent, typeof(ErrorData));
                    MessageBox.Show($"{errorData.Code}: {errorData.Message}", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }
        public bool Register(string username, string password, string email, string license)
        {
            if (!Initialized)
            {
                MessageBox.Show("Please initialize your application first!", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            try
            {
                HttpClient client = new();
                string url = ApiUrl + "/users/register";

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                string jsonData = $"{{\"username\":\"{username}\",\"password\":\"{password}\",\"email\":\"{email}\",\"license\":\"{license}\",\"hwid\":\"{Utilities.HWID()}\",\"lastIP\":\"{Utilities.IP()}\",\"applicationId\":\"{appData.Id}\"}}";
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(url, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string responseHash = response.Headers.GetValues("X-Response-Hash").FirstOrDefault();
                    string recalculatedHash = Security.CalculateResponseHash(response.Content.ReadAsStringAsync().Result);
                    if (responseHash != recalculatedHash)
                    {
                        MessageBox.Show("Possible malicious activity detected!", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(0);
                    }

                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    var serializer = new JavaScriptSerializer();
                    userData = (UserData)serializer.Deserialize(responseContent, typeof(UserData));
                    return true;
                }
                else
                {
                    string errorContent = response.Content.ReadAsStringAsync().Result;
                    var serializer = new JavaScriptSerializer();
                    ErrorData errorData = (ErrorData)serializer.Deserialize(errorContent, typeof(ErrorData));
                    MessageBox.Show($"{errorData.Code}: {errorData.Message}", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        public bool Login(string username, string password, string twoFactorCode)
        {
            if (!Initialized)
            {
                MessageBox.Show("Please initialize your application first!", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            try
            {
                HttpClient client = new();
                string url = ApiUrl + "/users/login";

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                string jsonData = $"{{\"username\":\"{username}\",\"password\":\"{password}\",\"twoFactorCode\":\"{twoFactorCode}\",\"hwid\":\"{Utilities.HWID()}\",\"lastIP\":\"{Utilities.IP()}\",\"applicationId\":\"{appData.Id}\"}}";
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(url, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string responseHash = response.Headers.GetValues("X-Response-Hash").FirstOrDefault();
                    string recalculatedHash = Security.CalculateResponseHash(response.Content.ReadAsStringAsync().Result);
                    if (responseHash != recalculatedHash)
                    {
                        MessageBox.Show("Possible malicious activity detected!", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(0);
                    }

                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    var serializer = new JavaScriptSerializer();
                    userData = (UserData)serializer.Deserialize(responseContent, typeof(UserData));
                    return true;
                }
                else
                {
                    string errorContent = response.Content.ReadAsStringAsync().Result;
                    var serializer = new JavaScriptSerializer();
                    ErrorData errorData = (ErrorData)serializer.Deserialize(errorContent, typeof(ErrorData));
                    MessageBox.Show($"{errorData.Code}: {errorData.Message}", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        public bool LoginLicenseOnly(string license)
        {
            if (!Initialized)
            {
                MessageBox.Show("Please initialize your application first!", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            try
            {
                HttpClient client = new();
                string url = ApiUrl + "/licenses/login";

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                string jsonData = $"{{\"license\":\"{license}\",\"hwid\":\"{Utilities.HWID()}\",\"lastIP\":\"{Utilities.IP()}\",\"applicationId\":\"{appData.Id}\"}}";
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(url, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string responseHash = response.Headers.GetValues("X-Response-Hash").FirstOrDefault();
                    string recalculatedHash = Security.CalculateResponseHash(response.Content.ReadAsStringAsync().Result);
                    if (responseHash != recalculatedHash)
                    {
                        MessageBox.Show("Possible malicious activity detected!", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(0);
                    }

                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    var serializer = new JavaScriptSerializer();
                    userData = (UserData)serializer.Deserialize(responseContent, typeof(UserData));
                    return true;
                }
                else
                {
                    string errorContent = response.Content.ReadAsStringAsync().Result;
                    var serializer = new JavaScriptSerializer();
                    ErrorData errorData = (ErrorData)serializer.Deserialize(errorContent, typeof(ErrorData));
                    MessageBox.Show($"{errorData.Code}: {errorData.Message}", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        public bool Extend(string username, string password, string license)
        {
            if (!Initialized)
            {
                MessageBox.Show("Please initialize your application first!", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            try
            {
                HttpClient client = new();
                string url = ApiUrl + "/users/upgrade";

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                string jsonData = $"{{\"username\":\"{username}\",\"password\":\"{password}\",\"license\":\"{license}\",\"hwid\":\"{Utilities.HWID()}\",\"applicationId\":\"{appData.Id}\"}}";
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PutAsync(url, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string responseHash = response.Headers.GetValues("X-Response-Hash").FirstOrDefault();
                    string recalculatedHash = Security.CalculateResponseHash(response.Content.ReadAsStringAsync().Result);
                    if (responseHash != recalculatedHash)
                    {
                        MessageBox.Show("Possible malicious activity detected!", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(0);
                    }

                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    var serializer = new JavaScriptSerializer();
                    userData = (UserData)serializer.Deserialize(responseContent, typeof(UserData));
                    return true;
                }
                else
                {
                    string errorContent = response.Content.ReadAsStringAsync().Result;
                    var serializer = new JavaScriptSerializer();
                    ErrorData errorData = (ErrorData)serializer.Deserialize(errorContent, typeof(ErrorData));
                    MessageBox.Show($"{errorData.Code}: {errorData.Message}", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        public void Log(string action)
        {
            if (!Initialized)
            {
                MessageBox.Show("Please initialize your application first!", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                HttpClient client = new();
                string url = ApiUrl + "/appLogs/";

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userData.Token);

                string jsonData = $"{{\"action\":\"{action}\",\"ip\":\"{Utilities.IP()}\",\"applicationId\":\"{appData.Id}\",\"userId\":\"{userData.Id}\"}}";
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(url, content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = response.Content.ReadAsStringAsync().Result;
                    var serializer = new JavaScriptSerializer();
                    ErrorData errorData = (ErrorData)serializer.Deserialize(errorContent, typeof(ErrorData));
                    MessageBox.Show($"{errorData.Code}: {errorData.Message}", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void DownloadFile(string fileId)
        {
            if (!Initialized)
            {
                MessageBox.Show("Please initialize your application first!", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                HttpClient client = new();
                string url = ApiUrl + $"/files/download/{fileId}";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userData.Token);
                HttpResponseMessage response = client.GetAsync(url).Result;
                string outputPath = string.Empty;

                if (response.IsSuccessStatusCode)
                {
                    // Extract the file name and extension from the response headers
                    if (response.Content.Headers.TryGetValues("Content-Disposition", out var contentDispositionValues))
                    {
                        string contentDisposition = contentDispositionValues.FirstOrDefault();
                        if (!string.IsNullOrEmpty(contentDisposition))
                        {
                            string[] parts = contentDisposition.Split('=');
                            if (parts.Length == 2)
                            {
                                string fileName = parts[1].Trim('"');
                                outputPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                            }
                        }
                    }
                    // Save the file if outputPath is not empty
                    if (!string.IsNullOrEmpty(outputPath))
                    {
                        Stream contentStream = response.Content.ReadAsStreamAsync().Result;
                        FileStream fileStream = File.Create(outputPath);
                        contentStream.CopyTo(fileStream);
                    }
                    else
                    {
                        MessageBox.Show("Unable to determine the file name.", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    string errorContent = response.Content.ReadAsStringAsync().Result;
                    var serializer = new JavaScriptSerializer();
                    ErrorData errorData = (ErrorData)serializer.Deserialize(errorContent, typeof(ErrorData));
                    MessageBox.Show($"{errorData.Code}: {errorData.Message}", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", appData.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}