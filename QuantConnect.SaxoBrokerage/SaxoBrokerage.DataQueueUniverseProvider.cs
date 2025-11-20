using System.Collections.Generic;
using QuantConnect.Brokerages.Saxo.Models.Enums;

namespace QuantConnect.Brokerages.Saxo;

/// <summary>
/// Represents a TradeStation route that contains a collection of routes for placing orders.
/// </summary>
public readonly struct SaxoRoute
{
    /// <summary>
    /// Gets the collection of routes associated with this TradeStation route.
    /// </summary>
    public IReadOnlyList<Route> Routes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TradeStationRoute"/> struct with the specified routes.
    /// </summary>
    /// <param name="routes">The collection of routes to be associated with this TradeStation route.</param>
    public SaxoRoute(IReadOnlyList<Route> routes) => Routes = routes;
}

/// <summary>
/// Represents a route used in TradeStation for placing orders.
/// </summary>
public readonly struct Route
{
    /// <summary>
    /// The ID that must be sent in the optional Route property of a POST order request, when specifying a route for an order.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// The asset type of the route. Valid Values are: STOCK, FUTURE, STOCKOPTION, and INDEXOPTION.
    /// </summary>
    public IReadOnlyList<SaxoAssetType> AssetTypes { get; }

    /// <summary>
    /// The name of the route.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Route"/> struct with the specified route ID, asset types, and name.
    /// </summary>
    /// <param name="id">The ID of the route.</param>
    /// <param name="assetTypes">The asset types associated with this route.</param>
    /// <param name="name">The name of the route.</param>
    public Route(string id, IReadOnlyList<SaxoAssetType> assetTypes, string name) => (Id, AssetTypes, Name) = (id, assetTypes, name);
}

