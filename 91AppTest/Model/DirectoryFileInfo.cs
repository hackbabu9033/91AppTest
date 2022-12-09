using System;
using System.Collections.Generic;
using System.Text;

namespace _91AppTest.Model
{
    public class DirectoryFileInfo
    {
        public Guid DirectoryId { get; set; }
        public string FileName { get; set; }
        public string DirectoryName { get; set; }
        public int FileSize { get; set; }
        public DateTimeOffset CreateDateTime { get; set; }
        public DateTimeOffset UpdateDateTime { get; set; }
        public string TreePath { get; set; }
        public int DirectoryLevel { get; set; }
    }

}
