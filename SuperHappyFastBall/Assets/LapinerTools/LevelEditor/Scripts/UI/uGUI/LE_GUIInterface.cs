using UnityEngine;
using System.Collections;
using LE_LevelEditor.Core;

namespace LE_LevelEditor.UI
{
	// To give you a better understanding of the GUI interface the structure of the example UI implementation is explained now. 
	// There are three example scenes and three example scripts provided in the Unity Asset Store package. 
	// Each example script works only in the corresponding example scene. The main scene 'LE_ExampleEditor' uses the 'LE_GUIInterface_uGUIimpl' script. 
	// This script assigns and implements all available delegates defined in 'LE_GUIInterface.Delegates'. 
	// This script keeps track of all UI elements in the scene which can be changed through delegate calls.

	// Let us go through the implementation of one certain UI element that shows the terrain paint texture selection. 
	// It allows to swap the selected paint texture and to add new textures. 
	// The 'LE_GUIInterface_uGUIimpl' class has a reference ('TERRAIN_PAINT_TEXTURES_PICKER') to this UI object, which is assigned in the inspector. 
	// In the 'SetupTerrainPaintTexturePickerEvents' function the events and delegates of the paint brush selection are assigned.

	// The 'LE_GUIInterface.Delegates.SetTerrainPaintTextures' delegate is assigned. It will be called by the level editor to inform the example UI 
	// that a terrain has been created/loaded with certain textures. This delegate has four parameters: the used texture array (textures on the terrain), 
	// the unused texture array (textures that can be added to terrain), the selected paint texture index and a bool indicating if more textures can be added. 
	// The UI will show the texture selection with the used textures highlighting the selected texture. If more paint textures can be added to the terrain, 
	// then an 'add texture' button will be created.

	// Now selection clicks on the texture picker need to be processed. When the player selects a texture in the texture picker than the 
	// 'LE_GUIInterface.OnTerrainPaintTextureChanged' callback is executed and the new selected texture index is passed as parameter. 
	// The level editor logic will change the selected brush paint texture.

	// Also the 'add texture' button needs to be processed. When a texture from the unused textures passed in the 
	// 'LE_GUIInterface.Delegates.SetTerrainPaintTextures' delegate is selected, then the 'LE_GUIInterface.OnTerrainPaintTextureAdded' 
	// callback is executed and the selected texture is passed as parameter. The level editor will add the new texture to the terrain.

	// Finally, the selected paint texture can change, for example if a new texture is added, then it is selected as paint texture. 
	// To keep the texture picker updated the data from the 'LE_GUIInterface.EventHandlers.OnTerrainPaintTextureChanged' 
	// event needs also to be propagated to the texture selection.

	/// <summary>
	/// With the 'LE_GUIInterface' it is possible to use any UI framework such as uGUI (new Unity 4.6 GUI), NGUI (a very popular UI 
	/// framework from Unity Asset Store) or even the old OnGUI (like used by the level editor before v1.20). This interface stands 
	/// between the level editor logic and the implementing UI. The level editor does not reference any UI components, which allows 
	/// to use any UI implementation. Take a look at the 'LE_GUIInterface_uGUIimpl...' classes. These example classes implement the 
	/// UI for different scenarios. The 'LE_GUIInterface' is structured in three sections: 'LE_GUIInterface.Delegates', 
	/// 'LE_GUIInterface.EventHandlers' and Callbacks/Event Wrappers. At the bottom of the 'LE_GUIInterface.cs' file you will find 
	/// callbacks that wrap events from 'LE_GUIInterface.EventHandlers'. You can assign these callbacks directly to uGUI (NGUI, etc.) 
	/// buttons, toggles and so on. You can also call these callbacks from scripts. These wrappers will check if the corresponding 
	/// events are assigned. If you have disabled some features for example the terrain logic, then some event will be not initialized 
	/// and stay 'null'.
	/// </summary>
	public class LE_GUIInterface : MonoBehaviour
	{
		/// <summary>
		/// This nested class contains methods that need to be implemented by your custom UI system. The level editor logic will call 
		/// these delegates when they are needed. Therefore, if you do not use a certain feature then you do not need to provide the 
		/// delegates used by this feature. For example, if you set 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' to 'false', then you can 
		/// safely ignore all delegates starting with 'LE_GUIInterface.Delegates.SetTerrain...' and keep their default value 'null', 
		/// because they will never be called. However, if you have forgotten to implement one of the delegates, then the level editor 
		/// will print an explanatory message to the debug log once the delegate is required.
		/// </summary>
		public class Delegates
		{
			// definitions
			public enum ETerrainUIMode { CREATE, EDIT }
			public enum EDraggedObjectState { NONE, IN_3D_PREVIEW, NOT_PLACEABLE }
			public delegate bool GetBoolFunction();
			public delegate float GetFloatFunction();

			// functions

			/// <summary>
			/// Return 'true' if the cursor is currently over the UI and 'false' otherwise. If the cursor is over the UI, 
			/// then the level editor will not allow to edit the terrain or place an object on the current cursor position. 
			/// You need to implement this function to prevent the editor from editing the terrain while the user is 
			/// interacting with the UI. This delegate is called every frame.
			/// </summary>
			public GetBoolFunction IsCursorOverUI = null;

			/// <summary>
			/// Return a float pixel offset that will be applied to the camera perspective gizmo. The gizmo will be moved by 
			/// the given offset in the left direction. This way you can place the gizmo in the right top corner or before the 
			/// UI on the right side or you can place it in the left top corner. The Camera Perspective Gizmo is a standalone 
			/// Unity tool available in the Unity Asset Store. You can find more information about the Camera Perspective Gizmo 
			/// on its homepage. This delegate is called in the first update loop. This delegate is not called if 
			/// 'LE_LevelEditorMain.IS_WITH_CAMERA_PERSPECTIVE_GIZMO' is set to 'false'.
			/// </summary>
			public GetFloatFunction GetCameraPerspectiveGizmoRightPixelOffset = null;
			/// <summary>
			/// To keep a focused object in the center of the visible screen, the level editor allows to use an oblique camera 
			/// projection. For example, without the oblique projection an object will be in the middle of the screen after 
			/// focusing on it (e.g. F-key). However, especially on low resolution devices the right menu could use almost half 
			/// of the screen. In this case the center of the screen is very close to the right menu. In the worst case only the 
			/// left half of a big object will be visible. Return a float pixel offset that defines the width of the right menu. 
			/// With the oblique projection the camera will render as if its view rect had an offset to the right. However, also 
			/// the screen behind the right menu will be rendered. This delegate will be called in the first update loop and after 
			/// every camera perspective switch (perspective or orthographic). This delegate will be called only if 
			/// 'LE_LevelEditorMain.IS_OBLIQUE_FOCUS_CENTERING' is set to 'true'. 
			/// </summary>
			public GetFloatFunction GetObliqueCameraPerspectiveRightPixelOffset = null;

			/// <summary>
			/// This delegate has a 'Texture2D' parameter representing the level icon that you might want to visualize. This 
			/// delegate will be called if a level with an icon in the metadata is loaded or the 'LE_GUIInterface.OnLevelRenderIconBtn' 
			/// callback is executed by your UI. The passed parameter is null if a level without an icon is loaded.
			/// </summary>
			public System.Action<Texture2D> SetLevelIcon = null;

			/// <summary>
			/// This delegate passes the terrain width as an int parameter. You might want to visualize it if you have a terrain creation 
			/// or size editing UI. This delegate will be called in the first update loop and every time a level is loaded or a terrain 
			/// is created/modified. This delegate will be called only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'.
			/// </summary>
			public System.Action<int> SetTerrainWidth = null;
			/// <summary>
			/// This delegate passes the terrain length as an int parameter. You might want to visualize it if you have a terrain creation 
			/// or size editing UI. This delegate will be called in the first update loop and every time a level is loaded or a terrain is 
			/// created/modified. This delegate will be called only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'.
			/// </summary>
			public System.Action<int> SetTerrainLength = null;
			/// <summary>
			/// This delegate passes the terrain height as an int parameter. You might want to visualize it if you have a terrain creation 
			/// or size editing UI. This delegate will be called in the first update loop and every time a level is loaded or a terrain is 
			/// created/modified. This delegate will be called only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'.
			/// </summary>
			public System.Action<int> SetTerrainHeight = null;
			/// <summary>
			/// This delegate will be used to set up the terrain base texture selection UI. It has a texture array parameter, which is always 
			/// set to match the textures from the terrain texture config asset assigned to 'LE_ConfigTerrain.TerrainTextureConfig'. 
			/// Additionally, it also has an int parameter, which identifies the initially selected texture index. This delegate will be 
			/// called in the first update loop and every time a level is loaded or a terrain is created/modified. This delegate will be 
			/// called only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' and 'LE_ConfigTerrain.IsBaseTextureSelection' are set to 'true'.
			/// </summary>
			public System.Action<Texture2D[], int> SetTerrainBaseTextures = null;
			/// <summary>
			/// This delegate will be used to set up the terrain brush selection UI. It has a texture array parameter, which is always set to match 
			/// the textures from the brush array assigned to 'LE_ConfigTerrain.Brushes'. Additionally, it also has an int parameter, which identifies 
			/// the currently selected brush index. This delegate will be called in the first update loop and every time a level is loaded or a 
			/// terrain is created/modified. This delegate will be called only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'.
			/// </summary>
			public System.Action<Texture2D[], int> SetTerrainBrushes = null;
			/// <summary>
			/// This delegate will be used to set up the terrain paint texture selection UI. It has four parameters: the used texture array 
			/// (textures on the terrain), the unused texture array (textures that can be added to terrain), the selected paint texture index and 
			/// a bool indicating if more textures can be added. If further texture can be added, then you need to display an add button. If the 
			/// add button is clicked, then the unused textures must be presented to the player, so that he can choose among them. This delegate 
			/// will be called in the first update loop and every time a level is loaded or a terrain is created/modified. This delegate will be 
			/// called only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'.
			/// </summary>
			public System.Action<Texture2D[], Texture2D[], int, bool> SetTerrainPaintTextures = null;
			/// <summary>
			/// This delegate will pass the currently used terrain brush size as a float value in range [0,1]. This delegate will be called 
			/// in the first update loop and every time a level is loaded or a terrain is created/modified. This delegate will be called only 
			/// if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'.
			/// </summary>
			public System.Action<float> SetTerrainEditBrushSize = null;
			/// <summary>
			/// This delegate will pass the currently used terrain brush amount as a float value in range [0,1]. This delegate will be called 
			/// in the first update loop and every time a level is loaded or a terrain is created/modified. This delegate will be called only 
			/// if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'.
			/// </summary>
			public System.Action<float> SetTerrainEditBrushAmount = null;
			/// <summary>
			/// This delegate will pass the currently used terrain brush target value as a float in range [0,1]. This delegate will be called 
			/// in the first update loop and every time a level is loaded or a terrain is created/modified. Additionally, this delegate will 
			/// be called multiple times after your UI has executed the 'LE_GUIInterface.OnTerrainReadPaintHeightBtn' callback. This delegate 
			/// will be called only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'.
			/// </summary>
			public System.Action<float> SetTerrainEditBrushTargetValue = null;
			/// <summary>
			/// This delegate has a bool parameter that indicates if the level editor is currently reading the terrain paint height. Use it to 
			/// visualize to the user that the height is read. This delegate will be called immediately after a 'LE_GUIInterface.OnTerrainReadPaintHeightBtn' 
			/// callback call (with 'true' as parameter). It will be called again once the terrain height reading stops (with 'false' as parameter). 
			/// This delegate will be called only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'.
			/// </summary>
			public System.Action<bool> SetTerrainIsReadingPaintHeight = null;
			/// <summary>
			/// This delegate has an enum parameter which can have one of two values: 'ETerrainUIMode.CREATE' or 'ETerrainUIMode.EDIT'. In the create 
			/// mode the level has no terrain and the player should be able to select the terrain properties such as size and base texture. In the edit 
			/// mode the level has already a terrain that can be edited, but no more terrains should be created. This delegate will be called in the 
			/// first update loop and every time a level is loaded or a terrain is created/modified. This delegate will be called only if 
			/// 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'.
			/// </summary>
			public System.Action<ETerrainUIMode> SetTerrainUIMode = null;

			/// <summary>
			/// This delegate will be used to set up the object selection UI. It has a 'LE_ObjectMap' parameter, which is always set to match the 
			/// assigned 'LE_LevelEditorMain.ROOT_OBJECT_MAP'. This delegate will be called in the first update loop. This delegate will be called 
			/// only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'.
			/// </summary>
			public System.Action<LE_ObjectMap> SetObjects = null;
			/// <summary>
			/// The UI has to handle 2D object drag & drop mechanics (image only, 3D UI will be handled by the level editor), because the level 
			/// editor does not know which UI is used and therefore cannot implement it. This delegate must return 'true' if an object is currently 
			/// dragged and 'false' otherwise. This delegate will be called multiple times per frame. This delegate will be called only if 
			/// 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'.
			/// </summary>
			public GetBoolFunction IsObjectDragged = null;
			/// <summary>
			/// The UI has to handle 2D object drag & drop mechanics (image only, 3D UI will be handled by the level editor), because the level
			/// editor does not know which UI is used and therefore cannot implement it. This delegate has a string parameter, which contains a 
			/// message explaining why the selected object cannot be placed in the level. For example, an object cannot be placed if the maximal 
			/// count is reached. This delegate will be called multiple times per frame. This delegate will be called only if 
			/// 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'.
			/// </summary>
			public System.Action<string> SetDraggableObjectMessage = null;
			/// <summary>
			/// The UI has to handle 2D object drag & drop mechanics (image only, 3D UI will be handled by the level editor), because the level 
			/// editor does not know which UI is used and therefore cannot implement it. This delegate has an enum parameter that can have one of 
			/// three values: 'EDraggedObjectState.NONE', 'EDraggedObjectState.IN_3D_PREVIEW' or 'EDraggedObjectState.NOT_PLACEABLE'. In the none 
			/// state nothing needs to be done. In the in 3D preview state the sprite or any preview texture should be hidden, because the level 
			/// editor already renders a 3D preview of the drag&dropped object. In the not placeable state the UI should make clear that the object 
			/// cannot be placed, for example by tinting its texture red. This delegate will be called every frame. This delegate will be called 
			/// only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'.
			/// </summary>
			public System.Action<EDraggedObjectState> SetDraggableObjectState = null;
			/// <summary>
			/// This delegate has a bool parameter which indicates if the focus button is interactable. For example, if no object is selected, 
			/// then the focus button should be greyed out to prevent user irritation. This delegate will be called every frame. This delegate 
			/// will be called only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'. 
			/// </summary>
			public System.Action<bool> SetIsSelectedObjectFocusBtnInteractable = null;
			/// <summary>
			/// This delegate has a bool parameter which indicates if the clone/duplicate button is interactable. For example, if no object is 
			/// selected, then the duplicate button should be greyed out to prevent user irritation. This delegate will be called every frame. 
			/// This delegate will be called only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'.
			/// </summary>
			public System.Action<bool> SetIsSelectedObjectDuplicateBtnInteractable = null;
			/// <summary>
			/// This delegate has a bool parameter which indicates if the delete button is interactable. For example, if no object is selected, 
			/// then the delete button should be greyed out to prevent user irritation. This delegate will be called every frame. 
			/// This delegate will be called only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'.
			/// </summary>
			public System.Action<bool> SetIsSelectedObjectDeleteBtnInteractable = null;
			/// <summary>
			/// This delegate has a bool parameter which indicates if the find button is interactable. For example, if no prefab is 
			/// selected, then the find button should be greyed out to prevent user irritation. This delegate will be called every frame. 
			/// This delegate will be called only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'.
			/// </summary>
			public System.Action<bool> SetIsSelectedPrefabFindBtnInteractable = null;
			/// <summary>
			/// This delegate has a bool parameter which indicates if the 'Rigidbody' sleep property menu is interactable. For example, 
			/// if no object is selected or the selected object has no rigidbody, then the sleep property menu should be hidden. 
			/// This delegate will be called every frame. This delegate will be called only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'.
			/// </summary>
			public System.Action<bool> SetIsSelectedObjectSleepPropertyInteractable = null;
			/// <summary>
			/// This delegate passes the current value of the 'IsSleepOnStartProperty' of the selected object. This delegate will be called in 
			/// every frame with a selected object having an editable 'IsSleepOnStartProperty'. 
			/// This delegate will be called only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'.
			/// </summary>
			public System.Action<bool> SetSelectedObjectIsSleepOnStartPropertyValue = null;
			/// <summary>
			/// This delegate has a bool parameter which indicates if the 'Material' color property menu is interactable. For example, 
			/// if no object is selected or the selected object does not support coloring, then the color property menu should be hidden. 
			/// This delegate will be called every frame. This delegate will be called only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'.
			/// </summary>
			public System.Action<bool> SetIsSelectedObjectColorPropertyInteractable = null;
			/// <summary>
			/// This delegate passes the current value of the 'ObjectColorProperty' of the selected object. 
			/// This delegate will be called in every frame with a selected object having an editable 'ObjectColorProperty'. 
			/// This delegate will be called only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'.
			/// </summary>
			public System.Action<Color> SetSelectedObjectColorPropertyValue = null;
			/// <summary>
			/// This delegate passes the index of the used variation and the names of all available variations.
			/// If nothing is selected, then null is passed instead of the variations array.
			/// This delegate will be called in every frame with a selected object having multiple variations. 
			/// This delegate will be called only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'.
			/// </summary>
			public System.Action<int, LE_ObjectVariationBase[]> SetSelectedObjectVariationPropertyValue = null;

			/// <summary>
			/// Show a popup which asks the player to confirm that he wants to delete the selected object.
			/// Invoke the passed callback with 'true' if the users answer is 'yes - delete object' and
			/// pass 'false' if the answer is 'no - do nothing'. This delegate is called immediately after 
			/// your UI calls <em>LE_GUIInterface.EventHandlers.OnSelectedObjectDeleteBtn</em>'.
			/// </summary>
			public System.Action<System.Action<bool>> ShowPopupConfirmDeleteObject = null;
			/// <summary>
			/// If for some reason you cannot use the latest Unity version and you have to run Unity 4.6.0 you will encounter a bug with level icon rendering.
			/// For more information search "This is another bug with Unity for Windows Phone 8" in the LE_LogicLevel.cs file. An example text that you might want to show:
			/// "A bug might have occurred! If the rendered level icon looks buggy, <b>please bring your device in landscape left orientatation (turn it around for 180 degree, while still facing the screen) and render level icon again.</b>"
			/// </summary>
			public System.Action ShowWP8RenderLevelIconBugDialog = null;

			public void OnDestroy()
			{
				// unregister all delegates
				IsCursorOverUI = null;

				GetCameraPerspectiveGizmoRightPixelOffset = null;
				GetObliqueCameraPerspectiveRightPixelOffset = null;

				SetLevelIcon = null;

				SetTerrainWidth = null;
				SetTerrainLength = null;
				SetTerrainHeight = null;
				SetTerrainBaseTextures = null;
				SetTerrainBrushes = null;
				SetTerrainPaintTextures = null;
				SetTerrainEditBrushSize = null;
				SetTerrainEditBrushAmount = null;
				SetTerrainEditBrushTargetValue = null;
				SetTerrainIsReadingPaintHeight = null;
				SetTerrainUIMode = null;

				IsObjectDragged = null;
				SetObjects = null;
				SetDraggableObjectMessage = null;
				SetDraggableObjectState = null;
				SetIsSelectedObjectFocusBtnInteractable = null;
				SetIsSelectedObjectDuplicateBtnInteractable = null;
				SetIsSelectedObjectDeleteBtnInteractable = null;
				SetIsSelectedPrefabFindBtnInteractable = null;
				SetIsSelectedObjectSleepPropertyInteractable = null;
				SetSelectedObjectIsSleepOnStartPropertyValue = null;
				SetIsSelectedObjectColorPropertyInteractable = null;
				SetSelectedObjectColorPropertyValue = null;
				SetSelectedObjectVariationPropertyValue = null;

				ShowPopupConfirmDeleteObject = null;
				ShowWP8RenderLevelIconBugDialog = null;
			}
		}

		/// <summary>
		/// This nested class defines the events that can be processed by the level editor. The level editor will register to these 
		/// event handlers and execute the expected behaviour once the UI rases an event. To simplify the event calls for you the 
		/// 'LE_GUIInterface' class contains wrapper methods at the bottom of this file. Calling the wrapper methods will guarantee 
		/// that the event handlers are initialized and that the passed event arguments are right and can be processed by the level editor. 
		/// If you disable features in the level editor for example by setting 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' to 'false' then it 
		/// is likely that some events are not assigned and have their default value 'null'.
		/// </summary>
		public class EventHandlers
		{
			// YOU SHOULD NOT USE THIS CLASS IN YOUR SCRIPTS, use the wrapper methods in the end of this file instead

			// definitions
			public enum ETerrainChangeHeightMode { RAISE, LOWER }
			public class BoolEventArgs : System.EventArgs { public readonly bool Value; public BoolEventArgs(bool p_value){ Value = p_value; } }
			public class IntEventArgs : System.EventArgs { public readonly int Value; public IntEventArgs(int p_value){ Value = p_value; } }
			public class FloatEventArgs : System.EventArgs { public readonly float Value; public FloatEventArgs(float p_value){ Value = p_value; } }
			public class StringEventArgs : System.EventArgs { public readonly string Value; public StringEventArgs(string p_value){ Value = p_value; } }
			public class ColorEventArgs : System.EventArgs { public readonly Color Value; public ColorEventArgs(Color p_value){ Value = p_value; } }
			public class TextureEventArgs : System.EventArgs { public readonly Texture2D Value; public TextureEventArgs(Texture2D p_value){ Value = p_value; } }
			public class EditModeEventArgs : System.EventArgs { public readonly LE_EEditMode EditMode; public EditModeEventArgs(LE_EEditMode p_editMode){ EditMode = p_editMode; } }
			public class TerrainEditModeEventArgs : System.EventArgs { public readonly LE_ETerrainEditMode EditMode; public TerrainEditModeEventArgs(LE_ETerrainEditMode p_editMode){ EditMode = p_editMode; } }
			public class TerrainChangeHeightModeEventArgs : System.EventArgs { public readonly ETerrainChangeHeightMode ChangeHeightMode; public TerrainChangeHeightModeEventArgs(ETerrainChangeHeightMode p_changeHeightMode){ ChangeHeightMode = p_changeHeightMode; } }
			public class ObjectEditSpaceEventArgs : System.EventArgs { public readonly LE_EObjectEditSpace EditSpace; public ObjectEditSpaceEventArgs(LE_EObjectEditSpace p_editSpace){ EditSpace = p_editSpace; } }
			public class ObjectEditModeEventArgs : System.EventArgs { public readonly LE_EObjectEditMode EditMode; public ObjectEditModeEventArgs(LE_EObjectEditMode p_editMode){ EditMode = p_editMode; } }
			public class ObjectSelectDraggableEventArgs : System.EventArgs { public readonly LE_Object ObjPrefab; public readonly string ResourcePath; public ObjectSelectDraggableEventArgs(LE_Object p_objPrefab, string p_resourcePath){ ObjPrefab = p_objPrefab; ResourcePath = p_resourcePath; } }

			// events

			/// <summary>
			/// The wrapper of this event takes an int parameter, which is converted to the 'LE_EEditMode (TERRAIN, OBJECT, NONE)' enum. 
			/// This event changes the edit mode of the level editor. You should not call this wrapper with the edit mode 0 ('TERRAIN') 
			/// if you have set the 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' to 'false'. The same applies to the ('OBJECT') mode. 
			/// In the example scene this wrapper is assigned to the tab buttons at top right.
			/// </summary>
			public System.EventHandler<EditModeEventArgs> OnEditModeBtn;
			/// <summary>
			/// This parameterless event wrapper will undo the last action, if there are actions that can be undone.
			/// In the example scene this wrapper is assigned to the 'LeftNav_UndoBtn' button.
			/// </summary>
			public System.EventHandler OnUndoBtn;
			/// <summary>
			/// This parameterless event wrapper will redo the last action, if there are actions that were undone.
			/// In the example scene this wrapper is assigned to the 'LeftNav_RedoBtn' button.
			/// </summary>
			public System.EventHandler OnRedoBtn;

			/// <summary>
			/// The wrapper of this event takes a string parameter, which is later converted to an int. If there was no terrain created/loaded, 
			/// then the width of the terrain that will be created later by the level editor is changed. If there is an editable terrain created, 
			/// then its width will change on the fly (this behaviour is not shown in the example, but you can see it by enabling the create terrain 
			/// UI with an existing terrain). This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
			/// In the example scene this wrapper is assigned to the width input text field of the create terrain menu.
			/// </summary>
			public System.EventHandler<StringEventArgs> OnTerrainWidthChanged;
			/// <summary>
			/// The wrapper of this event takes a string parameter, which is later converted to an int. If there was no terrain created/loaded, 
			/// then the length of the terrain that will be created later by the level editor is changed. If there is an editable terrain created, 
			/// then its length will change on the fly (this behaviour is not shown in the example, but you can see it by enabling the create terrain 
			/// UI with an existing terrain). This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
			/// In the example scene this wrapper is assigned to the length input text field of the create terrain menu.
			/// </summary>
			public System.EventHandler<StringEventArgs> OnTerrainLengthChanged;
			/// <summary>
			/// The wrapper of this event takes a string parameter, which is later converted to an int. If there was no terrain created/loaded, 
			/// then the height of the terrain that will be created later by the level editor is changed. If there is an editable terrain created, 
			/// then its height will change on the fly (this behaviour is not shown in the example, but you can see it by enabling the create terrain 
			/// UI with an existing terrain). This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
			/// In the example scene this wrapper is assigned to the height input text field of the create terrain menu.
			/// </summary>
			public System.EventHandler<StringEventArgs> OnTerrainHeightChanged;
			/// <summary>
			/// The wrapper of this event takes an int parameter. The passed parameter is used as a texture index in the texture array from the terrain 
			/// texture config assigned to 'LE_ConfigTerrain.TerrainTextureConfig'. This index stands for the first (base) texture of the later created terrain. 
			/// Call this wrapper only if there was no terrain created or loaded. This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
			/// In the example scene the 'LE_GUIInterface_uGUIimpl' script assigns this wrapper to the texture picker in the create terrain menu.
			/// </summary>
			public System.EventHandler<IntEventArgs> OnTerrainBaseTextureChanged;
			/// <summary>
			/// The wrapper of this event takes an int parameter. The passed parameter is used as a texture index in the brush texture array assigned to 
			/// 'LE_ConfigTerrain.Brushes'. The used brush will be changed in the terrain editor logic. Also, the brush projector texture will be changed. 
			/// This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
			/// In the example scene the 'LE_GUIInterface_uGUIimpl' script assigns this wrapper to the brush texture picker in the terrain edit menu.
			/// </summary>
			public System.EventHandler<IntEventArgs> OnTerrainBrushChanged;
			/// <summary>
			/// The wrapper of this event takes an int parameter. The passed parameter is used as a texture index in the texture array of the edited terrain. 
			/// The selected texture (splat prototype) will be applied to the terrain editor logic. 
			/// This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
			/// In the example scene the 'LE_GUIInterface_uGUIimpl' script assigns this wrapper to the paint texture picker in the terrain edit menu.
			/// </summary>
			public System.EventHandler<IntEventArgs> OnTerrainPaintTextureChanged;
			/// <summary>
			/// The wrapper of this event takes a 'Texture2D' parameter. The passed texture must be contained in the terrain texture config assigned to 
			/// 'LE_ConfigTerrain.TerrainTextureConfig'. The given texture will be added to the used terrain textures. 
			/// This texture will also be selected for painting. This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
			/// In the example scene the 'LE_GUIInterface_uGUIimpl' script calls this wrapper when the 'add new' button in the paint texture picker in the 
			/// terrain edit menu is clicked.
			/// </summary>
			public System.EventHandler<TextureEventArgs> OnTerrainPaintTextureAdded;
			/// <summary>
			/// The wrapper of this event takes a float parameter, which must be in range [0,1]. The brush size is calculated from the passed value like this: 
			/// 'max(0.002, value²)'. The brush has the size of the terrain if the size value is 1. 
			/// This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
			/// In the example scene this wrapper is assigned to the 'Size' slider of the terrain edit menu.
			/// </summary>
			public System.EventHandler<FloatEventArgs> OnTerrainEditBrushSizeChanged;
			/// <summary>
			/// The wrapper of this event takes a float parameter, which must be in range [0,1]. The amount is calculated from the passed value like this: 
			/// 'max(0.002, value)'. If you need to set a negative amount, for example to lower the terrain combine this event with 
			/// 'OnTerrainChangeHeightModeChanged'. This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
			/// In the example scene this wrapper is assigned to the 'Amount' slider of the terrain edit menu.
			/// </summary>
			public System.EventHandler<FloatEventArgs> OnTerrainEditBrushAmountChanged;
			/// <summary>
			/// The wrapper of this event takes a float parameter, which must be in range [0,1]. The target value is set in the terrain editor logic. 
			/// A value of 0 means that the terrain will be lowered to the bottom. A value of 0.5 will raise the terrain to the half of the terrain's 
			/// height and a value of one will raise the terrain to the maximal height. This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' 
			/// is set to 'true'. In the example scene this wrapper is assigned to the 'Target Value' slider of the terrain edit menu.
			/// </summary>
			public System.EventHandler<FloatEventArgs> OnTerrainEditBrushTargetValueChanged;
			/// <summary>
			/// The wrapper of this event takes a float parameter, which must be in range [0,1]. The smooth direction angle is calculated like this: 
			/// 'round(value*16)*22.5'. This way only 16 directions are available. This is required due to the smoothing function implementation. 
			/// This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
			/// In the example scene this wrapper is assigned to the 'Is Directed' toggle of the terrain smooth menu.
			/// </summary>
			public System.EventHandler<FloatEventArgs> OnTerrainEditDirectionChanged;
			/// <summary>
			/// The wrapper of this event takes a bool parameter, which indicates if the terrain is raised ('true') or lowered ('false'). 
			/// This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
			/// In the example scene this wrapper is assigned to the 'Raise' and 'Lower' toggle buttons of the raise/lower height terrain edit menu.
			/// </summary>
			public System.EventHandler<TerrainChangeHeightModeEventArgs> OnTerrainChangeHeightModeChanged;
			/// <summary>
			/// The wrapper of this event takes a bool parameter, which indicates if the terrain smoothing is directed. Use in combination with 
			/// 'OnTerrainEditDirectionChanged' to set the smooth direction. If the smooth function is directed, then only the neightbours in the 
			/// smooth direction are used to calculate the smoothed height for a certain point. This allows to smooth a mountain crest without lowering 
			/// it or to smooth a riverbed without raising it. This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
			/// In the example scene this wrapper is assigned to the 'Is Directed' toggle of the smooth terrain edit mode menu.
			/// </summary>
			public System.EventHandler<BoolEventArgs> OnTerrainIsDirectedSmoothChanged;
			/// <summary>
			/// This parameterless event wrapper will create a terrain if no terrain was created or loaded. Use in combination with 'OnTerrainWidthChanged, 
			/// OnTerrainLengthChanged, OnTerrainHeightChanged, OnTerrainBaseTextureChanged' to setup the terrain values. This event is available only if 
			/// 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. In the example scene this wrapper is assigned to the 'Create Terrain' button.
			/// </summary>
			public System.EventHandler OnTerrainCreateBtn;
			/// <summary>
			/// The wrapper of this event takes an int parameter, which is converted to the 'LE_ETerrainEditMode (CHANGE_HEIGHT, CHANGE_HEIGHT_TO_TARGET_VALUE, 
			/// SMOOTH_HEIGHT, DRAW_TEXTURE)' enum. This event changes the terrain edit mode of the terrain editor. 
			/// This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
			/// In the example scene this wrapper is assigned to the terrain edit mode buttons below the tab buttons at top right.
			/// </summary>
			public System.EventHandler<TerrainEditModeEventArgs> OnTerrainEditModeBtn;
			/// <summary>
			/// This parameterless event wrapper will bring the terrain editor in the read terrain paint height mode. The editor will read the terrain height 
			/// at the point where the next mouse click/touch will be. Use in combination with 'SetTerrainIsReadingPaintHeight' delegate to visualize the 
			/// reading process. Use in combination with 'SetTerrainEditBrushTargetValue' delegate to visualize the new read height. 
			/// This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
			/// In the example scene this wrapper is assigned to the 'Read Terrain Paint Height' button.
			/// </summary>
			public System.EventHandler OnTerrainReadPaintHeightBtn;

			/// <summary>
			/// The wrapper of this event takes a bool parameter, which is converted to the 'LE_EObjectEditSpace (LOCAL, GLOBAL)' enum. 
			/// This event changes the object edit space of the object editor. 
			/// This event is available only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'. 
			/// In the example scene this wrapper is assigned to the object edit space button below the tab buttons at top right.
			/// </summary>
			public System.EventHandler<ObjectEditSpaceEventArgs> OnObjectEditSpaceBtn;
			/// <summary>
			/// The wrapper of this event takes an int parameter, which is converted to the 'LE_EObjectEditMode (NO_EDIT, MOVE, ROTATE, SCALE)' enum. 
			/// This event changes the object edit mode of the object editor. 
			/// This event is available only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'. 
			/// In the example scene this wrapper is assigned to the object edit mode buttons below the tab buttons at top right.
			/// </summary>
			public System.EventHandler<ObjectEditModeEventArgs> OnObjectEditModeBtn;
			/// <summary>
			/// The wrapper of this event takes two parameters: a 'LE_Object' and a string. The passed 'LE_Object' is the resource reference to the 
			/// object prefab and the string parameter is the resource path to the prefab. The draggable object will be set in the object editor. 
			/// Use in combination with 'IsObjectDragged, SetDraggableObjectMessage, SetDraggableObjectState' delegates to implement drag&drop. 
			/// This event is available only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'. 
			/// In the example scene this wrapper is assigned to the leaf nodes of the object tree browser.
			/// </summary>
			public System.EventHandler<ObjectSelectDraggableEventArgs> OnObjectSelectDraggable;
			/// <summary>
			/// This parameterless event wrapper will focus the camera on the selected object. Call this wrapper only if an object is selected. 
			/// Use in combination with 'SetIsSelectedObjectFocusBtnInteractable' to disable UI if no object is selected. 
			/// This event is available only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'. 
			/// In the example scene this wrapper is assigned to the 'Focus' button in the objects menu.
			/// </summary>
			public System.EventHandler OnSelectedObjectFocusBtn;
			/// <summary>
			/// This parameterless event wrapper will duplicate/clone the selected object. Call this wrapper only if an object is selected and can 
			/// be duplicated. Use in combination with 'SetIsSelectedObjectDuplicateBtnInteractable' to disable UI if no object is selected or it 
			/// cannot be duplicated. This event is available only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'. 
			/// In the example scene this wrapper is assigned to the 'Duplicate' button in the objects menu.
			/// </summary>
			public System.EventHandler OnSelectedObjectDuplicateBtn;
			/// <summary>
			/// This parameterless event wrapper will delete the selected object. Call this wrapper only if an object is selected. 
			/// Use in combination with 'SetIsSelectedObjectDeleteBtnInteractable' to disable UI if no object is selected. 
			/// This event is available only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'. 
			/// In the example scene this wrapper is assigned to the 'Delete' button in the objects menu.
			/// </summary>
			public System.EventHandler OnSelectedObjectDeleteBtn;
			/// <summary>
			/// The wrapper of this event takes a bool parameter, which indicates if the rigidbody should be sent to sleep on level load. 
			/// Call this wrapper only if an object with a rigidbody is selected and the sleep setting for this object can be changed. 
			/// Use in combination with 'SetIsSelectedObjectSleepPropertyInteractable' delegate to disable UI if no object is selected or its 
			/// sleep setting is not available. Use in combination with 'SetSelectedObjectIsSleepOnStartPropertyValue' delegate to keep your UI updated. 
			/// This event is available only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'. 
			/// In the example scene this wrapper is assigned to the 'Sleep On Start' toggle in the object property menu.
			/// </summary>
			public System.EventHandler<BoolEventArgs> OnSelectedObjectIsSleepOnStartChanged;
			/// <summary>
			/// The wrapper of this event takes a 'Color' parameter, which is applied to the materials of the level object. Call this wrapper only if 
			/// an object is selected and the color setting for this object can be changed. Use in combination with 
			/// 'SetIsSelectedObjectColorPropertyInteractable' delegate to disable UI if no object is selected or its color setting is not available. 
			/// Use in combination with 'SetSelectedObjectColorPropertyValue' delegate to keep your UI updated. 
			/// This event is available only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'. 
			/// In the example scene the 'LE_GUIInterface_uGUIimpl' script assigns this wrapper to the color picker in the object property menu.
			/// </summary>
			public System.EventHandler<ColorEventArgs> OnSelectedObjectColorChanged;
			/// <summary>
			/// The wrapper of this event takes an int parameter as index for the variation, which is applied to the object. Call this wrapper only if 
			/// an object is selected and there are multiple variations available. Use in combination with 
			/// 'SetIsSelectedObjectColorPropertyInteractable' delegate to disable UI if no object is selected or its color setting is not available. 
			/// Use in combination with 'SetSelectedObjectVariationPropertyValue' delegate to keep your UI updated. 
			/// This event is available only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'. 
			/// In the example scene the 'LE_GUIInterface_uGUIimpl' script assigns this wrapper to the variation browser in the object property menu.
			/// </summary>
			public System.EventHandler<IntEventArgs> OnSelectedObjectVariationIndexChanged;
			/// <summary>
			/// This parameterless event wrapper will search the selected prefab in the scene.
			/// If an object instance of this prefab is found, then the editor will select this instance and focus on it.
			/// If there are multiple instances of this object and one istance is already selected, then the editor will focus on the next instance.
			/// Call this wrapper only if a prefab is selected.
			/// Use in combination with 'SetIsSelectedPrefabFindBtnInteractable' to disable UI if no prefab is selected.
			/// This event is available only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'. 
			/// In the example scene this wrapper is assigned to the 'Find' button in the objects menu.
			/// </summary>
			public System.EventHandler OnSelectedPrefabFindBtn;

			/// <summary>
			/// This parameterless event wrapper will save the level. Use in combination with 'LE_EventInterface.OnSave' to get the serialized level data. 
			/// In the example scene this wrapper is assigned to the 'Save' button. 
			/// </summary>
			public System.EventHandler OnLevelSaveBtn;
			/// <summary>
			/// This parameterless event wrapper will load the level. Use in combination with 'LE_EventInterface.OnLoad' to provide the serialized level data. 
			/// In the example scene this wrapper is assigned to the 'Load' button.
			/// </summary>
			public System.EventHandler OnLevelLoadBtn;
			/// <summary>
			/// This parameterless event wrapper will render the level icon by capturing the screen. Use in combination with 'SetLevelIcon' delegate to visualize the level icon. 
			/// In the example scene this wrapper is assigned to the 'Render Level Icon' button.
			/// </summary>
			public System.EventHandler OnLevelRenderIconBtn;

			public void OnDestroy()
			{
				// unregister all events
				OnEditModeBtn = null;
				OnUndoBtn = null;
				OnRedoBtn = null;

				OnTerrainWidthChanged = null;
				OnTerrainLengthChanged = null;
				OnTerrainHeightChanged = null;
				OnTerrainBaseTextureChanged = null;
				OnTerrainPaintTextureChanged = null;
				OnTerrainPaintTextureAdded = null;
				OnTerrainBrushChanged = null;
				OnTerrainEditBrushSizeChanged = null;
				OnTerrainEditBrushAmountChanged = null;
				OnTerrainEditBrushTargetValueChanged = null;
				OnTerrainEditDirectionChanged = null;
				OnTerrainChangeHeightModeChanged = null;
				OnTerrainIsDirectedSmoothChanged = null;
				OnTerrainCreateBtn = null;
				OnTerrainEditModeBtn = null;
				OnTerrainReadPaintHeightBtn = null;

				OnObjectEditSpaceBtn = null;
				OnObjectEditModeBtn = null;
				OnObjectSelectDraggable = null;
				OnSelectedObjectFocusBtn = null;
				OnSelectedObjectDuplicateBtn = null;
				OnSelectedObjectDeleteBtn = null;
				OnSelectedObjectIsSleepOnStartChanged = null;
				OnSelectedObjectColorChanged = null;
				OnSelectedObjectVariationIndexChanged = null;
				OnSelectedPrefabFindBtn = null;

				OnLevelSaveBtn = null;
				OnLevelLoadBtn = null;
				OnLevelRenderIconBtn = null;
			}
		}



// SINGLETON ---------------------------------------------------------------------------------------------------------------------

		// singleton pattern
		private static LE_GUIInterface s_instance = null;
		public static LE_GUIInterface Instance { get{ return s_instance; } }

		// register instance reference
		private void Awake()
		{
			if (s_instance != null) { Debug.LogError("LE_GUIInterface: there is already an instance of LE_GUIInterface in this scene!"); }
			s_instance = this;
		}

		// unregister all events and delegates on destroy. Also remove instance reference
		private void OnDestroy()
		{
			if (s_instance == this) { s_instance = null; }
			events.OnDestroy();
			delegates.OnDestroy();
		}

// DELEGATES & EVENTS-------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// These delegates need to be implemented by your custom UI system. The level editor logic will call 
		/// these delegates when they are needed. Therefore, if you do not use a certain feature then you do not need to provide the 
		/// delegates used by this feature. For example, if you set 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' to 'false', then you can 
		/// safely ignore all delegates starting with 'LE_GUIInterface.Delegates.SetTerrain...' and keep their default value 'null', 
		/// because they will never be called. However, if you have forgotten to implement one of the delegates, then the level editor 
		/// will print an explanatory message to the debug log once the delegate is required.
		/// </summary>
		public readonly Delegates delegates = new Delegates();

		/// <summary>
		/// These events can be processed by the level editor. The level editor will register to these 
		/// event handlers and execute the expected behaviour once the UI rases an event. To simplify the event calls for you the 
		/// 'LE_GUIInterface' class contains wrapper methods at the bottom of its file. Calling the wrapper methods will guarantee 
		/// that the event handlers are initialized and that the passed event arguments are right and can be processed by the level editor. 
		/// If you disable features in the level editor for example by setting 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' to 'false' then it 
		/// is likely that some events are not assigned and have their default value 'null'.
		/// </summary>
		public readonly EventHandlers events = new EventHandlers();

// CALLBACKS ---------------------------------------------------------------------------------------------------------------------
		// Callbacks that wrap events from 'LE_GUIInterface.EventHandlers'. 
		// You can assign these callbacks directly to uGUI (NGUI, etc.) buttons, toggles and so on. You can also call these callbacks from scripts. 
		// These wrappers will check if the corresponding events are assigned.


		/// <summary>
		/// The wrapper of this event takes an int parameter, which is converted to the 'LE_EEditMode (TERRAIN, OBJECT, NONE)' enum. 
		/// This event changes the edit mode of the level editor. You should not call this wrapper with the edit mode 0 ('TERRAIN') 
		/// if you have set the 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' to 'false'. The same applies to the ('OBJECT') mode. 
		/// In the example scene this wrapper is assigned to the tab buttons at top right.
		/// </summary>
		public void OnEditModeBtn(int p_editMode) { if (events.OnEditModeBtn != null) { events.OnEditModeBtn(this, new EventHandlers.EditModeEventArgs((LE_EEditMode)p_editMode)); } }
		/// <summary>
		/// This parameterless event wrapper will undo the last action, if there are actions that can be undone.
		/// In the example scene this wrapper is assigned to the 'LeftNav_UndoBtn' button.
		/// </summary>
		public void OnUndoBtn() { if (events.OnUndoBtn != null) { events.OnUndoBtn(this, System.EventArgs.Empty); } }
		/// <summary>
		/// This parameterless event wrapper will redo the last action, if there are actions that were undone.
		/// In the example scene this wrapper is assigned to the 'LeftNav_RedoBtn' button.
		/// </summary>
		public void OnRedoBtn() { if (events.OnRedoBtn != null) { events.OnRedoBtn(this, System.EventArgs.Empty); } }

		/// <summary>
		/// The wrapper of this event takes a string parameter, which is later converted to an int. If there was no terrain created/loaded, 
		/// then the width of the terrain that will be created later by the level editor is changed. If there is an editable terrain created, 
		/// then its width will change on the fly (this behaviour is not shown in the example, but you can see it by enabling the create terrain 
		/// UI with an existing terrain). This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
		/// In the example scene this wrapper is assigned to the width input text field of the create terrain menu.
		/// </summary>
		public void OnTerrainWidthChanged(string p_widthIntegerValue) {if (events.OnTerrainWidthChanged != null) { events.OnTerrainWidthChanged(this, new EventHandlers.StringEventArgs(p_widthIntegerValue)); }}
		/// <summary>
		/// The wrapper of this event takes a string parameter, which is later converted to an int. If there was no terrain created/loaded, 
		/// then the length of the terrain that will be created later by the level editor is changed. If there is an editable terrain created, 
		/// then its length will change on the fly (this behaviour is not shown in the example, but you can see it by enabling the create terrain 
		/// UI with an existing terrain). This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
		/// In the example scene this wrapper is assigned to the length input text field of the create terrain menu.
		/// </summary>
		public void OnTerrainLengthChanged(string p_lengthIntegerValue) {if (events.OnTerrainLengthChanged != null) { events.OnTerrainLengthChanged(this, new EventHandlers.StringEventArgs(p_lengthIntegerValue)); }}
		/// <summary>
		/// The wrapper of this event takes a string parameter, which is later converted to an int. If there was no terrain created/loaded, 
		/// then the height of the terrain that will be created later by the level editor is changed. If there is an editable terrain created, 
		/// then its height will change on the fly (this behaviour is not shown in the example, but you can see it by enabling the create terrain 
		/// UI with an existing terrain). This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
		/// In the example scene this wrapper is assigned to the height input text field of the create terrain menu.
		/// </summary>
		public void OnTerrainHeightChanged(string p_heightIntegerValue) {if (events.OnTerrainHeightChanged != null) { events.OnTerrainHeightChanged(this, new EventHandlers.StringEventArgs(p_heightIntegerValue)); }}
		/// <summary>
		/// The wrapper of this event takes an int parameter. The passed parameter is used as a texture index in the texture array from the terrain 
		/// texture config assigned to 'LE_ConfigTerrain.TerrainTextureConfig'. This index stands for the first (base) texture of the later created terrain. 
		/// Call this wrapper only if there was no terrain created or loaded. This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
		/// In the example scene the 'LE_GUIInterface_uGUIimpl' script assigns this wrapper to the texture picker in the create terrain menu.
		/// </summary>
		public void OnTerrainBaseTextureChanged(int p_baseTextureIndex) {if (events.OnTerrainBaseTextureChanged != null) { events.OnTerrainBaseTextureChanged(this, new EventHandlers.IntEventArgs(p_baseTextureIndex)); }}
		/// <summary>
		/// The wrapper of this event takes an int parameter. The passed parameter is used as a texture index in the texture array of the edited terrain. 
		/// The selected texture (splat prototype) will be applied to the terrain editor logic. 
		/// This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
		/// In the example scene the 'LE_GUIInterface_uGUIimpl' script assigns this wrapper to the paint texture picker in the terrain edit menu.
		/// </summary>
		public void OnTerrainPaintTextureChanged(int p_paintTextureIndex) {if (events.OnTerrainPaintTextureChanged != null) { events.OnTerrainPaintTextureChanged(this, new EventHandlers.IntEventArgs(p_paintTextureIndex)); }}
		/// <summary>
		/// The wrapper of this event takes a 'Texture2D' parameter. The passed texture must be contained in the terrain texture config assigned to 
		/// 'LE_ConfigTerrain.TerrainTextureConfig'. The given texture will be added to the used terrain textures. 
		/// This texture will also be selected for painting. This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
		/// In the example scene the 'LE_GUIInterface_uGUIimpl' script calls this wrapper when the 'add new' button in the paint texture picker in the 
		/// terrain edit menu is clicked.
		/// </summary>
		public void OnTerrainPaintTextureAdded(Texture2D p_texture) {if (events.OnTerrainPaintTextureAdded != null) { events.OnTerrainPaintTextureAdded(this, new EventHandlers.TextureEventArgs(p_texture)); }}
		/// <summary>
		/// The wrapper of this event takes an int parameter. The passed parameter is used as a texture index in the brush texture array assigned to 
		/// 'LE_ConfigTerrain.Brushes'. The used brush will be changed in the terrain editor logic. Also, the brush projector texture will be changed. 
		/// This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
		/// In the example scene the 'LE_GUIInterface_uGUIimpl' script assigns this wrapper to the brush texture picker in the terrain edit menu.
		/// </summary>
		public void OnTerrainBrushChanged(int p_brushIndex) {if (events.OnTerrainBrushChanged != null) { events.OnTerrainBrushChanged(this, new EventHandlers.IntEventArgs(p_brushIndex)); }}
		/// <summary>
		/// The wrapper of this event takes a float parameter, which must be in range [0,1]. The brush size is calculated from the passed value like this: 
		/// 'max(0.002, value²)'. The brush has the size of the terrain if the size value is 1. 
		/// This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
		/// In the example scene this wrapper is assigned to the 'Size' slider of the terrain edit menu.
		/// </summary>
		public void OnTerrainEditBrushSizeChanged(float p_brushSize) {if (events.OnTerrainEditBrushSizeChanged != null) { events.OnTerrainEditBrushSizeChanged(this, new EventHandlers.FloatEventArgs(p_brushSize)); }}
		/// <summary>
		/// The wrapper of this event takes a float parameter, which must be in range [0,1]. The amount is calculated from the passed value like this: 
		/// 'max(0.002, value)'. If you need to set a negative amount, for example to lower the terrain combine this event with 
		/// 'OnTerrainChangeHeightModeChanged'. This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
		/// In the example scene this wrapper is assigned to the 'Amount' slider of the terrain edit menu.
		/// </summary>
		public void OnTerrainEditBrushAmountChanged(float p_brushAmount) {if (events.OnTerrainEditBrushAmountChanged != null) { events.OnTerrainEditBrushAmountChanged(this, new EventHandlers.FloatEventArgs(p_brushAmount)); }}
		/// <summary>
		/// The wrapper of this event takes a float parameter, which must be in range [0,1]. The target value is set in the terrain editor logic. 
		/// A value of 0 means that the terrain will be lowered to the bottom. A value of 0.5 will raise the terrain to the half of the terrain's 
		/// height and a value of one will raise the terrain to the maximal height. This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' 
		/// is set to 'true'. In the example scene this wrapper is assigned to the 'Target Value' slider of the terrain edit menu.
		/// </summary>
		public void OnTerrainEditBrushTargetValueChanged(float p_brushTargetValue) {if (events.OnTerrainEditBrushTargetValueChanged != null) { events.OnTerrainEditBrushTargetValueChanged(this, new EventHandlers.FloatEventArgs(p_brushTargetValue)); }}
		/// <summary>
		/// The wrapper of this event takes a float parameter, which must be in range [0,1]. The smooth direction angle is calculated like this: 
		/// 'round(value*16)*22.5'. This way only 16 directions are available. This is required due to the smoothing function implementation. 
		/// This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
		/// In the example scene this wrapper is assigned to the 'Is Directed' toggle of the terrain smooth menu.
		/// </summary>
		public void OnTerrainEditDirectionChanged(float p_direction) {if (events.OnTerrainEditDirectionChanged != null) { events.OnTerrainEditDirectionChanged(this, new EventHandlers.FloatEventArgs(p_direction)); }}
		/// <summary>
		/// The wrapper of this event takes a bool parameter, which indicates if the terrain is raised ('true') or lowered ('false'). 
		/// This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
		/// In the example scene this wrapper is assigned to the 'Raise' and 'Lower' toggle buttons of the raise/lower height terrain edit menu.
		/// </summary>
		public void OnTerrainChangeHeightModeChanged(bool p_isRaiseHeight) {if (events.OnTerrainChangeHeightModeChanged != null) { events.OnTerrainChangeHeightModeChanged(this, new EventHandlers.TerrainChangeHeightModeEventArgs(p_isRaiseHeight ? EventHandlers.ETerrainChangeHeightMode.RAISE : EventHandlers.ETerrainChangeHeightMode.LOWER)); }}
		/// <summary>
		/// The wrapper of this event takes a bool parameter, which indicates if the terrain smoothing is directed. Use in combination with 
		/// 'OnTerrainEditDirectionChanged' to set the smooth direction. If the smooth function is directed, then only the neightbours in the 
		/// smooth direction are used to calculate the smoothed height for a certain point. This allows to smooth a mountain crest without lowering 
		/// it or to smooth a riverbed without raising it. This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
		/// In the example scene this wrapper is assigned to the 'Is Directed' toggle of the smooth terrain edit mode menu.
		/// </summary>
		public void OnTerrainIsDirectedSmoothChanged(bool p_isDirectedSmooth) {if (events.OnTerrainIsDirectedSmoothChanged != null) { events.OnTerrainIsDirectedSmoothChanged(this, new EventHandlers.BoolEventArgs(p_isDirectedSmooth)); }}
		/// <summary>
		/// This parameterless event wrapper will create a terrain if no terrain was created or loaded. Use in combination with 'OnTerrainWidthChanged, 
		/// OnTerrainLengthChanged, OnTerrainHeightChanged, OnTerrainBaseTextureChanged' to setup the terrain values. This event is available only if 
		/// 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. In the example scene this wrapper is assigned to the 'Create Terrain' button.
		/// </summary>
		public void OnTerrainCreateBtn() { if (events.OnTerrainCreateBtn != null) { events.OnTerrainCreateBtn(this, System.EventArgs.Empty); } }
		/// <summary>
		/// The wrapper of this event takes an int parameter, which is converted to the 'LE_ETerrainEditMode (CHANGE_HEIGHT, CHANGE_HEIGHT_TO_TARGET_VALUE, 
		/// SMOOTH_HEIGHT, DRAW_TEXTURE)' enum. This event changes the terrain edit mode of the terrain editor. 
		/// This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
		/// In the example scene this wrapper is assigned to the terrain edit mode buttons below the tab buttons at top right.
		/// </summary>
		public void OnTerrainEditModeBtn(int p_editMode) { if (events.OnTerrainEditModeBtn != null) { events.OnTerrainEditModeBtn(this, new EventHandlers.TerrainEditModeEventArgs((LE_ETerrainEditMode)p_editMode)); } }
		/// <summary>
		/// This parameterless event wrapper will bring the terrain editor in the read terrain paint height mode. The editor will read the terrain height 
		/// at the point where the next mouse click/touch will be. Use in combination with 'SetTerrainIsReadingPaintHeight' delegate to visualize the 
		/// reading process. Use in combination with 'SetTerrainEditBrushTargetValue' delegate to visualize the new read height. 
		/// This event is available only if 'LE_LevelEditorMain.IS_TERRAIN_EDITOR' is set to 'true'. 
		/// In the example scene this wrapper is assigned to the 'Read Terrain Paint Height' button.
		/// </summary>
		public void OnTerrainReadPaintHeightBtn() { if (events.OnTerrainReadPaintHeightBtn != null) { events.OnTerrainReadPaintHeightBtn(this, System.EventArgs.Empty); } }

		/// <summary>
		/// The wrapper of this event takes a bool parameter, which is converted to the 'LE_EObjectEditSpace (LOCAL, GLOBAL)' enum. 
		/// This event changes the object edit space of the object editor. 
		/// This event is available only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'. 
		/// In the example scene this wrapper is assigned to the object edit space button below the tab buttons at top right.
		/// </summary>
		public void OnObjectEditSpaceBtn(bool p_falseForLocal_trueForGlobal) { if (events.OnObjectEditSpaceBtn != null) { events.OnObjectEditSpaceBtn(this, new EventHandlers.ObjectEditSpaceEventArgs(p_falseForLocal_trueForGlobal ? LE_EObjectEditSpace.WORLD : LE_EObjectEditSpace.SELF)); } }
		/// <summary>
		/// The wrapper of this event takes an int parameter, which is converted to the 'LE_EObjectEditMode (NO_EDIT, MOVE, ROTATE, SCALE)' enum. 
		/// This event changes the object edit mode of the object editor. 
		/// This event is available only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'. 
		/// In the example scene this wrapper is assigned to the object edit mode buttons below the tab buttons at top right.
		/// </summary>
		public void OnObjectEditModeBtn(int p_editMode) { if (events.OnObjectEditModeBtn != null) { events.OnObjectEditModeBtn(this, new EventHandlers.ObjectEditModeEventArgs((LE_EObjectEditMode)p_editMode)); } }
		/// <summary>
		/// The wrapper of this event takes two parameters: a 'LE_Object' and a string. The passed 'LE_Object' is the resource reference to the 
		/// object prefab and the string parameter is the resource path to the prefab. The draggable object will be set in the object editor. 
		/// Use in combination with 'IsObjectDragged, SetDraggableObjectMessage, SetDraggableObjectState' delegates to implement drag&drop. 
		/// This event is available only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'. 
		/// In the example scene this wrapper is assigned to the leaf nodes of the object tree browser.
		/// </summary>
		public void OnObjectSelectDraggable(LE_Object p_objPrefab, string p_resourcePath) { if (events.OnObjectSelectDraggable != null) { events.OnObjectSelectDraggable(this, new EventHandlers.ObjectSelectDraggableEventArgs(p_objPrefab, p_resourcePath)); } }
		/// <summary>
		/// This parameterless event wrapper will focus the camera on the selected object. Call this wrapper only if an object is selected. 
		/// Use in combination with 'SetIsSelectedObjectFocusBtnInteractable' to disable UI if no object is selected. 
		/// This event is available only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'. 
		/// In the example scene this wrapper is assigned to the 'Focus' button in the objects menu.
		/// </summary>
		public void OnSelectedObjectFocusBtn() { if (events.OnSelectedObjectFocusBtn != null) { events.OnSelectedObjectFocusBtn(this, System.EventArgs.Empty); } }
		/// <summary>
		/// This parameterless event wrapper will duplicate/clone the selected object. Call this wrapper only if an object is selected and can 
		/// be duplicated. Use in combination with 'SetIsSelectedObjectDuplicateBtnInteractable' to disable UI if no object is selected or it 
		/// cannot be duplicated. This event is available only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'. 
		/// In the example scene this wrapper is assigned to the 'Duplicate' button in the objects menu.
		/// </summary>
		public void OnSelectedObjectDuplicateBtn() { if (events.OnSelectedObjectDuplicateBtn != null) { events.OnSelectedObjectDuplicateBtn(this, System.EventArgs.Empty); } }
		/// <summary>
		/// This parameterless event wrapper will delete the selected object. Call this wrapper only if an object is selected. 
		/// Use in combination with 'SetIsSelectedObjectDeleteBtnInteractable' to disable UI if no object is selected. 
		/// This event is available only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'. 
		/// In the example scene this wrapper is assigned to the 'Delete' button in the objects menu.
		/// </summary>
		public void OnSelectedObjectDeleteBtn() { if (events.OnSelectedObjectDeleteBtn != null) { events.OnSelectedObjectDeleteBtn(this, System.EventArgs.Empty); } }
		/// <summary>
		/// The wrapper of this event takes a bool parameter, which indicates if the rigidbody should be sent to sleep on level load. 
		/// Call this wrapper only if an object with a rigidbody is selected and the sleep setting for this object can be changed. 
		/// Use in combination with 'SetIsSelectedObjectSleepPropertyInteractable' delegate to disable UI if no object is selected or its 
		/// sleep setting is not available. Use in combination with 'SetSelectedObjectIsSleepOnStartPropertyValue' delegate to keep your UI updated. 
		/// This event is available only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'. 
		/// In the example scene this wrapper is assigned to the 'Sleep On Start' toggle in the object property menu.
		/// </summary>
		public void OnSelectedObjectIsSleepOnStartChanged(bool p_isSleepingOnStart) { if (events.OnSelectedObjectIsSleepOnStartChanged != null) { events.OnSelectedObjectIsSleepOnStartChanged(this, new EventHandlers.BoolEventArgs(p_isSleepingOnStart)); } }
		/// <summary>
		/// The wrapper of this event takes a 'Color' parameter, which is applied to the materials of the level object. Call this wrapper only if 
		/// an object is selected and the color setting for this object can be changed. Use in combination with 
		/// 'SetIsSelectedObjectColorPropertyInteractable' delegate to disable UI if no object is selected or its color setting is not available. 
		/// Use in combination with 'SetSelectedObjectColorPropertyValue' delegate to keep your UI updated. 
		/// This event is available only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'. 
		/// In the example scene the 'LE_GUIInterface_uGUIimpl' script assigns this wrapper to the color picker in the object property menu.
		/// </summary>
		public void OnSelectedObjectColorChanged(Color p_color) { if (events.OnSelectedObjectColorChanged != null) { events.OnSelectedObjectColorChanged(this, new EventHandlers.ColorEventArgs(p_color)); } }
		/// <summary>
		/// The wrapper of this event takes an int parameter as index for the variation, which is applied to the object. Call this wrapper only if 
		/// an object is selected and there are multiple variations available. Use in combination with 
		/// 'SetIsSelectedObjectColorPropertyInteractable' delegate to disable UI if no object is selected or its color setting is not available. 
		/// Use in combination with 'SetSelectedObjectVariationPropertyValue' delegate to keep your UI updated. 
		/// This event is available only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'. 
		/// In the example scene the 'LE_GUIInterface_uGUIimpl' script assigns this wrapper to the variation browser in the object property menu.
		/// </summary>
		public void OnSelectedObjectVariationIndexChanged(int p_variationIndex) { if (events.OnSelectedObjectVariationIndexChanged != null) { events.OnSelectedObjectVariationIndexChanged(this, new EventHandlers.IntEventArgs(p_variationIndex)); } }
		// <summary>
		/// This parameterless event wrapper will search the selected prefab in the scene.
		/// If an object instance of this prefab is found, then the editor will select this instance and focus on it.
		/// If there are multiple instances of this object and one istance is already selected, then the editor will focus on the next instance.
		/// Call this wrapper only if a prefab is selected.
		/// Use in combination with 'SetIsSelectedPrefabFindBtnInteractable' to disable UI if no prefab is selected.
		/// This event is available only if 'LE_LevelEditorMain.IS_OBJECT_EDITOR' is set to 'true'. 
		/// In the example scene this wrapper is assigned to the 'Find' button in the objects menu.
		/// </summary>
		public void OnSelectedPrefabFindBtn() { if (events.OnSelectedPrefabFindBtn != null) { events.OnSelectedPrefabFindBtn(this, System.EventArgs.Empty); } }

		/// <summary>
		/// This parameterless event wrapper will save the level. Use in combination with 'LE_EventInterface.OnSave' to get the serialized level data. 
		/// In the example scene this wrapper is assigned to the 'Save' button. 
		/// </summary>
		public void OnLevelSaveBtn() { if (events.OnLevelSaveBtn != null) { events.OnLevelSaveBtn(this, System.EventArgs.Empty); } }
		/// <summary>
		/// This parameterless event wrapper will load the level. Use in combination with 'LE_EventInterface.OnLoad' to provide the serialized level data. 
		/// In the example scene this wrapper is assigned to the 'Load' button.
		/// </summary>
		public void OnLevelLoadBtn() { if (events.OnLevelLoadBtn != null) { events.OnLevelLoadBtn(this, System.EventArgs.Empty); } }
		/// <summary>
		/// This parameterless event wrapper will render the level icon by capturing the screen. Use in combination with 'SetLevelIcon' delegate to visualize the level icon. 
		/// In the example scene this wrapper is assigned to the 'Render Level Icon' button.
		/// </summary>
		public void OnLevelRenderIconBtn() { if (events.OnLevelRenderIconBtn != null) { events.OnLevelRenderIconBtn(this, System.EventArgs.Empty); } }
	}
}
