using _91AppTest.Model;
using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace _91AppTest.Access
{
    public class FileSystemAccess
    {
        private readonly IConfiguration _config;

        public FileSystemAccess(IConfiguration config)
        {
            _config = config;
        }

        public IEnumerable<DirectoryFileInfo> GetFilesInDirectory(string directoryName)
        {
            var sql = @"DECLARE @DirectoryId hierarchyid  
SELECT @DirectoryId = TreeId FROM FileDirectories
  WHERE DirectoryName = @DirectoryName

SELECT T1.Id as DirectoryId,T1.DirectoryName,T1.DirectoryLevel,T2.Name as FileName,
CAST(TreeId AS nvarchar(100)) as TreePath,T2.FileSize,
T2.CreateDateTime,T2.UpdateDateTime FROM FileDirectories T1
left join Files T2 on T1.Id = T2.FK_DirectoryId
WHERE TreeId.IsDescendantOf(@DirectoryId) = 1";
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                var result = conn.Query<DirectoryFileInfo>(sql,
                    new 
                    {
                        @DirectoryName = directoryName
                    });
                return result;
            }
        }

        public Dictionary<string, IEnumerable<DirectoryFileInfo>> GetMovedDirectories(string orgDir, string targetDir)
        {
            var result = new Dictionary<string, IEnumerable<DirectoryFileInfo>>();
            var sql = @"DECLARE @DirectoryId hierarchyid  
SELECT @DirectoryId = TreeId FROM FileDirectories   
  WHERE DirectoryName = @DirectoryName

SELECT T1.Id as DirectoryId,T1.DirectoryName,
CAST(TreeId AS nvarchar(100)) as TreePath FROM FileDirectories T1
left join Files T2 on T1.Id = T2.FK_DirectoryId
WHERE TreeId.IsDescendantOf(@DirectoryId) = 1";
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                var filesInOrgDir = conn.Query<DirectoryFileInfo>(sql,
                    new
                    {
                        @DirectoryName = orgDir
                    });
                var filesInTargetDir = conn.Query<DirectoryFileInfo>(sql,
                    new
                    {
                        @DirectoryName = targetDir
                    });
                
                result.Add(orgDir, filesInOrgDir);
                result.Add(targetDir, filesInTargetDir);
                return result;
            }
        }


        public void UpdateDirHierarchy(List<UpdateDirectoryModel> updateDirectoryModels)
        {
            var updateDirSql = @"update FileDirectories
                    set TreeId = @TreeId,
                    DirectoryName = @DirectoryName
                    where Id = @DirectoryId";
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                var result = conn.Execute(updateDirSql,
                    updateDirectoryModels.Select(x=> new
                    {
                        TreeId = x.TreePath,
                        DirectoryName = x.DirectoryName,
                        DirectoryId = x.DirectoryId
                    }));
            }
        }

        public int DeleteDirWithFiles(string dirName)
        {
            var deleteDirWithFileSql = @"
DECLARE @DirectoryId hierarchyid  
SELECT @DirectoryId = TreeId FROM FileDirectories
  WHERE DirectoryName = @DirectoryName
delete from Files
where FK_DirectoryId in (
    select Id from FileDirectories
    WHERE TreeId.IsDescendantOf(@DirectoryId) = 1
)

delete from FileDirectories
where Id in (
    select Id from FileDirectories
    WHERE TreeId.IsDescendantOf(@DirectoryId) = 1
)";
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                return conn.Execute(deleteDirWithFileSql,new { @DirectoryName = dirName });
            }
        }

    }
}
