using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Companova.Xamarin.InAppPurchase.Service
{
    /// <summary>
    /// Interface definition for the Purchase Service
    /// </summary>
    public interface IInAppPurchaseService
    {
        /// <summary>
        /// Initializes the In-App Purchase infrastructure. For instance, connects to the Billing Client or creates the PaymentObserver
        /// </summary>
        /// <returns></returns>
        Task StartAsync();

        /// <summary>
        /// Shuts down the In-App Purchase infrastructure. For instance, disconnects from the Billing Client or removes the PaymentObserver
        /// </summary>
        /// <returns></returns>
        Task StopAsync();

        /// <summary>
        /// Loads products/in-app purchases from the Store
        /// </summary>
        /// <param name="productIds">Product Ids to load</param>
        /// <param name="productType">Product Types</param>
        /// <returns>List of loaded products</returns>
        Task<IEnumerable<Product>> LoadProductsAsync(string[] productIds, ProductType productType);

        /// <summary>
        /// Determines whether the purchases can be made/allowed on the device
        /// </summary>
        /// <returns></returns>
        bool CanMakePayments();

        /// <summary>
        /// Initializes the Purchase process for the give Product
        /// </summary>
        /// <param name="productId">Product Id to be purchased</param>
        /// <returns>Purchase Result</returns>
        Task<InAppPurchaseResult> PurchaseAsync(string productId);

        /// <summary>
        /// Restore purchases on the device
        /// </summary>
        /// <param name="productType">Product Types to be restored</param>
        /// <returns>List of Purchase Results</returns>
        Task<List<InAppPurchaseResult>> RestoreAsync(ProductType productType);

        /// <summary>
        /// Finalizes the Purchase (Android specific)
        /// </summary>
        /// <param name="token">Purchase Token</param>
        /// <param name="productType">Product Type</param>
        /// <returns></returns>
        Task FinalizePurchaseAsync(string token, ProductType productType);
    }
}
