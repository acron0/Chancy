using System;
using System.Collections.Generic;

namespace Chancy
{
	public static class Controller
	{
		#region Variables

		/// <summary
		/// New events are added to this list before being appended to the update list.
		/// </summary>
		static private List<Event> _pendingAdd;

		/// <summary>
		/// The current events.
		/// </summary>
		static private List<Event> _currentEvents;

		/// <summary
		/// Pending removal list.
		/// </summary>
		static private List<Event> _pendingRemove;

		#endregion

		#region Methods

		/// <summary>
		/// Static c'tor
		/// </summary>
		static Controller ()
		{
			_pendingAdd = new List<Event>();
			_currentEvents = new List<Event>();
			_pendingRemove = new List<Event>();
		}

		/// <summary>
		/// Pushes the event through an event init routine.
		/// </summary>
		/// <returns>
		/// Primed event.
		/// </returns>
		/// <param name='newEvent'>
		/// The event to be init
		/// </param>
		static internal Event InitEvent (Event newEvent)
		{
			if(newEvent.Next != null || newEvent.Previous != null)
				throw new ArgumentException("Only brand new events (no siblings) can be initialised.");

			// we automatically add a 'sequence' event to this root event
			return newEvent.Sequence();
		}

		/// <summary>
		/// Updates all the events with the specified deltaTime.
		/// </summary>
		/// <param name='deltaTime'>
		/// Delta time since last update.
		/// </param>
		static public void Update (float deltaTime)
		{
			foreach (Event e in _pendingAdd) 
			{
				e.StartEvent();
                _currentEvents.Add(e);
			}
            _pendingAdd.Clear();


			foreach (Event e in _currentEvents) 
			{
				if(e.UpdateEvent(deltaTime))
					_pendingRemove.Add(e);
			}

			foreach (Event e in _pendingRemove) 
			{
				e.EndEvent();
				_currentEvents.Remove(e);
			}
			_pendingRemove.Clear();
		}

		/// <summary>
		/// Adds an event to the 'pending add' list.
		/// </summary>
		/// <param name='newEvent'>
		/// The new event we're adding.
		/// </param>
		static internal void AddEvent(Event newEvent)
		{
			_pendingAdd.Add(newEvent);
		}

		#endregion
	}
}

