using System.Collections.Generic;

namespace QuantConnect.Brokerages.Saxo.Models.Interfaces;

public interface ISaxoError
{
    /// <summary>
    /// Gets the collection of errors associated with the positions.
    /// </summary>
    public List<SaxoError> Errors { get; }
}
