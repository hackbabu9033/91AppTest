using _91AppTest.Access;
using _91AppTest.FileSystemProcessor;
using _91AppTest.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Equivalency;
using System.Transactions;

namespace FileSystem.Test
{
    public class TestsFileServiceTest
    {
        private static ServiceProvider Container;
        [SetUp]
        public void Setup()
        {
            IConfiguration config = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json")
           .Build();
           
            Container = new ServiceCollection()
           .AddSingleton(config)
           .AddScoped<FileSystemAccess>()
           .AddScoped<FileSystemService>()
           .BuildServiceProvider();
        }

        [TestCaseSource(nameof(GetDirIntoTreeNode_TestCase))]
        public void GetFileDirectories(string dirName, FileDirectoryTreeNode expected)
        {
            var sut = Container.GetService<FileSystemService>();
            var expectedJson = JsonConvert.SerializeObject(expected);
            
            var result = sut.GetFileDirectories(dirName);
            var json = JsonConvert.SerializeObject(result);
            
            json.Should().BeEquivalentTo(expectedJson);
        }

        private static IEnumerable<object[]> GetDirIntoTreeNode_TestCase
        {
            get
            {
                return new[]
                {
                    new object[]
                    {
                        "Program Files",
                        new FileDirectoryTreeNode()
                        {
                            DirectoryId = new Guid("FE0323EB-E71D-498D-A315-B8F35BED8EED"),
                            DirectoryName = "Program Files",
                            TreePath = "/1/",
                            DirectoryLevel = 1,
                            FileInfos = new List<FileInfo>(),
                            ChildDirs = new List<FileDirectoryTreeNode>()
                            { 
                                new FileDirectoryTreeNode()
                                {
                                    DirectoryId = new Guid("EFA92076-E257-44AC-8C1D-85D12610A33D"),
                                    DirectoryName = "Azure Data Studio",
                                    TreePath = "/1/1/",
                                    DirectoryLevel = 2,
                                    FileInfos = new List<FileInfo>()
                                    { 
                                        new FileInfo()
                                        {
                                            FileName="chrome_100_percent.pak",
                                            FileSize = 10000,
                                            CreateDateTime = DateTimeOffset.Parse("2022-12-06 22:53:35.3000000 +00:00"),
                                            UpdateDateTime = DateTimeOffset.Parse("2022-12-06 22:53:35.3000000 +00:00"),
                                        },
                                        new FileInfo()
                                        {
                                            FileName="d3dcompiler_47.dll",
                                            FileSize = 2000,
                                            CreateDateTime = DateTimeOffset.Parse("2022-12-06 22:53:35.3033333 +00:00"),
                                            UpdateDateTime = DateTimeOffset.Parse("2022-12-06 22:53:35.3033333 +00:00"),
                                        },
                                        new FileInfo()
                                        {
                                            FileName="d3dcompiler_40.dll",
                                            FileSize = 2000,
                                            CreateDateTime = DateTimeOffset.Parse("2022-12-06 22:53:35.3033333 +00:00"),
                                            UpdateDateTime = DateTimeOffset.Parse("2022-12-06 22:53:35.3033333 +00:00"),
                                        }
                                    }
                                },
                                new FileDirectoryTreeNode()
                                {
                                    DirectoryId = new Guid("D9A56879-85F9-4284-ACDC-A034E80C1749"),
                                    DirectoryName = "CMake",
                                    TreePath = "/1/2/",
                                    DirectoryLevel = 2,
                                    FileInfos = new List<FileInfo>()
                                },
                                new FileDirectoryTreeNode()
                                {
                                    DirectoryId = new Guid("FA0ED713-92FD-4915-87E7-53956E591997"),
                                    DirectoryName = "Git",
                                    TreePath = "/1/3/",
                                    DirectoryLevel = 2,
                                    FileInfos = new List<FileInfo>()
                                    {
                                        new FileInfo()
                                        {
                                            FileName="git-bash.exe",
                                            FileSize = 10000,
                                            CreateDateTime = DateTimeOffset.Parse("2022-12-06 22:53:35.3033333 +00:00"),
                                            UpdateDateTime = DateTimeOffset.Parse("2022-12-06 22:53:35.3033333 +00:00"),
                                        },
                                        new FileInfo()
                                        {
                                            FileName="git-cmd.exe",
                                            FileSize = 10000,
                                            CreateDateTime = DateTimeOffset.Parse("2022-12-06 22:53:35.3066667 +00:00"),
                                            UpdateDateTime = DateTimeOffset.Parse("2022-12-06 22:53:35.3066667 +00:00"),
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new object[]
                    { 
                        "test123",
                        null
                    }

                };
            }
        }

        [TestCaseSource(nameof(MoveDirectory_TestCase))]
        public void MoveDirectory(string dirName,string targetDirName,
            IEnumerable<DirectoryFileInfo> expected)
        {
            var sut = Container.GetService<FileSystemService>();
            var access = Container.GetService<FileSystemAccess>();
            
            using (var scope = new TransactionScope())
            {
                sut.MoveDirectory(dirName, targetDirName);
                var result = access.GetFilesInDirectory(dirName);
                
                result.Should().BeEquivalentTo(expected);
            }
        }

        private static IEnumerable<object[]> MoveDirectory_TestCase
        {
            get
            {
                return new[]
                {
                    new object[]
                    {
                        "sqllite",
                        "Program Files",
                        new List<DirectoryFileInfo>()
                        { 
                            new DirectoryFileInfo()
                            { 
                                DirectoryId = new Guid("DD5ECEC3-4BFA-4050-B6B6-BAC641CD9E29"),
                                DirectoryName = "sqllite",
                                FileName = "sqldiff.exe",
                                FileSize = 500,
                                TreePath = "/1/4/",
                                DirectoryLevel = 2,
                                CreateDateTime = DateTimeOffset.Parse("2022-12-06 22:53:35.2933333 +00:00"),
                                UpdateDateTime = DateTimeOffset.Parse("2022-12-06 22:53:35.2933333 +00:00")
                            },
                            new DirectoryFileInfo()
                            {
                                DirectoryId = new Guid("DD5ECEC3-4BFA-4050-B6B6-BAC641CD9E29"),
                                DirectoryName = "sqllite",
                                TreePath = "/1/4/",
                                DirectoryLevel = 2,
                                FileName="sqlite3.def",
                                FileSize = 100,
                                CreateDateTime = DateTimeOffset.Parse("2022-12-06 22:53:35.2966667 +00:00"),
                                UpdateDateTime = DateTimeOffset.Parse("2022-12-06 22:53:35.2966667 +00:00")
                            },
                            new DirectoryFileInfo()
                            {
                                DirectoryId = new Guid("DD5ECEC3-4BFA-4050-B6B6-BAC641CD9E29"),
                                DirectoryName = "sqllite",
                                TreePath = "/1/4/",
                                DirectoryLevel = 2,
                                FileName="sqlite3.dll",
                                FileSize = 2000,
                                CreateDateTime = DateTimeOffset.Parse("2022-12-06 22:53:35.2966667 +00:00"),
                                UpdateDateTime = DateTimeOffset.Parse("2022-12-06 22:53:35.2966667 +00:00")
                            },
                            new DirectoryFileInfo()
                            {
                                DirectoryId = new Guid("DD5ECEC3-4BFA-4050-B6B6-BAC641CD9E29"),
                                DirectoryName = "sqllite",
                                TreePath = "/1/4/",
                                DirectoryLevel = 2,
                                FileName="sqlite3.exe",
                                FileSize = 2000,
                                CreateDateTime = DateTimeOffset.Parse("2022-12-06 22:53:35.2966667 +00:00"),
                                UpdateDateTime = DateTimeOffset.Parse("2022-12-06 22:53:35.2966667 +00:00")
                            }
                        }
                    }
                };
            }
        }

        [TestCase("Git", "test123", "target directory not exist")]
        [TestCase("temp", "sqllite", "org directory not exist")]
        public void MoveDirectory_IfDirNotExist_ShouldThrowException(string dirName,
            string targetDirName,
            string exceptionMsg)
        {
            var sut = Container.GetService<FileSystemService>();

            Action act = () => sut.MoveDirectory(dirName, targetDirName);

            act.Should().Throw<Exception>().WithMessage(exceptionMsg);
        }

        [TestCase("Program Files", "Azure Data Studio")]
        [TestCase("C", "Program Files")]
        public void MoveDirectory_IfTargetIsInChild_ShouldThrowException(string dirName,
            string targetDirName)
        {
            var sut = Container.GetService<FileSystemService>();

            Action act = () => sut.MoveDirectory(dirName, targetDirName);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("unable move top layer dir into its child layer");
        }

        [TestCase("Program Files")]
        public void DeleteDirectory(string dirName)
        {
            var sut = Container.GetService<FileSystemService>();
            var access = Container.GetService<FileSystemAccess>();

            using (var scope = new TransactionScope())
            {
                sut.DeleteDirectory(dirName);
                var result = access.GetFilesInDirectory(dirName);

                result.Should().BeEmpty();
            }
        }
    }
}