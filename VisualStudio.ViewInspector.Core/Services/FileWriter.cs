using System;
using System.IO;

namespace VisualStudio.ViewInspector.Core.Services
{
    class FileWriter : IDisposable
    {
        StreamWriter streamWriter;

        public FileWriter(string filePath)
        {
            streamWriter = new StreamWriter(filePath, true);
        }

        public void WriteLine(string text)
        {
            streamWriter.WriteLine(text);
        }

        public void Write(string text)
        {
            streamWriter.Write(text);
        }

        public void Dispose()
        {
            streamWriter.Dispose();
        }
    }
}

