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
        try
        {
            var interval = TimeSpan.FromMilliseconds(200);
            var timer = new RxPeriodicTimer(interval, _scheduler);
            do
            {
                await timer.WaitForNextTickAsync(stoppingToken);
                _calculator.Sum(1, 1); // implement your logic, this doesn't make sense and is only for demo purposes
            }
            while (!stoppingToken.IsCancellationRequested);
        }
        catch (OperationCanceledException)
        {
            // ignore, do nothing if when the Task.Delay throws exception because the host is being terminated
        }
    }
}
