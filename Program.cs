using TelegramBotApi.Api;

// Import console from RadLibrary 
using Console = RadLibrary.RadConsole.RadConsole;


namespace Tests
{
    public class Tests
    {
        public static async Task<int> Main()
        {
            Console.WriteLine("[green]Tests run!");
            string token = "token";

            // init bot
            Bot bot = new Bot(token);
            
            // test chat id 
            long ChatId = 000000000;
            
            // test reply markup
            List<List<string>> Buttons = new List<List<string>>();
            Buttons.Add(new List<string>{"Первая кнопка", "Вторая кнопка"});
            Buttons.Add(new List<string>{"Третья кнопка", "4я кнопка"});
            ReplyKeyboard kb = new ReplyKeyboard {ResizeKeyboard = true, Buttons = Buttons};

            // test inline keyboard and buttons
            InlineButton button = new InlineButton("тест", url:"https://google.com");
            Console.WriteLine("[yellow]Test button: " + button.GetUrlString());
            List<List<InlineButton>> InlineButtons = new List<List<InlineButton>>();
            InlineButtons.Add(new List<InlineButton> {button});
            InlineKeyboard inlineKeyboard = new InlineKeyboard(InlineButtons);

            // test send message
            Message message =  await bot.sendMessage(ChatId, "***Жирный текст***", ParseMode:ParseMode.Markdown, ProtectContent:false, markup:inlineKeyboard);
            Console.WriteLine($"[yellow]Send message. ID: {message.MessageId}");
            
            // test edit message text
            var editData = await bot.editMessageText(message.Chat.Id, message.MessageId, "Изменился", markup: inlineKeyboard);
            Console.WriteLine($"[yellow]Edit message.");
            
            
            
            return 1;
        }
    }
}
