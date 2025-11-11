using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QuantConnect.Brokerages.Saxo.Models;

public readonly struct SaxoAccount
{
    public List<Account> Accounts { get; }

    [JsonConstructor]
    public SaxoAccount(List<Account> accounts)
    {
        Accounts = accounts;
    }
}

public readonly struct Account
{
    public string AccountID { get; }
    public string AccountKey { get; }
    public bool Active { get; }
    public string ClientID { get; }
    public string ClientKey { get; }
    public string Currency { get; }

    [JsonConstructor]
    public Account(string accountID, string accountKey, bool active, string clientID, string clientKey, string currency)
    {
        AccountID = accountID;
        AccountKey = accountKey;
        Active = active;
        ClientID = clientID;
        ClientKey = clientKey;
        Currency = currency;
    }
}
