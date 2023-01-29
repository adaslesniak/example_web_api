namespace WorkerServicePlayground;

public class LaunchConfig
{
    internal string[] FoldersToObserve { get; }

    public LaunchConfig(string[] rawArguments) {
        FoldersToObserve = rawArguments.Where(argument =>
            Directory.Exists(argument)).ToArray();FoldersToObserve = new[] { @"C:\projects\Courses\web-api-playground\test_for_watch" };
    }

}
