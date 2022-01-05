namespace TelegramBotApi.Api;


public abstract class Markup
{
    public abstract string GetUrlString();
}

class ReplyKeyboard : Markup
{
    public ReplyKeyboard(List<List<string>> _Buttons = null, bool _ResizeKeyboard = true)
    {
        ResizeKeyboard = _ResizeKeyboard;
        Buttons = _Buttons;
    }
    public List<List<string>> Buttons { get; set; }
    public bool ResizeKeyboard { get; set; }

    public override string GetUrlString()
    {
        if (Buttons.Count == 0)
        {
            return null;
        }
        string TempString = "[";
        foreach (var TempList in Buttons)
        {
            TempString += "[\"" + String.Join("\",\"", TempList) + "\"],";
        }
        TempString += "]";
        string ReturnString = $"reply_markup={{\"keyboard\":{TempString.Replace("],]", "]]")},\"resize_keyboard\":{ResizeKeyboard.ToString().ToLower()}}}";
        return ReturnString;
    }
}

class InlineKeyboard : Markup
{
    
    private List<List<InlineButton>> _buttons { get; set; }
    
    public InlineKeyboard(List<List<InlineButton>> buttons)
    {
        _buttons = buttons;
    }

    public override string GetUrlString()
    {
        string tempString = "reply_markup={\"inline_keyboard\":[";

        foreach (var buttonsList in _buttons)
        {
            tempString += "[" +  buttonsList.Aggregate("", (current, item) => current + "," + item.GetUrlString(), x => x.Trim(',')) + "],";
        }
        tempString += "]}";
        tempString = tempString.Replace("],]", "]]");
        return tempString;
    }
}

class InlineButton
{
    private string _text { get; set; }
    private string _url { get; set; }
    private string _callbackData { get; set; }
    
    public InlineButton(string text, string url = null, string callbackData = null)
    {
        _text = text;
        _url = url;
        _callbackData = callbackData;
    }

    public string GetUrlString()
    {
        string tempString = _callbackData is not null ? $"\"callback_data\":\"{_callbackData}\"" : $"\"url\":\"{_url}\"";
        return $"{{\"text\":\"{_text}\", {tempString}}}";
    }
}



public class UpdateClass
{
    public long UpdateId { get; set; }
    public UpdateType UpdateType { get; set; }
    public Message Message { get; set;  }
    public CallbackQuery CallbackQuery { get; set; }
    
}

public class CallbackQuery
{
    public long Id { get; set; }
    public From From { get; set; }
    public Message Message { get; set; }
    public long ChatInstance { get; set; }
    public string Data { get; set; }
}

public class Message
{
    public long MessageId { get; set; }
    public long Date { get; set; }
    public string Text { get; set; }
    public From From { get; set; }
    public Chat Chat { get; set; }
}

public class From
{
    public long UserId { get; set; }
    public bool IsBot { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public string LanguageCode { get; set; }
}

public class Chat
{
    public long Id { get; set; }     // UserId
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public ChatType Type { get; set; }
}

public enum UpdateType : short
{
    message = 0,
    callback_query = 1
}

public enum ChatType : short
{
    Private = 0,
    Public = 1
}