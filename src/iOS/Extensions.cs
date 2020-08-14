using System;
using System.Collections.Generic;

using StoreKit;
using Foundation;

namespace Companova.Xamarin.InAppPurchase.Service
{
    internal static class Extensions
    {
        public static PurchaseError ToPurchaseError(this NSError inAppError)
        {
            nint errorCode = inAppError?.Code ?? -1;
            PurchaseError error = PurchaseError.Unknown;
            switch (errorCode)
            {
                case (int)SKError.Unknown:
                    error = PurchaseError.Unknown;
                    break;
                case (int)SKError.ClientInvalid:
                    error = PurchaseError.BillingUnavailable;
                    break;
                case (int)SKError.PaymentCancelled:
                    error = PurchaseError.UserCancelled;
                    break;
                case (int)SKError.PaymentInvalid:
                    error = PurchaseError.PaymentInvalid;
                    break;
                case (int)SKError.PaymentNotAllowed:
                    error = PurchaseError.PaymentNotAllowed;
                    break;
                case (int)SKError.ProductNotAvailable:
                    error = PurchaseError.ItemUnavailable;
                    break;
                case (int)SKError.CloudServicePermissionDenied:
                    error = PurchaseError.PermissionDenied;
                    break;
                case (int)SKError.CloudServiceNetworkConnectionFailed:
                    error = PurchaseError.NetworkConnectionFailed;
                    break;
                case (int)SKError.CloudServiceRevoked:
                    error = PurchaseError.CloudServiceRevoked;
                    break;
                case (int)SKError.PrivacyAcknowledgementRequired:
                    error = PurchaseError.PrivacyError;
                    break;
                case (int)SKError.UnauthorizedRequestData:
                    error = PurchaseError.UnauthorizedRequest;
                    break;
                case (int)SKError.InvalidOfferIdentifier:
                    error = PurchaseError.InvalidOffer;
                    break;
                case (int)SKError.InvalidSignature:
                    error = PurchaseError.InvalidSignature;
                    break;
                case (int)SKError.MissingOfferParams:
                    error = PurchaseError.MissingOfferParams;
                    break;
                case (int)SKError.InvalidOfferPrice:
                    error = PurchaseError.InvalidOfferPrice;
                    break;
                default:
                    error = PurchaseError.Unknown;
                    break;
            }

            return error;
        }

        public static InAppPurchaseResult ToInAppPurchase(this SKPaymentTransaction transaction)
        {
            SKPaymentTransaction p = transaction?.OriginalTransaction ?? transaction;

            if (p == null)
                return null;

            return new InAppPurchaseResult
            {
                TransactionDateUtc = NSDateToDateTimeUtc(transaction.TransactionDate),
                Id = p.TransactionIdentifier,
                ProductId = p.Payment?.ProductIdentifier ?? string.Empty,
                State = p.GetPurchaseState(),
                PurchaseToken = p.TransactionReceipt?.GetBase64EncodedString(NSDataBase64EncodingOptions.None) ?? string.Empty
            };
        }

        private static DateTime NSDateToDateTimeUtc(NSDate date)
        {
            var reference = new DateTime(2001, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            return reference.AddSeconds(date?.SecondsSinceReferenceDate ?? 0);
        }

        private static PurchaseState GetPurchaseState(this SKPaymentTransaction transaction)
        {
            if (transaction?.TransactionState == null)
                return PurchaseState.Unknown;

            switch (transaction.TransactionState)
            {
                case SKPaymentTransactionState.Restored:
                    return PurchaseState.Restored;
                case SKPaymentTransactionState.Purchasing:
                    return PurchaseState.Purchasing;
                case SKPaymentTransactionState.Purchased:
                    return PurchaseState.Purchased;
                case SKPaymentTransactionState.Failed:
                    return PurchaseState.Failed;
                case SKPaymentTransactionState.Deferred:
                    return PurchaseState.Deferred;
            }

            return PurchaseState.Unknown;
        }

        /// <remarks>
        /// Use Apple's sample code for formatting a SKProduct price
        /// https://developer.apple.com/library/ios/#DOCUMENTATION/StoreKit/Reference/SKProduct_Reference/Reference/Reference.html#//apple_ref/occ/instp/SKProduct/priceLocale
        /// Objective-C version:
        ///    NSNumberFormatter *numberFormatter = [[NSNumberFormatter alloc] init];
        ///    [numberFormatter setFormatterBehavior:NSNumberFormatterBehavior10_4];
        ///    [numberFormatter setNumberStyle:NSNumberFormatterCurrencyStyle];
        ///    [numberFormatter setLocale:product.priceLocale];
        ///    NSString *formattedString = [numberFormatter stringFromNumber:product.price];
        /// </remarks>
        public static string LocalizedPrice(this SKProduct product)
        {
            if (product?.PriceLocale == null)
                return string.Empty;

            var formatter = new NSNumberFormatter()
            {
                FormatterBehavior = NSNumberFormatterBehavior.Version_10_4,
                NumberStyle = NSNumberFormatterStyle.Currency,
                Locale = product.PriceLocale
            };
            var formattedString = formatter.StringFromNumber(product.Price);
            Console.WriteLine(" ** formatter.StringFromNumber(" + product.Price + ") = " + formattedString + " for locale " + product.PriceLocale.LocaleIdentifier);
            return formattedString;
        }

        public static string LocalizedPrice(this SKProductDiscount product)
        {
            if (product?.PriceLocale == null)
                return string.Empty;

            var formatter = new NSNumberFormatter()
            {
                FormatterBehavior = NSNumberFormatterBehavior.Version_10_4,
                NumberStyle = NSNumberFormatterStyle.Currency,
                Locale = product.PriceLocale
            };
            var formattedString = formatter.StringFromNumber(product.Price);
            Console.WriteLine(" ** formatter.StringFromNumber(" + product.Price + ") = " + formattedString + " for locale " + product.PriceLocale.LocaleIdentifier);
            return formattedString;
        }
    }
}
