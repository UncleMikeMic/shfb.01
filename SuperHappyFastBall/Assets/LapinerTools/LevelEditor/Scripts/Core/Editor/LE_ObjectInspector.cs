#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace LE_LevelEditor.Core
{
	[CustomEditor(typeof(LE_Object))] 
	public class LE_ObjectInspector : Editor
	{
		private GUIContent m_contentIconPath = new GUIContent("Icon Path",
			"Resource path of the object's icon texture, which will be displayed in the object selection. The texture will be generated from Unity's built-in preview and this property will be set automatically later. However, if you want to set custom icons you can use this property. There is no direct reference (only a string) to the texture here, because there is no need to load the texture outside the level editor (no icon needed ingame).");
		private GUIContent m_contentIsSmartMove = new GUIContent("Is Smart Move",
			"If 'true': a smart move handle will be displayed when this object is selected in the level editor.");
		private GUIContent m_contentIsMovable = new GUIContent("Is Movable",
			"If 'true': move handle will be displayed when this object is selected in the level editor.");
		private GUIContent m_contentIsMovableOnX = new GUIContent("Is Movable On X",
			"If 'true': move handle will have x axis.");
		private GUIContent m_contentIsMovableOnY = new GUIContent("Is Movable On Y",
			"If 'true': move handle will have y axis.");
		private GUIContent m_contentIsMovableOnZ = new GUIContent("Is Movable On Z",
			"If 'true': move handle will have z axis.");
		private GUIContent m_contentIsPlacementRotationByNormal = new GUIContent("Is Normal Oriented Placement",
			"If 'true': the object will be rotated to fit the surface normal when placed. For example when a car is drag and dropped on a steep hill the player will probably want it to touch the ground with all four wheels and not hang half in the hill. With this property set to true the car will be rotated so that its up vector will be equal to the normal of the hill. Applied also if the object is not marked as rotatable.");
		private GUIContent m_contentRotationRndEulerX = new GUIContent("Euler Rotation X Randomizer",
			"If '> 0': then the euler rotation (0°-360°) on the X axis of this object will be randomized when the object is placed. The euler rotation will be increased or decreased by a value between 0 and +- half of the value. For example, trees can use this value to make a forest look more varied.");
		private GUIContent m_contentRotationRndEulerY = new GUIContent("Euler Rotation Y Randomizer",
			"If '> 0': then the euler rotation (0°-360°) on the Y axis of this object will be randomized when the object is placed. The euler rotation will be increased or decreased by a value between 0 and +- half of the value. For example, trees can use this value to make a forest look more varied.");
		private GUIContent m_contentRotationRndEulerZ = new GUIContent("Euler Rotation Z Randomizer",
			"If '> 0': then the euler rotation (0°-360°) on the Z axis of this object will be randomized when the object is placed. The euler rotation will be increased or decreased by a value between 0 and +- half of the value. For example, trees can use this value to make a forest look more varied.");
		private GUIContent m_contentIsRotatable = new GUIContent("Is Rotatable",
			"If 'true': rotate handle will be displayed when this object is selected in the level editor.");
		private GUIContent m_contentIsRotatableAroundX = new GUIContent("Is Rotatable Around X",
			"If 'true': rotate handle will have x axis.");
		private GUIContent m_contentIsRotatableAroundY = new GUIContent("Is Rotatable Around Y",
			"If 'true': rotate handle will have y axis.");
		private GUIContent m_contentIsRotatableAroundZ = new GUIContent("Is Rotatable Around Z",
			"If 'true': rotate handle will have z axis.");
		private GUIContent m_contentUniformScaleRnd = new GUIContent("Uniform Scale Randomizer",
			"If '> 0': then the scale of this object will be randomized when the object is placed. The random scale change is always uniform, but applied only on scaleable axes. The scale will be increased or decreased by a value between 0 and +- half of the value. For example, trees can use this value to make a forest look more varied.");
		private GUIContent m_contentIsUniformScale = new GUIContent("Is Uniform Scale",
			"If 'true': scale handle will have only one axis. Scaling this axis will scale the object on all scaleable axes at same rate.");
		private GUIContent m_contentIsScaleable = new GUIContent("Is Scaleable",
			"If 'true': scale handle will be displayed when this object is selected in the level editor.");
		private GUIContent m_contentIsScaleableOnX = new GUIContent("Is Scaleable On X",
			"If 'true': scale handle will have x axis.");
		private GUIContent m_contentIsScaleableOnY = new GUIContent("Is Scaleable On Y",
			"If 'true': scale handle will have y axis.");
		private GUIContent m_contentIsScaleableOnZ = new GUIContent("Is Scaleable On Z",
			"If 'true': scale handle will have z axis.");
		private GUIContent m_contentIsRigidbodySleepingStart = new GUIContent("Is Sleeping On Start",
			"If 'true' and a rigidbody is attached to this object or its children then all attached rigidbodies will go into the sleep state (Rigidbody.Sleep()) when the level is loaded. This will increase performance in the first few frames if you level has many rigidbodies. For example if you have a pile of crates you don't want them to fall down before the player touches them, therefore you will want them to sleep. However, if you have a huge snowball that has to roll behind the player you will probably not want to send it to sleep.");
		private GUIContent m_contentIsRigidbodySleepingStartEditable = new GUIContent("Is Sleeping On Start Editable",
			"If 'true': when this object is selected in the editor then a property is presented to the end user that allows to change 'Is Sleeping On Start'.");
		private GUIContent m_contentMaxInstancesInLevel = new GUIContent("Max. Instances",
			"If 'not 0': the number of instances of this object in a level will be limited to the value of this property. For example if some of your objects are very detailed then you will want to limit the maximal count, because of performance reasons. Another example is the start position of the player, since it should exist only once. No more objects of this type can be drag&dropped or cloned into the level once the limit is reached.");
		private GUIContent m_contentIsWithColorProperty = new GUIContent("Is Color",
			"If 'true': a color picker will be displayed in the level editor. Additionally, a default color can be specified in the 'Color' property. Modifies the '_Color' property of all materials in all renderers including those in children.");
		private GUIContent m_contentSnapType = new GUIContent("Snap",
			"Choose among the available snap types: SNAP_DISABLED, SNAP_TO_TERRAIN, SNAP_TO_OBJECT, SNAP_TO_3D_GRID, SNAP_TO_2D_GRID_AND_TERRAIN");
		private GUIContent m_contentObjectSnapPoints = new GUIContent("Snap Points",
			"This array contains definitions of snap points for the 'Snap To Object' feature. A snap point defines the root location of the snap point and the objects which can be snapped to this point. The local transformation in the space of the point can be set for each snap object individually.");
		private GUIContent m_contentRootSnapPointIndex = new GUIContent("Root Snap Point",
			"if 'not -1': the snap point at given index will be deactivated when this object is created through being snapped to another object.");
		private GUIContent m_contentIsDrawSnapToObjectUI = new GUIContent("Built In UI",
			"if 'true': the built in UI for the snap object selection will be drawn. It uses the 'SnapToObjectUI' material from the resource folder.");
		private GUIContent m_contentSnapGridOffset = new GUIContent("Grid Offset",
			"Defines the offset of the snap grid.");
		private GUIContent m_contentSnapGridCellSize = new GUIContent("Grid Cell Size",
			"Defines the cell size of the snap grid.");
		private GUIContent m_contentIsLevelStreaming = new GUIContent("Level Streaming Enabled",
			"if 'true': then this object will be instantiated only if it is closer to the 'Main Camera' than the given instantiate distance. This way a level can be loaded stepwise. Additionally, performance is improved by destroying this object when it is further away from the camera than the given destroy distance.");
		private GUIContent m_contentLevelStreamingUpdateFrequency = new GUIContent("Update Frequency",
			"Defines the number of skipped frames between streamed object state updates. A low value should be used for fast games. However a low value (e.g. '0') will have performance implications if the number of streamed objects is very high (>300). A slow game can have a higher value. The value of '15' (with 30 FPS in average) means that streamed objects will be checked twice in a second which is fast enough for slow games.");
		private GUIContent m_contentLevelStreamingInstantiateDistance = new GUIContent("Instantiate Distance",
			"This object will be instantiated if it is closer to the camera than the given distance. This value is used ingame.");
		private GUIContent m_contentLevelStreamingDestroyDistance = new GUIContent("Destroy Distance",
			"This object will be destroyed if it is further away from the camera than the given distance. This value is used ingame.");
		private GUIContent m_contentLevelStreamingInstantiateDistanceInEditor = new GUIContent("Instantiate Distance(Editor)",
			"This object will be instantiated if it is closer to the camera than the given distance. This value is used in the editor.");
		private GUIContent m_contentLevelStreamingDestroyDistanceInEditor = new GUIContent("Destroy Distance(Editor)",
			"This object will be destroyed if it is further away from the camera than the given distance. This value is used in the editor.");
		private GUIContent m_contentIsTransformationCachedOnDestroyWhenLevelStreaming = new GUIContent("Cache Transformation",
			"if 'true': position, rotation and scale are cached when spawned objects are destroyed because they are too far away. The object will be instantiated with the transformation, which it had when it was destroyed, when it is close enough again.");
		private GUIContent m_contentVariationType = new GUIContent("Variation",
			"Choose among the available variation types: NONE, REPLACE_MATERIALS, ACTIVE_DEACTIVATE_OBJECTS");
		private GUIContent m_contentVariationName = new GUIContent("Name",
			"The name of this variation. It will be shown to the player/user.");
		private GUIContent m_contentVariationsDefaultIndex = new GUIContent("Variation Default Index",
			"When a new object is placed in the scene, then the variation from the Variations property with the index VariationsDefaultIndex will be selected. This property is ignored if no variations or exactly one variation were provided for the Variations property.");

		private enum EVariationType { NONE, REPLACE_MATERIALS, ACTIVE_DEACTIVATE_OBJECTS }

		private static bool s_isFoldoutTranslation = false;
		private static bool s_isFoldoutRotation = false;
		private static bool s_isFoldoutScale = false;
		private static bool s_isFoldoutPhysics = false;
		private static bool s_isFoldoutLevelStreaming = false;
		private static Dictionary<int, bool> s_isFoldoutVariation = new Dictionary<int, bool>();

		public override void OnInspectorGUI()
		{
			bool isChanged = false;
			serializedObject.Update();
			LE_Object obj = (LE_Object)target;

			Texture2D icon = (Texture2D)Resources.Load(obj.IconPath);
			if (icon != null)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Box(icon, GUILayout.Width(64), GUILayout.Height(64));
				EditorGUILayout.BeginVertical();
				GUILayout.Label(m_contentIconPath);
				obj.IconPath = EditorGUILayout.TextField(obj.IconPath);
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				obj.IconPath = EditorGUILayout.TextField(m_contentIconPath, obj.IconPath);
				EditorGUILayout.HelpBox("No icon found for resource path '" + obj.IconPath + "'!", MessageType.Error);
			}

			obj.MaxInstancesInLevel = EditorGUILayout.IntField(m_contentMaxInstancesInLevel, obj.MaxInstancesInLevel);

			EditorGUILayout.BeginHorizontal();
			obj.IsWithColorProperty = EditorGUILayout.ToggleLeft(m_contentIsWithColorProperty, obj.IsWithColorProperty);
			if (obj.IsWithColorProperty)
			{
				Color newColor = EditorGUILayout.ColorField(obj.ColorProperty);
				if (newColor != obj.ColorProperty)
				{
					obj.ColorProperty = newColor;
				}
			}
			EditorGUILayout.EndHorizontal();

			string translationSummaryExtra = obj.IsSmartMove?"smart,":"";
			string translationSummary = GetTransformationHandleSummary("Translation", "movable", translationSummaryExtra, obj.IsMovable, obj.IsMovableOnX, obj.IsMovableOnY, obj.IsMovableOnZ);
			s_isFoldoutTranslation = EditorGUILayout.Foldout(s_isFoldoutTranslation, translationSummary);
			if (s_isFoldoutTranslation)
			{
				obj.IsSmartMove = EditorGUILayout.ToggleLeft(m_contentIsSmartMove, obj.IsSmartMove);
				obj.IsMovable = EditorGUILayout.ToggleLeft(m_contentIsMovable, obj.IsMovable);
				if (obj.IsMovable)
				{
					EditorGUI.indentLevel++;
					obj.IsMovableOnX = EditorGUILayout.ToggleLeft(m_contentIsMovableOnX, obj.IsMovableOnX);
					obj.IsMovableOnY = EditorGUILayout.ToggleLeft(m_contentIsMovableOnY, obj.IsMovableOnY);
					obj.IsMovableOnZ = EditorGUILayout.ToggleLeft(m_contentIsMovableOnZ, obj.IsMovableOnZ);
					EditorGUI.indentLevel--;
				}
			}

			string rotationSummaryExtra = obj.IsPlacementRotationByNormal?"normal,":"";
			if (obj.RotationRndEulerX > 0f && !obj.IsPlacementRotationByNormal) { rotationSummaryExtra += "rndX[" + (-0.5f*obj.RotationRndEulerX) + "," + (0.5f*obj.RotationRndEulerX) + "],"; }
			if (obj.RotationRndEulerY > 0f) { rotationSummaryExtra += "rndY[" + (-0.5f*obj.RotationRndEulerY) + "," + (0.5f*obj.RotationRndEulerY) + "],"; }
			if (obj.RotationRndEulerZ > 0f && !obj.IsPlacementRotationByNormal) { rotationSummaryExtra += "rndZ[" + (-0.5f*obj.RotationRndEulerZ) + "," + (0.5f*obj.RotationRndEulerZ) + "],"; }
			string rotationSummary = GetTransformationHandleSummary("Rotation", "rotatable", rotationSummaryExtra, obj.IsRotatable, obj.IsRotatableAroundX, obj.IsRotatableAroundY, obj.IsRotatableAroundZ);
			s_isFoldoutRotation = EditorGUILayout.Foldout(s_isFoldoutRotation, rotationSummary);
			if (s_isFoldoutRotation)
			{
				GUI.enabled = !obj.IsPlacementRotationByNormal;
				if (GUI.enabled)
				{
					obj.RotationRndEulerX = Mathf.Min(360f, EditorGUILayout.FloatField(m_contentRotationRndEulerX, obj.RotationRndEulerX));
				}
				else
				{
					EditorGUILayout.TextField(m_contentRotationRndEulerX, "Ignored with 'Is Normal Oriented Placement' enabled");
				}
				GUI.enabled = true;
				obj.RotationRndEulerY = Mathf.Min(360f, EditorGUILayout.FloatField(m_contentRotationRndEulerY, obj.RotationRndEulerY));
				GUI.enabled = !obj.IsPlacementRotationByNormal;
				if (GUI.enabled)
				{
					obj.RotationRndEulerZ = Mathf.Min(360f, EditorGUILayout.FloatField(m_contentRotationRndEulerZ, obj.RotationRndEulerZ));
				}
				else
				{
					EditorGUILayout.TextField(m_contentRotationRndEulerX, "Ignored with 'Is Normal Oriented Placement' enabled");
				}
				GUI.enabled = true;
				obj.IsPlacementRotationByNormal = EditorGUILayout.ToggleLeft(m_contentIsPlacementRotationByNormal, obj.IsPlacementRotationByNormal);
				obj.IsRotatable = EditorGUILayout.ToggleLeft(m_contentIsRotatable, obj.IsRotatable);
				if (obj.IsRotatable)
				{
					EditorGUI.indentLevel++;
					obj.IsRotatableAroundX = EditorGUILayout.ToggleLeft(m_contentIsRotatableAroundX, obj.IsRotatableAroundX);
					obj.IsRotatableAroundY = EditorGUILayout.ToggleLeft(m_contentIsRotatableAroundY, obj.IsRotatableAroundY);
					obj.IsRotatableAroundZ = EditorGUILayout.ToggleLeft(m_contentIsRotatableAroundZ, obj.IsRotatableAroundZ);
					EditorGUI.indentLevel--;
				}
			}

			string scaleSummaryExtra = ((obj.IsScaleable && (obj.IsScaleableOnX || obj.IsScaleableOnY || obj.IsScaleableOnZ)) && obj.IsUniformScale)?"uniform,":"";
			if (obj.UniformScaleRnd != -1 && (obj.IsScaleableOnX || obj.IsScaleableOnY || obj.IsScaleableOnZ))
			{
				scaleSummaryExtra += "rnd";
				if (obj.IsScaleableOnX) { scaleSummaryExtra += "X"; }
				if (obj.IsScaleableOnY) { scaleSummaryExtra += "Y"; }
				if (obj.IsScaleableOnZ) { scaleSummaryExtra += "Z"; }
				scaleSummaryExtra += "[" + (-0.5f*obj.UniformScaleRnd) + "," + (0.5f*obj.UniformScaleRnd) + "],";
			}
			string scaleSummary = GetTransformationHandleSummary("Scale", "scaleable", scaleSummaryExtra, obj.IsScaleable, obj.IsScaleableOnX, obj.IsScaleableOnY, obj.IsScaleableOnZ);
			s_isFoldoutScale = EditorGUILayout.Foldout(s_isFoldoutScale, scaleSummary);
			if (s_isFoldoutScale)
			{
				obj.UniformScaleRnd = EditorGUILayout.FloatField(m_contentUniformScaleRnd, obj.UniformScaleRnd);
				obj.IsUniformScale = EditorGUILayout.ToggleLeft(m_contentIsUniformScale, obj.IsUniformScale);
				obj.IsScaleable = EditorGUILayout.ToggleLeft(m_contentIsScaleable, obj.IsScaleable);
				if (obj.IsScaleable)
				{
					EditorGUI.indentLevel++;
					obj.IsScaleableOnX = EditorGUILayout.ToggleLeft(m_contentIsScaleableOnX, obj.IsScaleableOnX);
					obj.IsScaleableOnY = EditorGUILayout.ToggleLeft(m_contentIsScaleableOnY, obj.IsScaleableOnY);
					obj.IsScaleableOnZ = EditorGUILayout.ToggleLeft(m_contentIsScaleableOnZ, obj.IsScaleableOnZ);
					EditorGUI.indentLevel--;
				}
			}

			string physicsSummary = "Physics [" + (obj.IsRigidbodySleepingStart?"sleep on start":"awake on start") + " - " + (obj.IsRigidbodySleepingStartEditable?"editable":"fixed") + "]";
			s_isFoldoutPhysics = EditorGUILayout.Foldout(s_isFoldoutPhysics, physicsSummary);
			if (s_isFoldoutPhysics)
			{
				obj.IsRigidbodySleepingStart = EditorGUILayout.ToggleLeft(m_contentIsRigidbodySleepingStart, obj.IsRigidbodySleepingStart);
				obj.IsRigidbodySleepingStartEditable = EditorGUILayout.ToggleLeft(m_contentIsRigidbodySleepingStartEditable, obj.IsRigidbodySleepingStartEditable);
			}

			string levelStreamingSummary = "Level Streaming [" + (obj.IsLevelStreaming?"game(" + obj.LevelStreamingInstantiateDistance + "," + obj.LevelStreamingDestroyDistance + ") editor(" + obj.LevelStreamingInstantiateDistanceInEditor + "," + obj.LevelStreamingDestroyDistanceInEditor + ")":"disabled") + "]";
			s_isFoldoutLevelStreaming = EditorGUILayout.Foldout(s_isFoldoutLevelStreaming, levelStreamingSummary);
			if (s_isFoldoutLevelStreaming)
			{
				obj.IsLevelStreaming = EditorGUILayout.ToggleLeft(m_contentIsLevelStreaming, obj.IsLevelStreaming);
				if (obj.IsLevelStreaming)
				{
					EditorGUI.indentLevel++;
					obj.LevelStreamingUpdateFrequency = EditorGUILayout.IntField(m_contentLevelStreamingUpdateFrequency, obj.LevelStreamingUpdateFrequency);
					obj.LevelStreamingInstantiateDistance = EditorGUILayout.FloatField(m_contentLevelStreamingInstantiateDistance, obj.LevelStreamingInstantiateDistance);
					obj.LevelStreamingDestroyDistance = EditorGUILayout.FloatField(m_contentLevelStreamingDestroyDistance, obj.LevelStreamingDestroyDistance);
					obj.LevelStreamingInstantiateDistanceInEditor = EditorGUILayout.FloatField(m_contentLevelStreamingInstantiateDistanceInEditor, obj.LevelStreamingInstantiateDistanceInEditor);
					obj.LevelStreamingDestroyDistanceInEditor = EditorGUILayout.FloatField(m_contentLevelStreamingDestroyDistanceInEditor, obj.LevelStreamingDestroyDistanceInEditor);
					obj.IsTransformationCachedOnDestroyWhenLevelStreaming = EditorGUILayout.ToggleLeft(m_contentIsTransformationCachedOnDestroyWhenLevelStreaming, obj.IsTransformationCachedOnDestroyWhenLevelStreaming);
					EditorGUI.indentLevel--;
				}
			}

			obj.SnapType = (LE_Object.ESnapType)EditorGUILayout.EnumPopup(m_contentSnapType, obj.SnapType);
			switch (obj.SnapType)
			{
				case LE_Object.ESnapType.SNAP_TO_TERRAIN:
					EditorGUILayout.HelpBox("SNAP_TO_TERRAIN: object will snap to terrain every time the terrain is changed or the object is . If normal oriented placement is active then the object's orientation will be changed accordingly after every terrain change or object's position change.", MessageType.Info);
					break;
				case LE_Object.ESnapType.SNAP_TO_OBJECT:
					EditorGUILayout.HelpBox("SNAP_TO_OBJECT: in this snap mode it is possible to snap other objects to this level object.", MessageType.Info);
					break;
				case LE_Object.ESnapType.SNAP_TO_3D_GRID:
					EditorGUILayout.HelpBox("SNAP_TO_3D_GRID: object can be placed only within the given grid on all axes.", MessageType.Info);
					break;
				case LE_Object.ESnapType.SNAP_TO_2D_GRID_AND_TERRAIN:
					EditorGUILayout.HelpBox("SNAP_TO_2D_GRID_AND_TERRAIN: object can be placed only within the given grid on the x and z axes. The y axis value is defined by the terrain.", MessageType.Info);
					break;
			}
			if (obj.SnapType == LE_Object.ESnapType.SNAP_TO_OBJECT)
			{
				// object snap definition
				EditorGUI.indentLevel++;
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_objectSnapPoints"), m_contentObjectSnapPoints, true);
				if(EditorGUI.EndChangeCheck())
				{
					serializedObject.ApplyModifiedProperties();
				}
				if (obj.ObjectSnapPoints.Length == 0)
				{
					isChanged = true;
					// init default values
					LE_ObjectSnapPoint[] snapPoints = new LE_ObjectSnapPoint[1];
					snapPoints[0] = new LE_ObjectSnapPoint();
					obj.ObjectSnapPoints = snapPoints;
				}
				// snap root point
				obj.RootSnapPointIndex = EditorGUILayout.IntField(m_contentRootSnapPointIndex, obj.RootSnapPointIndex);
				// snap to object UI
				obj.IsDrawSnapToObjectUI = EditorGUILayout.ToggleLeft(m_contentIsDrawSnapToObjectUI, obj.IsDrawSnapToObjectUI);
				EditorGUI.indentLevel--;
			}
			else if (obj.SnapType == LE_Object.ESnapType.SNAP_TO_3D_GRID)
			{
				obj.SnapGridOffset = EditorGUILayout.Vector3Field(m_contentSnapGridOffset, obj.SnapGridOffset);
				obj.SnapGridCellSize = EditorGUILayout.Vector3Field(m_contentSnapGridCellSize, obj.SnapGridCellSize);
			}
			else if (obj.SnapType == LE_Object.ESnapType.SNAP_TO_2D_GRID_AND_TERRAIN)
			{
				Vector2 offset2d = new Vector2(obj.SnapGridOffset.x, obj.SnapGridOffset.z);
				GUIContent contentSnapGridOffsetX = new GUIContent(m_contentSnapGridOffset);
				contentSnapGridOffsetX.text += " X";
				offset2d.x = EditorGUILayout.FloatField(contentSnapGridOffsetX, offset2d.x);
				GUIContent contentSnapGridOffsetY = new GUIContent(m_contentSnapGridOffset);
				contentSnapGridOffsetY.text += " Z";
				offset2d.y = EditorGUILayout.FloatField(contentSnapGridOffsetY, offset2d.y);
				obj.SnapGridOffset = new Vector3(offset2d.x, obj.SnapGridOffset.y, offset2d.y);

				Vector2 cellSize2d = new Vector2(obj.SnapGridCellSize.x, obj.SnapGridCellSize.z);
				GUIContent contentSnapGridCellSizeX = new GUIContent(m_contentSnapGridCellSize);
				contentSnapGridCellSizeX.text += " X";
				cellSize2d.x = EditorGUILayout.FloatField(contentSnapGridCellSizeX, cellSize2d.x);
				GUIContent contentSnapGridCellSizeY = new GUIContent(m_contentSnapGridCellSize);
				contentSnapGridCellSizeY.text += " Z";
				cellSize2d.y = EditorGUILayout.FloatField(contentSnapGridCellSizeY, cellSize2d.y);
				obj.SnapGridCellSize = new Vector3(cellSize2d.x, obj.SnapGridCellSize.y, cellSize2d.y);
			}

			EVariationType variationType = GetVariationType(obj);
			EVariationType newVariationType = (EVariationType)EditorGUILayout.EnumPopup(m_contentVariationType, variationType);
			Variation_ChangeTypeCheck(obj, variationType, newVariationType);
			switch (newVariationType)
			{
				case EVariationType.REPLACE_MATERIALS:
					EditorGUILayout.HelpBox("REPLACE_MATERIALS: will replace all materials of the selected renderers in this level objects. The player will be able to choose between these variations if he selects an instance of this level object in the scene.", MessageType.Info);
					break;
				case EVariationType.ACTIVE_DEACTIVATE_OBJECTS:
					EditorGUILayout.HelpBox("ACTIVE_DEACTIVATE_OBJECTS: will activate/deactivate sub objects of this level object. The player will be able to choose between these variations if he selects an instance of this level object in the scene.", MessageType.Info);
					break;
			}
			LE_ObjectVariationBase[] VariationsGeneric = obj.Variations;
			LE_ObjectVariationMaterial[] VariationsMaterial = obj.VariationsMaterial;
			LE_ObjectVariationActivateDeactivate[] VariationsActivateDeactivate = obj.VariationsActivateDeactivate;
			if (VariationsGeneric.Length > 1)
			{
				int newVariationIndex = Mathf.Clamp(EditorGUILayout.IntField(m_contentVariationsDefaultIndex, obj.VariationsDefaultIndex), 0, VariationsGeneric.Length-1);
				if (newVariationIndex != obj.VariationsDefaultIndex)
				{
					obj.VariationsDefaultIndex = newVariationIndex;
					isChanged = true;
				}
			}
			EditorGUI.indentLevel++;
			for (int i = 0; i < VariationsGeneric.Length; i++)
			{
				LE_ObjectVariationBase variation = VariationsGeneric[i];
				if (variation == null) { continue; }
				if (!s_isFoldoutVariation.ContainsKey(i)) { s_isFoldoutVariation[i] = false; }
				EditorGUILayout.BeginHorizontal();
				s_isFoldoutVariation[i] = EditorGUILayout.Foldout(s_isFoldoutVariation[i], "Variation: " + variation.GetName());
				if (GUILayout.Button("Remove"))
				{
					if (newVariationType == EVariationType.REPLACE_MATERIALS && i < VariationsMaterial.Length)
					{
						List<LE_ObjectVariationMaterial> list = new List<LE_ObjectVariationMaterial>(VariationsMaterial);
						list.RemoveAt(i);
						obj.VariationsMaterial = list.ToArray();
					}
					else if (newVariationType == EVariationType.ACTIVE_DEACTIVATE_OBJECTS && i < VariationsActivateDeactivate.Length)
					{
						List<LE_ObjectVariationActivateDeactivate> list = new List<LE_ObjectVariationActivateDeactivate>(VariationsActivateDeactivate);
						list.RemoveAt(i);
						obj.VariationsActivateDeactivate = list.ToArray();
					}
				}
				EditorGUILayout.EndHorizontal();
				if (s_isFoldoutVariation[i])
				{
					if (newVariationType == EVariationType.REPLACE_MATERIALS && i < VariationsMaterial.Length)
					{
						DrawVariationMaterial(obj, VariationsMaterial[i]);
					}
					else if (newVariationType == EVariationType.ACTIVE_DEACTIVATE_OBJECTS && i < VariationsActivateDeactivate.Length)
					{
						DrawVariationActivateDeactivate(obj, VariationsActivateDeactivate[i]);
					}
				}
			}
			if (newVariationType != EVariationType.NONE && GUILayout.Button("Add Variation"))
			{
				AddVariation(obj, newVariationType, "Variation " + VariationsGeneric.Length);
			}
			EditorGUI.indentLevel--;

			isChanged = SnapToTerrain_RotationAxisCheck(obj) || isChanged;
			isChanged = SnapToTerrain_MoveAxisCheck(obj) || isChanged;

			if (GUI.changed || isChanged)
			{
				EditorUtility.SetDirty(obj);
			}
		}

		private string GetTransformationHandleSummary(string p_type, string p_action, string p_extra, bool p_isEnabled, bool p_isEnabledOnX, bool p_isEnabledOnY, bool p_isEnabledOnZ)
		{
			string transformationSummary = p_type + " [" + p_extra;
			if (p_isEnabled && (p_isEnabledOnX || p_isEnabledOnY || p_isEnabledOnZ))
			{
				if (p_isEnabledOnX) { transformationSummary += "X"; }
				if (p_isEnabledOnY) { transformationSummary += "Y"; }
				if (p_isEnabledOnZ) { transformationSummary += "Z"; }
			}
			else
			{
				transformationSummary += "not " + p_action;
			}
			transformationSummary += "]";
			return transformationSummary;
		}

		private static bool SnapToTerrain_RotationAxisCheck(LE_Object p_obj)
		{
			bool isChanged = false;
			if ((p_obj.SnapType == LE_Object.ESnapType.SNAP_TO_TERRAIN || p_obj.SnapType == LE_Object.ESnapType.SNAP_TO_2D_GRID_AND_TERRAIN) &&
			    p_obj.IsPlacementRotationByNormal && p_obj.IsRotatable && (p_obj.IsRotatableAroundX || p_obj.IsRotatableAroundZ))
			{
				int userDecision = EditorUtility.DisplayDialogComplex(
					"Conflict (Terrain Snap -!- X/Z Rotation)",
					"Cannot rotate around x/z axis with 'Snap To Terrain' and 'Normal Oriented Placement' enabled. " +
					"Custom rotation would be overwritten every time the terrain is changed (even if the " +
					"change does not affect this object). Do you want to:\n" +
					"A: disable x/z rotation\n" +
					"B: disable terrain snapping\n" +
					"C: disable normal oriented placement",
					"A: Snap + Y Rot.",
					"B: No Snap",
					"C: No Oriented Placement");
				if (userDecision == 0)
				{
					// user wants 'Snap + Y Rot.'
					p_obj.IsRotatableAroundX = false;
					p_obj.IsRotatableAroundZ = false;
					EditorUtility.DisplayDialog("Conflict Solved", "X/Z Rotation was disabled!", "ok");
				}
				else if (userDecision == 1)
				{
					// user wants 'No Snap'
					p_obj.SnapType = LE_Object.ESnapType.SNAP_DISABLED;
					EditorUtility.DisplayDialog("Conflict Solved", "'Snap To Terrain' was disabled!", "ok");
				}
				else
				{
					// user wants 'No Oriented Placement'
					p_obj.IsPlacementRotationByNormal = false;
					EditorUtility.DisplayDialog("Conflict Solved", "'Normal Oriented Placement' was disabled!", "ok");
				}
				isChanged = true;
			}
			return isChanged;
		}

		private static bool SnapToTerrain_MoveAxisCheck(LE_Object p_obj)
		{
			bool isChanged = false;
			if ((p_obj.SnapType == LE_Object.ESnapType.SNAP_TO_TERRAIN || p_obj.SnapType == LE_Object.ESnapType.SNAP_TO_2D_GRID_AND_TERRAIN) &&
			    p_obj.IsMovable && p_obj.IsMovableOnY)
			{
				bool userDecision = EditorUtility.DisplayDialog(
					"Conflict (Terrain Snap -!- Y Translation)",
					"Cannot move on y axis with 'Snap To Terrain' enabled. " +
					"Movement would be ignored, because object would snap back to terrain. Do you want to:\n" +
					"A: disable y translation\n" +
					"B: disable terrain snapping",
					"A: No Y Move Axis",
					"B: No Snap");
				if (userDecision)
				{
					// user wants 'No Y Move Axis'
					p_obj.IsMovableOnY = false;
					EditorUtility.DisplayDialog("Conflict Solved", "Y move axis was disabled!", "ok");
				}
				else
				{
					// user wants 'No Snap'
					p_obj.SnapType = LE_Object.ESnapType.SNAP_DISABLED;
					EditorUtility.DisplayDialog("Conflict Solved", "'Snap To Terrain' was disabled!", "ok");
				}
				isChanged = true;
			}
			return isChanged;
		}

		private static void Variation_ChangeTypeCheck(LE_Object p_obj, EVariationType p_variationType, EVariationType p_newVariationType)
		{
			if (p_variationType != p_newVariationType)
			{
				if (p_obj.Variations.Length > 0)
				{
					bool userDecision = EditorUtility.DisplayDialog(
						"Change Variation Type",
						"All variation data will be lost if you change the variation type.",
						"Continue (clear current data)",
						"Cancel");
					if (userDecision)
					{
						InitDefaultVariation(p_obj, p_newVariationType);
					}
				}
				else
				{
					InitDefaultVariation(p_obj, p_newVariationType);
				}
			}
		}

		private static void InitDefaultVariation(LE_Object p_obj, EVariationType p_variationType)
		{
			p_obj.VariationsDefaultIndex = 0;
			p_obj.VariationsMaterial = new LE_ObjectVariationMaterial[0];
			p_obj.VariationsActivateDeactivate = new LE_ObjectVariationActivateDeactivate[0];
			if (p_variationType != EVariationType.NONE)
			{
				AddVariation(p_obj, p_variationType, "Default");
			}
		}

		private static void AddVariation(LE_Object p_obj, EVariationType p_variationType, string p_variationName)
		{
			switch (p_variationType)
			{
				case EVariationType.REPLACE_MATERIALS:
				{
					List<LE_ObjectVariationMaterial> variations = new List<LE_ObjectVariationMaterial>(p_obj.VariationsMaterial);
					variations.Add(new LE_ObjectVariationMaterial(p_variationName));
					p_obj.VariationsMaterial = variations.ToArray();
					break;
				}
				case EVariationType.ACTIVE_DEACTIVATE_OBJECTS:
				{
					List<LE_ObjectVariationActivateDeactivate> variations = new List<LE_ObjectVariationActivateDeactivate>(p_obj.VariationsActivateDeactivate);
					variations.Add(new LE_ObjectVariationActivateDeactivate(p_variationName));
					p_obj.VariationsActivateDeactivate = variations.ToArray();
					break;
				}
				default:
					Debug.LogError("LE_ObjectInspector: AddVariation: " + p_variationType + " is unknown!");
					return;
			}
		}

		private static EVariationType GetVariationType(LE_Object p_obj)
		{
			if (p_obj.Variations.Length > 0)
			{
				if (p_obj.Variations[0] is LE_ObjectVariationMaterial)
				{
					return EVariationType.REPLACE_MATERIALS;
				}
				if (p_obj.Variations[0] is LE_ObjectVariationActivateDeactivate)
				{
					return EVariationType.ACTIVE_DEACTIVATE_OBJECTS;
				}
			}
			return EVariationType.NONE;
		}

		private void DrawVariationMaterial(LE_Object p_obj, LE_ObjectVariationMaterial p_variation)
		{
			if (p_variation.LoadNFixReferences(p_obj))
			{
				EditorUtility.SetDirty(p_obj);
			}
			p_variation.Name = EditorGUILayout.TextField(m_contentVariationName, p_variation.Name);
			if (p_variation.Renderers.Length > 0)
			{
				for (int rI = 0; rI < p_variation.Renderers.Length && rI < p_variation.RendererMaterials.Length; rI++)
				{
					Renderer renderer = p_variation.Renderers[rI];
					EditorGUILayout.LabelField(renderer.name, EditorStyles.boldLabel);

					EditorGUI.indentLevel++;
					LE_ObjectVariationMaterial.Materials materials = p_variation.RendererMaterials[rI];
					for (int mI = 0; mI < materials.m_materials.Length; mI++)
					{
						materials.m_materials[mI] = (Material)EditorGUILayout.ObjectField(materials.m_materials[mI], typeof(Material), false);
					}
					EditorGUI.indentLevel--;
				}
			}
			else
			{
				EditorGUILayout.HelpBox("This object cannot have material variations, because it has no renderers with materials!", MessageType.Error);
			}
		}

		private void DrawVariationActivateDeactivate(LE_Object p_obj, LE_ObjectVariationActivateDeactivate p_variation)
		{
			if (p_variation.LoadNFixReferences(p_obj))
			{
				EditorUtility.SetDirty(p_obj);
			}
			p_variation.Name = EditorGUILayout.TextField(m_contentVariationName, p_variation.Name);
			if (p_variation.Objs.Length > 0)
			{
				for (int i = 0; i < p_variation.Objs.Length && i < p_variation.ObjIsActivateStates.Length; i++)
				{
					GameObject obj = p_variation.Objs[i];
					p_variation.ObjIsActivateStates[i] = EditorGUILayout.ToggleLeft(obj.name, p_variation.ObjIsActivateStates[i], EditorStyles.boldLabel);
				}
			}
			else
			{
				EditorGUILayout.HelpBox("This object cannot have activate/deactivate sub object variations, because it has no sub objects!", MessageType.Error);
			}
		}
	}
}
#endif
