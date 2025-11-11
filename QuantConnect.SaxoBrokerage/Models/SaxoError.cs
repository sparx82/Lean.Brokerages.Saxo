using Newtonsoft.Json;

namespace QuantConnect.Brokerages.Saxo.Models;

public readonly struct SaxoError
{
    /// <summary>
    /// The AccountID of the error, may contain multiple Account IDs in comma separated format.
    /// </summary>
    public string AccountID { get; }

    /// <summary>
    /// The Error.
    /// </summary>
    public string Error { get; }

    /// <summary>
    /// The error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SaxoError"/> struct.
    /// </summary>
    /// <param name="accountID">The ID of the account associated with the error.</param>
    /// <param name="error">The type of the error.</param>
    /// <param name="message">The error message.</param>
    [JsonConstructor]
    public SaxoError(string accountID, string error, string message)
    {
        AccountID = accountID;
        Error = error;
        Message = message;
    }
}
