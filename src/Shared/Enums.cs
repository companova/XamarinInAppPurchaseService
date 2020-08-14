using System;

namespace Companova.Xamarin.InAppPurchase.Service
{
    /// <summary>
    /// Product Type: Subscription, Consumable, Non-Consumable
    /// </summary>
    public enum ProductType
    {
        /// <summary>
        /// Unknown/Invalid Product Type
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Subscription Type
        /// </summary>
        Subscription,
        /// <summary>
        /// Consumable Type
        /// </summary>
        Consumable,
        /// <summary>
        /// Non-Consumable Type
        /// </summary>
        NonConsumable,
    }

    /// <summary>
    /// Purchase state of a Product
    /// </summary>
    public enum ProductState
    {
        /// <summary>
        /// The Purchase state of Product is unknown (usually not Purchased)
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Product has been purchased and in good standing (Active)
        /// </summary>
        Active,
        /// <summary>
        /// Pending Purchase. The payment is pending
        /// </summary>
        Pending,
        /// <summary>
        /// Free Product. Could be used to promote Free products/apps
        /// </summary>
        Free,
    }

    /// <summary>
    /// Gets the current status of the purchase
    /// </summary>
    public enum PurchaseState
    {
        /// <summary>
        /// Purchase state unknown
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Purchased and in good standing
        /// </summary>
        Purchased = 1,
        /// <summary>
        /// Purchase was canceled
        /// </summary>
        Canceled = 2,
        /// <summary>
        /// Purchase was refunded
        /// </summary>
        Refunded = 3,
        /// <summary>
        /// In the process of being processed
        /// </summary>
        Purchasing,
        /// <summary>
        /// Transaction has failed
        /// </summary>
        Failed,
        /// <summary>
        /// Was restored.
        /// </summary>
        Restored,
        /// <summary>
        /// In queue, pending external action
        /// </summary>
        Deferred,
        /// <summary>
        /// In free trial
        /// </summary>
        FreeTrial,
        /// <summary>
        /// Pending Purchase
        /// </summary>
        PaymentPending,
        /// <summary>
        /// Free Product
        /// </summary>
        Free,
    }

    /// <summary>
    /// Type of purchase error
    /// </summary>
    public enum PurchaseError
    {
        /// <summary>
        /// Unknown Error
        /// </summary>
        Unknown,
        /// <summary>
        /// Billing API version is not supported for the type requested (Android), client error (iOS)
        /// </summary>
        BillingUnavailable,
        /// <summary>
        /// Developer issue
        /// </summary>
        DeveloperError,
        /// <summary>
        /// Product sku not available
        /// </summary>
        ItemUnavailable,
        /// <summary>
        /// Other error
        /// </summary>
        GeneralError,
        /// <summary>
        /// User cancelled the purchase
        /// </summary>
        UserCancelled,
        /// <summary>
        /// App store unavailable on device
        /// </summary>
        AppStoreUnavailable,
        /// <summary>
        /// User is not allowed to authorize payments
        /// </summary>
        PaymentNotAllowed,
        /// <summary>
        /// One of the payment parameters was not recognized by app store
        /// </summary>
        PaymentInvalid,
        /// <summary>
        /// The requested product is invalid
        /// </summary>
        InvalidProduct,
        /// <summary>
        /// The product request failed
        /// </summary>
        ProductRequestFailed,
        /// <summary>
        /// The user has not allowed access to Cloud service information
        /// </summary>
        PermissionDenied,
        /// <summary>
        /// The device could not connect to the network
        /// </summary>
        NetworkConnectionFailed,
        /// <summary>
        /// The user has revoked permission to use this cloud service
        /// </summary>
        CloudServiceRevoked,
        /// <summary>
        /// The user has not yet acknowledged Apple’s privacy policy for Apple Music
        /// </summary>
        PrivacyError,
        /// <summary>
        /// The app is attempting to use a property for which it does not have the required entitlement
        /// </summary>
        UnauthorizedRequest,
        /// <summary>
        /// The offer identifier cannot be found or is not active
        /// </summary>
        InvalidOffer,
        /// <summary>
        /// The signature in a payment discount is not valid
        /// </summary>
        InvalidSignature,
        /// <summary>
        /// Parameters are missing in a payment discount
        /// </summary>
        MissingOfferParams,
        /// <summary>
        /// The price you specified in App Store Connect is no longer valid.
        /// </summary>
        InvalidOfferPrice,
        /// <summary>
        /// Restoring the transaction failed
        /// </summary>
        RestoreFailed,
        /// <summary>
        /// Network connection is down
        /// </summary>
        ServiceUnavailable,
        /// <summary>
        /// Product is already owned
        /// </summary>
        AlreadyOwned,
        /// <summary>
        /// Item is not owned and can not be consumed
        /// </summary>
        NotOwned,
        /// <summary>
        /// Billing Client Service is Disconnected
        /// </summary>
        ServiceDisconnected,
    }
}
