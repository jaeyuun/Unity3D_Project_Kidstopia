using OpenApiFormat;
using System;
using System.IO;
using System.Text; // encoding
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerRequest
{
    public string player_id;
    public string role;
    public string message;

}

public class OpenAIRequest
{
    public string openAi_key = "Secret Key"; // api secret key

    private static HttpClient client;

    public delegate void StringEvnet(string _string);
    public StringEvnet completedRepostEvent;

    private string api_URL = "";
    private const string authoirzationHeader = "Bearer";
    private const string userAgentHeader = "User-Agent";

    private string body_json = string.Empty;

    public void Init()
    {
        CreateHttpClient();
    }

    private void CreateHttpClient()
    {
        client = new HttpClient();
        // HttpRequestHeaders Setting
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(authoirzationHeader, openAi_key);
        client.DefaultRequestHeaders.Add(userAgentHeader, "okgodoit/dotnet_openai_api"); // gpt-3.5-turbo sdk, https://github.com/OkGoDoIt/OpenAI-API-dotnet
    }

    private async Task<string> ClientResponse<SendData>(SendData request) // ChatRequest or STTRequest
    {
        if (client == null)
        {
            CreateHttpClient();
        }
        api_URL = ((URL)request).Get_API_URL(); // URL 가져오기        



        string jsonContent = string.Empty;
        StringContent stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        Debug.Log(api_URL);
        Debug.Log(stringContent);

        // 데이터 전송 후 대기
        using(HttpResponseMessage response = await client.PostAsync(api_URL, stringContent)) // only post
        {
            if (response.IsSuccessStatusCode)
            { // 응답 성공
                return await response.Content.ReadAsStringAsync(); // string으로 return
            }
            else
            { // 응답 실패
                throw new HttpRequestException($"Error calling OpenAI API to get completion. HTTP status code: {response.StatusCode}. Request body: {jsonContent}");
            }
        }
    }

    public void ResponseJson(string player_id, string player_audio)
    {
        string request_body = string.Empty;
        try
        {
            if (!ConnectionCheck(connection))
            {
                return;
            }

            // player role, message body =
            request_body = string.Format(@"UPDATE chatGPT_request SET stringContent = json_object(
                                           'role', 'user',
                                           'message', '{0}'
                                           WHERE 'player_id' = '{1}';",
                                           player_audio, player_id
                                           );

            MySqlCommand cmd = new MySqlCommand(request_body, connection);
            reader = cmd.ExecuteReader();
            if (!reader.IsClosed)
            {
                reader.Close();
                return;
            }


        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            if (!reader.IsClosed)
            {
                reader.Close();
                return;
            }
        }
    }
}
