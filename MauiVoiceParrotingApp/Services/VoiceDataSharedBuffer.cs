namespace MauiVoiceParrotingApp.Services;

internal class VoiceDataSharedBuffer
{
    public readonly object Lock = new object();

    private Type dataType;
    private object dataObject;


    public void Set<T>(T dataObject)
    {
        dataType = typeof(T);
        this.dataObject = dataObject;
    }

    public T Get<T>()
    {
        if (dataObject == null) throw new NullReferenceException();
        if (typeof(T) != dataType) throw new InvalidCastException();

        return (T)dataObject;
    }

    //~VoiceDataSharedBuffer() 
    //{
        
    //}

    
}
