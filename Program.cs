using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Simple.OData.Client;

namespace ODataTestConsoleApp
{
    class Program
    {
        public static async Task<HttpResponseMessage> GetToken()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://login.microsoftonline.com/rfsuy.com/oauth2/v2.0/token");
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", "your_client_id"),
                new KeyValuePair<string, string>("client_secret", "your_client_secret"),
                new KeyValuePair<string, string>("scope", "https://usnconeboxax1aos.cloud.onebox.dynamics.com/.default"),
            });
            return await client.SendAsync(request);
        }

        static async Task Main(string[] args)
        {
            HttpResponseMessage responseToken;

            Console.WriteLine("Indique con números una de las siguientes opciones:");
            Console.WriteLine("1. Consultar entidad de artistas sin libreria");
            Console.WriteLine("2. Crear un nuevo artista sin libreria");
            Console.WriteLine("3. Consultar entidad de artistas con libreria");
            Console.WriteLine("4. Crear un nuevo artista con libreria");
            Console.WriteLine("5. Consultar la entidad artista con sus obras asociadas con libreria");
            var responseUser = Console.ReadLine();

            switch (responseUser)
            {
                case ("1"):
                    responseToken = await GetToken();
                    if (responseToken.IsSuccessStatusCode)
                    {
                        var content = await responseToken.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<dynamic>(content);
                        string varAccessToken = data.access_token;
                        var newClient = new HttpClient();
                        newClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", varAccessToken);


                        var queryParams = new Dictionary<string, string>
                        {
                            // Descomentar si se quiere agregar un filtro por algun campo, por ejemplo filtrar x DataAreaId = SAM
                            //{ "$filter", "dataAreaId%20eq%20%27sam%27" },
                            { "cross-company", "true" }
                        };
                        var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                        var requestUri = $"https://usnconeboxax1aos.cloud.onebox.dynamics.com/data/DanielArtistTable2E?{queryString}";

                        var newResponse = await newClient.GetAsync(requestUri);
                        if (newResponse.IsSuccessStatusCode)
                        {
                            var newContent = await newResponse.Content.ReadAsStringAsync();
                            var oDataContent = JsonConvert.DeserializeObject<dynamic>(newContent);
                            Console.WriteLine(oDataContent);
                        }
                        else
                        {
                            Console.WriteLine("Request failed: " + responseToken.StatusCode);
                        }

                    }
                    else
                    {
                        Console.WriteLine("Request failed with status code: " + responseToken.StatusCode);
                    }
                    break;

                case ("2"):
                    responseToken = await GetToken();
                    if (responseToken.IsSuccessStatusCode)
                    {
                        var content = await responseToken.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<dynamic>(content);
                        string varAccessToken = data.access_token;

                        var newClient = new HttpClient();
                        newClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", varAccessToken);

                        Artist artist = new Artist();
                        Console.WriteLine("Ingrese el ID del artista");
                        artist.ArtistId = Console.ReadLine();
                        Console.WriteLine("Ingrese el nombre del artista");
                        artist.Name = Console.ReadLine();

                        var newClientJSON = JsonConvert.SerializeObject(artist);

                        Console.WriteLine(newClientJSON.ToString());

                        var requestContent = new StringContent(newClientJSON, Encoding.UTF8, "application/json");

                        var newResponse = await newClient.PostAsync("https://usnconeboxax1aos.cloud.onebox.dynamics.com/data/DanielArtistTable2E", requestContent);

                        //string responseContent = await newResponse.Content.ReadAsStringAsync();

                        if (newResponse.IsSuccessStatusCode)
                        {
                            Console.WriteLine("Se ha generado el artista con éxito");
                        }

                    }
                    else
                    {
                        Console.WriteLine("Request failed with status code: " + responseToken.StatusCode);
                    }

                    Console.ReadLine();                    
                    break;

                case ("3"):
                    responseToken = await GetToken();
                    if (responseToken.IsSuccessStatusCode)
                    {
                        var content = await responseToken.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<dynamic>(content);
                        string varAccessToken = data.access_token;

                        var settings = new ODataClientSettings(new Uri("https://usnconeboxax1aos.cloud.onebox.dynamics.com/data/"));
                        settings.BeforeRequest += delegate (HttpRequestMessage message)
                        {
                            message.Headers.Add("Authorization", "Bearer " + varAccessToken);
                        };
                        var client2 = new ODataClient(settings);
                        var artists = await client2.For("DanielArtistTable2E").FindEntriesAsync();
                        foreach (var artist in artists)
                        {
                            Console.WriteLine(artist["Name"]);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Request failed with status code: " + responseToken.StatusCode);
                    }
                    
                    break;


                case ("4"):
                    responseToken = await GetToken();
                    if (responseToken.IsSuccessStatusCode)
                    {
                        var content = await responseToken.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<dynamic>(content);
                        string varAccessToken = data.access_token;

                        Artist artist = new Artist();
                        Console.WriteLine("Ingrese el ID del artista");
                        artist.ArtistId = Console.ReadLine();
                        Console.WriteLine("Ingrese el nombre del artista");
                        artist.Name = Console.ReadLine();


                        var settings = new ODataClientSettings(new Uri("https://usnconeboxax1aos.cloud.onebox.dynamics.com/data/"));
                        settings.BeforeRequest += delegate (HttpRequestMessage message)
                        {
                            message.Headers.Add("Authorization", "Bearer " + varAccessToken);
                        };
                        var client2 = new ODataClient(settings);

                        try
                        {
                            var artistPost = await client2
                                .For("DanielArtistTable2E")
                                .Set(new { ArtistId = artist.ArtistId, Name = artist.Name })
                                .InsertEntryAsync();
                            Console.WriteLine(artistPost["Name"]);
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }

                    }
                    else
                    {
                        Console.WriteLine("Request failed with status code: " + responseToken.StatusCode);
                    }

                    Console.ReadLine();
                    break;


                case ("5"):
                    responseToken = await GetToken();
                    if (responseToken.IsSuccessStatusCode)
                    {
                        var content = await responseToken.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<dynamic>(content);
                        string varAccessToken = data.access_token;

                        var settings = new ODataClientSettings(new Uri("https://usnconeboxax1aos.cloud.onebox.dynamics.com/data/"));
                        settings.BeforeRequest += delegate (HttpRequestMessage message)
                        {
                            message.Headers.Add("Authorization", "Bearer " + varAccessToken);
                        };
                        var client2 = new ODataClient(settings);
                        var artists = await client2
                            .For("DanielArtistTable2E")
                            .Expand("DanielPieceTable2E")
                            .FindEntriesAsync();

                        Console.WriteLine(JsonConvert.SerializeObject(artists));

                    }
                    else
                    {
                        Console.WriteLine("Request failed with status code: " + responseToken.StatusCode);
                    }

                    break;


                default:
                    Console.WriteLine("La opción ingresada es incorrecta");
                    break;

            }
        }
    }
}