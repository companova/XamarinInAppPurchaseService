using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using UIKit;
using StoreKit;
using Foundation;
using System.Linq;
using System.Diagnostics;

namespace Companova.Xamarin.InAppPurchase.Service
{
    /// <summary>
    /// Key class that provides access to StoreKit in-app-purchases features.
    /// Call:
    /// - Start to add an Observer to the Queue
    /// - Stop to remove the Observer from the Queue
    /// - LoadProductsAsync to retrieve a list of Products
    /// - CanMakePayment to check if the user can buy in-app-purchases
    /// - PurchaseAsync to buy a product
    /// - RestoreAsync to restore previously bought products
    /// </summary>
    [Preserve(AllMembers = true)]
    public class InAppPurchaseService : IInAppPurchaseService
    {
        static bool IsiOS112 => UIDevice.CurrentDevice.CheckSystemVersion(11, 2);

        // Payment Observer which will get notifications about Purchased and Restored products
        private PaymentObserver _paymentObserver;

        /// <summary>
        /// Initializes the Payment Observer and adds it to the Payment Queue
        /// </summary>
        public Task StartAsync()
        {
            // Make sure we don't get initialized twice
            if (_paymentObserver != null)
                throw new InAppPurchaseException(PurchaseError.DeveloperError, "PaymentObserver has been already created");

            _paymentObserver = new PaymentObserver();
            // Start Observing the Payment Queue
            SKPaymentQueue.DefaultQueue.AddTransactionObserver(_paymentObserver);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes the Observer from the Payment Queue
        /// </summary>
        public Task StopAsync()
        {
            try
            {
                // Disconnect only if we were connected 
                if (_paymentObserver != null)
                {
                    SKPaymentQueue.DefaultQueue.RemoveTransactionObserver(_paymentObserver);
                    _paymentObserver = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to RemoveTransactionObserver: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns product info for the specified Ids
        /// </summary>
        /// <param name="productIds">Product Ids</param>
        /// <param name="productType">Product Type - not used for iOS</param>
        /// <returns></returns>
        public async Task<IEnumerable<Product>> LoadProductsAsync(string[] productIds, ProductType productType)
        {
            NSString[] nsProductIds = productIds.Select(i => new NSString(i)).ToArray();
            // Try to get Product List from the Store
            NSSet nsSetOfproductIds = NSSet.MakeNSObjectSet<NSString>(nsProductIds);

            // Create a Delegeate which will receieve product data.
            // The callback is async, so the TaskCompletionSource is used to wait for App Store to respond.
            ProductRequestDelegate productRequestDelegate = new ProductRequestDelegate();

            // Set up product request for in-app purchase
            SKProductsRequest productsRequest = new SKProductsRequest(nsSetOfproductIds);
            productsRequest.Delegate = productRequestDelegate;

            /* Here is how the EventHadler could be used instead of the Delegate
            productsRequest.ReceivedResponse += (object sender, SKProductsRequestResponseEventArgs e) =>
            {
                var products = e.Response.Products;
                var product = products[0];
                Console.WriteLine(
                    $"{product.ProductIdentifier}, " +
                    $"{product.LocalizedDescription}," +
                    $"{product.PriceLocale.CurrencySymbol}{product.Price}");

                //product.Discounts
            };*/

            // Start the StoreKit Request. Products will be sent back to the Delegate function(s)
            productsRequest.Start();

            // Wait for the Delegate to get Products and signal the Task to be complete
            // For more about TaskCompletionSource read: https://devblogs.microsoft.com/pfxteam/the-nature-of-taskcompletionsourcetresult/
            SKProduct[] products = await productRequestDelegate.WaitForResponse();

            return products.Select(p => new Product
            {
                FormattedPrice = p.LocalizedPrice(),
                MicrosPrice = (long)(p.Price.DoubleValue * 1000000d),
                Name = p.LocalizedTitle,
                ProductId = p.ProductIdentifier,
                Description = p.LocalizedDescription,
                CurrencyCode = p.PriceLocale?.CurrencyCode ?? string.Empty,
                LocalizedIntroductoryPrice = IsiOS112 ? (p.IntroductoryPrice?.LocalizedPrice() ?? string.Empty) : string.Empty,
                MicrosIntroductoryPrice = IsiOS112 ? (long)((p.IntroductoryPrice?.Price?.DoubleValue ?? 0) * 1000000d) : 0,
            });
        }

        /// <summary>
        /// Determines whether the User can make payments on the given device
        /// </summary>
        /// <returns>true/false</returns>
        public bool CanMakePayments()
        {
            return SKPaymentQueue.CanMakePayments;
        }

        /// <summary>
        /// Initializes an async process to purchas the product. Only one purchase request can be happening at a time
        /// </summary>
        /// <param name="productId">Product to buy</param>
        /// <returns>Purchase object</returns>
        public async Task<InAppPurchaseResult> PurchaseAsync(string productId)
        {
            // First, set the Completion Task Source. The Payment Observer will handle only one request at a time
            TaskCompletionSource<InAppPurchaseResult> transactionPurchased = new TaskCompletionSource<InAppPurchaseResult>();
            _paymentObserver.SetTransactionPurchasedTask(transactionPurchased);

            SKPayment payment = SKPayment.CreateFrom(productId);
            SKPaymentQueue.DefaultQueue.AddPayment(payment);

            // Wait till the Task is complete (e.g. Succeeded or Failed - which will result in Exception)
            return await transactionPurchased.Task;
        }

        /// <summary>
        /// Restores all previous purchases
        /// </summary>
        /// <param name="productType">Not used of iOS</param>
        /// <returns>An array of previous purchases</returns>
        public async Task<List<InAppPurchaseResult>> RestoreAsync(ProductType productType)
        {
            TaskCompletionSource<List<InAppPurchaseResult>> transactionsRestored = new TaskCompletionSource<List<InAppPurchaseResult>>();
            _paymentObserver.SetTransactionRestoreTask(transactionsRestored);

            // Start receiving restored transactions
            SKPaymentQueue.DefaultQueue.RestoreCompletedTransactions();
            List<InAppPurchaseResult> list = await transactionsRestored.Task;

            return list;
        }

        /// <summary>
        /// Not used of iOS
        /// </summary>
        /// <param name="token"></param>
        /// <param name="productType"></param>
        /// <returns></returns>
        public Task FinalizePurchaseAsync(string token, ProductType productType)
        {
            return Task.CompletedTask;
        }
    }

    [Preserve(AllMembers = true)]
    internal class ProductRequestDelegate : NSObject, ISKProductsRequestDelegate, ISKRequestDelegate
    {
        // Task Complete Source to signal the Task to be Complete when we get the Products back
        private TaskCompletionSource<SKProduct[]> _productsReceived;

        internal ProductRequestDelegate()
        {
            _productsReceived = new TaskCompletionSource<SKProduct[]>();
        }

        internal Task<SKProduct[]> WaitForResponse() =>
            _productsReceived.Task;


        [Export("request:didFailWithError:")]
        public void RequestFailed(SKRequest request, NSError error)
        {
            // Get Info for the Exception
            string description = error.LocalizedDescription ?? string.Empty;
            PurchaseError purchaseError = error.ToPurchaseError();
            // Failed to Restore. Set the Exception to the Task, so the caller can react to the issue
            _productsReceived.TrySetException(new InAppPurchaseException(purchaseError, description));
        }

        public void ReceivedResponse(SKProductsRequest request, SKProductsResponse response)
        {
            SKProduct[] skProducts = response.Products;

            if (skProducts != null)
            {
                _productsReceived.TrySetResult(skProducts);

                //Debug.WriteLine($"{skProducts[0].ProductIdentifier}, {skProducts[0].LocalizedDescription}" +
                //    $", {skProducts[0].PriceLocale.CurrencySymbol}{skProducts[0].Price}");

                return;
            }

            // No products came back. Must be invalid Product Id
            _productsReceived.TrySetException(
                new InAppPurchaseException(PurchaseError.InvalidProduct, "Invalid Product"));
        }
    }

    #region Observer

    [Preserve(AllMembers = true)]
    internal class PaymentObserver : SKPaymentTransactionObserver
    {
        /// <summary>
        /// Task Source that is set to Complete when the individual purchase is complete
        /// Only one purchase is supported at a time
        /// </summary>
        private TaskCompletionSource<InAppPurchaseResult> _transactionPurchased { get; set; }
        /// <summary>
        /// Task Source that is to Complete when the Restore process is complete
        /// </summary>
        private TaskCompletionSource<List<InAppPurchaseResult>> _transactionsRestored { get; set; }

        /// <summary>
        /// Sets the await Task Source that is signaled once the Purchase process is finished
        /// </summary>
        /// <param name="newCompletionSource">Task Source that the Caller will await for</param>
        public void SetTransactionPurchasedTask(TaskCompletionSource<InAppPurchaseResult> newCompletionSource)
        {
            if (_transactionPurchased != null)
                throw new InAppPurchaseException(PurchaseError.DeveloperError, "Another Purchase is in progress");

            _transactionPurchased = newCompletionSource;
        }

        /// <summary>
        /// Sets the await Task Source that is signaled once the Restore process is finished
        /// </summary>
        /// <param name="newCompletionSource">Task Source that the Caller will await for</param>
        public void SetTransactionRestoreTask(TaskCompletionSource<List<InAppPurchaseResult>> newCompletionSource)
        {
            if (_transactionsRestored != null)
                throw new InAppPurchaseException(PurchaseError.DeveloperError, "Another Transaction Restore operation is in progress");

            _transactionsRestored = newCompletionSource;
        }

        // A list that holds Restored Transactions
        private List<SKPaymentTransaction> _restoredTransactions = new List<SKPaymentTransaction>();

        public PaymentObserver()
        {
        }

        public override bool ShouldAddStorePayment(SKPaymentQueue queue, SKPayment payment, SKProduct product)
        {
            return true;
        }

        public override void UpdatedTransactions(SKPaymentQueue queue, SKPaymentTransaction[] transactions)
        {
            Debug.WriteLine("UpdatedTransactions called...");

            foreach (SKPaymentTransaction transaction in transactions)
            {
                if (transaction?.TransactionState == null)
                    break;

                Debug.WriteLine($"Updated Transaction | {transaction.TransactionState}; ProductId = {transaction.Payment?.ProductIdentifier ?? string.Empty}");

                switch (transaction.TransactionState)
                {
                    case SKPaymentTransactionState.Restored:
                        _restoredTransactions.Add(transaction);
                        SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);
                        break;
                    case SKPaymentTransactionState.Purchased:
                        _transactionPurchased?.TrySetResult(transaction.ToInAppPurchase());
                        // Reset the Task
                        _transactionPurchased = null;
                        SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);
                        break;
                    case SKPaymentTransactionState.Failed:
                        PurchaseError? error = transaction?.Error?.ToPurchaseError();
                        string description = transaction?.Error?.LocalizedDescription ?? string.Empty;
                        // Failed Transaction. Set the Exception to the Task, so the caller can react to the issue
                        _transactionPurchased?.TrySetException(
                            new InAppPurchaseException(error ?? PurchaseError.GeneralError, description));

                        // Reset the Task
                        _transactionPurchased = null;
                        SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);
                        break;
                    case SKPaymentTransactionState.Purchasing:
                    case SKPaymentTransactionState.Deferred:
                    default:
                        break;
                }
            }
        }

        public override void RestoreCompletedTransactionsFinished(SKPaymentQueue queue)
        {
            Debug.WriteLine("RestoreCompletedTransactionsFinished called...");

            try
            {
                // This is called after all restored transactions have hit UpdatedTransactions and are removed from the Queue
                // Convert all Restored transactions to the InAppBillingPurchases and return
                List<InAppPurchaseResult> purchases = _restoredTransactions.Select(rt =>
                            rt.ToInAppPurchase()).ToList();

                // Clear out the list of incoming restore transactions for future requests
                _restoredTransactions.Clear();

                _transactionsRestored?.TrySetResult(purchases);
                _transactionsRestored = null;
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Exception {ex.ToString()}");

                _transactionsRestored?.TrySetException(new InAppPurchaseException(PurchaseError.Unknown, ex.ToString()));
                _transactionsRestored = null;
            }
        }

        public override void RestoreCompletedTransactionsFailedWithError(SKPaymentQueue queue, NSError error)
        {
            Debug.WriteLine($"RestoreCompletedTransactionsFailedWithError called... _transactionsRestored is null = {_transactionsRestored==null}");

            try
            {
                // Get Info for the Exception
                string description = error?.LocalizedDescription ?? string.Empty;
                PurchaseError purchaseError = error.ToPurchaseError();

                // Failed to Restore. Set the Exception to the Task, so the caller can react to the issue
                _transactionsRestored?.TrySetException(new InAppPurchaseException(purchaseError, description));
                _transactionsRestored = null;
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Exception {ex.ToString()}");

                _transactionsRestored?.TrySetException(new InAppPurchaseException(PurchaseError.Unknown, ex.ToString()));
                _transactionsRestored = null;
            }
        }
    }

    #endregion
}
