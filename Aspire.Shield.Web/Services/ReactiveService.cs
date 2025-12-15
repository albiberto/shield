using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Aspire.Shield.Web.Model;

namespace Aspire.Shield.Web.Services;

public class ReactiveService : IDisposable
{
    // ReplaySubject mantiene gli ultimi 50 eventi in buffer
    // Quando un nuovo subscriber si connette, riceve subito questi eventi
    // Poi continua a ricevere i nuovi eventi in tempo reale
    private readonly ReplaySubject<SampleModel> _subject = new(bufferSize: 50);

    public IAsyncObservable<SampleModel> Observable => 
        _subject
            .ObserveOn(TaskPoolScheduler.Default)
            .ToAsyncObservable();

    public void Dispose() => _subject.Dispose();

    public void OnNext(SampleModel sample) => _subject.OnNext(sample);
}