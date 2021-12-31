using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects;
using Kucoin.Net;
using Kucoin.Net.Objects;

namespace candle_maker
{
    class Program
    {
        static void Main() { MainAsync().GetAwaiter().GetResult(); }

        static async Task MainAsync()
        {
            var sc = new KucoinSocketClient(new KucoinSocketClientOptions()
            {
                ApiCredentials = new KucoinApiCredentials("xxx", "xxx", "xxx"),
                AutoReconnect = true,
            });

            Dictionary<ulong, Quote> candles = new Dictionary<ulong, Quote>();
            ulong minsPrev = 0;
            var BTC = await sc.Spot.SubscribeToTickerUpdatesAsync("BTC-USDT", data => {
                decimal price = (decimal)data.Data.LastTradePrice;
                decimal vol = (decimal)data.Data.LastTradeQuantity;
                ulong mins = (ulong)(DateTime.UtcNow - new DateTime(2001, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalMinutes;
                if (!candles.ContainsKey(mins))
                {
                    candles.Add(mins, new Quote() { Date = DateTime.Now, Open = price, High = price, Low = price, Close = price, Volume = vol });
                }
                else
                {
                    if (price > candles[mins].High) candles[mins].High = price;
                    if (price < candles[mins].Low) candles[mins].Low = price;
                    candles[mins].Volume += vol;
                    candles[mins].Close = price;
                }

                //we completed a candle so show it
                if (minsPrev != mins && minsPrev != 0)
                {
                    Quote candle = candles[minsPrev];
                    Console.WriteLine($"D: {candle.Date.ToShortTimeString()}, O: {candle.Open}, H: {candle.High}, L: {candle.Low}, C: {candle.Close}, V: {candle.Volume}");
                }
                minsPrev = mins;
            });

            Console.ReadLine();
        }
    }

    public class Quote
    {
        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }
    }
}
