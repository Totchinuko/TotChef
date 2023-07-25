using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tot
{
    public struct CommandCode
    {
        public int code;
        public string message;

        public const int UnknownError = 1;
        public const int MissingArgument = 10;
        public const int FileNotFound = 100;
        public const int FileLocked = 101;
        public const int DirectoryNotFound = 102;
        public const int DevKitPathInvalid = 200;
        public const int ModNameIsInvalid = 201;
        public const int RepositoryIsDirty = 202;
        public const int RepositoryWrongBranch = 203;
        public const int CookingFailure = 204;

        public static CommandCode Success(string message) => new CommandCode { code = 0, message = message };
        public static CommandCode Success() => new CommandCode { code = 0, message = "" };
        public static CommandCode Unknown() => new CommandCode { code = 0, message = "Internal Error" };
        public static CommandCode NotFound(DirectoryInfo directory) => new CommandCode { code = DirectoryNotFound, message = $"Directory not found: {directory.FullName}" };
        public static CommandCode NotFound(FileInfo file) => new CommandCode { code = FileNotFound, message = $"File not found: {file.FullName}" };
        public static CommandCode Forbidden(FileInfo file) => new CommandCode { code = FileNotFound, message = $"File cannot be accessed: {file.FullName}" };
        public static CommandCode MissingArg(string name) => new CommandCode { code = MissingArgument, message = $"Missing argument {name}" };
    }
}
