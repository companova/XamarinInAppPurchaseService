﻿using System;

namespace Companova.Xamarin.InAppPurchase.Service
{
    /// <summary>
    /// Represents the Product
    /// </summary>
    [Preserve(AllMembers = true)]
    public class Product
    {
        /// <summary>
        /// Name of the product
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the product
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Product ID or Sku
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// Localized Price (not including tax)
        /// </summary>
        public string FormattedPrice { get; set; }

        /// <summary>
        /// ISO 4217 currency code for price. For example, if price is specified in British pounds sterling is "GBP".
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Price in micro-units, where 1,000,000 micro-units equal one unit of the 
        /// currency. For example, if price is "€7.99", price_amount_micros is "7990000". 
        /// This value represents the localized, rounded price for a particular currency.
        /// </summary>
        public long MicrosPrice { get; set; }

        /// <summary>
        /// Gets or sets the localized introductory price.
        /// </summary>
        /// <value>The localized introductory price.</value>
        public string LocalizedIntroductoryPrice { get; set; }

        /// <summary>
        /// Introductory price of the product in micor-units
        /// </summary>
        /// <value>The introductory price.</value>
        public long MicrosIntroductoryPrice { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Plugin.InAppBilling.Abstractions.InAppBillingProduct"/>
        /// has introductory price. This is an optional value in the answer from the server, requires a boolean to check if this exists
        /// </summary>
        /// <value><c>true</c> if has introductory price; otherwise, <c>false</c>.</value>
        public bool HasIntroductoryPrice => !string.IsNullOrEmpty(LocalizedIntroductoryPrice);

        /// <summary>
        /// Indicates the Purchase state of the Product.
        /// Active, Pending, Free or Unknown/Not Purchased
        /// Free could be used for promotional product (e.g. Links to other Apps)
        /// </summary>
        public ProductState State { get; set; }

        /// <summary>
        /// Source of the product Image/Logo
        /// </summary>
        public string ImageSource { get; set; }
    }
}
