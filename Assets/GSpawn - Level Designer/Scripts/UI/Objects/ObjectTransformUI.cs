#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GSPAWN
{
    // Note: Works with the current selection only.
    public class ObjectTransformUI : ScriptableObject
    {
        private IEnumerable<GameObject>         _targetObjects;
        private List<Transform>                 _targetTransforms   = new List<Transform>();
        private TransformDiffCheck.DiffInfo     _transformDiffInfo;
        private ObjectLayerDiff                 _objectLayerDiff;
        private bool[]                          _positionDiff       = new bool[3];
        private bool[]                          _rotationDiff       = new bool[3];
        private bool[]                          _scaleDiff          = new bool[3];
        private IMGUIContainer                  _settingsContainer;
        private VisualElement                   _positionContainer;
        private VisualElement                   _rotationContainer;
        private VisualElement                   _scaleContainer;
        private VisualElement                   _snapButtonsContainer;
        private VisualElement                   _alignButtonsContainer;
        private VisualElement                   _randomizationContainer;
        private VisualElement                   _noObjectsAvailable;
        private Button                          _snapAllAxesBtn;

        public Func<bool>                       visibilityCondition { get; set; }

        public void onTargetObjectsChanged()
        {
            TransformEx.getTransforms(_targetObjects, _targetTransforms);
            _transformDiffInfo  = TransformDiffCheck.checkLocalDiff(_targetTransforms);
            _objectLayerDiff    = ObjectLayerDiffCheck.checkDiff(_targetTransforms);
        }

        public void removeObjects(List<GameObject> gameObjects)
        {
            _targetTransforms.RemoveAll(item => gameObjects.Contains(item.gameObject));
            _transformDiffInfo  = TransformDiffCheck.checkLocalDiff(_targetTransforms);
            _objectLayerDiff    = ObjectLayerDiffCheck.checkDiff(_targetTransforms);
        }

        public void removeNullObjects()
        {
            _targetTransforms.RemoveAll(item => item == null);
            _transformDiffInfo  = TransformDiffCheck.checkLocalDiff(_targetTransforms);
            _objectLayerDiff    = ObjectLayerDiffCheck.checkDiff(_targetTransforms);
        }

        public void refresh()
        {
            onTargetObjectsChanged();
            refreshTooltips();
        }

        public void refreshTooltips()
        {
            if (_snapAllAxesBtn != null)
                _snapAllAxesBtn.tooltip = ShortcutProfileDb.instance.activeProfile.getShortcutUITooltip(ObjectSelectionShortcutNames.snapAllAxes, "Snap objects to the closest grid point along all 3 axes.");
        }

        public void build(IEnumerable<GameObject> targetObjects, VisualElement parent)
        {
            _targetObjects = targetObjects;
            onTargetObjectsChanged();

            createNoObjectsAvailableLabel(parent);

            var dummyContainer = new IMGUIContainer();
            dummyContainer.style.height = 0.0f;
            parent.Add(dummyContainer);
            dummyContainer.onGUIHandler += () =>
            {
                if (visibilityCondition != null && !visibilityCondition()) return;
                if (_targetTransforms.Count == 0)
                {
                    _settingsContainer.setDisplayVisible(false);
                    _positionContainer.setDisplayVisible(false);
                    _rotationContainer.setDisplayVisible(false);
                    _scaleContainer.setDisplayVisible(false);
                    _snapButtonsContainer.setDisplayVisible(false);
                    _alignButtonsContainer.setDisplayVisible(false);
                    _randomizationContainer.setDisplayVisible(false);
                    _noObjectsAvailable.setDisplayVisible(true);
                }
                else
                {
                    _settingsContainer.setDisplayVisible(true);
                    _positionContainer.setDisplayVisible(true);
                    _rotationContainer.setDisplayVisible(true);
                    _scaleContainer.setDisplayVisible(true);
                    _snapButtonsContainer.setDisplayVisible(true);
                    _alignButtonsContainer.setDisplayVisible(true);
                    _randomizationContainer.setDisplayVisible(true);
                    _noObjectsAvailable.setDisplayVisible(false);
                }
            };

            _settingsContainer                  = UI.createIMGUIContainer(parent);
            _settingsContainer.style.alignSelf  = Align.FlexStart;
            _settingsContainer.style.height     = 20.0f;
            _settingsContainer.style.width      = 200.0f;
            _settingsContainer.style.marginTop  = 0.0f;
            _settingsContainer.style.marginLeft = UIValues.smallIconSize + 6.0f;
            _settingsContainer.onGUIHandler     += () =>
            {
                if (visibilityCondition != null && !visibilityCondition()) return;
                if (_targetTransforms.Count != 0)
                {
                    const float labelWidth = 40.0f;
                    EditorUIEx.saveLabelWidth();
                    EditorUIEx.saveShowMixedValue();
                    EditorGUIUtility.labelWidth = labelWidth;

                    // Layers
                    EditorGUI.showMixedValue = _objectLayerDiff.layer;
                    int layer = _objectLayerDiff.layer ? LayerEx.getMaxLayer() + 1 : _targetTransforms[0].gameObject.layer;

                    if (!_objectLayerDiff.layer)
                    {
                        const float top = 3.0f;
                        float left = -UIValues.smallIconSize - 2.0f;
                     
                        if (PluginObjectLayerDb.instance.isLayerTerrainMesh(layer))
                        {
                            Rect rect = new Rect(left, top - 1.0f, UIValues.smallIconSize, UIValues.smallIconSize);
                            GUI.DrawTexture(rect, TexturePool.instance.terrain);
                        }
                        else if (PluginObjectLayerDb.instance.isLayerSphericalMesh(layer))
                        {
                            Rect rect = new Rect(left, top - 1.0f, UIValues.smallIconSize, UIValues.smallIconSize);
                            GUI.DrawTexture(rect, TexturePool.instance.greenSphere);
                        }
                    }

                    EditorGUI.BeginChangeCheck();
                    int newLayer = EditorGUILayout.LayerField("Layer ", layer, GUILayout.Width(150.0f));
                    if (EditorGUI.EndChangeCheck())
                    {
                        foreach (var t in _targetTransforms)
                        {
                            UndoEx.record(t.gameObject);
                            t.gameObject.layer = newLayer;
                        }

                        // Note: Needs to be updated after layers are changed.
                        _objectLayerDiff = ObjectLayerDiffCheck.checkDiff(_targetTransforms);
                    }

                    EditorUIEx.restoreShowMixedValue();
                    EditorUIEx.restoreLabelWidth();
                }
            };

            const float floatFieldWidth = 80.0f;
            _positionContainer = createChannelRow(TransformChannel.Position,
                () => 
                {
                    // Note: When objects are deleted from the scene this needs to be here.
                    //       It seems that putting this in the 'dummyContainer.onGUIHandler'
                    //       doesn't work. 
                    _targetTransforms.RemoveAll(item => item == null);
               
                    if (_targetTransforms.Count == 0) return;                   
                    _transformDiffInfo.getPositionDiff(_positionDiff);
                    var result = EditorUIEx.vector3FieldEx(_targetTransforms[0].localPosition, _positionDiff, floatFieldWidth);
                    if (result.hasChanged)
                    {
                        UndoEx.recordTransforms(_targetTransforms);
                        foreach (var transform in _targetTransforms)
                        {
                            Vector3 newVal = transform.localPosition;
                            newVal[result.changedAxisIndex] = result.newAxisValue;
                            transform.localPosition = newVal;
                        }

                        ObjectEvents.onObjectsTransformedByUI();
                    }
                }, parent);
            _rotationContainer = createChannelRow(TransformChannel.Rotation,
                () =>
                {
                    if (_targetTransforms.Count == 0) return;
                    _transformDiffInfo.getRotationDiff(_rotationDiff);
                    var result = EditorUIEx.vector3FieldEx(_targetTransforms[0].localEulerAngles, _rotationDiff, floatFieldWidth);
                    if (result.hasChanged)
                    {
                        UndoEx.recordTransforms(_targetTransforms);
                        foreach (var transform in _targetTransforms)
                        {
                            Vector3 newVal = transform.localEulerAngles;
                            newVal[result.changedAxisIndex] = result.newAxisValue;
                            transform.localEulerAngles = newVal;
                        }

                        ObjectEvents.onObjectsTransformedByUI();
                    }
                }, parent);
            _scaleContainer = createChannelRow(TransformChannel.Scale,
                () =>
                {
                    if (_targetTransforms.Count == 0) return;
                    _transformDiffInfo.getScaleDiff(_scaleDiff);
                    var result = EditorUIEx.vector3FieldEx(_targetTransforms[0].localScale, _scaleDiff, floatFieldWidth);
                    if (result.hasChanged)
                    {
                        UndoEx.recordTransforms(_targetTransforms);
                        foreach (var transform in _targetTransforms)
                        {
                            Vector3 newVal = transform.localScale;
                            newVal[result.changedAxisIndex] = result.newAxisValue;
                            transform.localScale = newVal;
                        }

                        ObjectEvents.onObjectsTransformedByUI();
                    }
                }, parent);

            _snapButtonsContainer = createSnapButtons(parent);
            _alignButtonsContainer = createAlignButtons(parent);
            _randomizationContainer = createRandomizationControls(parent);

            refreshTooltips();
        }

        private void createNoObjectsAvailableLabel(VisualElement parent)
        {
            _noObjectsAvailable = new Label("No objects available.");
            _noObjectsAvailable.style.unityFontStyleAndWeight = FontStyle.Bold;
            _noObjectsAvailable.style.marginLeft = 5.0f;
            _noObjectsAvailable.style.color = UIValues.infoLabelColor;
            parent.Add(_noObjectsAvailable);
        }

        private Button createResetButton(string tooltip, VisualElement parent)
        {
            var button = UI.createButton(TexturePool.instance.refresh, UI.ButtonStyle.Push, parent);
            button.style.setBackgroundImage(TexturePool.instance.refresh, true);
            button.style.unityBackgroundImageTintColor = Color.white;
            button.tooltip = tooltip;
            parent.Add(button);

            return button;
        }

        private Label createTransformChannelLabel(string text, VisualElement parent)
        {
            var label               = new Label(text);
            label.style.marginTop   = 1.0f;
            label.style.width       = 50.0f;
            label.style.flexGrow    = 1.0f;
            parent.Add(label);

            return label;
        }

        private VisualElement createChannelRow(TransformChannel channel, Action fieldIMGUIHandler, VisualElement parent)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            parent.Add(row);

            string channelName = "position";
            if (channel == TransformChannel.Rotation) channelName = "rotation";
            else if (channel == TransformChannel.Scale) channelName = "scale";

            Button resetBtn = createResetButton("Reset " + channelName + (channel != TransformChannel.Scale ? " to 0." : " to 1."), row);
            if (channel == TransformChannel.Position)
            {
                resetBtn.clicked += () =>
                {
                    UndoEx.recordTransforms(_targetTransforms);
                    foreach (var transform in _targetTransforms)
                        transform.localPosition = Vector3.zero;

                    ObjectEvents.onObjectsTransformedByUI();
                };
            }
            else
            if (channel == TransformChannel.Rotation)
            {
                resetBtn.clicked += () =>
                {
                    UndoEx.recordTransforms(_targetTransforms);
                    foreach (var transform in _targetTransforms)
                        transform.localRotation = Quaternion.identity;

                    ObjectEvents.onObjectsTransformedByUI();
                };
            }
            else 
            if (channel == TransformChannel.Scale)
            {
                resetBtn.clicked += () =>
                {
                    UndoEx.recordTransforms(_targetTransforms);
                    foreach (var transform in _targetTransforms)
                        transform.localScale = Vector3.one;

                    ObjectEvents.onObjectsTransformedByUI();
                };
            }

            createTransformChannelLabel(channelName.replaceAt(0, Char.ToUpper(channelName[0])), row);

            var container = new IMGUIContainer();
            row.Add(container);
            container.style.overflow = Overflow.Hidden;
            container.onGUIHandler += fieldIMGUIHandler;

            return row;
        }

        private VisualElement createSnapButtons(VisualElement parent)
        {
            var snapRow                 = new VisualElement();
            parent.Add(snapRow);
            snapRow.style.flexDirection = FlexDirection.Row;

            const float buttonWidth = 90.0f;

            var snapBtn         = new Button();
            snapRow.Add(snapBtn);
            snapBtn.text        = "Snap X";
            snapBtn.tooltip     = "Snap objects to the closest grid point along the X axis.";
            snapBtn.style.width = buttonWidth * 0.6f;
            snapBtn.clicked     += () =>
            {
                UndoEx.recordTransforms(_targetTransforms);
                PluginScene.instance.grid.snapTransformsAxis(_targetTransforms, 0);
                ObjectEvents.onObjectsTransformedByUI();
            };

            snapBtn             = new Button();
            snapRow.Add(snapBtn);
            snapBtn.text        = "Snap Y";
            snapBtn.tooltip     = "Snap objects to the closest grid point along the Y axis.";
            snapBtn.style.width = buttonWidth;
            snapBtn.style.width = buttonWidth * 0.6f;
            snapBtn.style.marginLeft = UIValues.actionButtonLeftMargin;
            snapBtn.clicked     += () =>
            {
                UndoEx.recordTransforms(_targetTransforms);
                PluginScene.instance.grid.snapTransformsAxis(_targetTransforms, 1);
                ObjectEvents.onObjectsTransformedByUI();
            };

            snapBtn             = new Button();
            snapRow.Add(snapBtn);
            snapBtn.text        = "Snap Z";
            snapBtn.tooltip     = "Snap objects to the closest grid point along the Z axis.";
            snapBtn.style.width = buttonWidth;
            snapBtn.style.width = buttonWidth * 0.6f;
            snapBtn.style.marginLeft = UIValues.actionButtonLeftMargin;
            snapBtn.clicked     += () =>
            {
                UndoEx.recordTransforms(_targetTransforms);
                PluginScene.instance.grid.snapTransformsAxis(_targetTransforms, 2);
                ObjectEvents.onObjectsTransformedByUI();
            };

            _snapAllAxesBtn             = new Button();
            snapRow.Add(_snapAllAxesBtn);
            _snapAllAxesBtn.text        = "Snap all axes";
            _snapAllAxesBtn.style.width = buttonWidth;
            _snapAllAxesBtn.style.marginLeft = UIValues.actionButtonLeftMargin;
            _snapAllAxesBtn.clicked     += () =>
            {
                UndoEx.recordTransforms(_targetTransforms);
                PluginScene.instance.grid.snapTransformsAllAxes(_targetTransforms);
                ObjectEvents.onObjectsTransformedByUI();
            };

            return snapRow;
        }

        private VisualElement createAlignButtons(VisualElement parent)
        {
            var alignRow = new VisualElement();
            parent.Add(alignRow);
            alignRow.style.flexDirection = FlexDirection.Row;

            const float buttonWidth = 90.0f;

            var alignButton         = new Button();
            alignRow.Add(alignButton);
            alignButton.text        = "Align X";
            alignButton.tooltip     = "Align the object positions along the X axis.";
            alignButton.style.width = buttonWidth * 0.6f;           
            alignButton.clicked     += () =>
            {
                ObjectAlignment.alignObjects(_targetObjects, 0, true);
                ObjectEvents.onObjectsTransformedByUI();
            };

            alignButton             = new Button();
            alignRow.Add(alignButton);
            alignButton.text        = "Align Y";
            alignButton.tooltip     = "Align the object positions along the Y axis.";
            alignButton.style.width = buttonWidth * 0.6f;
            alignButton.style.marginLeft = UIValues.actionButtonLeftMargin;
            alignButton.clicked     += () =>
            {
                ObjectAlignment.alignObjects(_targetObjects, 1, true);
                ObjectEvents.onObjectsTransformedByUI();
            };

            alignButton             = new Button();
            alignRow.Add(alignButton);
            alignButton.text        = "Align Z";
            alignButton.tooltip     = "Align the object positions along the Z axis.";
            alignButton.style.width = buttonWidth * 0.6f;
            alignButton.style.marginLeft = UIValues.actionButtonLeftMargin;
            alignButton.clicked     += () =>
            {
                ObjectAlignment.alignObjects(_targetObjects, 2, true);
                ObjectEvents.onObjectsTransformedByUI();
            };

            return alignRow;
        }

        private VisualElement createRandomizationControls(VisualElement parent)
        {
            VisualElement rndParent = new VisualElement();
            parent.Add(rndParent);

            UI.createRowSeparator(rndParent);

            var randomizeLabel = UI.createSectionLabel("Randomization", rndParent);
            randomizeLabel.style.marginLeft = 3.0f;
            randomizeLabel.style.marginTop = 3.0f;

            // Offset
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            rndParent.Add(row);

            var offsetLabel = new Label("Offset");
            offsetLabel.style.marginLeft = 3.0f;
            offsetLabel.style.marginTop = 3.0f;
            offsetLabel.style.width = 60.0f;
            row.Add(offsetLabel);

            const float buttonWidth = 35.0f;
            var btn = new Button();
            btn.style.width = buttonWidth;
            btn.text = "X";
            btn.tooltip = "Apply random offset along the X axis.";
            btn.style.color = DefaultSystemValues.xAxisColor;
            btn.clicked += () => 
            { ObjectSelection.instance.applyRandomOffset(0); };
            row.Add(btn);

            btn = new Button();
            btn.style.width = buttonWidth;
            btn.style.marginLeft = -3.0f;
            btn.text = "Y";
            btn.tooltip = "Apply random offset along the Y axis.";
            btn.style.color = DefaultSystemValues.yAxisColor;
            btn.clicked += () => 
            { ObjectSelection.instance.applyRandomOffset(1); };
            row.Add(btn);

            btn = new Button();
            btn.style.width = buttonWidth;
            btn.style.marginLeft = -3.0f;
            btn.text = "Z";
            btn.tooltip = "Apply random offset along the Z axis.";
            btn.style.color = DefaultSystemValues.zAxisColor;
            btn.clicked += () =>
            { ObjectSelection.instance.applyRandomOffset(2); };
            row.Add(btn);

            btn = new Button();
            btn.style.width = buttonWidth + 16.0f;
            btn.style.marginLeft = -3.0f;
            btn.text = "Reset";
            btn.tooltip = "Reset positions to original values.";
            btn.clicked += () =>
            { ObjectSelection.instance.resetRandomizationPositions(); };
            row.Add(btn);

            UI.createColumnSeparator(row).style.flexGrow = 1.0f;
            var offsetOverlapToggle = UI.createIconButton(
                ObjectSelection.instance.offsetRand_AvoidOverlaps ? TexturePool.instance.overlap : TexturePool.instance.overlap_Off, 
                16.0f, row);
            offsetOverlapToggle.tooltip = "When enabled, no offset is applied where it would cause objects to overlap with each other.";
            offsetOverlapToggle.clicked += () => 
            {
                ObjectSelection.instance.offsetRand_AvoidOverlaps = !ObjectSelection.instance.offsetRand_AvoidOverlaps;
                offsetOverlapToggle.style.backgroundImage = ObjectSelection.instance.offsetRand_AvoidOverlaps ? TexturePool.instance.overlap : TexturePool.instance.overlap_Off;
            };

            row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginLeft = 63.0f;
            rndParent.Add(row);

            btn = new Button();
            btn.style.width = buttonWidth;
            btn.text = "XY";
            btn.tooltip = "Apply random offset in the XY plane.";
            btn.style.color = DefaultSystemValues.zAxisColor;
            btn.clicked += () => 
            { ObjectSelection.instance.applyRandomOffset(0, 1); };
            row.Add(btn);

            btn = new Button();
            btn.style.width = buttonWidth;
            btn.style.marginLeft = -3.0f;
            btn.text = "YZ";
            btn.tooltip = "Apply random offset in the YZ plane.";
            btn.style.color = DefaultSystemValues.xAxisColor;
            btn.clicked += () => 
            { ObjectSelection.instance.applyRandomOffset(1, 2); };
            row.Add(btn);

            btn = new Button();
            btn.style.width = buttonWidth;
            btn.style.marginLeft = -3.0f;
            btn.text = "ZX";
            btn.tooltip = "Apply random offset in the ZX plane.";
            btn.style.color = DefaultSystemValues.yAxisColor;
            btn.clicked += () => 
            { ObjectSelection.instance.applyRandomOffset(2, 0); };
            row.Add(btn);

            var minMaxParent = new VisualElement();
            rndParent.Add(minMaxParent);
            minMaxParent.style.marginLeft = 30.0f;

            var floatField = UI.createFloatField("_offsetRand_Min", ObjectSelection.instance.serializedObject, "Min", "Minimum applied offset.", 0.0f, minMaxParent);
            floatField.bindMinValueProperty("_offsetRand_Max", "_offsetRand_Min", ObjectSelection.instance.serializedObject);
            floatField.bindMaxValueProperty("_offsetRand_Min", "_offsetRand_Max", ObjectSelection.instance.serializedObject);
            floatField.labelElement.style.marginRight = -80.0f;

            floatField = UI.createFloatField("_offsetRand_Max", ObjectSelection.instance.serializedObject, "Max", "Maximum applied offset.", 0.0f, minMaxParent);
            floatField.bindMinValueProperty("_offsetRand_Max", "_offsetRand_Min", ObjectSelection.instance.serializedObject);
            floatField.bindMaxValueProperty("_offsetRand_Min", "_offsetRand_Max", ObjectSelection.instance.serializedObject);
            floatField.labelElement.style.marginRight = -80.0f;

            UI.createRowSeparator(rndParent);

            // Rotation
            row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            rndParent.Add(row);

            var rotationLabel = new Label("Rotation");
            rotationLabel.style.marginLeft  = 3.0f;
            rotationLabel.style.marginTop   = 3.0f;
            rotationLabel.style.width       = 60.0f;
            row.Add(rotationLabel);

            btn = new Button();
            btn.style.width = buttonWidth;
            btn.text = "X";
            btn.tooltip = "Randomize rotation around the X axis.";
            btn.style.color = DefaultSystemValues.xAxisColor;
            btn.clicked += () => 
            { ObjectSelection.instance.randomizeRotation(0); };
            row.Add(btn);

            btn = new Button();
            btn.style.width = buttonWidth;
            btn.style.marginLeft = -3.0f;
            btn.text = "Y";
            btn.tooltip = "Randomize rotation around the Y axis.";
            btn.style.color = DefaultSystemValues.yAxisColor;
            btn.clicked += () => 
            { ObjectSelection.instance.randomizeRotation(1); };
            row.Add(btn);

            btn = new Button();
            btn.style.width = buttonWidth;
            btn.style.marginLeft = -3.0f;
            btn.text = "Z";
            btn.tooltip = "Randomize rotation around the Z axis.";
            btn.style.color = DefaultSystemValues.zAxisColor;
            btn.clicked += () =>
            { ObjectSelection.instance.randomizeRotation(2); };
            row.Add(btn);

            btn = new Button();
            btn.style.width = buttonWidth + 16.0f;
            btn.style.marginLeft = -3.0f;
            btn.text = "Reset";
            btn.tooltip = "Reset rotations to original values.";
            btn.clicked += () =>
            { ObjectSelection.instance.resetRandomizationRotations(); };
            row.Add(btn);

            UI.createColumnSeparator(row).style.flexGrow = 1.0f;
            var rotationOverlapToggle = UI.createIconButton(
                ObjectSelection.instance.rotationRand_AvoidOverlaps ? TexturePool.instance.overlap : TexturePool.instance.overlap_Off, 
                16.0f, row);
            rotationOverlapToggle.tooltip = "When enabled, rotation will not be affected where it would cause objects to overlap with each other.";
            rotationOverlapToggle.clicked += () => 
            {
                ObjectSelection.instance.rotationRand_AvoidOverlaps = !ObjectSelection.instance.rotationRand_AvoidOverlaps;
                rotationOverlapToggle.style.backgroundImage = ObjectSelection.instance.rotationRand_AvoidOverlaps ? TexturePool.instance.overlap : TexturePool.instance.overlap_Off;
            };

            // Scale
            row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            rndParent.Add(row);

            var scaleLabel = new Label("Scale");
            scaleLabel.style.marginLeft  = 3.0f;
            scaleLabel.style.marginTop   = 3.0f;
            scaleLabel.style.width       = 60.0f;
            row.Add(scaleLabel);

            btn = new Button();
            btn.style.width = buttonWidth;
            btn.text = "All";
            btn.tooltip = "Randomize scale on all axes.";
            btn.clicked += () => 
            { ObjectSelection.instance.randomizeScale(); };
            row.Add(btn);

            btn = new Button();
            btn.style.width = buttonWidth + 16.0f;
            btn.style.marginLeft = -3.0f;
            btn.text = "Reset";
            btn.tooltip = "Reset scale to original values.";
            btn.clicked += () =>
            { ObjectSelection.instance.resetRandomizationScaleValues(); };
            row.Add(btn);

            UI.createColumnSeparator(row).style.flexGrow = 1.0f;
            var scaleOverlapToggle = UI.createIconButton(
                ObjectSelection.instance.scaleRand_AvoidOverlaps ? TexturePool.instance.overlap : TexturePool.instance.overlap_Off, 
                16.0f, row);
            scaleOverlapToggle.tooltip = "When enabled, scale will not be affected where it would cause objects to overlap with each other.";
            scaleOverlapToggle.clicked += () => 
            {
                ObjectSelection.instance.scaleRand_AvoidOverlaps = !ObjectSelection.instance.scaleRand_AvoidOverlaps;
                scaleOverlapToggle.style.backgroundImage = ObjectSelection.instance.scaleRand_AvoidOverlaps ? TexturePool.instance.overlap : TexturePool.instance.overlap_Off;
            };

            minMaxParent = new VisualElement();
            rndParent.Add(minMaxParent);
            minMaxParent.style.marginLeft = 30.0f;

            floatField = UI.createFloatField("_scaleRand_Min", ObjectSelection.instance.serializedObject, "Min", "Minimum scale.", 0.0f, minMaxParent);
            floatField.bindMinValueProperty("_scaleRand_Max", "_scaleRand_Min", ObjectSelection.instance.serializedObject);
            floatField.bindMaxValueProperty("_scaleRand_Min", "_scaleRand_Max", ObjectSelection.instance.serializedObject);
            floatField.labelElement.style.marginRight = -80.0f;

            floatField = UI.createFloatField("_scaleRand_Max", ObjectSelection.instance.serializedObject, "Max", "Maximum scale.", 0.0f, minMaxParent);
            floatField.bindMinValueProperty("_scaleRand_Max", "_scaleRand_Min", ObjectSelection.instance.serializedObject);
            floatField.bindMaxValueProperty("_scaleRand_Min", "_scaleRand_Max", ObjectSelection.instance.serializedObject);
            floatField.labelElement.style.marginRight = -80.0f;

            return rndParent;
        }
    }
}
#endif