using System;
using System.IO;
using System.Net;
namespace Global;

#if GLOBAL_SYS
class PartialHTTPStream : Stream, IDisposable {
#else
class EasyPartialHTTPStream : Stream, IDisposable {
#endif
    Stream? stream = null;
    WebResponse? resp = null;
    public string Url {
        get; private set;
    }
    public override bool CanRead {
        get { return true; }
    }
    public override bool CanWrite {
        get { return false; }
    }
    public override bool CanSeek {
        get { return true; }
    }
    long position = 0;
    public override long Position {
        get { return position; }
        set {
            long len = Length;
            if (value < 0 || value > len) {
                throw new ArgumentException($"Position out of range: {value}");
            }
            position = value;
        }
    }
    long? length;
    public override long Length {
        get {
            if (length == null) {
                HttpWebRequest? req = null;
                try {
#pragma warning disable SYSLIB0014
                    req = HttpWebRequest.CreateHttp(Url);
#pragma warning restore SYSLIB0014
                    req.Method = "HEAD";
                    req.AllowAutoRedirect = true;
                    length = req.GetResponse().ContentLength;
                } finally {
                    if (req != null) {
                        // 連続呼び出しでエラーになる場合があるのでその対策
                        req.Abort();
                    }
                }
            }
            return length.Value;
        }
    }
#if GLOBAL_SYS
    public PartialHTTPStream(string url) {
#else
    public EasyPartialHTTPStream(string url) {
#endif
        var m = EasySystem.FindFirstMatch(
            url,
            @"^(https://github[.]com/[^/]+/[^/]+/)blob(/.+)$",
            @"^(https://gitlab[.]com/nuget-tools/nuget-assets/-/)blob(/.+)$"
            );
        if (m != null) {
            url = m[1] + "raw" + m[2];
        }
        Url = url;
    }
    public override void SetLength(long value) {
        throw new NotImplementedException();
    }
    public override int Read(byte[] buffer, int offset, int count) {
        if (count <= 0) {
            return 0;
        }

        HttpWebRequest? req = null;
        try {
#pragma warning disable SYSLIB0014
            req = HttpWebRequest.CreateHttp(Url);
#pragma warning restore SYSLIB0014
            req.AddRange(Position);
            req.AllowAutoRedirect = true;
            resp = req.GetResponse();
            int rest = count;
            int nread = 0;
            using (Stream stream = resp.GetResponseStream()) {
                while (true) {
                    int len = stream.Read(buffer, offset, rest);
                    if (len == 0) {
                        break;
                    }

                    nread += len;
                    offset += len;
                    rest -= len;
                }
            }
            Position += nread;
            return nread;
        } finally {
            if (req != null) {
                // 連続呼び出しでエラーになる場合があるのでその対策
                req.Abort();
            }
        }
    }
    public override void Write(byte[] buffer, int offset, int count) {
        throw new NotImplementedException();
    }
    public override long Seek(long pos, SeekOrigin origin) {
        switch (origin) {
            case SeekOrigin.End:
                Position = Length + pos;
                break;
            case SeekOrigin.Begin:
                Position = pos;
                break;
            case SeekOrigin.Current:
                Position += pos;
                break;
        }
        return Position;
    }
    public override void Flush() {
    }
    protected override void Dispose(bool disposing) {
        if (stream != null) {
            stream.Dispose();
            stream = null;
        }
        if (resp != null) {
            resp.Dispose();
            resp = null;
        }
    }
}
