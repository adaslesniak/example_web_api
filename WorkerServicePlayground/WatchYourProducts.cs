using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WorkerServicePlayground;


/// <summary>
/// this guy starts observing directories and in case of changes
/// in some .csv files in uploads them to web
/// after changing config it must be restarted
/// started tasks may still finish after app is stopped
/// </summary>
class WatchYourProducts : BackgroundService
{
    readonly ILogger<WatchYourProducts> log;
    readonly WebClient comm;
    readonly List<FileSystemWatcher> activeObservers = new();

    public WatchYourProducts(WebClient withComm, ILogger<WatchYourProducts> withLog) =>
        (comm, log) = (withComm, withLog);

    //TODO this must be configurable by user
    string[] ConfiguredPaths() => new[] {
        @"C:\projects\Courses\web-api-playground\test_for_watch"
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        var exitState = 0;
        try { //have no idea why this try catch is required but it's recomended by MS and I assume they know their product
            await Task.Run(async () => {
                //as of now reconfig requires restart, it could watch configuration file and do it on the go
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
        var watcher = new FileSystemWatcher(folderPath);
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.Changed += Upload; //this is called twice when copying file into destination
        watcher.Error += LogError;
        watcher.Filter = "*.csv";
        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;
        return watcher;
    }

    //TODO: it shouldn't be direct... we should give some time
    //(keep some queue, when was last modified and then if in this queue is that it was over 10 seconds, upload newest version)
    //and it's called twice
    void Upload(object _, FileSystemEventArgs fileEvent) =>
        Task.Factory.StartNew(() => {
            if(false == TryReadFile(fileEvent.FullPath, out var content)) {
                log.LogError($"filed to read file {fileEvent.FullPath}");
                return;
            }
            comm.UploadNewCsv(content, didSucceed => {
                if(didSucceed) {
                    log.LogInformation($"successfully uploaded file: {fileEvent.FullPath}");
                }
                else {
                    log.LogError($"Could not upload file: {fileEvent.FullPath}");
                }
            });
        });

    //This can be very long as we wait for user to close the file, he can have 
    //it opened (and therefore locked access to it) for hours or days
    bool TryReadFile(string path, out string content) {
        var maxTime = TimeSpan.FromHours(72);
        var waitingTime = TimeSpan.Zero;
        while(waitingTime < maxTime) {
            StreamReader fileAccess = null;
            if(false == File.Exists(path)) {
                content = string.Empty;
                return false;
            }
            try {
                fileAccess = File.OpenText(path);
                content = fileAccess.ReadToEnd();
                return true;
            } catch { }
            finally {
                fileAccess?.Close();
            }
        }
        content = string.Empty;
        return false;
    }

    void LogError(object _, ErrorEventArgs errorEvent) {
        if(errorEvent.GetException() is not Exception error) {
            return;
        }
        log.LogError((EventId)0, error, "Watcher was overwhelemed");
    }
}