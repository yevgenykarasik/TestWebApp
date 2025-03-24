/*
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
*/

using System.Collections.Concurrent;
using System.Timers;

namespace TestWebApp;
public class TestMicroservice
{
    public static int maxNumberOfMsgsPerPhoneNumber;
    public static int maxNumberOfMsgsPerSecondPerAccount;
    public static Dictionary<string, List<string>> listOfTheAccountPhones = new Dictionary<string, List<string>>();
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

        var accountsPropConfig = configuration.GetSection("Accounts").GetChildren();
        /*
         * The children are {
         *                   "Account1": {"Phones": [ "6138280000", "6138280001" ]},
         *                   "Account2": {"Phones": [ "6132610000", "6132610001" ]}
         *                  }
         */

        foreach (var accountPropConf in accountsPropConfig)
        {
            var accountProps = accountPropConf.GetChildren();
            //The first child is {"Phones": [ "6138280000", "6138280001" ]}
            var accountId = accountPropConf.Key;
            var phonesOfTheAccount = new List<string>();

            foreach (var accountProp in accountProps)
            {
                foreach (var x in accountProp.GetChildren())
                {
                    if (accountProp.Key == "Phones")
                    {
                        var phoneNumber = x.Value;
                        phonesOfTheAccount.Add(phoneNumber);
                        numberOfMsgsSentFromPhone[phoneNumber] = 0;
                    }
                }
            }
            listOfTheAccountPhones.Add(accountId, phonesOfTheAccount);
        }

        foreach (var accountId in listOfTheAccountPhones.Keys)
        {
            numberOfMsgsSentFromAccount[accountId] = 0;
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
        foreach (var accountId in listOfTheAccountPhones.Keys)
        {
            long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            app.Logger.LogInformation(milliseconds + " reset counters for accounts but don't reset counters for phones as there is no requirement for that in the assignment");
            numberOfMsgsSentFromAccount[accountId] = 0;
        }
    }
}

