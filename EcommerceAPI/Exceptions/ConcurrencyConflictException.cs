namespace EcommerceAPI.Exceptions
{
    public class ConcurrencyConflictException:ApiException
    {
        public ConcurrencyConflictException(string message) : base(StatusCodes.Status409Conflict,message)
        {

        }

        public ConcurrencyConflictException(string message, Exception innerException) : base(StatusCodes.Status409Conflict, message,innerException)
        {

        }
    }
}
