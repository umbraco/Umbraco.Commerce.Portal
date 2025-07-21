using System;

namespace Umbraco.Commerce.Portal.Exceptions;

internal class StoreDataNotFoundException : Exception
{
    private const string DefaultMessage = "Unable to get store data";

    public StoreDataNotFoundException()
        : base(DefaultMessage)
    {
    }

    public StoreDataNotFoundException(string message)
        : base(message)
    {
    }

    public StoreDataNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
