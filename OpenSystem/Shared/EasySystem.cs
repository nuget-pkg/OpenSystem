using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

// ReSharper disable RedundantArgumentDefaultValue

// ReSharper disable AccessToStaticMemberViaDerivedType

// ReSharper disable InconsistentNaming

// ReSharper disable EmptyGeneralCatchClause
#if USE_EASY_OBJECT
using static Global.EasyObject;
#endif

// ReSharper disable once CheckNamespace
namespace Global;
#if GLOBAL_SYS
public static partial class Sys {
#else
// ReSharper disable once PartialTypeWithSinglePart
public static partial class EasySystem {
#endif
    public static bool SilentFlag = false;
    public static bool IsWindowsPlatform() {
#if NETFRAMEWORK
        return Environment.OSVersion.Platform == PlatformID.Win32NT;
#else
            return OperatingSystem.IsWindows();
#endif
    }

    // public static void ConsoleClearCurrentLine()
    // {
    //     int currentLine = Console.CursorTop;
    //     Console.SetCursorPosition(0, Console.CursorTop);
    //     Console.Write(new string(' ', Console.WindowWidth));
    //     Console.SetCursorPosition(0, currentLine);
    // }
    public static string ProfilePath() {
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            .Replace('/', Path.DirectorySeparatorChar);
    }
    public static string ProfilePath(string appName) {
        string baseFolder = ProfilePath();
        return $"{baseFolder}/{appName}".Replace('/', Path.DirectorySeparatorChar);
    }
    public static string ProfilePath(string orgName, string appName) {
        string baseFolder = ProfilePath();
        return $"{baseFolder}/{appName}".Replace('/', Path.DirectorySeparatorChar);
    }
    public static List<string> TextToLines(string text) {
        List<string> lines = [];
        using (StringReader sr = new StringReader(text)) {
            string? line;
            while ((line = sr.ReadLine()) != null) {
                lines.Add(line);
            }
        }
        return lines;
    }
    public static string LimitStringLength(string s, int limit, string ellipsis = "...") {
        UTF32Encoding enc = new UTF32Encoding();
        byte[] byteUtf32 = enc.GetBytes(s);
        if (byteUtf32.Length <= limit * 4) {
            return s;
        }
        ArraySegment<byte> segment = new ArraySegment<byte>(byteUtf32, 0, limit * 4);
        byteUtf32 = segment.ToArray();
        string decodedString = enc.GetString(byteUtf32);
        return decodedString + ellipsis;
    }
    public static Process? OpenUrl(string url) {
        ProcessStartInfo pi = new ProcessStartInfo() {
            FileName = url,
            UseShellExecute = true,
        };
        return Process.Start(pi);
    }
    public static string ZipDirectory(string dir, string? zipout = null, string comment = "") {
        dir = Path.GetFullPath(dir);
        if (zipout == null) {
            zipout = dir + ".zip";
        }
        if (Directory.Exists(dir) && !string.IsNullOrWhiteSpace(dir) && !string.IsNullOrWhiteSpace(zipout)) {
            try {
                using var zip = EasyZipStorer.Create(zipout, comment); // true for stream
                zip.EncodeUTF8 = true;
                zip.ForceDeflating = true;
                foreach (string listDir in
                         Directory.EnumerateDirectories(dir, "*", SearchOption.TopDirectoryOnly)) {
                    // Add folders with files to the archive
                    try {
                        zip.AddDirectory(EasyZipStorer.Compression.Deflate, listDir, string.Empty);
                    }
                    catch
                    {
                    }
                }
                foreach (string listFiles in Directory.EnumerateFiles(dir, "*.*", SearchOption.TopDirectoryOnly)) {
                    // Add residual files in the current directory to the archive.
                    try {
                        zip.AddFile(EasyZipStorer.Compression.Deflate, listFiles, Path.GetFileName(listFiles));
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }
        return zipout;
    }
    public static void Exit(int exitCoed) {
        Console.Error.Write($"Exit() was called with exitCode: {exitCoed}." + "\n");
        Environment.Exit(exitCoed);
    }
    public static string? FindHome(DirectoryInfo dir) {
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files) {
            if (file.Name == ".bashrc" || file.Name == ".profile") {
                return dir.FullName;
            }
        }
        DirectoryInfo? parent = dir.Parent;
        if (parent == null) {
            return null;
        }
        return FindHome(parent);
    }
    public static string? FindGitRoot(string dir) {
        return FindGitRoot(new DirectoryInfo(dir));
    }
    public static string? FindGitRoot(DirectoryInfo dir) {
        DirectoryInfo[] files = dir.GetDirectories();
        foreach (DirectoryInfo file in files) {
            if (file.Name == ".git") {
                return dir.FullName;
            }
        }
        DirectoryInfo? parent = dir.Parent;
        if (parent == null) {
            return null;
        }
        return FindGitRoot(parent);
    }
    public static string GetCwd() {
        return Directory.GetCurrentDirectory();
    }
    public static void SetCwd(string path) {
        path = CygpathWindows(path);
        if (!SilentFlag) {
            Console.Error.WriteLine($"SetCwd(): {path}");
        }
        Prepare(path);
        Directory.SetCurrentDirectory(path);
    }
    public static string GetFullPath(string path) {
        path = CygpathWindows(path);
        return Path.GetFullPath(path);
    }
    public static string GetFileName(string path) {
        path = CygpathWindows(path);
        return Path.GetFileName(path);
    }
    public static string GetDirectoryName(string path) {
        path = CygpathWindows(path);
        return Path.GetDirectoryName(path)!;
    }
    public static string GetBaseName(string path, bool strongAlgorithm) {
        if (strongAlgorithm) {
            try {
                path = path.Trim();
                path = path.Replace("\\", "/");
                var split = path.Split('/');
                if (split.Length == 0) {
                    return path;
                }
                return split[split.Length - 1];
            }
            catch {
                return path;
            }
        }
        path = CygpathWindows(path);
        return Path.GetFileNameWithoutExtension(Path.GetFileName(path));
    }
    public static Process? LaunchProcess(string exePath, string[] args, Dictionary<string, string>? vars = null) {
        exePath = CygpathWindows(exePath);
        var argList = "";
        for (var i = 0; i < args.Length; i++) {
            if (i > 0) argList += " ";
            if (args[i].Contains(" "))
                argList += $"\"{args[i]}\"";
            else
                argList += args[i];
        }
        var process = new Process();
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.FileName = exePath;
        process.StartInfo.Arguments = argList;
        if (vars != null)
            foreach (var key in vars.Keys)
                process.StartInfo.EnvironmentVariables[key] = vars[key];
        var result = process.Start();
        if (!result) return null;
        return process;
    }
    public static string GetProcessStdout(Encoding encoding, string exe, params string[] args) {
        string cmdArgs = "";
        for (int i = 0; i < args.Length; i++) {
            if (i != 0) {
                cmdArgs += " ";
            }
            cmdArgs += string.Format("\"{0}\"", args[i]);
        }
        StringBuilder outputBuilder;
        ProcessStartInfo processStartInfo;
        Process process;
        outputBuilder = new StringBuilder();
        processStartInfo = new ProcessStartInfo();
        processStartInfo.StandardOutputEncoding = encoding;
        processStartInfo.CreateNoWindow = true;
        processStartInfo.RedirectStandardOutput = true;
        processStartInfo.RedirectStandardInput = true;
        processStartInfo.UseShellExecute = false;
        processStartInfo.FileName = exe;
        processStartInfo.Arguments = cmdArgs;
        process = new Process {
            StartInfo = processStartInfo,
            EnableRaisingEvents = true
        };
        process.OutputDataReceived += delegate(object _, DataReceivedEventArgs e) {
            if (!SilentFlag) {
                Console.Error.WriteLine(e.Data);
            }
            outputBuilder.Append(e.Data + "\n");
        };
        process.Start();
        process.BeginOutputReadLine();
        process.WaitForExit();
        process.CancelOutputRead();
        string output = outputBuilder.ToString();
        output = output.Trim() + "\n";
        return output;
    }
    public static string? FindExePath(string exe) {
        string cwd = "";
        return FindExePath(exe, cwd);
    }
    public static string? FindExePath(string exe, string cwd) {
        cwd = CygpathWindows(cwd);
        exe = Environment.ExpandEnvironmentVariables(exe);
        if (Path.IsPathRooted(exe)) {
            if (!File.Exists(exe)) {
                return null;
            }
            return Path.GetFullPath(exe);
        }
        string pathList = Environment.GetEnvironmentVariable("PATH") ?? "";
        pathList = $"{cwd};{pathList}";
        foreach (string test in pathList.Split(';')) {
            string path = test.Trim();
            if (!string.IsNullOrEmpty(path) && File.Exists(Path.Combine(path, exe))) {
                return Path.GetFullPath(Path.Combine(path, exe));
            }
            string baseName = Path.GetFileNameWithoutExtension(exe);
            if (!string.IsNullOrEmpty(path) && File.Exists(Path.Combine(path, $"{baseName}.bin", exe))) {
                return Path.GetFullPath(Path.Combine(path, $"{baseName}.bin", exe));
            }
        }
        return null;
    }
    public static string? FindExePath(string exe, Assembly assembly) {
        int bit = IntPtr.Size * 8;
        string? cwd = AssemblyDirectory(assembly);
        if (cwd == null) return null;
        string? result = FindExePath(exe, cwd);
        if (result == null) {
            result = FindExePath(exe, $"{cwd}\\{bit}bit");
            if (result == null) {
                cwd = Path.Combine(cwd, "assets");
                result = FindExePath(exe, $"{cwd}\\{bit}bit");
            }
        }
        return result;
    }
    public static string? FindExeRecursive(string rootDirectory, string exeName) {
        try {
            IEnumerable<string> exeFiles =
                Directory.EnumerateFiles(rootDirectory, "*.exe", SearchOption.AllDirectories);
            Console.WriteLine($"EXE files found under {rootDirectory}:");
            foreach (string file in exeFiles) {
                Console.WriteLine(file);
                EasyObjectClassic.Debug(file);
                string baseName = EasySystem.SafeBaseName(file);
                if (string.Equals(baseName, exeName, StringComparison.CurrentCultureIgnoreCase)) {
                    return file;
                }
            }
        }
        catch (UnauthorizedAccessException ex) {
            Console.Error.WriteLine($"Access denied to one or more directories: {ex.Message}");
        }
        catch (DirectoryNotFoundException ex) {
            Console.Error.WriteLine($"Directory not found: {ex.Message}");
        }
        catch (Exception ex) {
            Console.Error.WriteLine($"An error occurred: {ex.Message}");
        }
        return null;
    }
    public static string? AssemblyDirectory(Assembly assembly) {
#pragma warning disable SYSLIB0012
        string? codeBase = assembly.CodeBase;
#pragma warning restore SYSLIB0012
        if (codeBase == null) return null;
        UriBuilder uri = new UriBuilder(codeBase);
        string path = Uri.UnescapeDataString(uri.Path);
        return Path.GetDirectoryName(path)!;
    }
    public static string CygpathWindows(string path) {
        path = path.Replace(@"\", "/");
        if (Environment.OSVersion.Platform != PlatformID.Win32NT) {
            return path;
        }
        List<string>? m = FindFirstMatch(
            path,
            "^/([a-zA-z])[/]?$",
            "^/([a-zA-z])/(.+)$",
            "^/mnt/([a-zA-z])[/]?$",
            "^/mnt/([a-zA-z])/(.+)$"
        );
        if (m != null) {
            if (m.Count == 2) {
                path = $"{m[1].ToUpper()}:/";
            }
            else if (m.Count == 3) {
                path = $"{m[1].ToUpper()}:/{m[2]}";
            }
        }
        path = path.Replace('/', Path.DirectorySeparatorChar);
        return path;
    }
    public static string[] ExpandWildcard(string path) {
        path = CygpathWindows(path);
        string dir = Path.GetDirectoryName(path)!;
        if (string.IsNullOrEmpty(dir)) {
            dir = ".";
        }
        string fname = Path.GetFileName(path);
        string[] files = Directory.GetFileSystemEntries(dir, fname);
        List<string> result = [];
        for (int i = 0; i < files.Length; i++) {
            result.Add(Path.GetFullPath(files[i]));
        }
        return result.ToArray();
    }
    public static string[] ExpandWildcardList(params string[] pathList) {
        pathList = (string[])pathList.Clone();
        for (int i = 0; i < pathList.Length; i++) {
            pathList[i] = CygpathWindows(pathList[i]);
        }
        List<string> result = [];
        for (int i = 0; i < pathList.Length; i++) {
            string[] files = ExpandWildcard(pathList[i]);
            result.AddRange(files.ToList());
        }
        return result.ToArray();
    }
    public static void FreeHGlobal(IntPtr x) {
        Marshal.FreeHGlobal(x);
    }
    public static IntPtr StringToWideAddr(string s) {
        return Marshal.StringToHGlobalUni(s);
    }
    public static string WideAddrToString(IntPtr s) {
        return Marshal.PtrToStringUni(s)!;
    }
    public static IntPtr StringToUTF8Addr(string s) {
        int len = Encoding.UTF8.GetByteCount(s);
        byte[] buffer = new byte[len + 1];
        Encoding.UTF8.GetBytes(s, 0, s.Length, buffer, 0);
        IntPtr nativeUtf8 = Marshal.AllocHGlobal(buffer.Length);
        Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);
        return nativeUtf8;
    }
    public static string UTF8AddrToString(IntPtr s) {
        int len = 0;
        while (Marshal.ReadByte(s, len) != 0) {
            ++len;
        }
        byte[] buffer = new byte[len];
        Marshal.Copy(s, buffer, 0, buffer.Length);
        return Encoding.UTF8.GetString(buffer);
    }
    public static IntPtr ReassignThreadLocalStringPointer(ThreadLocal<IntPtr> ptr, string s) {
        if (ptr.Value != IntPtr.Zero) {
            FreeHGlobal(ptr.Value);
            ptr.Value = IntPtr.Zero;
        }
        ptr.Value = StringToUTF8Addr(s);
        return ptr.Value;
    }
    public static int RunToConsole(string exePath, string[] args, Dictionary<string, string>? vars = null) {
        exePath = CygpathWindows(exePath);
        string argList = "";
        for (int i = 0; i < args.Length; i++) {
            if (i > 0) {
                argList += " ";
            }
            if (args[i].Contains(" ")) {
                argList += $"\"{args[i]}\"";
            }
            else {
                argList += args[i];
            }
        }
        Process process = new Process();
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.FileName = exePath;
        process.StartInfo.Arguments = argList;
        if (vars != null) {
            Dictionary<string, string>.KeyCollection keys = vars.Keys;
            foreach (string key in keys) {
                process.StartInfo.EnvironmentVariables[key] = vars[key];
            }
        }
        process.OutputDataReceived += (_, e) => { Console.WriteLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { Console.Error.WriteLine(e.Data); };
        process.Start();
        Console.CancelKeyPress += delegate { process.Kill(); };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
        process.CancelOutputRead();
        process.CancelErrorRead();
        return process.ExitCode;
    }
    public static byte[]? ToUtf8Bytes(string? s) {
        if (s is null) {
            return null;
        }
        byte[] bytes = Encoding.UTF8.GetBytes(s);
        return bytes;
    }
    public static void Prepare(string dirPath) {
        dirPath = CygpathWindows(dirPath);
        Directory.CreateDirectory(dirPath);
    }
    public static void PrepareForFile(string filePath) {
        filePath = CygpathWindows(filePath);
        Prepare(Path.GetDirectoryName(filePath)!);
    }
    public static void DownloadBinaryFromUrl(string url, string destinationPath) {
        destinationPath = CygpathWindows(destinationPath);
        PrepareForFile(destinationPath);
#pragma warning disable SYSLIB0014
        // ReSharper disable once RedundantNameQualifier
        WebRequest objRequest = System.Net.HttpWebRequest.Create(url);
#pragma warning restore SYSLIB0014
        WebResponse objResponse = objRequest.GetResponse();
        byte[] buffer = new byte[32768];
        using Stream? input = objResponse.GetResponseStream();
        if (input == null) {
            return;
        }
        using FileStream output = new FileStream(destinationPath, FileMode.CreateNew);
        int bytesRead;
        while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0) {
            output.Write(buffer, 0, bytesRead);
        }
    }
    public static List<string>? FindFirstMatch(string s, params string[] patterns) {
        foreach (string pattern in patterns) {
            Regex r = new Regex(pattern);
            Match m = r.Match(s);
            if (m.Success) {
                List<string> groups = [];
                for (int i = 0; i < m.Groups.Count; i++) {
                    groups.Add(m.Groups[i].Value);
                }
                return groups;
            }
        }
        return null;
    }
    public static Dictionary<string, string> QueryParameterDictionary(string url) {
        Uri uri = new Uri(url);
        string queryString = uri.Query;
        NameValueCollection queryParameters = HttpUtility.ParseQueryString(queryString);
        Dictionary<string, string> dict = [];
        foreach (string? key in queryParameters.AllKeys) {
            dict[key!] = queryParameters[key!]!;
        }
        return dict;
    }
    public static string? FindQueryParameter(string url, string name) {
        Dictionary<string, string> dict = QueryParameterDictionary(url);
        if (dict.ContainsKey(name)) {
            return dict[name];
        }
        return null;
    }
    public static string RemoveStringSuffix(string input, string suffix) {
        if (input.EndsWith(suffix)) {
            return input.Remove(input.Length - suffix.Length, suffix.Length);
        }
        return input;
    }
    public static string RemoveSurrogatePair(string str, string replaceSurrogate = "✅") {
        return UniversalTransformer.ReplaceSurrogatePair(str, replaceSurrogate: replaceSurrogate);
    }
    public static string AdjustFileName(string fileName, string replaceSurrogate = "✅") {
        return UniversalTransformer.SafeFileName(fileName, replaceSurrogate: replaceSurrogate);
    }
    public static string AdjustMetaData(string metadata, string replaceSurrogate = "✅") {
        return UniversalTransformer.SafeMetaData(metadata, replaceSurrogate: replaceSurrogate);
    }
    public static string GetEnv(string name, string fallback = "") {
        return Environment.GetEnvironmentVariable(name) ?? fallback;
    }
    public static void SetEnv(string name, string value) {
        Environment.SetEnvironmentVariable(name, value);
    }
    public static string SafeBaseName(string baseName) {
        return UniversalTransformer.SafeBaseName(baseName, followRecommendation: false);
    }
    public static string HomeFile(params string[] relatives) {
        string home = GetEnv("HOME", "");
        if (home == "") {
            home = ProfilePath();
        }
        string result = home;
        foreach (string x in relatives) {
            string relative = SafeBaseName(x);
            result = Path.Combine(result, relative);
        }
        PrepareForFile(result);
        return result;
    }
    public static string HomeFolder(params string[] relatives) {
        string home = GetEnv("HOME", "");
        if (home == "") {
            home = ProfilePath();
        }
        string result = home;
        foreach (string x in relatives) {
            string relative = SafeBaseName(x);
            result = Path.Combine(result, relative);
        }
        Prepare(result);
        return result;
    }
    public static string? GitProjectFile(string startDir, params string[] relatives) {
        string? root = FindGitRoot(startDir);
        if (root == null) {
            return null;
        }
        string result = root;
        foreach (string x in relatives) {
            string relative = SafeBaseName(x);
            result = Path.Combine(result, relative);
        }
        PrepareForFile(result);
        return result;
    }
    public static string? GitProjectFolder(string startDir, params string[] relatives) {
        string? root = FindGitRoot(startDir);
        if (root == null) {
            return null;
        }
        string result = root;
        foreach (string x in relatives) {
            string relative = SafeBaseName(x);
            result = Path.Combine(result, relative);
        }
        Prepare(result);
        return result;
    }
    public static async Task<string> GetResponseString(
        string baseUrl,
        Dictionary<string, string>? queryParameters
    ) {
        if (queryParameters == null) {
            queryParameters = [];
        }
        HttpClient httpClient = new HttpClient();
        HttpResponseMessage response = await httpClient.GetAsync(
            $"{baseUrl}?{await new FormUrlEncodedContent(queryParameters).ReadAsStringAsync()}"
        );
        string contents = await response.Content.ReadAsStringAsync();
        return contents;
    }
    public static void Sleep(int milliseconds) {
        Thread.Sleep(milliseconds);
    }
    public static void SaveAllLines(
        string path, IEnumerable<string> lines, string separator = "\n") {
        using StreamWriter writer = new StreamWriter(path);
        foreach (string line in lines) {
            writer.Write(line);
            writer.Write(separator);
        }
    }
    public static void SaveAllText(string path, string text) {
        List<string> lines = TextToLines(text);
        SaveAllLines(path, lines, "\n");
    }
#if USE_EASY_OBJECT
        public static void DumpObjectAsJson(
            object? x,
            bool compact = false,
            string newline = "\n",
            bool keyAsSymbol = false,
            bool removeSurrogatePair = false) {
            string json = EasyObject.FromObject(x)
                .ToJson(
                indent: !compact,
                keyAsSymbol: keyAsSymbol,
                removeSurrogatePair: removeSurrogatePair
                );
            Console.Write(json + newline);
        }
        public static void Crash(object? message = null, int exitCode = 1) {
            EasyObject.Abort(message, exitCode);
        }
#endif
}