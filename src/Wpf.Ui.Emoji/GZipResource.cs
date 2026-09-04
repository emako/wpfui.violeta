using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace Wpf.Ui.Emoji;

public class GZipResourceStream : StreamReader
{
    public GZipResourceStream(string name)
        : base(CreateStream(Assembly.GetCallingAssembly(), name))
    { }

    public GZipResourceStream(Assembly assembly, string name)
        : base(CreateStream(assembly, name))
    { }

    private static GZipStream CreateStream(Assembly assembly, string name)
        => new(assembly.GetManifestResourceStream(name), CompressionMode.Decompress);

    protected override void Dispose(bool disposing)
    {
        var gzip_stream = BaseStream as GZipStream;
        var resource_stream = gzip_stream?.BaseStream as Stream;

        base.Dispose(disposing);

        if (!m_disposed)
        {
            if (disposing)
            {
                gzip_stream?.Dispose();
                resource_stream?.Dispose();
            }

            m_disposed = true;
        }
    }

    private bool m_disposed = false;
}
