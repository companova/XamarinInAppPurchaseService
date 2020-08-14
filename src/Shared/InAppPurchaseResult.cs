using System;

namespace Companova.Xamarin.InAppPurchase.Service
{
    /// <summary>
    /// In App Purchase Results
    /// </summary>
    [Preserve(AllMembers = true)]
    public class InAppPurchaseResult
    {
        /// <summary>
        /// Purchase/Order Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Trasaction date in UTC
        /// </summary>
        public DateTime TransactionDateUtc { get; set; }

        /// <summary>
        /// Product Id/Sku
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// Indicates whether the purchase has been already acknowledge.
        /// </summary>
        public bool Acknowledged { get; set; }

        /// <summary>
        /// Indicates whether the subscription renewes automatically. If true, the subscription is active, else false the user has canceled.
        /// </summary>
        public bool AutoRenewing { get; set; }

        /// <summary>
        /// Unique token identifying the purchase for a given item
        /// </summary>
        public string PurchaseToken { get; set; }

        /// <summary>
        /// Gets the current purchase/subscription state
        /// </summary>
        public PurchaseState State { get; set; }
    }
}
