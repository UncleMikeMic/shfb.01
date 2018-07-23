using System;
using System.Collections.Generic;

namespace LE_LevelEditor.Events
{
	/// <summary>
	/// The event interface ('LE_EventInterface' class) of the Multiplatform Runtime Level Editor allows you to implement the 
	/// required behaviour for your game. Here you define how your data is stored and loaded, which meta data your levels need. 
	/// The event based interface minimizes the amount of classes that you need to access this way the editor's code is hidden 
	/// from the code of your game. This approach allows changes in the editor code, without the need for change in your code. 
	/// In other words the changes that you will need to make in your code after importing an updated version of the level editor 
	/// are minimal as long as you use the event interface. I will give my best to keep the 'LE_EventInterface' class as static 
	/// as possible.
	/// </summary>
	public static class LE_EventInterface
	{
		/// <summary>
		/// This event allows you to add additional meta data to your levels. For example the gold medal score could be 
		/// stored in each level. More details: 
		/// http://www.freebord-game.com/index.php/multiplatform-runtime-level-editor/documentation/add-meta-data
		/// </summary>
		public static EventHandler<LE_CollectMetaDataEvent> OnCollectMetaDataBeforeSave;
		/// <summary>
		/// This event will be raised in the level editor when the user clicks on the save button. Using this event will 
		/// allow you to get a serialized version of your level saved in two byte arrays. More details:
		/// http://www.freebord-game.com/index.php/multiplatform-runtime-level-editor/documentation/save
		/// </summary>
		public static EventHandler<LE_SaveEvent> OnSave;
		/// <summary>
		/// The Multiplatform Runtime Level Editor provides different mechanics for you to load a level from one or two 
		/// byte arrays. This event will be raised in the level editor when the user clicks on the load button. More details:
		/// http://www.freebord-game.com/index.php/multiplatform-runtime-level-editor/documentation/load
		/// </summary>
		public static EventHandler<LE_LoadEvent> OnLoad;
		/// <summary>
		/// This event will be raised when all loading is done for a level that is loaded for editing via the LE_LoadEvent callbacks.
		/// </summary>
		public static EventHandler OnLoadedLevelInEditor;
		/// <summary>
		/// This event will be raised after (or during) any change to the currently edited level. For example it will be 
		/// raised when the terrain or an object is modified. It will also be raised when the level icon is rendered. The 
		/// event args are always empty. 
		/// </summary>
		public static EventHandler<LE_LevelDataChangedEvent> OnChangeLevelData;
		/// <summary>
		/// This event will be raised when an object is selected in the scene by being clicked/touched or through duplicating an object.
		/// </summary>
		public static EventHandler<LE_ObjectSelectedEvent> OnObjectSelectedInScene;
		/// <summary>
		/// This event will be raised when an object is placed through drag and drop or after duplicating an object. 
		/// More details: 
		/// http://www.freebord-game.com/index.php/multiplatform-runtime-level-editor/documentation/count-level-objects
		/// </summary>
		public static EventHandler<LE_ObjectPlacedEvent> OnObjectPlaced;
		/// <summary>
		/// This event will be raised when an object is about to be placed through drag and drop. This event is called 
		/// in every frame in which an object is dragged over something by the player. You can use this event to allow or 
		/// disallow object placement of a specific level object on a certain location. More details: 
		/// http://www.freebord-game.com/index.php/multiplatform-runtime-level-editor/documentation/level-object-placement-limitation
		/// </summary>
		public static EventHandler<LE_ObjectDragEvent> OnObjectDragged;
		/// <summary>
		/// This event is fired when the terrain was created with the "Create Terrain" button in the level editor. 
		/// You can get the terrain GameObject from the event arguments. 
		/// </summary>
		public static EventHandler<LE_TerrainCreatedEvent> OnTerrainCreated;

		/// <summary>
		/// Call this method to remove all references to event handlers that have been set. You should unregister your 
		/// event handlers one by one or call this function otherwise memory leaks are possible. 
		/// </summary>
		public static void UnregisterAll()
		{
			OnCollectMetaDataBeforeSave = null;
			OnSave = null;
			OnLoad = null;
			OnLoadedLevelInEditor = null;
			OnChangeLevelData = null;
			OnObjectSelectedInScene = null;
			OnObjectPlaced = null;
			OnObjectDragged = null;
			OnTerrainCreated = null;
		}
	}
}