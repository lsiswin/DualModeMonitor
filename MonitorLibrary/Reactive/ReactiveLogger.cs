using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace MonitorLibrary.Reactive
{
    public class ReactiveLogger : IDisposable
    {
        private readonly Subject<LogEvent> _logSubject = new Subject<LogEvent>();
        private readonly IDisposable _disposable;

        // 暴露可订阅的日志流（只读，外部无法直接推送）
        public IObservable<LogEvent> LogEvents => _logSubject.AsObservable();

        public ReactiveLogger()
        {
            // 确保在订阅时自动处理异常（避免流中断）
            _disposable = _logSubject
                .ObserveOn(Scheduler.CurrentThread) // 确保在主线程处理（如UI）
                .Subscribe(
                    _ => { }, // 无操作（防止空订阅）
                    ex => Console.WriteLine($"[ReactiveLogger] Unhandled error: {ex}") // 全局错误处理
                );
        }

        // 便捷日志方法（与 ILogger 一致的接口风格）
        public void LogInformation(string message) => Log(LogLevel.Information, message);

        public void LogDebug(string message) => Log(LogLevel.Debug, message);

        public void LogWarning(string message) => Log(LogLevel.Warning, message);

        public void LogError(string message, Exception exception = null) =>
            Log(LogLevel.Error, message, exception);

        public void LogCritical(string message, Exception exception = null) =>
            Log(LogLevel.Critical, message, exception);

        // 核心日志方法（线程安全，通过 Subject 推送）
        public void Log(LogLevel level, string message, Exception exception = null)
        {
            _logSubject.OnNext(new LogEvent(level, message, exception));
        }

        public void Dispose()
        {
            _logSubject.OnCompleted(); // 通知所有订阅者流结束
            _disposable.Dispose(); // 清理内部订阅
            _logSubject.Dispose(); // 释放 Subject 资源
        }
    }
}
