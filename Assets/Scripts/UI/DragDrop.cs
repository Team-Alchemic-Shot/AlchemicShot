using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Handles drag and drop functionality for elements in the configuration UI.
/// </summary>
public class DragDrop
{
    private VisualElement _draggedElement;
    private VisualElement _dragPreview;
    private ElementSlotData _draggedData;
    private Vector2 _dragStartPosition;
    private Vector2 _dragOffset;
    private VisualElement _rootElement;
    private bool _isDragging;
    private int _activePointerId = -1;
    
    public Action<ElementSlotData, ElementSlotData> OnElementMoved;
    public Action<ElementSlotData> OnElementQuickMoved;
    
    public DragDrop(VisualElement root)
    {
        _rootElement = root;
        
        // Register global handlers on root
        _rootElement.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        _rootElement.RegisterCallback<PointerUpEvent>(OnPointerUp);
    }
    
    public void RegisterElementSlot(VisualElement element, ElementSlotData slotData)
    {
        // Store slot data
        element.userData = slotData;
        
        // Register new callbacks (no need to unregister, RegisterCallback handles duplicates)
        element.RegisterCallback<PointerDownEvent>(OnPointerDownCallback);
        element.RegisterCallback<PointerEnterEvent>(OnPointerEnterCallback);
        element.RegisterCallback<PointerLeaveEvent>(OnPointerLeaveCallback);
    }
    
    private void OnPointerDownCallback(PointerDownEvent evt)
    {
        var element = evt.currentTarget as VisualElement;
        var slotData = element?.userData as ElementSlotData;
        
        Debug.Log($"[DragDrop] PointerDown on element: {element?.name ?? "null"}, slotData: {slotData != null}, element: {slotData?.Element?.elementName ?? "null"}");
        
        // Only process if there's an element in this slot
        if (slotData == null || slotData.Element == null)
        {
            Debug.Log("[DragDrop] No element to drag");
            return;
        }
        
        // Check for shift+click quick-move
        if (evt.shiftKey)
        {
            Debug.Log($"[DragDrop] Shift+click detected - quick-moving {slotData.Element.elementName}");
            OnElementQuickMoved?.Invoke(slotData);
            evt.StopPropagation();
            return;
        }
        
        // Clean up any previous drag state
        if (_isDragging)
        {
            Debug.Log("[DragDrop] Cleaning up previous drag before starting new one");
            CleanupDrag();
        }
        
        Debug.Log($"[DragDrop] Starting drag of {slotData.Element.elementName} from Gun {slotData.GunIndex} MagazineSlot {slotData.MagazineIndex}");
        
        _isDragging = true;
        _draggedElement = element;
        _draggedData = slotData;
        _activePointerId = evt.pointerId;
        _dragStartPosition = (Vector2)evt.position;
        
        // Calculate offset from element's world position to mouse
        var elementBounds = element.worldBound;
        _dragOffset = (Vector2)evt.position - new Vector2(elementBounds.x, elementBounds.y);
        
        Debug.Log($"[DragDrop] Offset: {_dragOffset}, StartPos: {_dragStartPosition}, ElementBounds: {elementBounds}");
        
        // Create visual preview
        CreateDragPreview(element, slotData, (Vector2)evt.position);
        
        // DON'T capture the pointer - it prevents other elements from receiving events
        // element.CapturePointer(evt.pointerId);
        evt.StopPropagation();
    }
    
    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!_isDragging || _dragPreview == null || evt.pointerId != _activePointerId)
        {
            // Debug spam reduction - only log occasionally
            if (UnityEngine.Random.value < 0.01f && _isDragging)
            {
                Debug.Log($"[DragDrop] PointerMove ignored - isDragging:{_isDragging}, hasPreview:{_dragPreview != null}, pointerMatch:{evt.pointerId == _activePointerId}");
            }
            return;
        }
        
        // Move the preview to follow cursor (use absolute positioning)
        Vector2 position = (Vector2)evt.position - _dragOffset;
        _dragPreview.style.left = position.x;
        _dragPreview.style.top = position.y;
        
        // Log occasionally to avoid spam
        if (UnityEngine.Random.value < 0.05f)
        {
            Debug.Log($"[DragDrop] Moving preview to {position}");
        }
        
        evt.StopPropagation();
    }
    
    private void OnPointerUp(PointerUpEvent evt)
    {
        Debug.Log($"[DragDrop] PointerUp - isDragging:{_isDragging}, pointerId:{evt.pointerId}, active:{_activePointerId}");
        
        if (!_isDragging || evt.pointerId != _activePointerId)
        {
            Debug.Log("[DragDrop] PointerUp ignored - not dragging or wrong pointer");
            return;
        }
        
        Debug.Log($"[DragDrop] Pointer up at {evt.position}");
        
        // Find what we're dropping onto
        VisualElement dropTarget = FindDropTarget((Vector2)evt.position);
        
        if (dropTarget != null && dropTarget != _draggedElement)
        {
            // Get the drop target's data
            ElementSlotData targetData = GetSlotData(dropTarget);
            
            if (targetData != null)
            {
                Debug.Log($"[DragDrop] Dropping onto Gun {targetData.GunIndex} MagazineSlot {targetData.MagazineIndex}");
                // Notify that an element was moved
                OnElementMoved?.Invoke(_draggedData, targetData);
            }
            else
            {
                Debug.LogWarning("[DragDrop] Drop target has no slot data");
            }
        }
        else
        {
            Debug.Log($"[DragDrop] No valid drop target found (dropTarget: {dropTarget?.name ?? "null"})");
        }
        
        // Cleanup (this also releases pointer if captured)
        CleanupDrag();
        
        evt.StopPropagation();
    }
    
    private void OnPointerEnterCallback(PointerEnterEvent evt)
    {
        var element = evt.currentTarget as VisualElement;
        
        if (_isDragging && element != _draggedElement && element != null)
        {
            // Highlight valid drop targets
            element.AddToClassList("drop-target-highlight");
            element.style.borderTopColor = Color.yellow;
            element.style.borderBottomColor = Color.yellow;
            element.style.borderLeftColor = Color.yellow;
            element.style.borderRightColor = Color.yellow;
        }
    }
    
    private void OnPointerLeaveCallback(PointerLeaveEvent evt)
    {
        if (evt.currentTarget is VisualElement element)
        {
            element.RemoveFromClassList("drop-target-highlight");
            element.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f);
            element.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f);
            element.style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f);
            element.style.borderRightColor = new Color(0.3f, 0.3f, 0.3f);
        }
    }
    
    private void CreateDragPreview(VisualElement sourceElement, ElementSlotData slotData, Vector2 startPosition)
    {
        _dragPreview = new VisualElement();
        _dragPreview.style.position = Position.Absolute;
        _dragPreview.style.width = sourceElement.resolvedStyle.width;
        _dragPreview.style.height = sourceElement.resolvedStyle.height;
        _dragPreview.style.backgroundColor = new Color(0.2f, 0.5f, 0.8f, 0.7f);
        _dragPreview.style.borderTopWidth = 2;
        _dragPreview.style.borderBottomWidth = 2;
        _dragPreview.style.borderLeftWidth = 2;
        _dragPreview.style.borderRightWidth = 2;
        _dragPreview.style.borderTopColor = Color.white;
        _dragPreview.style.borderBottomColor = Color.white;
        _dragPreview.style.borderLeftColor = Color.white;
        _dragPreview.style.borderRightColor = Color.white;
        _dragPreview.pickingMode = PickingMode.Ignore; // Don't interfere with drop detection
        
        // Position it at the cursor
        Vector2 position = startPosition - _dragOffset;
        _dragPreview.style.left = position.x;
        _dragPreview.style.top = position.y;
        
        // Add attachment name label
        Label nameLabel = new Label(slotData.Element.elementName);
        nameLabel.style.color = Color.white;
        nameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        nameLabel.style.fontSize = 12;
        nameLabel.pickingMode = PickingMode.Ignore;
        _dragPreview.Add(nameLabel);
        
        _rootElement.Add(_dragPreview);
        
        Debug.Log($"[DragDrop] Created preview for {slotData.Element.elementName} at {position}");
    }
    
    private void CleanupDrag()
    {
        Debug.Log($"[DragDrop] Cleaning up drag - isDragging:{_isDragging}, hasPreview:{_dragPreview != null}, hasElement:{_draggedElement != null}");
        
        if (_dragPreview != null)
        {
            _dragPreview.RemoveFromHierarchy();
            _dragPreview = null;
        }
        
        // Release pointer if it was captured
        if (_draggedElement != null && _activePointerId >= 0)
        {
            _draggedElement.ReleasePointer(_activePointerId);
        }
        
        // Remove all highlights
        var allElements = _rootElement.Query<VisualElement>(className: "drop-target-highlight").ToList();
        Debug.Log($"[DragDrop] Removing highlights from {allElements.Count} elements");
        foreach (var element in allElements)
        {
            element.RemoveFromClassList("drop-target-highlight");
            element.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f);
            element.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f);
            element.style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f);
            element.style.borderRightColor = new Color(0.3f, 0.3f, 0.3f);
        }
        
        _draggedElement = null;
        _draggedData = null;
        _isDragging = false;
        _activePointerId = -1;
        
        Debug.Log("[DragDrop] Cleanup complete");
    }
    
    private VisualElement FindDropTarget(Vector2 position)
    {
        // Use panel's Pick to find element at position
        var pickedElement = _rootElement.panel.Pick(position);
        
        // Walk up the hierarchy to find an attachment slot
        while (pickedElement != null)
        {
            if (pickedElement.ClassListContains("magazine-slot") || 
                pickedElement.ClassListContains("inventory-slot"))
            {
                return pickedElement;
            }
            pickedElement = pickedElement.parent;
        }
        
        return null;
    }
    
    private ElementSlotData GetSlotData(VisualElement element)
    {
        // This will be set when registering the slot
        return element.userData as ElementSlotData;
    }
}

/// <summary>
/// Data structure to track what's in a slot and where it is
/// </summary>
public class ElementSlotData
{
    public Element Element;
    public SlotType Type;
    public int GunIndex;           // Which gun slot (0-4)
    public int MagazineIndex;      // For magazine slots: index in magazine blueprint (-1 = none)
    public int InventoryIndex;     // For inventory slots: which inventory slot
    public int ElementCount;       // For inventory slots: how many elements in the stack
    
    public ElementSlotData(SlotType type, int gunIndex = -1, int magazineIndex = -1, int inventoryIndex = -1, int elementCount = 0)
    {
        Type = type;
        GunIndex = gunIndex;
        MagazineIndex = magazineIndex;
        InventoryIndex = inventoryIndex;
        ElementCount = elementCount;
    }
}

public enum SlotType
{
    MagazineSlot,   // Element slot in a gun magazine
    InventorySlot   // Element in inventory storage
}