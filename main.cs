using System;
using NBitcoin;
using NBitcoin.RPC;
using NBXplorer.DerivationStrategy;
using NBXplorer;
using Spectre.Console;
using NBXplorer.Client;
using QBitNinja.Client;
using System.Transactions;
using QBitNinja.Client.Models;
using System.Net;
using NBitcoin.Protocol;

namespace tempwallet
{
    class Program
    {
        static void Main(string[] args)
        {

            var network = Network.Main;
            var privateKey = new Key();
            var publicKey = privateKey.PubKey;
            var bitcoinPrivateKey = privateKey.GetWif(network);
            var address = publicKey.GetAddress(ScriptPubKeyType.Segwit, Network.Main); 
            BitcoinSecret secret = privateKey.GetBitcoinSecret(Network.Main);
            var totalWidth = Console.WindowWidth;
            var labelWidth = 27;
            var padding = (totalWidth - (labelWidth)) / 2;
            var centeredLine = $"[bold springgreen3_1]Address[/]: {address}".PadLeft(padding + labelWidth);
            Console.WriteLine("\n");
            AnsiConsole.MarkupLine(centeredLine);
            Console.WriteLine("\n\n\n\n");

            while (true)
            {
                var inp = AnsiConsole.Ask<string>("CSTemp\t→");
                if (inp == null)
                {
                    Console.WriteLine("Please Enter a Valid Command");
                }
                else if (inp == "help" || inp == "Help")
                {
                    Console.WriteLine("\nCommands\nSend\nBalance\nPrivateKey\n");
                }
                else if (inp == "PrivateKey" || inp == "privatekey")
                {
                    Console.WriteLine(bitcoinPrivateKey);
                } else if (inp == "Send" || inp == "send")
                {
                    var ands = AnsiConsole.Ask<string>("Enter Target Address: ");
                    var recipientAddress = BitcoinAddress.Create(ands, Network.Main);
                    var url = $"https://blockchain.info/q/addressbalance/{address}";
                    using (var client = new HttpClient())
                    {
                        var response = client.GetAsync(url).Result;
                        var content = response.Content.ReadAsStringAsync().Result;
                        if (response.IsSuccessStatusCode)
                        {
                            var balance = long.Parse(content);
                            var balanceBTC = (double)balance / 100000000;
                            var amountToSend = AnsiConsole.Ask<decimal>("Enter the amount of BTC to send: ");
                            if ((double)amountToSend > balanceBTC)
                            {
                                Console.WriteLine("Insufficient balance. Cannot send transaction.");
                            }
                            else
                            {
                                var init = Network.Main.CreateTransaction();
                                var txBuilder = Network.Main.CreateTransactionBuilder();
                                Coin[] coins = init.Outputs.AsCoins().ToArray();
                                var tx = txBuilder
                                    .AddKeys(secret.PrivateKey)
                                    .Send(recipientAddress, Money.Coins(amountToSend))
                                    .SetChange(secret.GetAddress(ScriptPubKeyType.Legacy));

                                var transaction = txBuilder.BuildTransaction(true);
                                var node = Node.Connect(Network.Main, "localhost");
                                node.VersionHandshake();
                                node.SendMessage(new TxPayload(transaction));
                                node.Disconnect();
                            }
                            Console.WriteLine($"Wallet balance: {balanceBTC.ToString("0.00000000")} BTC");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to retrieve balance: {content}");
                        }
                    }
                   


                }
                else if (inp == "balance" || inp == "Balance")
                {
                    var url = $"https://blockchain.info/q/addressbalance/{address}";
                    using (var client = new HttpClient())
                    {
                        var response = client.GetAsync(url).Result;
                        var content = response.Content.ReadAsStringAsync().Result;
                        if (response.IsSuccessStatusCode)
                        {
                            var balance = long.Parse(content);
                            var balanceBTC = (double)balance / 100000000;
                            Console.WriteLine($"Wallet balance: {balanceBTC.ToString("0.00000000")} BTC");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to retrieve balance: {content}");
                        }
                    }
                }
            }
        }
    }
}
