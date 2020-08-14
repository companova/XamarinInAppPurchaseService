using System;

namespace Companova.Xamarin.InAppPurchase.Service
{
    /// <summary>
    /// In-App Service Specifi Exception
    /// </summary>
    [Preserve(AllMembers = true)]
    public class InAppPurchaseException : Exception
    {
        /// <summary>
        /// Type of error
        /// </summary>
        public PurchaseError PurchaseError { get; }

        /// <summary>
        /// Default ctor
        /// </summary>
        /// <param name="error">Purchase Error</param>
        /// <param name="ex">Inner Exception</param>
        public InAppPurchaseException(PurchaseError error, Exception ex) : base("Unable to process purchase.", ex)
        {
            PurchaseError = error;
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        /// <param name="error">Purchase Error</param>
        public InAppPurchaseException(PurchaseError error) : base("Unable to process purchase.")
        {
            PurchaseError = error;
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        /// <param name="error">Purchase Error</param>
        /// <param name="message">Error Message</param>
        public InAppPurchaseException(PurchaseError error, string message) : base(message)
        {
            PurchaseError = error;
        }

        /// <summary>
        /// Converts Exception to a string
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return $"Error: {PurchaseError}. {base.ToString()}";
        }
    }
}
