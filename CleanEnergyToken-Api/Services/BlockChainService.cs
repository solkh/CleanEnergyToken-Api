using System.Numerics;
using Nethereum.HdWallet;
using Nethereum.StandardTokenEIP20;
using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3.Accounts;
using Microsoft.AspNetCore.Identity;
using CleanEnergyToken_Api.Models;
using Microsoft.EntityFrameworkCore;
using CleanEnergyToken_Api.Extentions;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace CleanEnergyToken_Api.Services
{
    public interface IBlockChainService
    {
        string[] GenerateMnemonic();
        Account? GetWalletFromMnemonic(string mnemonicPhrase);
        Task<BigInteger> GetWalletBalanceAsync(string walletAddress);
        Task<TransferEventDTO[]> GetWalletTransactionsAsync(string walletAddress);
        Task<TransactionReceipt?> SafeTransferCET(string privateKey, string senderAddress, string recipientAddress, BigInteger amount);
    }


    public class BlockChainService : IBlockChainService
    {
        private const string URL = "https://goerli.infura.io/v3/1aee37f357bd4ade899698df8b9e1a58";
        //const string URL = "https://mainnet.infura.io/v3/1aee37f357bd4ade899698df8b9e1a58";
        private const string CONTRACT_ADDRESS = "0x45265a7CA12FF5FB9A0e1aB992f9313b796b45e9";
        private readonly ILogger<IBlockChainService> _logger;
        private readonly INotificationService _notificationService;
        private readonly UserManager<AppUser> _userManager;
        public BlockChainService(INotificationService notificationService,
                                 UserManager<AppUser> userManager,
                                 ILogger<BlockChainService> logger)
        {
            _notificationService = notificationService;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Generate a new 12-word mnemonic phrase
        /// </summary>
        /// <returns></returns>
        public string[] GenerateMnemonic() =>
            new NBitcoin.Mnemonic(NBitcoin.Wordlist.English, NBitcoin.WordCount.Twelve).Words;


        /// <summary>
        /// Get Wallet From Mnemonic
        /// </summary>
        /// <returns></returns>
        public Account? GetWalletFromMnemonic(string mnemonicPhrase)
        {
            try
            {
                // Create a wallet from the mnemonic phrase
                var wallet = new Wallet(mnemonicPhrase, "SeedPassword");

                // Get the Ethereum account derived from the wallet
                var account = wallet.GetAccount(0);

                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Query Wallet Address Balance
        /// </summary>
        /// <param name="walletAddress"></param>
        /// <returns></returns>
        public async Task<BigInteger> GetWalletBalanceAsync(string address)
        {
            var web3 = new Web3(URL);
            var tokenService = new StandardTokenService(web3, CONTRACT_ADDRESS);

            // Check an address balance
            var symbol = await tokenService.SymbolQueryAsync();
            var tokens = await tokenService.BalanceOfQueryAsync(address);

            return tokens;
        }

        /// <summary>
        /// Get List of Wallet Transactions
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task<TransferEventDTO[]> GetWalletTransactionsAsync(string address)
        {
            var web3 = new Web3(URL);
            var transferEventHandler = web3.Eth.GetEvent<TransferEventDTO>(CONTRACT_ADDRESS);
            var filterAllTransferEventsForContract = transferEventHandler.CreateFilterInput();
            var allChanges = await transferEventHandler.GetAllChangesAsync(filterAllTransferEventsForContract);

            return allChanges.Select(x => x.Event).ToArray();
        }



        public async Task<TransactionReceipt?> SafeTransferCET(string privateKey, string senderAddress, string recipientAddress, BigInteger amount)
        {
            if (privateKey.StartsWith("0x"))
                privateKey = privateKey.Substring(2);

            var reciverId = (await _userManager.Users.FirstOrDefaultAsync(x => x.Address == recipientAddress))?.Id;
            if (reciverId == null)
                return null;

            var account = new Account(Convert.FromHexString(privateKey), Nethereum.Signer.Chain.Goerli);
            var web3 = new Web3(account, URL);
            var contract = web3.Eth.ERC20.GetContractService(CONTRACT_ADDRESS);
            var receipt = await contract.ApproveRequestAndWaitForReceiptAsync(recipientAddress, amount);
            Thread.Sleep(1000);
            if (receipt?.HasErrors() == true || !receipt.Succeeded())
            {
                _logger.LogError(receipt.Logs.ToString());
                return null;
            }

            _logger.LogInformation("APPROVED transaction from {senderAddress} => {recipientAddress}: {amount} SMRG", senderAddress, recipientAddress, amount);
            receipt = await contract.TransferRequestAndWaitForReceiptAsync(recipientAddress, amount);
            if (receipt?.HasErrors() == true)
            {
                _logger.LogError(receipt.Logs.ToString());
            }
            _logger.LogInformation("SMARGE tokens transferred successfully. Transaction hash: {TransactionHash}", receipt?.TransactionHash);

            await _notificationService.SendCETRecived(reciverId, amount);


            return receipt;
        }
    }

    [Event("Transfer")]
    public class TransferEventDTO : IEventDTO
    {
        [Parameter("address", "_from", 1, true)]
        public string From { get; set; }

        [Parameter("address", "_to", 2, true)]
        public string To { get; set; }

        [Parameter("uint256", "_value", 3, false)]
        public BigInteger Value { get; set; }
    }
}
