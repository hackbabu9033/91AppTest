using System;
using System.Collections.Generic;
using System.Text;

namespace _91AppTest.Model
{
    public class FileDirectoryTreeNode
    {
        public Guid DirectoryId { get; set; }

        public string DirectoryName { get; set; }
        public string TreePath { get; set; }
        public int DirectoryLevel { get; set; }
        public List<FileInfo> FileInfos { get; set; }

        public List<FileDirectoryTreeNode> ChildDirs { get; set; } 
    }
}
