using System;
using Android.BillingClient.Api;

namespace Companova.Xamarin.InAppPurchase.Service
{
    internal static class Extensions
    {
        public static PurchaseError ToPurchaseError(this BillingResponseCode code)
        {
            PurchaseError error;
            switch (code)
            {
                case BillingResponseCode.BillingUnavailable:
                    error = PurchaseError.BillingUnavailable;
                    break;
                case BillingResponseCode.DeveloperError:
                    error = PurchaseError.DeveloperError;
                    break;
                case BillingResponseCode.Error:
                    error = PurchaseError.GeneralError;
                    break;
                case BillingResponseCode.FeatureNotSupported:
                    error = PurchaseError.GeneralError;
                    break;
                case BillingResponseCode.ItemAlreadyOwned:
                    error = PurchaseError.AlreadyOwned;
                    break;
                case BillingResponseCode.ItemNotOwned:
                    error = PurchaseError.NotOwned;
                    break;
                case BillingResponseCode.ItemUnavailable:
                    error = PurchaseError.ItemUnavailable;
                    break;
                case BillingResponseCode.ServiceDisconnected:
                    error = PurchaseError.ServiceDisconnected;
                    break;
                case BillingResponseCode.ServiceTimeout:
                    error = PurchaseError.NetworkConnectionFailed;
                    break;
                case BillingResponseCode.ServiceUnavailable:
                    error = PurchaseError.ServiceUnavailable;
                    break;
                case BillingResponseCode.UserCancelled:
                    error = PurchaseError.UserCancelled;
                    break;
                default:
                    error = PurchaseError.Unknown;
                    break;
            }

            return error;
        }

        public static InAppPurchaseResult ToInAppPurchase(this Purchase p)
        {
            return new InAppPurchaseResult
            {
                TransactionDateUtc = new DateTime(p.PurchaseTime),
                Id = p.OrderId,
                ProductId = p.Sku,
                Acknowledged = p.IsAcknowledged,
                AutoRenewing = p.IsAutoRenewing,
                State = p.GetPurchaseState(),
                PurchaseToken = p.PurchaseToken
            };
        }

        private static PurchaseState GetPurchaseState(this Purchase transaction)
        {
            if (transaction?.PurchaseState == null)
                return PurchaseState.Unknown;

            switch (transaction.PurchaseState)
            {
                case Android.BillingClient.Api.PurchaseState.Unspecified:
                    return PurchaseState.Unknown;
                case Android.BillingClient.Api.PurchaseState.Pending:
                    return PurchaseState.PaymentPending;
                case Android.BillingClient.Api.PurchaseState.Purchased:
                    return PurchaseState.Purchased;
            }

            return PurchaseState.Unknown;
        }
    }
}
