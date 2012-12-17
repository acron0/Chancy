Chancy
======

Chancy is an event flow control system in C#; it facilitates the composition of complex event structures primarily for use in games, UIs and other visual applications.

The goal was to create an intuitive, flexible framework that was easy to read, even when things got complicated (a DSL would be ideal but C# 3.5 does not support it without tools).

The result:

    var event = Event.Create()
                     .Wait(1.0f)
                     .MoveToX(transform, 100.0f, 1.0f)
                     .Log("Hi, I'm here!")
                     .UniformScaleTo(transform, 2.0f, 0.5f)
                     
Evented actions are represented as extensions, making it infinitely extensible and customizable. As an example, here's the contents of the MoveToX function:

    public static Event MoveToX(this Event ev, Transform transform, float x, float duration)
    {
        float startX = transform.x;
        
        ev.Updated += (args) =>
        {
            float newX = ((x-startX) * Math.Min(1.0f, args.TotalTime / duration)) + startX;
            transform.x = newX;
            return args.TotalTime >= duration;
        };
        
        ev.Ended += (args) =>
        {
            transform.x = x;
        };
        
        return ev.Extend();
    }
                     
TODO:
 - Add sequence types: compound, chain, loop
 - Investigate to remove 'Event.Extend()' dependence.
