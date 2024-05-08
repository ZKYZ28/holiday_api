using Holiday.Api.Repository.CustomErrors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Holiday.Api.Core.Utilities
{
    public class LocationValidator
    {
        private const string GoogleApiKey = "AIzaSyDIE6IRIvdtr9OBehphohaLyT_TNQ-ltZE";

        public static async Task<bool> IsAddressValidAsync(string address)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string encodedAddress = Uri.EscapeDataString(address);
                    string apiUrl = $"https://maps.googleapis.com/maps/api/geocode/json?address={encodedAddress}&key={GoogleApiKey}";

                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        
                        JObject jsonResponse = JObject.Parse(responseBody);
                        
                        if (jsonResponse["status"].ToString() == "OK" && jsonResponse["results"].HasValues)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                throw new LocationException("Une erreur inattendue s'est produite lors de la validation de l'adresse.", ex);
            }
            catch (JsonException ex)
            {
                throw new LocationException("Une erreur inattendue s'est produite lors de la validation de l'adresse.", ex);
            }
            catch (Exception ex)
            {
                throw new LocationException("Une erreur inattendue s'est produite lors de la validation de l'adresse.", ex);
            }
            return false;
        }
    }
    
}