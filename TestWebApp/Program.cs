using System.Collections.Concurrent;
using System.Timers;

namespace TestWebApp;
public class TestMicroservice
{
    public static int maxNumberOfMsgsPerPhoneNumber;
    public static int maxNumberOfMsgsPerSecondPerAccount;
    public static Dictionary<string, List<string>> accountPhones = new Dictionary<string, List<string>>();
    public static ConcurrentDictionary<string, int> numberOfMsgsSentFromPhone = new ConcurrentDictionary<string, int>();
    public static ConcurrentDictionary<string, int> numberOfMsgsSentFromAccount = new ConcurrentDictionary<string, int>();

    public static WebApplication app;
    private static System.Timers.Timer timer;
    //constructor:
    public TestMicroservice() { }

    private static void InitializeTheTestMicroservice()
    {
        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
        // Duplicate here any configuration sources you use.
        configurationBuilder.AddJsonFile("appsettings.json");

        IConfiguration configuration = configurationBuilder.Build();

        var Limitations = configuration.GetSection("Limitations");
        if (!Int32.TryParse(Limitations["MaxNumberOfMsgsPerPhoneNumber"], out maxNumberOfMsgsPerPhoneNumber))
        {
            throw new InvalidOperationException("MaxNumberOfMsgsPerPhoneNumber cannot be read from the config file");
        }
        if (!Int32.TryParse(Limitations["MaxNumberOfMsgsPerSecondPerAccount"], out maxNumberOfMsgsPerSecondPerAccount))
        {
            throw new InvalidOperationException("MaxNumberOfMsgsPerSecondPerAccount cannot be read from the config file");
        }

        timer = new System.Timers.Timer(1000);
        timer.Elapsed += ResetAccountMsgCountersEverySecond;
        // Enable the Timer
        timer.Enabled = true;
    }

    static void Main(string[] args)
    {
        //TestMicroservice testMicroservice = new TestMicroservice();
        InitializeTheTestMicroservice();
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        //builder.Services.AddOpenApi();

        //builder.Services.AddFeatureManagement();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        app = builder.Build();
        app.Logger.LogInformation("Starting the app");

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            //app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }

    public static void ResetAccountMsgCountersEverySecond(Object source, ElapsedEventArgs e)
    {
        app.Logger.LogInformation("Reset counters for accounts but don't reset counters for phones as there is no requirement for that in the assignment");
        foreach (var accountId in accountPhones.Keys)
        {
            numberOfMsgsSentFromAccount[accountId] = 0;
        }
    }
}

