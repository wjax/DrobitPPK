using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ControlCenter.Extras
{
    public class DrobitTools
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        //public static readonly string LOCAL_DROBIT_FOLDER = @"C:\DROBIT";
        //public static readonly string LOCAL_DOWNLOADS_SUBFOLDER = @"C:\DROBIT\DOWNLOADS";
        //public static readonly string LOCAL_PROJECTS_SUBFOLDER = @"C:\DROBIT\PROJECTS";

        private const int LEAP_SECONDS = 18;

        public static string AbsolutizePath(string path)
        {
            const string REGEX_PATH = @"^[A-Za-z]+:";
            string absolutePath;

            if (Regex.IsMatch(path, REGEX_PATH))
                absolutePath = path;
            else
                absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

            return absolutePath;
        }

        public static string GetMainDrobitFolder()
        {
            return ControlCenter.Properties.Settings.Default["DrobitMainPath"].ToString();
        }

        public static string GetDOWNLOADSDrobitFolder()
        {
            return ConcatenatePath(new string[] { GetMainDrobitFolder(), "DOWNLOADS" });
        }
        public static string GetDOWNLOADSDrobitFolder4DrobitEq(string ID)
        {
            return ConcatenatePath(new string[] { GetDOWNLOADSDrobitFolder(), ID });
        }


        public static string GetPROJECTSDrobitFolder()
        {
            return ConcatenatePath(new string[] { GetMainDrobitFolder(), "PROJECTS" });
        }

        public static bool CheckFolders()
        {
            if (Directory.Exists(GetMainDrobitFolder()))
            {
                CreateFolderIfNotExists(GetDOWNLOADSDrobitFolder());
                CreateFolderIfNotExists(GetPROJECTSDrobitFolder());
                return true;
            }
            else
                return false;
        }

        public static bool CheckFolderExists(string path)
        {
            if (Directory.Exists(path))
                return true;
            else
                return false;
        }

        public static List<string> GetFolderContent(string folderuri, bool wantFile, bool wantFolder)
        {
            if (!CheckFolderExists(folderuri))
                return new List<string>();

            string[] folders = new string[0];
            string[] files = new string[0];

            if (wantFolder)
                folders = Directory.GetDirectories(folderuri);
            if (wantFile)
                files =  Directory.GetFiles(folderuri);

            List<string> final = new List<string>();

            foreach (string s in folders)
                final.Add(s);
            foreach (string s in files)
                final.Add(s);

            return final;

        }

        public static string ConcatenatePath(string[] paths)
        {
            string sPath = "";
            sPath = Path.Combine(paths);

            return sPath;
        }

        public static void CreateFolderIfNotExists(string path)
        {
            DirectoryInfo info = Directory.CreateDirectory(path);
        }

        public static long millisFromEpoch(DateTime d)
        {
            return (long)(d - UnixEpoch).TotalMilliseconds;
        }

        public static long millisFromEpochNow()
        {
            return (long)(DateTime.Now - UnixEpoch).TotalMilliseconds;
        }

        public static long microsFromEpoch(DateTime d)
        {
            return (long)(((d - UnixEpoch).TotalMilliseconds) * 1000);
        }

        public static DateTime currentDate()
        {
            return new DateTime();
        }
        public static DateTime fromMillis(long millis)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(millis);
            DateTime result = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            result = result.Add(time);

            return result;

        }

        public static string fromMillisNice(long millis)
        {
            string format = "dd MMM HH:mm:ss";
            DateTime d = fromMillis(millis);

            string date = d.ToString(format); ;

            return date;

        }

        public static DateTime GetTimeFromGps(int weeknumber, double seconds, bool useLEAP)
        {
            DateTime datum = new DateTime(1980, 1, 6, 0, 0, 0, DateTimeKind.Utc);

            try
            {
                DateTime week = datum.AddDays(weeknumber * 7);
                DateTime time = week.AddSeconds(seconds);

                if (useLEAP)
                    return time.AddSeconds(-LEAP_SECONDS);
                else
                    return time;

            } catch (Exception e)
            {
                return datum;
            }
        }

        public static void GetGPSFromRINEX(int year_, int month, int day, int hour, int minute, double seconds, out int gpsWeek, out double gpsSeconds)
        {
            int year = year_;
            int nDigits = year.ToString().Length;
            if (nDigits < 4)
                year = CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(year);

            DateTime date = new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Utc);
            date = date.AddSeconds(seconds);

            GetGPSFromUTC(date, out gpsWeek, out gpsSeconds, false);
        }

        public static void GetGPSFromUTC(DateTime time, out int gpsWeek, out double gpsSecs, bool useLEAP)
        {
            DateTime datum = new DateTime(1980, 1, 6, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan t = time - datum;

            if (useLEAP)
                t = t.Add(new TimeSpan(0, 0, LEAP_SECONDS));
            
            gpsWeek = (int)(t.TotalSeconds / (86400 * 7));
            gpsSecs = (double)(t.TotalSeconds - (double)gpsWeek * 86400 * 7);


        }

        public static long GetMillisFromEpoch(DateTime date)
        {
            DateTime dt1970 = new DateTime(1970, 1, 1, 0,0,0,DateTimeKind.Utc);
            TimeSpan span = date - dt1970;
            return (long)span.TotalMilliseconds;
        }


        public static string CopyFile2Folder(string fileUri, string path2copy)
        {
            string destinationUri = Path.Combine(path2copy, Path.GetFileName(fileUri));
            File.Copy(fileUri, destinationUri, true);

            return destinationUri;
        }

        public static string CopyFile2File(string fileUri, string destFile)
        {
            File.Copy(fileUri, destFile, true);

            return destFile;
        }

        public static void EmptyDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Array.ForEach(Directory.GetFiles(path), File.Delete);

                string[] subFolderPaths = Directory.GetDirectories(path);
                foreach (string subDir in subFolderPaths)
                    EmptyDirectory(subDir);
            }
        }

        public static bool IsRinexVersion3x(string uri)
        {
            int MAXLINES = 10;
            int i = 0;
            foreach (string line in File.ReadLines(uri))
            {
                if (Regex.IsMatch(line, @"3\..*RINEX VERSION"))
                    return true;

                if (i > MAXLINES)
                    return false;

                i++;
            }
            return false;
        }
        public static bool IsCompactRinex(string uri)
        {
            int MAXLINES = 10;
            int i = 0;
            foreach (string line in File.ReadLines(uri))
            {
                if (Regex.IsMatch(line, @"COMPACT.*RINEX"))
                    return true;

                if (i > MAXLINES)
                    return false;

                i++;
            }
            return false;
        }

        public static bool CopyFolderContents(string SourcePath, string DestinationPath, bool onlyFiles=false)
        {
            try
            {
                if (Directory.Exists(SourcePath))
                {
                    if (Directory.Exists(DestinationPath) == false)
                    {
                        Directory.CreateDirectory(DestinationPath);
                    }

                    foreach (string files in Directory.GetFiles(SourcePath))
                    {
                        FileInfo fileInfo = new FileInfo(files);
                        fileInfo.CopyTo(Path.Combine(DestinationPath, fileInfo.Name), true);
                    }
                    if (!onlyFiles)
                    {
                        foreach (string drs in Directory.GetDirectories(SourcePath))
                        {
                            DirectoryInfo directoryInfo = new DirectoryInfo(drs);
                            if (CopyFolderContents(drs, Path.Combine(DestinationPath, directoryInfo.Name)) == false)
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public static string NormalizeString4FS(string file)
        {
            if (file == null || file.Equals(""))
                return null;
            //string regexSearch = new string(Path.GetInvalidFileNameChars());
            //Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            //return r.Replace(file, "");

            System.Diagnostics.Debug.WriteLine("NormalizeString4FS");

            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

            foreach (char c in invalid)
            {
                file = file.Replace(c.ToString(), "");
            }

            return file;
            //string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            //string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            //return System.Text.RegularExpressions.Regex.Replace(file, invalidRegStr, "_");
        }

        public static string GenerateDrobitDownloadFolderURI(string drobitSerial)
        {
            return ConcatenatePath(new string[] { GetDOWNLOADSDrobitFolder(), drobitSerial });
        }

        public static string GenerateDrobitDownloadFolder4ProjectURI(string drobitSerial, string recordingName)
        {
            string nameSanitized = NormalizeString4FS(recordingName);
            return DrobitTools.ConcatenatePath(new string[] { GenerateDrobitDownloadFolderURI(drobitSerial), nameSanitized });
        }

        public static string GetDescription(Enum en)
        {
            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            return en.ToString();
        }

        

    }
}
