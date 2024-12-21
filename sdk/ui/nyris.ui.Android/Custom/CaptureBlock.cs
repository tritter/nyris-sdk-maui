using Kotlin.Jvm.Functions;
using Object = Java.Lang.Object;

namespace Nyris.UI.Android.Custom;

public class CaptureBlock<T> : Object, IFunction1 where T : Object
{
    private readonly Action<T> OnInvoked;

    public CaptureBlock(Action<T> onInvoked)
    {
        OnInvoked = onInvoked;
    }

    public Object Invoke(Object? objParameter)
    {
        try
        {
            T parameter = (T)objParameter;
            OnInvoked.Invoke(parameter);
            return null;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}