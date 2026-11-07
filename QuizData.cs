using System.Text.Json;
using System.Text.Json.Serialization;

namespace DrCeuticsQuiz;

/// <summary>Cấu hình game – đọc từ khối "config" trong questions.json.</summary>
public sealed class QuizConfig
{
    public int QuestionsPerGame { get; set; } = 3;   // số câu mỗi lượt
    public int PassScore { get; set; } = 2;          // số câu đúng tối thiểu để nhận quà
    public int TimePerQuestion { get; set; } = 20;   // giây / câu
    public int AutoNextMs { get; set; } = 3500;      // tự chuyển câu sau khi hiện đáp án
    public int ResultTimeout { get; set; } = 45;     // giây – màn kết quả tự về trang chủ
    public int IdleTimeout { get; set; } = 60;       // giây – không ai chạm thì về trang chủ
}

public sealed class Question
{
    [JsonPropertyName("question")]
    public string Text { get; set; } = "";
    public List<string> Options { get; set; } = new();
    public int Answer { get; set; }                  // vị trí đáp án đúng (0 = đáp án đầu)
    public string Explain { get; set; } = "";
}

public sealed class QuizData
{
    public QuizConfig Config { get; set; } = new();
    public List<Question> Questions { get; set; } = new();

    static readonly JsonSerializerOptions Opts = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static QuizData Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "questions.json");
        try
        {
            if (File.Exists(path))
            {
                var d = JsonSerializer.Deserialize<QuizData>(File.ReadAllText(path), Opts);
                if (d is { Questions.Count: > 0 }) return d;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Lỗi đọc questions.json: " + ex.Message);
        }
        // Dự phòng nếu thiếu file
        return new QuizData
        {
            Questions =
            {
                new Question { Text = "Không đọc được questions.json – DrCeutics là thương hiệu của nước nào?",
                               Options = { "Việt Nam", "Hàn Quốc", "Pháp", "Nhật Bản" }, Answer = 0,
                               Explain = "Kiểm tra lại file questions.json cạnh file chạy." }
            }
        };
    }
}

/// <summary>Thống kê lượt chơi trong ngày (chạm logo 5 lần để xem).</summary>
public sealed class Stats
{
    public string Day { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");
    public int Plays { get; set; }
    public int Wins { get; set; }

    static string FilePath
    {
        get
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DrCeuticsQuiz");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "stats.json");
        }
    }

    public static Stats Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var s = JsonSerializer.Deserialize<Stats>(File.ReadAllText(FilePath));
                if (s != null && s.Day == DateTime.Today.ToString("yyyy-MM-dd")) return s;
            }
        }
        catch { /* bỏ qua, tạo mới */ }
        return new Stats();
    }

    public void Save()
    {
        try { File.WriteAllText(FilePath, JsonSerializer.Serialize(this)); } catch { }
    }
}
