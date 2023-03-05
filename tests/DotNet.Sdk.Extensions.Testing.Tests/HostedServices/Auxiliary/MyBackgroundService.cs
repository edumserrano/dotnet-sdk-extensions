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

    public static TimeSpan Period => TimeSpan.FromMilliseconds(500);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // using RX so I can time travel on tests with the TestScheduler.
            // with it I can simulate time passing quicker instead of having to wait
            // for the defined interval.
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
