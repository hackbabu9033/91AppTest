using System;
using System.Collections.Generic;
using System.Text;

namespace _91AppTest.Model
{
    public class UpdateDirectoryModel
    {
        public Guid DirectoryId { get; set; }

        public string DirectoryName { get; set; }
        public string TreePath { get; set; }
    }
}
