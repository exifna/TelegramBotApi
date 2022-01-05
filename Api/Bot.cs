using System.Text.Json;

namespace TelegramBotApi.Api;

public class Bot
{
    public Bot(string _botToken)
    {
        BotToken = _botToken;
        BaseUrl = $"https://api.telegram.org/bot{BotToken}";
        Client = new HttpClient();
    }

    private string BotToken { get; }
    private string BaseUrl { get; }
    private HttpClient Client { get; }

    public async Task<Dictionary<string, object>> getNewEvents()
    {
        var Ok = true;
        try
        {
            var Url = $"{BaseUrl}/getUpdates";

            var ServerData = await Client.GetStringAsync(Url);
            var TempraryData = JsonSerializer.Deserialize<Dictionary<string, object>>(ServerData);
            TempraryData["ok"] = bool.Parse(TempraryData["ok"].ToString());
            Ok = (bool) TempraryData["ok"];
            var result = new List<UpdateClass>();
            try
            {
                TempraryData["result"] =
                    JsonSerializer.Deserialize<List<Dictionary<string, object>>>(TempraryData["result"].ToString());
                var TempVariable = (List<Dictionary<string, object>>) TempraryData["result"];
                for (var i = 0; i < TempVariable.Count; i++)
                {
                    // Get update type.
                    var updateType = TempVariable[i].ContainsKey("message")
                        ? UpdateType.message
                        : UpdateType.callback_query;

                    // Get "from" data.
                    var TempData = (Dictionary<string, object>) JsonSerializer.Deserialize<Dictionary<string, object>>(
                        TempVariable[i][updateType == UpdateType.message ? "message" : "callback_query"].ToString());

                    Message mess = null;
                    try
                    {
                        mess = convertDictionaryToMessage(TempData);
                    }
                    catch
                    {
                        mess = convertDictionaryToMessage(
                            JsonSerializer.Deserialize<Dictionary<string, object>>(TempData["message"].ToString()));
                    }

                    // Generate final data.
                    long UpdateId = int.Parse(TempVariable[i]["update_id"].ToString());
                    var Update = new UpdateClass();
                    if (updateType == UpdateType.message)
                    {
                        Update = new UpdateClass
                        {
                            UpdateType = updateType,
                            UpdateId = UpdateId,
                            CallbackQuery = null,
                            Message = mess
                        };
                        result.Add(Update);
                    }

                    if (updateType == UpdateType.callback_query)
                    {
                        Update = new UpdateClass
                        {
                            UpdateType = updateType,
                            UpdateId = UpdateId,
                            Message = null,
                            CallbackQuery = new CallbackQuery
                            {
                                Id = long.Parse(
                                    JsonSerializer.Deserialize<Dictionary<string, object>>(
                                        TempVariable[i]["callback_query"].ToString())["id"].ToString()),
                                ChatInstance =
                                    long.Parse(JsonSerializer.Deserialize<Dictionary<string, object>>(
                                        TempVariable[i]["callback_query"].ToString())["chat_instance"].ToString()),
                                Data = JsonSerializer.Deserialize<Dictionary<string, object>>(
                                    TempVariable[i]["callback_query"].ToString())["data"].ToString(),
                                From = mess.From,
                                Message = mess
                            }
                        };
                        result.Add(Update);
                    }

                    try
                    {
                        Url =
                            $"{BaseUrl}/getUpdates?offset={result[result.Count - 1].UpdateId + 1}";
                        await Client.GetAsync(Url);
                    }
                    catch
                    {
                    }
                }

                TempraryData["result"] = result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                TempraryData["result"] = null;
            }

            return new Dictionary<string, object>
            {
                {"ok", Ok},
                {"result", TempraryData["result"]}
            };
        }
        catch (Exception e)
        {
            return new Dictionary<string, object> {{"ok", false}, {"result", new List<Dictionary<string, object>>()}};
        }
    }

    public async Task<Message> sendMessage(long ChatId, string Text, Markup markup = null,
        ParseMode ParseMode = ParseMode.Unknown, bool DisableWebPreview = true, bool ProtectContent = false)
    {
        var ParseModeUrl = ParseMode is ParseMode.Markdown ? "&parse_mode=markdown" :
            ParseMode is ParseMode.HTML ? "&parse_mode=HTML" : "";


        var MarkupUrl = markup != null ? "&" + markup.GetUrlString() : "";
        var Url =
            $"{BaseUrl}/sendMessage?chat_id={ChatId}&text={Text}{MarkupUrl}{ParseModeUrl}&disable_web_page_preview={DisableWebPreview}&protect_content={ProtectContent}";

        var ReturnedData = await Client.GetStringAsync(Url);
        var JsonData = JsonSerializer.Deserialize<Dictionary<string, object>>(
            JsonSerializer.Deserialize<Dictionary<string, object>>(ReturnedData)["result"].ToString());
        return convertDictionaryToMessage(JsonData);
    }

    public async Task<Message> editMessageText(long ChatId, long MessageId, string Text,
        ParseMode parseMode = ParseMode.Unknown, bool DisableWebPreview = false, Markup markup = null)
    {
        var ParseModeUrl = parseMode is ParseMode.Markdown ? "&parse_mode=markdown" :
            parseMode is ParseMode.HTML ? "&parse_mode=HTML" : "";

        string markupUrl = markup is not null ? "&" + markup.GetUrlString() : "";
        
        var Url = $"{BaseUrl}/editMessageText?chat_id={ChatId}&message_id={MessageId}&text={Text}{ParseModeUrl}&disable_web_page_preview={DisableWebPreview}{markupUrl}";
        return convertDictionaryToMessage( JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Deserialize<Dictionary<string, object>>(await Client.GetStringAsync(Url))["result"].ToString()));
    }

    public async Task<Dictionary<string, object>> deleteMessage(long ChatId, long MessageId)
    {
        return JsonSerializer.Deserialize<Dictionary<string, object>>(
            await Client.GetStringAsync(
                $"{BaseUrl}/deleteMessage?chat_id={ChatId}&message_id={MessageId}"));
    }

    public Message convertDictionaryToMessage(Dictionary<string, object> inputData)
    {
        var FromData =
            JsonSerializer.Deserialize<Dictionary<string, object>>(inputData["from"].ToString());
        var ChatData =
            JsonSerializer.Deserialize<Dictionary<string, object>>(inputData["chat"].ToString());
        var from = new From
        {
            UserId = long.Parse(FromData["id"].ToString()),
            IsBot = bool.Parse(FromData["is_bot"].ToString()),
            FirstName = FromData["first_name"].ToString(),
            LastName = FromData.ContainsKey("last_name") ? FromData["last_name"].ToString() : null,
            Username = FromData.ContainsKey("username") ? FromData["username"].ToString() : null,
            LanguageCode = FromData.ContainsKey("language_code") ? FromData["language_code"].ToString() : null
        };
        var chat = new Chat
        {
            Id = long.Parse(ChatData["id"].ToString()),
            FirstName = ChatData["first_name"].ToString(),
            LastName = ChatData.ContainsKey("last_name") ? ChatData["last_name"].ToString() : null,
            Type = ChatData["type"].ToString() == "private" ? ChatType.Private : ChatType.Public,
            Username = ChatData.ContainsKey("username") ? ChatData["username"].ToString() : null
        };
        var _message = new Message
        {
            MessageId = long.Parse(inputData["message_id"].ToString()),
            Date = long.Parse(inputData["date"].ToString()),
            Text = inputData["text"].ToString(),
            From = from,
            Chat = chat
        };
        return _message;
    }
}

public enum ParseMode : short
{
    HTML = 0,
    Markdown = 1,
    Unknown = 110
}