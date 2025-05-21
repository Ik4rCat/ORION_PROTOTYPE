using System;
using System.Collections.Generic;

namespace Orion.Backend.Models
{
    [Serializable]
    public class GitCommit
    {
        public string Id { get; set; }
        public string SHA { get; set; }
        public string Message { get; set; }
        public string AuthorName { get; set; }
        public string AuthorEmail { get; set; }
        public DateTime CommitDate { get; set; }
        public string BranchName { get; set; }
        public string TaskItemId { get; set; }

        public GitCommit()
        {
            Id = Guid.NewGuid().ToString();
        }

        public GitCommit(string sha, string message, string authorName, string authorEmail, 
            DateTime commitDate, string branchName, string taskItemId)
        {
            Id = Guid.NewGuid().ToString();
            SHA = sha;
            Message = message;
            AuthorName = authorName;
            AuthorEmail = authorEmail;
            CommitDate = commitDate;
            BranchName = branchName;
            TaskItemId = taskItemId;
        }
    }

    [Serializable]
    public class GitRepo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string ProjectId { get; set; }
        public List<string> BranchNames { get; set; }
        public DateTime LastSyncTime { get; set; }

        public GitRepo()
        {
            Id = Guid.NewGuid().ToString();
            BranchNames = new List<string>();
            LastSyncTime = DateTime.UtcNow;
        }

        public GitRepo(string name, string url, string projectId)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            Url = url;
            ProjectId = projectId;
            BranchNames = new List<string>();
            LastSyncTime = DateTime.UtcNow;
        }
    }
} 