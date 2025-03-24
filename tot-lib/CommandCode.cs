namespace tot_lib;

public static class CommandCode
{
    public const int CookingFailure = 204;
    public const int DevKitPathInvalid = 200;
    public const int DirectoryNotFound = 102;
    public const int FileLocked = 101;
    public const int FileNotFound = 100;
    public const int MissingArgument = 10;
    public const int ModNameIsInvalid = 201;
    public const int RepositoryIsDirty = 202;
    public const int RepositoryWrongBranch = 203;
    public const int RepositoryInvalid = 203;
    public const int UnknownError = 1;

    public static Exception Error(string message)
    {
        return new CommandException(UnknownError, message);
    }

    public static Exception Exception(Exception ex)
    {
        return new CommandException(UnknownError, ex.Message);
    }

    public static Exception Forbidden(FileInfo file)
    {
        return new CommandException(FileNotFound, $"File cannot be accessed: {file.FullName}");
    }

    public static Exception MissingArg(string name)
    {
        return new CommandException(MissingArgument, $"Missing argument {name}");
    }

    public static Exception NotFound(DirectoryInfo directory)
    {
        return new CommandException(DirectoryNotFound, $"Directory not found: {directory.FullName}");
    }

    public static Exception NotFound(FileInfo file)
    {
        return new CommandException(FileNotFound, $"File not found: {file.FullName}");
    }

    public static Exception Unknown()
    {
        return new CommandException(UnknownError, "Internal Error");
    }
}

public class CommandException : Exception
{
    public CommandException(int code, string message) : base(message)
    {
        Code = code;
    }

    public CommandException(string message) : base(message)
    {
        Code = CommandCode.UnknownError;
    }

    public int Code { get; }
}