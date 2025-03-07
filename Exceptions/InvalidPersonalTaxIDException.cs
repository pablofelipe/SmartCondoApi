namespace SmartCondoApi.Exceptions
{
    public class InvalidPersonalTaxIDException : Exception
    {
        public InvalidPersonalTaxIDException(string message) : base(message) { }
    }
}
