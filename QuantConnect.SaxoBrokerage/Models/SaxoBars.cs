/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using Newtonsoft.Json;
using System;

namespace QuantConnect.Brokerages.Saxo.Models;

public readonly struct SaxoBars
{
    public readonly SaxoChartInfo ChartInfo { get; }

    public readonly SaxoChartSample[] Data { get; }

    public readonly int DataVersion { get; }

    public readonly DisplayAndFormat DisplayAndFormat { get; }

   [JsonConstructor]
    public SaxoBars(SaxoChartInfo chartInfo, SaxoChartSample[] data, int dataVersion, DisplayAndFormat displayAndFormat)
    {
        ChartInfo = chartInfo;
        Data = data;
        DataVersion = dataVersion;
        DisplayAndFormat = displayAndFormat;
    }
}

public readonly struct SaxoChartInfo
{
    public readonly int DelayedByMinutes { get; }
    public readonly string ExchangeId { get; }
    public readonly DateTime FirstSampleTime { get; }
    public readonly int Horizon { get; }

    [JsonConstructor]
    public SaxoChartInfo(int delayedByMinutes, string exchangeId, DateTime firstSampleTime, int horizon)
    {
        DelayedByMinutes = delayedByMinutes;
        ExchangeId = exchangeId;
        FirstSampleTime = firstSampleTime;
        Horizon = horizon;
    }
}

public readonly struct SaxoChartSample
{
    public readonly DateTime Time { get; }
    public readonly decimal Close { get; }
    public readonly decimal CloseAsk { get; }
    public readonly decimal CloseBid { get; }
    public readonly decimal Growth { get; }
    public readonly decimal High { get; }
    public readonly decimal HighAsk { get; }
    public readonly decimal HighBid { get; }
    public readonly decimal Interest { get; }
    public readonly decimal Low { get; }
    public readonly decimal LowAsk { get; }
    public readonly decimal LowBid { get; }
    public readonly decimal Open { get; }
    public readonly decimal OpenAsk { get; }
    public readonly decimal OpenBid { get; }
    public readonly decimal Volume { get; }
    [JsonConstructor]
    public SaxoChartSample(DateTime time, decimal close, decimal closeAsk, decimal closeBid, decimal growth, decimal high, decimal highAsk, decimal highBid,
        decimal interest, decimal low, decimal lowAsk, decimal lowBid, decimal open, decimal openAsk, decimal openBid, decimal volume)
    {
        Time = time;
        Close = close;
        CloseAsk = closeAsk;
        CloseBid = closeBid;
        Growth = growth;
        High = high;
        HighAsk = highAsk;
        HighBid = highBid;
        Interest = interest;
        Low = low;
        LowAsk = lowAsk;
        LowBid = lowBid;
        Open = open;
        OpenAsk = openAsk;
        OpenBid = openBid;
        Volume = volume;
    }
}

public readonly struct DisplayAndFormat
{
    public readonly string Currency { get; }
    public readonly int Decimals { get; }
    public readonly string Description { get; }
    public readonly string Format { get; }
    public readonly int NumeratorDecimals { get; }
    public readonly string Symbol { get; }

    [JsonConstructor]
    public DisplayAndFormat(string currency, int decimals, string description, string format, int numeratorDecimals, string symbol)
    {
        Currency = currency;
        Decimals = decimals;
        Description = description;
        Format = format;
        NumeratorDecimals = numeratorDecimals;
        Symbol = symbol;
    }
}