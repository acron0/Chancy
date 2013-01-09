Chancy
======

Chancy is a closure-based event sequencing system in C#; it facilitates the composition of complex event structures primarily for use in games, UIs and other visual applications.

The goal was to create an intuitive, flexible framework that was easy to read, even when things got complicated (a DSL would be ideal but C# 3.5 does not support it without tools).

The result:

    var event = Event.Create()
                     .Wait(1.0f)
                     .MoveToX(transform, 100.0f, 1.0f)
                     .Log("Hi, I'm here!")
                     .UniformScaleTo(transform, 2.0f, 0.5f)
                     .Start();
                     
More complex flow control can be represented by collections (Compound/Sequence/Loop):

    var event = Event.Create()
                     .Wait(1.0)
                	 .Compound(e => {
                     	e.MoveToY(transform, 20.f, 1.0f)
                     	 .ScaleToX(transform, 3.0f, 0.5f); 
                     });


    var event = Event.Create()
                     .Log("Starting animation...")
                     .Wait(1.0f)
                     .Compound(e => {
                     	e.ScaleY(transform, 1.0f, 3.0f, 1.0f)
                     	 .RotateZ(transform, 0.0f, 180.0f, 1.0f)
					 	 .Sequence(e2 => {					 	 
						 	e2.PlaySound("squawk.wav")
					 	      .Wait(0.5f)
					 	  	  .Callback(TriggerScoreAnimation); 
					 	 })
					 })
					 .Wait(0.5f)
					 .Log("Animation ended!");
                     
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
