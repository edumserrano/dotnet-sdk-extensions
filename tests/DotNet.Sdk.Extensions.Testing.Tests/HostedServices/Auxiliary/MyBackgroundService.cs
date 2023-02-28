namespace DotNet.Sdk.Extensions.Testing.Tests.HostedServices.Auxiliary;

public class MyBackgroundService : BackgroundService
{
    private readonly ICalculator _calculator;

    public MyBackgroundService(ICalculator calculator)
    {
        _calculator = calculator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMilliseconds(1000);
#if NET6_0 || NET7_0
        using var timer = new PeriodicTimer(interval);
        while (!stoppingToken.IsCancellationRequested)
        {
            await timer.WaitForNextTickAsync(stoppingToken);
            _calculator.Sum(1, 1); // implement your logic, this doesn't make sense and is only for demo purposes
        }
#else
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(interval, stoppingToken);
            _calculator.Sum(1, 1); // implement your logic, this doesn't make sense and is only for demo purposes
        }
#endif
    }
}
