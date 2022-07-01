using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Graph;


class ProtectedApiCallHelper
{

    public ProtectedApiCallHelper(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    public HttpClient HttpClient { get; private set; }

    public async Task CallWebApiAndProcessResultASync(string webApiUrl, string accessToken, Pass.Graph.EmailObject emailContext)
    {
        if (!string.IsNullOrEmpty(accessToken))
        {
            var defaultRequestHeaders = HttpClient.DefaultRequestHeaders;
            if (defaultRequestHeaders.Accept == null || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
            {
                HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);

            var jsonEmailContext = JsonConvert.SerializeObject(emailContext);
            var dataEmailContext = new StringContent(jsonEmailContext, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await HttpClient.PostAsync(webApiUrl, dataEmailContext);
            if (response.IsSuccessStatusCode)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("成功发送邮件");
                Console.ResetColor();

            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to call the Web Api: {response.StatusCode}");
                string content = await response.Content.ReadAsStringAsync();

                // Note that if you got reponse.Code == 403 and reponse.content.code == "Authorization_RequestDenied"
                // this is because the tenant admin as not granted consent for the application to call the Web API
                Console.WriteLine($"Content: {content}");
            }
            Console.ResetColor();
        }
    }
}

    

    
