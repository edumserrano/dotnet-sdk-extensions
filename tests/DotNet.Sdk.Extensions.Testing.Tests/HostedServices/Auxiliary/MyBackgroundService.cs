namespace DotNet.Sdk.Extensions.Testing.Tests.HostedServices.Auxiliary;

public class MyBackgroundService : BackgroundService
{
    private readonly ICalculator _calculator;
    private readonly IScheduler _scheduler;

    public MyBackgroundService(ICalculator calculator, IScheduler scheduler)
    {
        _calculator = calculator;
        _scheduler = scheduler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMilliseconds(500);
        Observable
            .Interval(interval, _scheduler)
            .Subscribe(_ =>
            {
                _calculator.Sum(1, 1); // implement your logic, this doesn't make sense and is only for demo purposes
            }, stoppingToken);
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // ignore, do nothing if when the Task.Delay throws exception because the host is being terminated
        }
    }
}
