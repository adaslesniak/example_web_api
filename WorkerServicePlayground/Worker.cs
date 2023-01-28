using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WorkerServicePlayground;

class WatchYourProducts : BackgroundService
{
    readonly ILogger<WatchYourProducts> log;
    readonly WebClient comm;

    internal WatchYourProducts(WebClient withComm, ILogger<WatchYourProducts> withLog) => 
        (comm, log) = (withComm, withLog);

    List<FileSystemWatcher> activeObservers = new();

    //TODO this must be configurable by user
    string[] ConfiguredPaths() => new[] {
        @"C:\projects\Courses\web-api-playground\test_for_watch"
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        var exitState = 0;
        try { //have no idea why this try catch is required but it's recomended by MS and I assume they know their product
            await Task.Run(() => {
                //TODO: as of now reconfig requires restart, it should watch configuration file and do it on the go
                ReloadObservers();
                stoppingToken.WaitHandle.WaitOne();
            });
        }
        catch(Exception error) {
            log.LogError(error, "{Message}", error.Message);
            exitState = 1;
        }
        finally {

            foreach(var existing in new List<FileSystemWatcher>(activeObservers)) {
                KillObserver(existing);
            }
            Environment.Exit(exitState);
        }
    }

    void ReloadObservers() {
        var pathsToObserve = ConfiguredPaths();
        foreach(var path in pathsToObserve) {
            if(activeObservers.FirstOrDefault(observer => observer.Path == path) is not null) {
                continue;
            }
            activeObservers.Add(StartWatch(path));
        }
        var obsolete = activeObservers.Where(observer => false == pathsToObserve.Contains(observer.Path));
        foreach(var outdated in obsolete) {
            KillObserver(outdated);
        }
    }

    void KillObserver(FileSystemWatcher junk) {
        junk.EnableRaisingEvents = false;
        junk.Dispose();
        activeObservers.Remove(junk);
    }

    FileSystemWatcher StartWatch(string folderPath) {
        using var watcher = new FileSystemWatcher(folderPath);
        watcher.NotifyFilter = NotifyFilters.CreationTime
                             | NotifyFilters.LastWrite
                             | NotifyFilters.Size;
        watcher.Changed += SendToWeb;
        watcher.Created += SendToWeb;
        watcher.Error += LogError;
        watcher.Filter = "*.csv";
        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;
        return watcher;
    }

    void SendToWeb(object _, FileSystemEventArgs fileEvent) =>
        Task.Factory.StartNew(() => {
            try {
                var content = string.Empty;
                using(var fileAccess = new StreamReader(fileEvent.FullPath)) {
                    content = fileAccess.ReadToEnd();
                }

                comm.UploadNewData(content, inCaseOfProblems: () => {
                    log.LogError($"Could not upload file: {fileEvent.FullPath}");
                });
            } catch(Exception error) {
                log.LogError((EventId)0, error, "Can't access file");
            }
        });

    void LogError(object _, ErrorEventArgs errorEvent) {
        if(errorEvent.GetException() is not Exception error) {
            return;
        }
        log.LogError((EventId)0, error, "Watcher was overwhelemed");
    }
}