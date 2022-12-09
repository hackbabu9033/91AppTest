using System;
using System.Collections.Generic;
using System.Text;

namespace _91AppTest.Model
{
    public class FileInfo
    {
        public string FileName { get; set; }
        public int FileSize { get; set; }
        public DateTimeOffset CreateDateTime { get; set; }
        public DateTimeOffset UpdateDateTime { get; set; }
    }
}
