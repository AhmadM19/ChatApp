namespace ChatApp.Exceptions
{
    public class DuplicateProfileException:Exception
    {
        public DuplicateProfileException(string? username)
    : base($"profile with username {username} already exists")
        {
        }
    }
}
