using System;
public class Maybe<T> //The maybe is a really important class that either is something or nothing. It allows is to be sure that elements are not null and reminds us that that is possible.
{
    public T value {get; private set;}
    public bool is_some {get; private set;}

    public Maybe(T _val) {
        value = _val;
        is_some = true;
    }    
    public Maybe() {
        is_some = false;
    }

    

    public Maybe<A> fmap<A>(Func<T,A> _func) {
        if (is_some) return new Maybe<A>(_func(value));
        return new Maybe<A>();
    }

    public Maybe<A> bind<A>(Func<T,Maybe<A>> _func ) {
        if (is_some) return _func(value);
        return new Maybe<A>();
    }
}