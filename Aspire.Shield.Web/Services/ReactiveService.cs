using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Aspire.Shield.Web.Model;

namespace Aspire.Shield.Web.Services;

public class ReactiveService : IDisposable
{
    private readonly Subject<SampleModel> _subject = new();

    public IAsyncObservable<SampleModel> Observable => 
        _subject
            .ObserveOn(TaskPoolScheduler.Default)
            .ToAsyncObservable();

    public void Dispose() => _subject.Dispose();

    public void OnNext(SampleModel sample) => _subject.OnNext(sample);
}