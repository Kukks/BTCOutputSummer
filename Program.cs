using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace outputsummer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ulong result = 0;
            
            if (args?.Any() is not true)
            {
                args = AskForTxIds();
            }

            while (args.Any())
            {
                HttpClient client = new HttpClient();
                var tasks = args.Select(s => GetTx(s, client));
                var results= await Task.WhenAll(tasks);
                foreach (var l in results)
                {
                    result += (ulong)l;
                }
                
                Console.WriteLine($"current total: {result/8} sats");
                
                args = AskForTxIds();
            }
            Console.ReadLine();
        }

        private static string[] AskForTxIds()
        {
            string[] args;
            Console.WriteLine("enter tx ids to sum up (or just enter to end): ");
            args = Console.ReadLine()?.Split(" ");
            return args;
        }


        private static async Task<long> GetTx(string txId, HttpClient client, int tries = 0)
        {
            try
            {;
                Console.WriteLine($"querying txid {txId}");
                var req = await client.GetAsync($"https://blockstream.info/api/tx/{txId}");
                var json = await JsonSerializer.DeserializeAsync<Tx>(await req.Content.ReadAsStreamAsync());
                var res =  json.Outs.Select(txout => txout.Value).Sum();
                Console.WriteLine($"txid {txId} = {res} sats");
                return res;
            }
            catch (Exception e)
            {
                Console.WriteLine($"failed querying txid {txId} try #{tries+1}");
                if (tries >= 10) throw;
                await Task.Delay(500);
                return await GetTx(txId, client, tries + 1);

            }
        }
    }

    public class Tx
    {
        [JsonPropertyName("vout")]
        public List<TxOut> Outs { get; set; }
    }

    public class TxOut
    {
        [JsonPropertyName("value")]
        public long Value { get; set; }
    }
}
