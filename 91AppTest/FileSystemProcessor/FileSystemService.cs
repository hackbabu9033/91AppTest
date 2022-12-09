using _91AppTest.Access;
using _91AppTest.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _91AppTest.FileSystemProcessor
{
    public class FileSystemService
    {
        private readonly FileSystemAccess _fileSystemAccess;
        public FileSystemService(FileSystemAccess fileSystemAccess)
        {
            _fileSystemAccess = fileSystemAccess;
        }

        public FileDirectoryTreeNode GetFileDirectories(string directoryName)
        {
            var fileContents = _fileSystemAccess.GetFilesInDirectory(directoryName);
            FileDirectoryTreeNode treeNode = ConvertFilesToTreeNode(fileContents);
            return treeNode;
        }

        public void MoveDirectory(string movedDirName, string targetDirName)
        {
            if (string.IsNullOrWhiteSpace(movedDirName) || string.IsNullOrWhiteSpace(targetDirName))
            {
                throw new ArgumentNullException();
            }
            var directories = _fileSystemAccess.GetMovedDirectories(movedDirName, targetDirName);
            if (directories[movedDirName] == null || !directories[movedDirName].Any())
                throw new Exception("org directory not exist");
            if (directories[targetDirName] == null || !directories[targetDirName].Any())
                throw new Exception("target directory not exist");

            var DirTreeNode = ConvertFilesToTreeNode(directories[movedDirName]);
            var targetDirTreeNode = ConvertFilesToTreeNode(directories[targetDirName]);
            if (DirTreeNode.TreePath == targetDirTreeNode.TreePath)
            {
                return;
            }
            if (targetDirTreeNode.TreePath.Contains(DirTreeNode.TreePath))
            {
                throw new InvalidOperationException("unable move top layer dir into its child layer");
            }
            MoveDirIntoTarget(DirTreeNode, targetDirTreeNode);
            var updateModels = FlattenDiretoryTreeToList(DirTreeNode);
            _fileSystemAccess.UpdateDirHierarchy(updateModels);
        }

        public void DeleteDirectory(string directoryName)
        {
            _fileSystemAccess.DeleteDirWithFiles(directoryName);
        }

        private List<UpdateDirectoryModel> FlattenDiretoryTreeToList(
            FileDirectoryTreeNode dirTreeNode)
        {
            var result = new List<UpdateDirectoryModel>();
            var queue = new Queue<FileDirectoryTreeNode>();
            queue.Enqueue(dirTreeNode);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                result.Add(new UpdateDirectoryModel()
                {
                    DirectoryId = node.DirectoryId,
                    DirectoryName = node.DirectoryName,
                    TreePath = node.TreePath
                });
                if (node.ChildDirs == null)
                {
                    continue;
                }
                foreach (var dir in node.ChildDirs)
                {
                    queue.Enqueue(dir);
                }
            }
            return result;
        }

        private FileDirectoryTreeNode ConvertFilesToTreeNode(IEnumerable<DirectoryFileInfo> fileContents)
        {
            var a = fileContents
                            .OrderBy(x => x.DirectoryLevel).ThenBy(x => x.TreePath)
                            .GroupBy(x => new { x.TreePath, x.DirectoryLevel });
            FileDirectoryTreeNode treeNode = null;
            foreach (var item in a)
            {
                var node = new FileDirectoryTreeNode()
                {
                    DirectoryId = item.FirstOrDefault().DirectoryId,
                    DirectoryName = item.FirstOrDefault().DirectoryName,
                    TreePath = item.Key.TreePath,
                    DirectoryLevel = item.Key.DirectoryLevel,
                    FileInfos = item.Where(x => !string.IsNullOrWhiteSpace(x.FileName))
                    .Select(x => new FileInfo()
                    {
                        FileName = x.FileName,
                        FileSize = x.FileSize,
                        CreateDateTime = x.CreateDateTime,
                        UpdateDateTime = x.UpdateDateTime,
                    }).ToList()
                };

                AddNode(ref treeNode, node);
            }

            return treeNode;
        }

        private void AddNode(ref FileDirectoryTreeNode rootNode,
            FileDirectoryTreeNode insertedNode)
        {
            if (rootNode == null)
            {
                rootNode = insertedNode;
                return;
            }

            var queue = new Queue<FileDirectoryTreeNode>();
            queue.Enqueue(rootNode);
            var insertedNodePath = insertedNode.TreePath;
            string curPath;
            while (queue.Count > 0)
            {
                var popup = queue.Dequeue();
                var children = popup.ChildDirs;
                if (children== null || !children.Any())
                {
                    popup.ChildDirs = new List<FileDirectoryTreeNode>() { insertedNode };
                    break;
                }
                var ancestorNode = children.Where(x => insertedNodePath.StartsWith(x.TreePath))
                                   .FirstOrDefault();
                if (ancestorNode == null)
                {
                    popup.ChildDirs.Add(insertedNode);
                    break;
                }
                queue.Enqueue(ancestorNode);
            }
        }

        private void MoveDirIntoTarget(FileDirectoryTreeNode updatedNode, 
            FileDirectoryTreeNode targetNode) 
        {
            string newNodePath;
            if (targetNode.ChildDirs == null || !targetNode.ChildDirs.Any())
            {
                newNodePath = $"{targetNode.TreePath}1/";
            }
            else
            {
                var childDirNamesInTarget = targetNode.ChildDirs.Select(x => x.DirectoryName);
                updatedNode.DirectoryName = GetNewValidDirName(childDirNamesInTarget, updatedNode.DirectoryName);
                newNodePath = $"{targetNode.TreePath}{targetNode.ChildDirs.Count + 1}/";
            }
            UpdateNodePath(newNodePath, updatedNode);
        }

        private string GetNewValidDirName(IEnumerable<string> dirNames,string updatedDirName)
        {
            int flag = 1;
            string result = updatedDirName;
            while (dirNames.Any(x=>x == result))
            {
                result = $"{updatedDirName}({flag++})";
            }
            return updatedDirName;
        }

        private void UpdateNodePath(string nodePath, FileDirectoryTreeNode updatedNode)
        {
            updatedNode.TreePath = nodePath;
            if (updatedNode.ChildDirs == null || !updatedNode.ChildDirs.Any())
            {
                return;
            }
            for (int i = 0; i < updatedNode.ChildDirs.Count; i++)
            {
                UpdateNodePath($"{nodePath}{i+1}/", updatedNode.ChildDirs[i]);
            }
        }
    }

}
