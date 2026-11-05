using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using MiniMarketApp.Settings;

namespace MiniMarketApp.Services.Payments;

public class PayPhoneApiLinkService
{
    private readonly HttpClient _httpClient;
    private readonly PayPhoneSettings _settings;

    public PayPhoneApiLinkService(
        HttpClient httpClient,
        IOptions<PayPhoneSettings> options)
    {
        _httpClient = httpClient;
        _settings = options.Value;
    }

    public async Task<string> CreatePaymentLinkAsync(
        decimal total,
        string clientTransactionId,
        string reference)
    {
        int amountInCents = (int)Math.Round(
            total * 100,
            MidpointRounding.AwayFromZero);

        var request = new PayPhoneLinkRequest
        {
            Amount = amountInCents,
            AmountWithoutTax = amountInCents,
            AmountWithTax = 0,
            Tax = 0,
            Currency = "USD",
            ClientTransactionId = clientTransactionId,
            Reference = reference,
            StoreId = null // Forzamos a nulo para omitirlo en la petición
        };
        
        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "https://pay.payphonetodoesposible.com/api/Links");

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _settings.Token);

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var requestJson = System.Text.Json.JsonSerializer.Serialize(request, options);
        Console.WriteLine("ENVIANDO A PAYPHONE: " + requestJson);

        httpRequest.Content = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(httpRequest);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"PayPhone respondió con error: {content}");
        }

        return content.Trim('"');
    }
}
