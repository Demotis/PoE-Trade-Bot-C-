﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Globalization;

namespace TradeBotSharedLib.Models
{


    public partial class CurrenciesJson
    {
        [JsonProperty("lines")]
        public Line[] Lines { get; set; }

        [JsonProperty("currencyDetails")]
        public CurrencyDetail[] CurrencyDetails { get; set; }
    }

    public partial class CurrencyDetail
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("icon")]
        public Uri Icon { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("poeTradeId")]
        public long PoeTradeId { get; set; }
    }

    public partial class Line
    {
        [JsonProperty("currencyTypeName")]
        public string CurrencyTypeName { get; set; }

        [JsonProperty("pay")]
        public Receive Pay { get; set; }

        [JsonProperty("receive")]
        public Receive Receive { get; set; }

        [JsonProperty("paySparkLine")]
        public PaySparkLine PaySparkLine { get; set; }

        [JsonProperty("receiveSparkLine")]
        public ReceiveSparkLine ReceiveSparkLine { get; set; }

        [JsonProperty("chaosEquivalent")]
        public double ChaosEquivalent { get; set; }

        [JsonProperty("lowConfidencePaySparkLine")]
        public PaySparkLine LowConfidencePaySparkLine { get; set; }

        [JsonProperty("lowConfidenceReceiveSparkLine")]
        public ReceiveSparkLine LowConfidenceReceiveSparkLine { get; set; }

        [JsonProperty("detailsId")]
        public string DetailsId { get; set; }
    }

    public partial class PaySparkLine
    {
        [JsonProperty("data")]
        public double?[] Data { get; set; }

        [JsonProperty("totalChange")]
        public double TotalChange { get; set; }
    }

    public partial class ReceiveSparkLine
    {
        [JsonProperty("data")]
        public double?[] Data { get; set; }

        [JsonProperty("totalChange")]
        public double TotalChange { get; set; }
    }

    public partial class Receive
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("league_id")]
        public long LeagueId { get; set; }

        [JsonProperty("pay_currency_id")]
        public long PayCurrencyId { get; set; }

        [JsonProperty("get_currency_id")]
        public long GetCurrencyId { get; set; }

        [JsonProperty("sample_time_utc")]
        public DateTimeOffset SampleTimeUtc { get; set; }

        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }

        [JsonProperty("data_point_count")]
        public long DataPointCount { get; set; }

        [JsonProperty("includes_secondary")]
        public bool IncludesSecondary { get; set; }
    }

    public partial class CurrenciesJson
    {
        public static CurrenciesJson FromJson(string json) => JsonConvert.DeserializeObject<CurrenciesJson>(json, TradeBotSharedLib.Models.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this CurrenciesJson self) => JsonConvert.SerializeObject(self, TradeBotSharedLib.Models.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
