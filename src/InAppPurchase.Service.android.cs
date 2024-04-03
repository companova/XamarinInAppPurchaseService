using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Android.App;
using Android.Util;
using Android.Runtime;

using Android.BillingClient.Api;

namespace Companova.Xamarin.InAppPurchase.Service
{
    /// <summary>
    /// Key class that provides access to Billing Client in-app-purchases features.<br/>
    /// Call:<br/>
    /// <see cref="StartAsync"/> to connect to Billing Client<br/>
    /// <see cref="StopAsync"/> to disconnect from Billing Client<br/>
    /// <see cref="LoadProductsAsync(string[], ProductType)"/> to retrieve a list of Products<br/>
    /// <see cref="PurchaseAsync(string)"/> to buy a product<br/>
    /// <see cref="RestoreAsync(ProductType)"/> to restore previously bought products<br/>
    /// </summary>
    [Preserve(AllMembers = true)]
    public class InAppPurchaseService : Java.Lang.Object, IPurchasesUpdatedListener, IBillingClientStateListener,
        IInAppPurchaseService
    {

        #region IBillingClientStateListener

        /// <summary>
        /// Callback used by Billing Client
        /// </summary>
        /// <param name="result"></param>
        public void OnBillingSetupFinished(BillingResult result)
        {
            Log.Debug(_billingTag, $"In OnBillingSetupFinished: Code: {result.ResponseCode}, Message: {result.DebugMessage}");

            if (result.ResponseCode == BillingResponseCode.Ok)
            {
                // Read https://devblogs.microsoft.com/pfxteam/the-nature-of-taskcompletionsourcetresult/
                // Return just Task vs Task<T>
                _connected?.SetResult(null);
            }
            else
            {
                _connected?.TrySetException(new InAppPurchaseException(
                    result.ResponseCode.ToPurchaseError(),
                    result.DebugMessage));
            }
        }

        /// <summary>
        /// Callback used by Billing Client
        /// </summary>
        public void OnBillingServiceDisconnected()
        {
            Log.Debug(_billingTag, "In OnBillingServiceDisconnected");
        }

        #endregion

        #region IPurchasesUpdatedListener

        /// <summary>
        /// Callback used by Billing Client
        /// </summary>
        /// <param name="result"></param>
        /// <param name="listOfPurchases"></param>
        public void OnPurchasesUpdated(BillingResult result, IList<Purchase> listOfPurchases)
        {
            Log.Debug(_billingTag, $"In OnPurchasesUpdated: {result.ResponseCode}, Message: {result.DebugMessage}");

            // We succeeded only when the ReponseCode is Ok.

            if (result.ResponseCode == BillingResponseCode.Ok)
            {
                // Success. The Item has been Purchased
                _transactionPurchased?.TrySetResult(listOfPurchases?[0].ToInAppPurchase());
                return;
            }

            // Otherwise, it is an error (even for BillingResponseCode.ItemAlreadyOwned
            _transactionPurchased?.TrySetException(new InAppPurchaseException(result.ResponseCode.ToPurchaseError(),
                result.DebugMessage));
            _transactionPurchased = null;

            return;
        }

        #endregion

        /// <summary>
        /// Default Constructor for In-App-Purchase Service Implemenation on Android
        /// </summary>
        public InAppPurchaseService()
        { 
        }

        // Current Activity. Used to launch the Billing flow
        private static Activity _activity = null;

        // Billing Client. It is a one-time use and needs to be initialized every time.
        // See https://github.com/android/play-billing-samples/blob/34b49bc0929e5cead8e69df7a6e0d41793fe645f/ClassyTaxiJava/app/src/main/java/com/example/android/classytaxijava/billing/BillingClientLifecycle.java#L93
        private BillingClient _billingClient = null;

        // Task Source that is set to Complete when the Billing Client is connected
        // https://devblogs.microsoft.com/pfxteam/the-nature-of-taskcompletionsourcetresult/
        private TaskCompletionSource<object> _connected;

        /// Task Source that is set to Complete when the individual purchase is complete
        /// Only one purchase is supported at a time
        private TaskCompletionSource<InAppPurchaseResult> _transactionPurchased;

        // A dictionary of retrieved products
        private readonly Dictionary<string, SkuDetails> _retrievedProducts = new Dictionary<string, SkuDetails>();

        // Log Tag
        private const string _billingTag = "InAppPurchase";

        /// <summary>
        /// Sets Current Activity. Required to launch the Billing flow
        /// </summary>
        /// <param name="activity">Current Activity</param>
        public void SetActivity(Activity activity)
        {
            _activity = activity;
        }

        /// <summary>
        /// Initializes and Connects to Billing Client. Needs to be called in the Activity OnCreate
        /// </summary>
        public Task StartAsync()
        {
            Log.Debug(_billingTag, "Build BillingClient. connection...");

            // Setup the Task Source to wait for Connection
            if (_connected != null)
                throw new InAppPurchaseException(PurchaseError.DeveloperError, "BillingClient has been already started");

            _connected = new TaskCompletionSource<object>();

            _billingClient = BillingClient.NewBuilder(Application.Context)
                .SetListener(this)
                .EnablePendingPurchases()
                .Build();

            // Attempt to connect to the service
            if (!_billingClient.IsReady)
            {
                Log.Debug(_billingTag, "Start Connection...");
                _billingClient.StartConnection(this);
            }
            else
            {
                // Already connected. Complete the _connected Task Source
                _connected.TrySetResult(null);
            }

            // Return awaitable Task which is signaled when the BillingClient calls OnBillingServiceDisconnected
            return _connected.Task;
        }

        /// <summary>
        /// Disconnects BillingClient. Needs to be called in the Activity OnDestroy
        /// </summary>
        public Task StopAsync()
        {
            try
            {
                // Reset the Task Source for Billing Client Connection
                _connected?.TrySetCanceled();
                _connected = null;

                if (_billingClient != null && _billingClient.IsReady)
                {
                    _billingClient.EndConnection();
                    _billingClient.Dispose();
                    _billingClient = null;
                }
            }
            catch (Exception ex)
            {
                Log.Debug(_billingTag, $"Unable to EndConnection: {ex.Message}");
            }

            Log.Debug(_billingTag, "Disconnected");

            // Completed Task
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns product info for the specified Ids
        /// </summary>
        /// <param name="productIds">Product Ids</param>
        /// <param name="productType">Product Type (either BillingClient.SkuType.Inapp or BillingClient.SkuType.Subs)</param>
        /// <returns></returns>
        public async Task<IEnumerable<Product>> LoadProductsAsync(string[] productIds, ProductType productType)
        {
            string skuType = GetBillingSkuType(productType);

            // Build the Sku Params
            SkuDetailsParams skuParams = SkuDetailsParams.NewBuilder()
                .SetSkusList(productIds)
                .SetType(skuType)
                .Build();

            // Query the Play store
            QuerySkuDetailsResult queryResult = await _billingClient.QuerySkuDetailsAsync(skuParams);
            BillingResult result = queryResult?.Result;

            if (result == null)
            {
                // Failed to get products. Set the Exception to the Task, so the caller can react to the issue
                throw new InAppPurchaseException(PurchaseError.Unknown, "BillingResult is null");
            }

            if (result.ResponseCode != BillingResponseCode.Ok)
            {
                PurchaseError purchaseError = result.ResponseCode.ToPurchaseError();
                // Failed to get products. Set the Exception to the Task, so the caller can react to the issue
                throw new InAppPurchaseException(purchaseError, result.DebugMessage);
            }

            // Wait till the products are received in the callback
            IList<SkuDetails> skuDetails = queryResult?.SkuDetails;
            if (skuDetails == null)
                skuDetails = new List<SkuDetails>();

            // Add more Skus to the Dictionary of SkuDetails
            // We need SkuDetails to initiate the Purchase
            foreach (SkuDetails sku in skuDetails)
                _retrievedProducts.TryAdd(sku.Sku, sku);

            // Return products
            return skuDetails.Select(p => new Product
            {
                Name = p.Title,
                Description = p.Description,
                CurrencyCode = p.PriceCurrencyCode,
                FormattedPrice = p.Price,
                ProductId = p.Sku,
                MicrosPrice = p.PriceAmountMicros,
                LocalizedIntroductoryPrice = p.IntroductoryPrice,
                MicrosIntroductoryPrice = p.IntroductoryPriceAmountMicros
            });
        }

        /// <summary>
        /// Determines whether the User can make payments on the given device
        /// </summary>
        /// <returns>true/false</returns>
        public bool CanMakePayments()
        {
            // Always true for Android
            return true;
        }

        /// <summary>
        /// Initializes an async process to purchas the product. Only one purchase request can be happening at a time
        /// </summary>
        /// <param name="productId">Product to buy</param>
        /// <returns>Purchase object</returns>
        public async Task<InAppPurchaseResult> PurchaseAsync(string productId)
        {
            if (_billingClient == null || !_billingClient.IsReady)
                throw new InAppPurchaseException(PurchaseError.DeveloperError, "Billing Client is not connected");

            // Make sure no purchases are being currently made 
            if (_transactionPurchased != null && !_transactionPurchased.Task.IsCanceled)
                throw new InAppPurchaseException(PurchaseError.DeveloperError, "Another Purchase is in progress");

            // First, get the SkuDetail
            SkuDetails sku;
            if (!_retrievedProducts.TryGetValue(productId, out sku))
                throw new InAppPurchaseException(PurchaseError.DeveloperError,
                    $"Cannot find a retrieved Product with {productId} SKU. Products must be first queried from the Play Store");

            // Build FlowParam for the Purchase
            BillingFlowParams flowParams = BillingFlowParams.NewBuilder()
                    .SetSkuDetails(sku)
                    .Build();

            // Set a new Task Source to wait for completion
            _transactionPurchased = new TaskCompletionSource<InAppPurchaseResult>();
            Task<InAppPurchaseResult> taskPurchaseComplete = _transactionPurchased.Task;

            //_billingClient.QueryPurchaseHistoryAsync(BillingClient.SkuType.Inapp, this);

            // Initiate the Billing Process.
            BillingResult response = _billingClient.LaunchBillingFlow(_activity, flowParams);
            if (response.ResponseCode != BillingResponseCode.Ok)
            {
                // Reset the in-app-purchase flow
                _transactionPurchased?.TrySetCanceled();
                _transactionPurchased = null;
                throw new InAppPurchaseException(response.ResponseCode.ToPurchaseError(), response.DebugMessage);
            }

            // Wait till the Task is complete (e.g. Succeeded or Failed - which will result in Exception)
            InAppPurchaseResult purchase = await taskPurchaseComplete;
            _transactionPurchased = null;

            return purchase;
        }

        /// <summary>
        /// Restores all previous purchases
        /// </summary>
        /// <param name="productType">Product Type (inapp or subs)</param>
        /// <returns>An array of previous purchases</returns>
        public async Task<List<InAppPurchaseResult>> RestoreAsync(ProductType productType)
        {
            if (_billingClient == null || !_billingClient.IsReady)
                throw new InAppPurchaseException(PurchaseError.DeveloperError, "Billing Client is not connected");

            string skuType = GetBillingSkuType(productType);

            // Query existing Purchases. Much simpler in 5.0 than in 4.0
            QueryPurchasesParams query = QueryPurchasesParams.NewBuilder().SetProductType(skuType).Build();
            QueryPurchasesResult purchasesResult = await _billingClient.QueryPurchasesAsync(query);

            IList<Purchase> listOfRestoredPurchases = purchasesResult.Purchases;

            // Otherwise, create an array and return it
            List<InAppPurchaseResult> purchases = new List<InAppPurchaseResult>(listOfRestoredPurchases.Count);
            foreach (Purchase p in listOfRestoredPurchases)
            {
                Log.Debug(_billingTag, $"Sku: {p.Products.FirstOrDefault()}, Acknowledged: {p.IsAcknowledged}, State: {p.PurchaseState}");

                // Convert BillingClient Purchases
                purchases.Add(p.ToInAppPurchase());
            }

            // Return purchases
            return purchases;
        }

        private string GetBillingSkuType(ProductType productType)
        {
            switch (productType)
            {
                case ProductType.Subscription:
                    return BillingClient.SkuType.Subs;
                case ProductType.Consumable:
                case ProductType.NonConsumable:
                default:
                    return BillingClient.SkuType.Inapp;
            }
        }

        /// <summary>
        /// Finalizes the Purchase by calling AcklowledgePurchase or ConsumePurchase
        /// </summary>
        /// <param name="token">Purchase Token</param>
        /// <param name="productType">Product Type</param>
        /// <returns></returns>
        public async Task FinalizePurchaseAsync(string token, ProductType productType)
        {
            if (_billingClient == null || !_billingClient.IsReady)
                throw new InAppPurchaseException(PurchaseError.DeveloperError, "Billing Client is not connected");

            switch (productType)
            {
                case ProductType.Subscription:
                // Subscriptions are acknowledge the same way as non-consumable
                // https://developer.android.com/google/play/billing/integrate#acknowledge
                case ProductType.NonConsumable:
                    await AcklowledgePurchaseAsync(token);
                    break;
                case ProductType.Consumable:
                    await ConsumePurchaseAsync(token);
                    break;
                default:
                    throw new InAppPurchaseException(PurchaseError.DeveloperError, $"Unsupported Product Type {productType}");
            }
        }

        private async Task ConsumePurchaseAsync(string token)
        {
            ConsumeParams consumeParams =
                ConsumeParams.NewBuilder()
                .SetPurchaseToken(token)
                .Build();

            // Consume the Consumable Product
            ConsumeResult consumeResult = await _billingClient.ConsumeAsync(consumeParams);
            BillingResult result = consumeResult?.BillingResult;

            if (result == null)
            {
                // Failed to get result back.
                throw new InAppPurchaseException(PurchaseError.Unknown, "BillingResult is null");
            }

            if (result.ResponseCode != BillingResponseCode.Ok)
            {
                throw new InAppPurchaseException(
                    result.ResponseCode.ToPurchaseError(),
                    result.DebugMessage);
            }

            // Otherwise, the ConsumeAsync succeeded. 
        }

        private async Task AcklowledgePurchaseAsync(string token)
        {
            AcknowledgePurchaseParams acknowledgePurchaseParams =
                AcknowledgePurchaseParams.NewBuilder()
                    .SetPurchaseToken(token)
                    .Build();

            // Consume the Non-Consumable Product
            BillingResult result = await _billingClient.AcknowledgePurchaseAsync(acknowledgePurchaseParams);

            if (result == null)
            {
                // Failed to get result back.
                throw new InAppPurchaseException(PurchaseError.Unknown, "BillingResult is null");
            }

            if (result.ResponseCode != BillingResponseCode.Ok)
            {
                throw new InAppPurchaseException(
                    result.ResponseCode.ToPurchaseError(),
                    result.DebugMessage);
            }

            // Otherwise, the Acknowledgement succeeded. 
        }
    }
}
