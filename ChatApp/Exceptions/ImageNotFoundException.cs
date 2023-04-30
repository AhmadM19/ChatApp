namespace ChatApp.Exceptions
{
    public class ImageNotFoundException : Exception
    {
        public ImageNotFoundException(string id):
            base($"Image with {id} doesn't exist")
        {
        }
    }
}
