namespace ChatApp.Exceptions
{
    public class ProfileNotFoundException : Exception
    {
        public ProfileNotFoundException(string? username) 
            : base($"profile with username {username} doesn't exist")
        {
        }
    }
}
