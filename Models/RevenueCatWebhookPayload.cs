using System.Text.Json.Serialization;

namespace PhotoAiBackend.Models;

public class RevenueCatWebhookPayload
{
    [JsonPropertyName("api_version")]
    public string ApiVersion { get; set; }

    [JsonPropertyName("event")]
    public RevenueCatEvent Event { get; set; }
}

public class RevenueCatEvent
{
    [JsonPropertyName("aliases")]
    public List<string> Aliases { get; set; }

    [JsonPropertyName("app_id")]
    public string AppId { get; set; }

    [JsonPropertyName("app_user_id")]
    public string AppUserId { get; set; }

    [JsonPropertyName("commission_percentage")]
    public double? CommissionPercentage { get; set; }

    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    [JsonPropertyName("entitlement_id")]
    public string EntitlementId { get; set; }

    [JsonPropertyName("entitlement_ids")]
    public List<string> EntitlementIds { get; set; }

    [JsonPropertyName("environment")]
    public string Environment { get; set; }

    [JsonPropertyName("event_timestamp_ms")]
    public long EventTimestampMs { get; set; }

    [JsonPropertyName("expiration_at_ms")]
    public long? ExpirationAtMs { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("is_family_share")]
    public bool? IsFamilyShare { get; set; }

    [JsonPropertyName("metadata")]
    public object Metadata { get; set; }

    [JsonPropertyName("offer_code")]
    public string OfferCode { get; set; }

    [JsonPropertyName("original_app_user_id")]
    public string OriginalAppUserId { get; set; }

    [JsonPropertyName("original_transaction_id")]
    public string OriginalTransactionId { get; set; }

    [JsonPropertyName("period_type")]
    public string PeriodType { get; set; }

    [JsonPropertyName("presented_offering_id")]
    public string PresentedOfferingId { get; set; }

    [JsonPropertyName("price")]
    public double? Price { get; set; }

    [JsonPropertyName("price_in_purchased_currency")]
    public double? PriceInPurchasedCurrency { get; set; }

    [JsonPropertyName("product_id")]
    public string ProductId { get; set; }

    [JsonPropertyName("purchased_at_ms")]
    public long? PurchasedAtMs { get; set; }

    [JsonPropertyName("renewal_number")]
    public int? RenewalNumber { get; set; }

    [JsonPropertyName("store")]
    public string Store { get; set; }

    [JsonPropertyName("subscriber_attributes")]
    public Dictionary<string, SubscriberAttribute> SubscriberAttributes { get; set; }

    [JsonPropertyName("takehome_percentage")]
    public double? TakehomePercentage { get; set; }

    [JsonPropertyName("tax_percentage")]
    public double? TaxPercentage { get; set; }

    [JsonPropertyName("transaction_id")]
    public string TransactionId { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }
}

public class SubscriberAttribute
{
    [JsonPropertyName("updated_at_ms")]
    public long UpdatedAtMs { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}