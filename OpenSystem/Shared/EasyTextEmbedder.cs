using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
namespace Global;

#if GLOBAL_SYS
public static class TextEmbedder {
#else
public static class EasyTextEmbedder {
#endif
    //const long MinimumCheckLength = 8192;
    const long MinimumCheckLength = 512;
#if GLOBAL_SYS
    static TextEmbedder() {
#else
    static EasyTextEmbedder() {
#endif
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
    internal class SearchResult {
        public long Length {
            get; set;
        }
        public long StartPos {
            get; set;
        }
        public long EndPos {
            get; set;
        }
    }
    private static long GetLength(string path) {
        try {
            if (path.StartsWith("http:") || path.StartsWith("https:")) {
#if GLOBAL_SYS
                using (var fs = new PartialHTTPStream(path)) {
#else
                using (var fs = new EasyPartialHTTPStream(path)) {
#endif
                    return fs.Length;
                }
            }
            using (var fs = File.OpenRead(path)) {
                return fs.Length;
            }
        } catch {
            return 0;
        }
    }
    private static SearchResult CheckTailBytes(long offset, byte[] bytes) {
        SearchResult result = new SearchResult() {
            Length = offset + bytes.Length,
            StartPos = -1,
            EndPos = -1
        };
        const string neutral = "IBM437";
        string part = Encoding.GetEncoding(neutral).GetString(bytes);
        string pattern = @"\[/embed(:[-0-9a-zA-Z]+)?\]\s*$";
        Match m = Regex.Match(part, pattern);
        if (m.Success) {
            string startTag = $"\n//[embed{m.Groups[1].Value}]";
            string endTag = $"[/embed{m.Groups[1].Value}]";
            result.EndPos = part.LastIndexOf(endTag);
            if (result.EndPos >= 0) {
                int idx = part.LastIndexOf(startTag, (int)result.EndPos);
                if (idx >= 0) {
                    result.Length = offset + idx;
                    result.StartPos = idx + startTag.Length;
                    long len = result.EndPos - result.StartPos;
                    string s = part.Substring((int)result.StartPos, (int)len);
                }
            }
        }
        return result;
    }
    public static byte[] GetHeadBytes(string path, long size) {
        if (GetLength(path) == 0) {
            return new byte[0];
        }
        if (path.StartsWith("http:") || path.StartsWith("https:")) {
#if GLOBAL_SYS
            using (var fs = new PartialHTTPStream(path)) {
#else
            using (var fs = new EasyPartialHTTPStream(path)) {
#endif
                long fileLen = fs.Length;
                if (size > fileLen) {
                    size = fileLen;
                }
                byte[] result = new byte[size];
                fs.Read(result, 0, result.Length);
                return result;
            }
        }
        using (var fs = File.OpenRead(path)) {
            long fileLen = fs.Length;
            if (size > fileLen) {
                size = fileLen;
            }
            byte[] result = new byte[size];
            fs.Read(result, 0, result.Length);
            return result;
        }
    }
    public static byte[] GetTailBytes(string path, long size) {
        if (GetLength(path) == 0) {
            return new byte[0];
        }
        if (path.StartsWith("http:") || path.StartsWith("https:")) {
#if GLOBAL_SYS
            using (var fs = new PartialHTTPStream(path)) {
#else
            using (var fs = new EasyPartialHTTPStream(path)) {
#endif
                long fileLen = fs.Length;
                if (size > fileLen) {
                    size = fileLen;
                }
                long pos = fileLen - size;
                byte[] result = new byte[size];
                fs.Seek(pos, SeekOrigin.Begin);
                fs.Read(result, 0, result.Length);
                return result;
            }
        }
        using (var fs = File.OpenRead(path)) {
            long fileLen = fs.Length;
            if (size > fileLen) {
                size = fileLen;
            }
            long pos = fileLen - size;
            byte[] result = new byte[size];
            fs.Seek(pos, SeekOrigin.Begin);
            fs.Read(result, 0, result.Length);
            return result;
        }
    }
    public static bool HasEmbeddedText(string path) {
        return ExtractEmbeddedText(path) != null;
    }
    public static string GetRandomDigits(/*int length*/) {
        return Guid.NewGuid().ToString("D");
    }
    public static void RemoveEmbeddedText(string path) {
        if (path.StartsWith("http:") || path.StartsWith("https:")) {
            throw new InvalidOperationException("Cannot remove embedded text from a URL");
        }
        long fileLen = GetLength(path);
        if (fileLen == 0) {
            return;
        }
        long contentSize = GetOriginalContentSize(path);
        if (fileLen == contentSize) {
            return;
        }
        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Write)) {
            fs.SetLength(contentSize);
        }
    }
    public static void InjectEmbeddedText(string path, string text) {
        if (path.StartsWith("http:") || path.StartsWith("https:")) {
            throw new InvalidOperationException("Cannot inject embedded text to a URL");
        }
        if (HasEmbeddedText(path)) {
            RemoveEmbeddedText(path);
        }
        string randomDigits = GetRandomDigits();
        string embedText = $"\n//[embed:{randomDigits}]{text}[/embed:{randomDigits}]\n";
        byte[] embedBytes = Encoding.UTF8.GetBytes(embedText);
        using (var fs = new FileStream(path, FileMode.Append, FileAccess.Write)) {
            fs.Write(embedBytes, 0, embedBytes.Length);
        }
    }
    public static string? ExtractEmbeddedText(string path) {
        long fileLen = GetLength(path);
        if (fileLen == 0) {
            return null;
        }
        long checkLen = MinimumCheckLength;
        while (true) {
            if (checkLen > fileLen) {
                checkLen = fileLen;
            }
            byte[] check = GetTailBytes(path, checkLen);
            SearchResult checkResult = CheckTailBytes(fileLen - checkLen, check);
            if (checkResult.EndPos < 0) {
                return null;
            }
            if (checkResult.StartPos >= 0) {
                long len = checkResult.EndPos - checkResult.StartPos;
                byte[] result = new byte[len];
                Array.Copy(check, checkResult.StartPos, result, 0, len);
                return Encoding.UTF8.GetString(result).Trim();
            }
            if (checkLen >= fileLen) {
                return null;
            }
            checkLen *= 2;
        }
    }
    public static long GetOriginalContentSize(string path) {
        long fileLen = GetLength(path);
        if (fileLen == 0) {
            return 0;
        }
        long checkLen = MinimumCheckLength;
        while (true) {
            if (checkLen > fileLen) {
                checkLen = fileLen;
            }
            byte[] check = GetTailBytes(path, checkLen);
            SearchResult checkResult = CheckTailBytes(fileLen - checkLen, check);
            if (checkResult.EndPos < 0) {
                return checkResult.Length;
            }
            if (checkResult.StartPos >= 0) {
                return checkResult.Length;
            }
            if (checkLen >= fileLen) {
                return checkResult.Length;
            }
            checkLen *= 2;
        }
    }
    public static string? GetOriginalContentAsText(string path) {
        long size = GetOriginalContentSize(path);
        return Encoding.UTF8.GetString(GetHeadBytes(path, size));
    }
    public static byte[]? GetOriginalContentAsBytes(string path) {
        long size = GetOriginalContentSize(path);
        return GetHeadBytes(path, size);
    }
}
