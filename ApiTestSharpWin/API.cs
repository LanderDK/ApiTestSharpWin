using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Threading;
using System.Security.Principal;
using System.Management;
using System.Net.Sockets;
using RestSharp;

namespace BlitzWare
{
    class API
    {
        internal class Constants
        {
            public static string apiUrl = "https://api.blitzware.xyz/api/";
            //public static string apiUrl = "http://localhost:9000/api/";

            public static bool initialized = false;

            public static bool started = false;

            public static bool breached = false;

            public static DateTime timeSent = DateTime.Now;

            public static string HWID()
            {
                string uuid = string.Empty;

                ManagementClass mc = new ManagementClass("Win32_ComputerSystemProduct");
                ManagementObjectCollection moc = mc.GetInstances();

                foreach (ManagementObject mo in moc)
                {
                    uuid = mo.Properties["UUID"].Value.ToString();
                    break;
                }

                return uuid;
            }

            public static string IP()
            {
                string externalIpString = new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
                return externalIpString;
            }
        }
        internal class ApplicationSettings
        {
            public static string id { get; set; }

            public static bool status { get; set; }

            public static bool hwidCheck { get; set; }

            public static bool developerMode { get; set; }

            public static string programHash { get; set; }

            public static string version { get; set; }

            public static string downloadLink { get; set; }

            public static bool freeMode { get; set; }

            public static bool login { get; set; }

            public static string name { get; set; }

            public static bool register { get; set; }

            public static string TotalUsers { get; set; }
        }
        internal class User
        {
            public static string ID { get; set; }

            public static string Username { get; set; }

            public static string Password { get; set; }

            public static string Email { get; set; }

            public static string HWID { get; set; }

            public static string IP { get; set; }

            public static string Expiry { get; set; }

            public static string LastLogin { get; set; }

            public static string RegisterDate { get; set; }
        }

        internal class OnProgramStart
        {
            public static string Name = null;

            public static void Initialize(string name, string secret, string version)
            {
                Name = name;
                try
                {
                    Security.Start();
                    var client = new RestClient(Constants.apiUrl);
                    var request = new RestRequest("applications/initialize", Method.Post);
                    request.AddHeader("Content-Type", "application/json");
                    request.AddJsonBody(new { name = name, secret = secret, version = version });
                    var response = client.Execute(request);
                    var content = response.Content;
                    dynamic content2 = JsonConvert.DeserializeObject(content);

                    if (Security.MaliciousCheck(Constants.timeSent))
                    {
                        MessageBox.Show("Possible malicious activity detected!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Process.GetCurrentProcess().Kill();
                    }
                    if (Constants.breached)
                    {
                        MessageBox.Show("Possible malicious activity detected!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Process.GetCurrentProcess().Kill();
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        Constants.initialized = true;
                        ApplicationSettings.id = content2.id;
                        ApplicationSettings.status = content2.status;
                        ApplicationSettings.hwidCheck = content2.hwidCheck;
                        ApplicationSettings.programHash = content2.programHash;
                        ApplicationSettings.version = content2.version;
                        ApplicationSettings.downloadLink = content2.downloadLink;
                        ApplicationSettings.developerMode = content2.developerMode;
                        ApplicationSettings.freeMode = content2.freeMode;

                        if (ApplicationSettings.freeMode)
                            MessageBox.Show("Application is in Free Mode!", Name, MessageBoxButtons.OK, MessageBoxIcon.Information);

                        if (ApplicationSettings.developerMode)
                        {
                            MessageBox.Show("Application is in Developer Mode, bypassing integrity and update check!", Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            File.Create(Environment.CurrentDirectory + "/integrity.log").Close();
                            string hash = Security.Integrity(Process.GetCurrentProcess().MainModule.FileName);
                            File.WriteAllText(Environment.CurrentDirectory + "/integrity.log", hash);
                            MessageBox.Show("Your applications hash has been saved to integrity.txt, please refer to this when your application is ready for release!", Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            if (ApplicationSettings.version != version)
                            {
                                MessageBox.Show($"Update {ApplicationSettings.version} available, redirecting to update!", Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                Process.Start(ApplicationSettings.downloadLink);
                                Process.GetCurrentProcess().Kill();
                            }
                            if (content2.integrityCheck == true)
                            {
                                if (ApplicationSettings.programHash != Security.Integrity(Process.GetCurrentProcess().MainModule.FileName))
                                {
                                    MessageBox.Show($"File has been tampered with, couldn't verify integrity!", Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    Process.GetCurrentProcess().Kill();
                                }
                            }
                        }
                        if (ApplicationSettings.status == false)
                        {
                            MessageBox.Show("Looks like this application is disabled, please try again later!", Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Process.GetCurrentProcess().Kill();
                        }
                    }
                    else
                    {
                        //Console.WriteLine(content2.code);
                        if (content2.code == "UNAUTHORIZED")
                        {
                            MessageBox.Show((string)content2.message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Process.GetCurrentProcess().Kill();
                        }
                        else if (content2.code == "NOT_FOUND")
                        {
                            MessageBox.Show((string)content2.message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Process.GetCurrentProcess().Kill();
                        }
                        else if (content2.code == "VALIDATION_FAILED")
                        {
                            MessageBox.Show("Failed to initialize your application correctly in Program.cs!\n\nDetials:\n"
                                + Convert.ToString(content2.details), Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Process.GetCurrentProcess().Kill();
                        }
                    }
                    Security.End();
                }
                catch (Exception ex)
                {
                    if (ex.InnerException.ToString().Contains("Unable to connect to the remote server"))
                    {
                        MessageBox.Show("Unable to connect to the remote server!", Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Process.GetCurrentProcess().Kill();
                    }
                    else
                    {
                        MessageBox.Show(ex.Message, Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Process.GetCurrentProcess().Kill();
                    }
                }
            }
        }

        public static bool Login(string username, string password)
        {
            if (!Constants.initialized)
            {
                MessageBox.Show("Please initialize your application first!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            try
            {
                Security.Start();
                Constants.timeSent = DateTime.Now;
                var client = new RestClient(Constants.apiUrl);
                var request = new RestRequest("users/login", Method.Post);
                request.AddHeader("Content-Type", "application/json");
                request.AddJsonBody(new { username = username, password = password, hwid = Constants.HWID(), lastIP = Constants.IP(), appId = ApplicationSettings.id });
                var response = client.Execute(request);
                var content = response.Content;
                dynamic content2 = JsonConvert.DeserializeObject(content);

                if (Security.MaliciousCheck(Constants.timeSent))
                {
                    MessageBox.Show("Possible malicious activity detected!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Process.GetCurrentProcess().Kill();
                }
                if (Constants.breached)
                {
                    MessageBox.Show("Possible malicious activity detected!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Process.GetCurrentProcess().Kill();
                }

                if (response.IsSuccessStatusCode)
                {
                    User.ID = content2.id;
                    User.Username = content2.username;
                    User.Password = content2.password;
                    User.Email = content2.email;
                    User.Expiry = content2.expiryDate;
                    User.LastLogin = content2.lastLogin;
                    User.IP = content2.lastIP;
                    User.HWID = content2.hwid;
                    Security.End();
                    return true;
                }
                else
                {
                    //Console.WriteLine(content2.code);
                    if (content2.code == "UNAUTHORIZED")
                    {
                        MessageBox.Show((string)content2.message, OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (content2.code == "NOT_FOUND")
                    {
                        MessageBox.Show((string)content2.message, OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (content2.code == "VALIDATION_FAILED")
                    {
                        MessageBox.Show(Convert.ToString(content2.details), OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    Security.End();
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException.ToString().Contains("Unable to connect to the remote server"))
                    MessageBox.Show("Unable to connect to the remote server!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show(ex.Message, OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Security.End();
                return false;
            }
        }

        public static bool Register(string username, string password, string email, string license)
        {
            if (!Constants.initialized)
            {
                MessageBox.Show("Please initialize your application first!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            try
            {
                Security.Start();
                Constants.timeSent = DateTime.Now;
                var client = new RestClient(Constants.apiUrl);
                var request = new RestRequest("users/register", Method.Post);
                request.AddHeader("Content-Type", "application/json");
                request.AddJsonBody(new { username = username, password = password, email = email, license = license, hwid = Constants.HWID(), lastIP = Constants.IP(), id = ApplicationSettings.id });
                var response = client.Execute(request);
                var content = response.Content;
                dynamic content2 = JsonConvert.DeserializeObject(content);

                if (Security.MaliciousCheck(Constants.timeSent))
                {
                    MessageBox.Show("Possible malicious activity detected!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Process.GetCurrentProcess().Kill();
                }
                if (Constants.breached)
                {
                    MessageBox.Show("Possible malicious activity detected!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Process.GetCurrentProcess().Kill();
                }

                if (response.IsSuccessStatusCode)
                {
                    User.ID = content2.id;
                    User.Username = content2.username;
                    User.Password = content2.password;
                    User.Email = content2.email;
                    User.Expiry = content2.expiryDate;
                    User.LastLogin = content2.lastLogin;
                    User.IP = content2.lastIP;
                    User.HWID = content2.hwid;
                    Security.End();
                    return true;
                }
                else
                {
                    //Console.WriteLine(content2.code);
                    if (content2.code == "NOT_FOUND")
                    {
                        MessageBox.Show((string)content2.message, OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (content2.code == "ER_DUP_ENTRY")
                    {
                        MessageBox.Show("User with this username already exists!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (content2.code == "FORBIDDEN")
                    {
                        MessageBox.Show("User with this username already exists!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (content2.code == "VALIDATION_FAILED")
                    {
                        MessageBox.Show(Convert.ToString(content2.details), OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    Security.End();
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException.ToString().Contains("Unable to connect to the remote server"))
                    MessageBox.Show("Unable to connect to the remote server!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show(ex.Message, OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Security.End();
                return false;
            }
        }

        public static bool ExtendSub(string username, string password, string license)
        {
            if (!Constants.initialized)
            {
                MessageBox.Show("Please initialize your application first!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            try
            {
                Security.Start();
                Constants.timeSent = DateTime.Now;
                var client = new RestClient(Constants.apiUrl);
                var request = new RestRequest("users/upgrade", Method.Put);
                request.AddHeader("Content-Type", "application/json");
                request.AddJsonBody(new { username = username, password = password, license = license, hwid = Constants.HWID(), appId = ApplicationSettings.id });
                var response = client.Execute(request);
                var content = response.Content;
                dynamic content2 = JsonConvert.DeserializeObject(content);

                if (Security.MaliciousCheck(Constants.timeSent))
                {
                    MessageBox.Show("Possible malicious activity detected!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Process.GetCurrentProcess().Kill();
                }
                if (Constants.breached)
                {
                    MessageBox.Show("Possible malicious activity detected!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Process.GetCurrentProcess().Kill();
                }

                if (response.IsSuccessStatusCode)
                {
                    User.ID = content2.id;
                    User.Username = content2.username;
                    User.Password = content2.password;
                    User.Email = content2.email;
                    User.Expiry = content2.expiryDate;
                    User.LastLogin = content2.lastLogin;
                    User.IP = content2.lastIP;
                    User.HWID = content2.hwid;
                    Security.End();
                    return true;
                }
                else
                {
                    //Console.WriteLine(content2.code);
                    if (content2.code == "NOT_FOUND")
                    {
                        MessageBox.Show((string)content2.message, OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (content2.code == "ER_DUP_ENTRY")
                    {
                        MessageBox.Show("User with this username already exists!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (content2.code == "FORBIDDEN")
                    {
                        MessageBox.Show("User with this username already exists!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (content2.code == "VALIDATION_FAILED")
                    {
                        MessageBox.Show(Convert.ToString(content2.details), OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (content2.code == "UNAUTHORIZED")
                    {
                        MessageBox.Show((string)content2.message, OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    Security.End();
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException.ToString().Contains("Unable to connect to the remote server"))
                    MessageBox.Show("Unable to connect to the remote server!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show(ex.Message, OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Security.End();
                return false;
            }
        }

        public static void Log(string username, string action)
        {
            if (!Constants.initialized)
            {
                MessageBox.Show("Please initialize your application first!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Process.GetCurrentProcess().Kill();
            }
            try
            {
                Security.Start();
                Constants.timeSent = DateTime.Now;
                var client = new RestClient(Constants.apiUrl);
                var request = new RestRequest("appLogs/", Method.Post);
                request.AddHeader("Content-Type", "application/json");
                request.AddJsonBody(new { username = username, action = action, ip = Constants.IP(), appId = ApplicationSettings.id });
                var response = client.Execute(request);
                var content = response.Content;
                dynamic content2 = JsonConvert.DeserializeObject(content);

                if (Security.MaliciousCheck(Constants.timeSent))
                {
                    MessageBox.Show("Possible malicious activity detected!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Process.GetCurrentProcess().Kill();
                }
                if (Constants.breached)
                {
                    MessageBox.Show("Possible malicious activity detected!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Process.GetCurrentProcess().Kill();
                }

                if (response.IsSuccessStatusCode)
                {
                    Security.End();
                }
                else
                {
                    //Console.WriteLine(content2.code);
                    if (content2.code == "UNAUTHORIZED")
                    {
                        MessageBox.Show((string)content2.message, OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (content2.code == "NOT_FOUND")
                    {
                        MessageBox.Show((string)content2.message, OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (content2.code == "VALIDATION_FAILED")
                    {
                        MessageBox.Show(Convert.ToString(content2.details), OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    Security.End();
                    Process.GetCurrentProcess().Kill();

                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException.ToString().Contains("Unable to connect to the remote server"))
                    MessageBox.Show("Unable to connect to the remote server!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show(ex.Message, OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Security.End();
                Process.GetCurrentProcess().Kill();
            }
        }

        internal class Security
        {
            public static void Start()
            {
                string drive = Path.GetPathRoot(Environment.SystemDirectory);
                if (Constants.started)
                {
                    MessageBox.Show("A session has already been started, please end the previous one!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Process.GetCurrentProcess().Kill();
                }
                else
                {
                    using (StreamReader sr = new StreamReader($@"{drive}Windows\System32\drivers\etc\hosts"))
                    {
                        string contents = sr.ReadToEnd();
                        if (contents.Contains("localhost:9000"))
                        {
                            Constants.breached = true;
                            MessageBox.Show("DNS redirecting has been detected!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Process.GetCurrentProcess().Kill();
                        }
                    }
                    Constants.started = true;
                }
            }
            public static void End()
            {
                if (!Constants.started)
                {
                    MessageBox.Show("No session has been started, closing for security reasons!", OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Process.GetCurrentProcess().Kill();
                }
                else
                {
                    Constants.started = false;
                }
            }
            public static string Integrity(string filename)
            {
                string result;
                using (MD5 md = MD5.Create())
                {
                    using (FileStream fileStream = File.OpenRead(filename))
                    {
                        byte[] value = md.ComputeHash(fileStream);
                        result = BitConverter.ToString(value).Replace("-", "").ToLowerInvariant();
                    }
                }
                return result;
            }
            public static bool MaliciousCheck(DateTime date)
            {
                DateTime dt1 = date; //time sent
                DateTime dt2 = DateTime.Now; //time received
                TimeSpan d3 = dt1 - dt2;
                if (Convert.ToInt32(d3.Seconds.ToString().Replace("-", "")) >= 5 || Convert.ToInt32(d3.Minutes.ToString().Replace("-", "")) >= 1)
                {
                    Constants.breached = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}

// GET HWID OF DISKDRIVE
/*string command = @"wmic diskdrive where DeviceID='\\\\.\\PHYSICALDRIVE0' get serialnumber";
Process process = new Process();
process.StartInfo.FileName = "cmd.exe";
process.StartInfo.Arguments = "/c " + command;
process.StartInfo.RedirectStandardOutput = true;
process.StartInfo.UseShellExecute = false;
process.Start();

string output = process.StandardOutput.ReadToEnd();
string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
string serialNumber = lines[1].Replace(".", "").Trim();
Console.WriteLine(serialNumber);

process.WaitForExit();*/