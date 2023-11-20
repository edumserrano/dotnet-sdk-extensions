namespace DotNet.Sdk.Extensions.Testing.Tests.HostedServices.Auxiliary;

public class MyBackgroundService(ICalculator calculator, IScheduler scheduler): BackgroundService
{
    private readonly ICalculator _calculator = calculator;
    private readonly IScheduler _scheduler = scheduler;

    public static TimeSpan Period => TimeSpan.FromMilliseconds(100);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var timer = new RxPeriodicTimer(Period, _scheduler);
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
