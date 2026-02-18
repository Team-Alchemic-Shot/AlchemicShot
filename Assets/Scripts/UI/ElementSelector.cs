using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
public class ElementSelector : MonoBehaviour
{
    public UIDocument Document;
    public Inventory InventoryRef; // Reference to gun inventory
    public ElementInventory ElementInventoryRef; // Reference to element inventory for drag-drop operations
    
    private VisualElement _root;
    private VisualElement _inventoryContainer; // Separate container for inventory (can be hidden)
    private VisualElement _gunsContainer;
    private Button _closeButton;

    private const int MaxGunSlots = 9; // TODO

    private readonly List<GunSlotUI> _gunSlots = new();
    private readonly List<InventorySlotUI> _inventorySlots = new();
    private bool _isVisible;
    private DragDrop _dragDropHandler;
    private GunSelectionPanel _gunSelectionPanel; // Top-screen gun selection (always visible)
    
    private class GunSlotUI
    {
        public VisualElement Container;
        public Image Icon;
        public Label Name;
        public List<MagazineSlotUI> MagazineSlots = new();
        public int GunIndex;
        public Gun Gun; // Store reference for updates
    }
    
    private class MagazineSlotUI
    {
        public VisualElement Container;
        public Image Icon;
        public Label Name;
        public ElementSlotData SlotData;
    }
    
    private class InventorySlotUI
    {
        public VisualElement Container;
        public Image Icon;
        public Label Name;
        public ElementSlotData SlotData;
    }

    void Awake()
    {
        if (Document == null)
        {
            Debug.LogError("InventoryUI: UIDocument is not assigned!");
            enabled = false;
            return;
        }

        _root = Document.rootVisualElement;
        
        if (_root == null)
        {
            Debug.LogError("InventoryUI: Root visual element is null!");
            enabled = false;
            return;
        }

        // Clear any existing UI elements from the template
        _root.Clear();

        CreateInventoryUI();
        
        // Initialize gun selection panel (always visible at top)
        _gunSelectionPanel = new GunSelectionPanel();
        _root.Add(_gunSelectionPanel.Panel);
        
        // Set up inventory reference for gun selection
        if (InventoryRef != null)
        {
            _gunSelectionPanel.SetInventory(InventoryRef);
        }
        
        // Hide inventory panel initially (but keep gun selection visible)
        _inventoryContainer.style.display = DisplayStyle.None;
        _isVisible = false;
        
        Time.timeScale = 1f;
        // Keep cursor visible for mouse aiming in top-down game
        UnityEngine.Cursor.lockState = CursorLockMode.Confined; // Confined keeps cursor in window but visible
        UnityEngine.Cursor.visible = true;
        
        // Initialize drag-drop system
        _dragDropHandler = new DragDrop(_root);
        _dragDropHandler.OnElementMoved += HandleElementMoved;
        _dragDropHandler.OnElementQuickMoved += HandleElementQuickMoved;
    }

    void OnEnable()
    {
        RegisterInventoryEvents();
    }
    
    void Start()
    {
        // Ensure inventory is connected to gun selection panel
        if (InventoryRef != null && _gunSelectionPanel != null)
        {
            _gunSelectionPanel.SetInventory(InventoryRef);
            _gunSelectionPanel.RefreshGuns();
        }

        RefreshAllGunSlots();
        SyncGunSelectionFromInventory();
        
        // Ensure inventory is hidden on start
        if (_inventoryContainer != null)
        {
            _inventoryContainer.style.display = DisplayStyle.None;
            _isVisible = false;
        }
    }
    
    void Update()
    {
        // Handle number key inputs for gun selection (1-5)
        if (_gunSelectionPanel != null)
        {
            for (int i = 0; i < 5; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    _gunSelectionPanel.SelectGun(i);
                }
            }
            
            // Update ammo and reload progress for all gun slots
            _gunSelectionPanel.UpdateGunSlots();
            _gunSelectionPanel.Tick(Time.unscaledDeltaTime);
        }
        
        // Note: I key toggle is handled by PlayerInventoryController, not here
    }
    
    void OnDestroy()
    {
        UnregisterInventoryEvents();
        if (_dragDropHandler != null)
        {
            _dragDropHandler.OnElementMoved -= HandleElementMoved;
            _dragDropHandler.OnElementQuickMoved -= HandleElementQuickMoved;
        }
    }

    void OnDisable()
    {
        UnregisterInventoryEvents();
    }

    private void RegisterInventoryEvents()
    {
        if (InventoryRef == null)
        {
            return;
        }

        InventoryRef.OnGunChanged += HandleGunChanged;
        InventoryRef.OnGunListChanged += HandleGunListChanged;
    }

    private void UnregisterInventoryEvents()
    {
        if (InventoryRef == null)
        {
            return;
        }

        InventoryRef.OnGunChanged -= HandleGunChanged;
        InventoryRef.OnGunListChanged -= HandleGunListChanged;
    }

    private void HandleGunChanged(Gun gun)
    {
        RefreshAllGunSlots();
        _gunSelectionPanel?.RefreshGuns();
        SyncGunSelectionFromInventory();
        _gunSelectionPanel?.NotifyGunSwitched();
    }

    private void HandleGunListChanged()
    {
        RefreshAllGunSlots();
        _gunSelectionPanel?.RefreshGuns();
        SyncGunSelectionFromInventory();
        _gunSelectionPanel?.NotifyGunSwitched();
    }

    void HandleElementMoved(ElementSlotData source, ElementSlotData target)
    {
        if (source == null || target == null)
        {
            return;
        }

        if (InventoryRef == null && (source.Type == SlotType.MagazineSlot || target.Type == SlotType.MagazineSlot))
        {
            Debug.LogWarning("Cannot move element - Inventory reference not set!");
            return;
        }
        if (ElementInventoryRef == null && (source.Type == SlotType.InventorySlot || target.Type == SlotType.InventorySlot))
        {
            Debug.LogWarning("Cannot move element - Element inventory reference not set!");
            return;
        }

        if (source.Type == SlotType.MagazineSlot && target.Type == SlotType.MagazineSlot)
        {
            if (TrySwapMagazineSlots(source, target))
            {
                RefreshGunSlots(source.GunIndex, target.GunIndex);
            }
        }
        else if (source.Type == SlotType.InventorySlot && target.Type == SlotType.MagazineSlot)
        {
            if (TryMoveInventoryToMagazine(source, target))
            {
                RefreshInventorySlots(source.InventoryIndex, target.InventoryIndex);
                RefreshGunSlots(target.GunIndex);
            }
        }
        else if (source.Type == SlotType.MagazineSlot && target.Type == SlotType.InventorySlot)
        {
            if (TryMoveMagazineToInventory(source, target))
            {
                RefreshGunSlots(source.GunIndex);
                RefreshInventorySlots(target.InventoryIndex);
            }
        }
        else if (source.Type == SlotType.InventorySlot && target.Type == SlotType.InventorySlot)
        {
            if (TrySwapInventorySlots(source, target))
            {
                RefreshInventorySlots(source.InventoryIndex, target.InventoryIndex);
            }
        }
    }
    
    void HandleElementQuickMoved(ElementSlotData source)
    {
        if (source == null)
        {
            return;
        }

        if (InventoryRef == null && source.Type == SlotType.MagazineSlot)
        {
            Debug.LogWarning("Cannot quick-move element - Inventory reference not set!");
            return;
        }

        if (ElementInventoryRef == null && source.Type == SlotType.InventorySlot)
        {
            Debug.LogWarning("Cannot quick-move element - Element inventory reference not set!");
            return;
        }

        if (source.Type == SlotType.MagazineSlot)
        {
            int targetIndex = FindFirstInventorySlotForElement(GetMagazineElement(source.GunIndex, source.MagazineIndex));
            if (targetIndex < 0)
            {
                Debug.LogWarning("[InventoryUI] No available inventory slot for quick-move.");
                return;
            }

            var targetData = new ElementSlotData(SlotType.InventorySlot, inventoryIndex: targetIndex);
            if (TryMoveMagazineToInventory(source, targetData))
            {
                RefreshGunSlots(source.GunIndex);
                RefreshInventorySlots(targetIndex);
            }
        }
        else if (source.Type == SlotType.InventorySlot)
        {
            int selectedGunIndex = GetSelectedGunIndex();
            if (selectedGunIndex < 0)
            {
                Debug.LogWarning("[InventoryUI] No gun selected for quick-move!");
                return;
            }

            int emptyMagazineSlot = FindFirstEmptyMagazineSlot(selectedGunIndex);
            if (emptyMagazineSlot < 0)
            {
                Debug.LogWarning("[InventoryUI] No empty magazine slot available.");
                return;
            }

            var targetData = new ElementSlotData(SlotType.MagazineSlot, gunIndex: selectedGunIndex, magazineIndex: emptyMagazineSlot);
            if (TryMoveInventoryToMagazine(source, targetData))
            {
                RefreshInventorySlots(source.InventoryIndex);
                RefreshGunSlots(selectedGunIndex);
            }
        }
    }

    void UpdateInventorySlot(int index)
    {
        if (index < 0 || index >= _inventorySlots.Count || ElementInventoryRef == null)
            return;
        
        var inventorySlot = _inventorySlots[index];
        var stack = ElementInventoryRef.GetSlot(index);
        
        if (stack != null && !stack.IsEmpty)
        {
            inventorySlot.Icon.sprite = stack.Element.elementIcon;
            inventorySlot.Name.text = stack.Count > 1
                ? $"{stack.Element.elementName} x{stack.Count}"
                : stack.Element.elementName;
            inventorySlot.Name.style.color = new Color(0.9f, 0.9f, 0.9f);
            inventorySlot.SlotData.Element = stack.Element;
            inventorySlot.SlotData.ElementCount = stack.Count;
        }
        else
        {
            inventorySlot.Icon.sprite = null;
            inventorySlot.Name.text = "Empty";
            inventorySlot.Name.style.color = new Color(0.5f, 0.5f, 0.5f);
            inventorySlot.SlotData.Element = null;
            inventorySlot.SlotData.ElementCount = 0;
        }
        
        inventorySlot.Container.userData = inventorySlot.SlotData;
        
        if (_dragDropHandler != null)
        {
            _dragDropHandler.RegisterElementSlot(inventorySlot.Container, inventorySlot.SlotData);
        }
    }

    private Gun GetGunAtIndex(int index)
    {
        if (InventoryRef == null || index < 0 || index >= InventoryRef.guns.Count)
        {
            return null;
        }

        return InventoryRef.guns[index];
    }

    private ElementStack GetInventoryStack(int index)
    {
        if (ElementInventoryRef == null)
        {
            return null;
        }

        return ElementInventoryRef.GetSlot(index);
    }

    private Element GetMagazineElement(int gunIndex, int magazineIndex)
    {
        var gun = GetGunAtIndex(gunIndex);
        if (gun == null || gun.magazineBlueprint == null || gun.magazineBlueprint.bullets == null)
        {
            return null;
        }

        if (magazineIndex < 0 || magazineIndex >= gun.magazineBlueprint.bullets.Length)
        {
            return null;
        }

        return gun.magazineBlueprint.bullets[magazineIndex]?.element;
    }

    private bool TrySetMagazineElement(int gunIndex, int magazineIndex, Element element)
    {
        var gun = GetGunAtIndex(gunIndex);
        if (gun == null || gun.magazineBlueprint == null || gun.magazineBlueprint.bullets == null)
        {
            return false;
        }

        var bullets = gun.magazineBlueprint.bullets;
        if (magazineIndex < 0 || magazineIndex >= bullets.Length)
        {
            return false;
        }

        if (element == null)
        {
            bullets[magazineIndex] = null;
        }
        else
        {
            bullets[magazineIndex] ??= new BulletData();
            bullets[magazineIndex].element = element;
            bullets[magazineIndex].isEmpty = false;
        }

        gun.UpdateMagazineFromBlueprint();
        return true;
    }

    private bool TrySwapMagazineSlots(ElementSlotData source, ElementSlotData target)
    {
        if (source.GunIndex == target.GunIndex && source.MagazineIndex == target.MagazineIndex)
        {
            return false;
        }

        var sourceElement = GetMagazineElement(source.GunIndex, source.MagazineIndex);
        var targetElement = GetMagazineElement(target.GunIndex, target.MagazineIndex);

        if (!TrySetMagazineElement(source.GunIndex, source.MagazineIndex, targetElement))
        {
            return false;
        }

        if (!TrySetMagazineElement(target.GunIndex, target.MagazineIndex, sourceElement))
        {
            return false;
        }

        return true;
    }

    private bool TryMoveInventoryToMagazine(ElementSlotData source, ElementSlotData target)
    {
        var sourceStack = GetInventoryStack(source.InventoryIndex);
        if (sourceStack == null || sourceStack.IsEmpty)
        {
            return false;
        }

        var sourceElement = sourceStack.Element;
        var targetElement = GetMagazineElement(target.GunIndex, target.MagazineIndex);

        if (targetElement != null && targetElement != sourceElement && sourceStack.Count > 1)
        {
            return false;
        }

        if (!TrySetMagazineElement(target.GunIndex, target.MagazineIndex, sourceElement))
        {
            return false;
        }

        if (targetElement != null && targetElement != sourceElement)
        {
            sourceStack.Set(targetElement, 1);
        }
        else
        {
            sourceStack.Remove(1);
        }

        return true;
    }

    private bool TryMoveMagazineToInventory(ElementSlotData source, ElementSlotData target)
    {
        var sourceElement = GetMagazineElement(source.GunIndex, source.MagazineIndex);
        if (sourceElement == null)
        {
            return false;
        }

        var targetStack = GetInventoryStack(target.InventoryIndex);
        if (targetStack == null)
        {
            return false;
        }

        if (targetStack.IsEmpty)
        {
            if (!TrySetMagazineElement(source.GunIndex, source.MagazineIndex, null))
            {
                return false;
            }
            targetStack.Set(sourceElement, 1);
            return true;
        }

        if (targetStack.Element == sourceElement)
        {
            if (!TrySetMagazineElement(source.GunIndex, source.MagazineIndex, null))
            {
                return false;
            }
            targetStack.Add(1);
            return true;
        }

        if (targetStack.Count == 1)
        {
            var swapElement = targetStack.Element;
            if (!TrySetMagazineElement(source.GunIndex, source.MagazineIndex, swapElement))
            {
                return false;
            }
            targetStack.Set(sourceElement, 1);
            return true;
        }

        return false;
    }

    private bool TrySwapInventorySlots(ElementSlotData source, ElementSlotData target)
    {
        var sourceStack = GetInventoryStack(source.InventoryIndex);
        var targetStack = GetInventoryStack(target.InventoryIndex);
        if (sourceStack == null || targetStack == null)
        {
            return false;
        }

        var sourceElement = sourceStack.Element;
        var sourceCount = sourceStack.Count;
        var targetElement = targetStack.Element;
        var targetCount = targetStack.Count;

        if (sourceStack.IsEmpty && targetStack.IsEmpty)
        {
            return false;
        }

        if (sourceElement == null)
        {
            targetStack.Clear();
        }
        else
        {
            targetStack.Set(sourceElement, sourceCount);
        }

        if (targetElement == null)
        {
            sourceStack.Clear();
        }
        else
        {
            sourceStack.Set(targetElement, targetCount);
        }

        return true;
    }

    private void RefreshGunSlots(params int[] indices)
    {
        if (indices == null || indices.Length == 0)
        {
            return;
        }

        var seen = new HashSet<int>();
        foreach (var index in indices)
        {
            if (index < 0 || !seen.Add(index))
            {
                continue;
            }

            UpdateGunSlot(index, GetGunAtIndex(index));
        }
    }

    private void RefreshInventorySlots(params int[] indices)
    {
        if (indices == null || indices.Length == 0)
        {
            return;
        }

        var seen = new HashSet<int>();
        foreach (var index in indices)
        {
            if (index < 0 || !seen.Add(index))
            {
                continue;
            }

            UpdateInventorySlot(index);
        }
    }

    private int FindFirstInventorySlotForElement(Element element)
    {
        if (ElementInventoryRef == null || _inventorySlots.Count == 0 || element == null)
        {
            return -1;
        }

        int emptyIndex = -1;
        for (int i = 0; i < _inventorySlots.Count; i++)
        {
            var stack = ElementInventoryRef.GetSlot(i);
            if (stack == null || stack.IsEmpty)
            {
                if (emptyIndex < 0)
                {
                    emptyIndex = i;
                }
                continue;
            }

            if (stack.Element == element)
            {
                return i;
            }
        }

        return emptyIndex;
    }

    private int FindFirstEmptyMagazineSlot(int gunIndex)
    {
        var gun = GetGunAtIndex(gunIndex);
        if (gun == null || gun.magazineBlueprint == null || gun.magazineBlueprint.bullets == null)
        {
            return -1;
        }

        var bullets = gun.magazineBlueprint.bullets;
        for (int i = 0; i < bullets.Length; i++)
        {
            if (bullets[i] == null || bullets[i].element == null)
            {
                return i;
            }
        }

        return -1;
    }
    
    void CreateInventoryUI()
    {
        // Create inventory container (this is what we hide/show with I key)
        _inventoryContainer = new VisualElement();
        _inventoryContainer.style.position = Position.Absolute;
        _inventoryContainer.style.width = new Length(100, LengthUnit.Percent);
        _inventoryContainer.style.height = new Length(100, LengthUnit.Percent);
        _inventoryContainer.style.top = 0;
        _inventoryContainer.style.left = 0;
        
        var mainContainer = new VisualElement();
        mainContainer.style.flexGrow = 1;
        mainContainer.style.backgroundColor = new Color(0, 0, 0, 0.9f);
        mainContainer.style.paddingTop = 20;
        mainContainer.style.paddingBottom = 20;
        mainContainer.style.paddingLeft = 20;
        mainContainer.style.paddingRight = 20;
        
        var header = new Label("INVENTORY");
        header.style.fontSize = 32;
        header.style.unityFontStyleAndWeight = FontStyle.Bold;
        header.style.color = Color.white;
        header.style.marginBottom = 15;
        header.style.unityTextAlign = TextAnchor.UpperCenter;
        mainContainer.Add(header);
        
        // Content container (horizontal layout) with max height
        var contentContainer = new VisualElement();
        contentContainer.style.flexDirection = FlexDirection.Row;
        contentContainer.style.flexGrow = 1;
        contentContainer.style.maxHeight = new StyleLength(new Length(70, LengthUnit.Percent));
        
        // Left side: Guns with magazine slots (vertical) with scroll
        var leftPanel = new VisualElement();
        leftPanel.style.flexGrow = 1;
        leftPanel.style.marginRight = 20;
        
        var gunsLabel = new Label("GUNS & MAGAZINES");
        gunsLabel.style.fontSize = 20;
        gunsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        gunsLabel.style.color = new Color(1f, 0.78f, 0.39f);
        gunsLabel.style.marginBottom = 10;
        leftPanel.Add(gunsLabel);
        
        // Scrollable container for guns
        var gunsScrollView = new ScrollView(ScrollViewMode.Vertical);
        gunsScrollView.style.flexGrow = 1;
        
        _gunsContainer = new VisualElement();
        gunsScrollView.Add(_gunsContainer);
        leftPanel.Add(gunsScrollView);
        
        // Create gun slots
        for (int i = 0; i < MaxGunSlots; i++)
        {
            CreateGunSlot(i);
        }
        
        // Right side: Items (grid) with scroll
        var rightPanel = new VisualElement();
        rightPanel.style.width = 250;
        rightPanel.style.minWidth = 250;
        
        var itemsLabel = new Label("ELEMENTS");
        itemsLabel.style.fontSize = 20;
        itemsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        itemsLabel.style.color = new Color(0.39f, 0.78f, 1f);
        itemsLabel.style.marginBottom = 10;
        rightPanel.Add(itemsLabel);
        
        var itemsScrollView = new ScrollView(ScrollViewMode.Vertical);
        itemsScrollView.style.flexGrow = 1;
        
        var _itemGrid = new VisualElement();
        _itemGrid.name = "ItemGrid";
        _itemGrid.style.flexDirection = FlexDirection.Row;
        _itemGrid.style.flexWrap = Wrap.Wrap;
        _itemGrid.style.justifyContent = Justify.FlexStart;
        _itemGrid.style.alignContent = Align.FlexStart;
        
        int inventorySlotCount = ElementInventoryRef != null ? ElementInventoryRef.SlotCount : 0;
        if (ElementInventoryRef == null)
        {
            Debug.LogWarning("ElementInventory reference is not set. Inventory UI will be empty.");
        }

        for (int i = 0; i < inventorySlotCount; i++)
        {
            var inventorySlot = CreateInventorySlot(i);
            _itemGrid.Add(inventorySlot.Container);
            _inventorySlots.Add(inventorySlot);
        }
        
        itemsScrollView.Add(_itemGrid);
        rightPanel.Add(itemsScrollView);
        
        contentContainer.Add(leftPanel);
        contentContainer.Add(rightPanel);
        mainContainer.Add(contentContainer);
        
        // Close button
        _closeButton = new Button(Toggle);
        _closeButton.text = "Close (I)";
        _closeButton.style.marginTop = 15;
        _closeButton.style.height = 35;
        _closeButton.style.fontSize = 16;
        _closeButton.style.backgroundColor = new Color(0.39f, 0.2f, 0.2f);
        _closeButton.style.color = Color.white;
        mainContainer.Add(_closeButton);
        
        // Add mainContainer to inventory container
        _inventoryContainer.Add(mainContainer);
        
        // Add inventory container to root
        _root.Add(_inventoryContainer);
    }
    
    void CreateGunSlot(int index)
    {
        var gunSlot = new GunSlotUI();
        gunSlot.GunIndex = index;
        
        gunSlot.Container = new VisualElement();
        gunSlot.Container.style.marginBottom = 10;
        gunSlot.Container.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.7f);
        gunSlot.Container.style.borderTopWidth = 2;
        gunSlot.Container.style.borderBottomWidth = 2;
        gunSlot.Container.style.borderLeftWidth = 2;
        gunSlot.Container.style.borderRightWidth = 2;
        gunSlot.Container.style.borderTopColor = new Color(0.4f, 0.4f, 0.4f);
        gunSlot.Container.style.borderBottomColor = new Color(0.4f, 0.4f, 0.4f);
        gunSlot.Container.style.borderLeftColor = new Color(0.4f, 0.4f, 0.4f);
        gunSlot.Container.style.borderRightColor = new Color(0.4f, 0.4f, 0.4f);
        gunSlot.Container.style.paddingTop = 8;
        gunSlot.Container.style.paddingBottom = 8;
        gunSlot.Container.style.paddingLeft = 8;
        gunSlot.Container.style.paddingRight = 8;
        gunSlot.Container.style.flexDirection = FlexDirection.Column;
        
        // Top row: Gun info
        var topRow = new VisualElement();
        topRow.style.flexDirection = FlexDirection.Row;
        topRow.style.marginBottom = 10;
        
        var gunInfo = new VisualElement();
        gunInfo.style.width = 150;
        gunInfo.style.minWidth = 150;
        gunInfo.style.alignItems = Align.Center;
        gunInfo.style.marginRight = 10;
        
        var slotLabel = new Label("Slot " + (index + 1));
        slotLabel.style.fontSize = 10;
        slotLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
        slotLabel.style.marginBottom = 3;
        gunInfo.Add(slotLabel);
        
        gunSlot.Icon = new Image();
        gunSlot.Icon.style.width = 80;
        gunSlot.Icon.style.height = 80;
        gunSlot.Icon.style.backgroundColor = new Color(0, 0, 0, 0.3f);
        gunSlot.Icon.style.marginBottom = 5;
        gunInfo.Add(gunSlot.Icon);
        
        gunSlot.Name = new Label("Empty");
        gunSlot.Name.style.color = Color.white;
        gunSlot.Name.style.fontSize = 14;
        gunSlot.Name.style.unityTextAlign = TextAnchor.UpperCenter;
        gunSlot.Name.style.whiteSpace = WhiteSpace.Normal;
        gunSlot.Name.style.width = 140;
        gunInfo.Add(gunSlot.Name);
        
        topRow.Add(gunInfo);

        // Placeholder for magazine slots (will be populated when gun is assigned)
        var attachmentsContainer = new VisualElement
        {
            name = "MagazineContainer"
        };
        attachmentsContainer.style.flexDirection = FlexDirection.Row;
        attachmentsContainer.style.flexWrap = Wrap.Wrap;
        attachmentsContainer.style.flexGrow = 1;
        
        topRow.Add(attachmentsContainer);
        gunSlot.Container.Add(topRow);
        
        _gunsContainer.Add(gunSlot.Container);
        _gunSlots.Add(gunSlot);
    }
    
    MagazineSlotUI CreateMagazineSlot()
    {
        var slot = new MagazineSlotUI
        {
            Container = new VisualElement()
        };
        slot.Container.AddToClassList("magazine-slot"); // For drag-drop detection
        slot.Container.style.width = 60;
        slot.Container.style.height = 75;
        slot.Container.style.marginTop = 2;
        slot.Container.style.marginBottom = 2;
        slot.Container.style.marginLeft = 2;
        slot.Container.style.marginRight = 2;
        slot.Container.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        slot.Container.style.borderTopWidth = 1;
        slot.Container.style.borderBottomWidth = 1;
        slot.Container.style.borderLeftWidth = 1;
        slot.Container.style.borderRightWidth = 1;
        slot.Container.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f);
        slot.Container.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f);
        slot.Container.style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f);
        slot.Container.style.borderRightColor = new Color(0.3f, 0.3f, 0.3f);
        slot.Container.style.paddingTop = 4;
        slot.Container.style.paddingBottom = 4;
        slot.Container.style.paddingLeft = 4;
        slot.Container.style.paddingRight = 4;
        slot.Container.style.alignItems = Align.Center;
        slot.Container.style.display = DisplayStyle.None; // Hidden by default
        
        slot.Icon = new Image();
        slot.Icon.style.width = 35;
        slot.Icon.style.height = 35;
        slot.Icon.style.backgroundColor = new Color(0, 0, 0, 0.3f);
        slot.Icon.style.marginBottom = 3;
        
        slot.Name = new Label("");
        slot.Name.style.color = new Color(0.8f, 0.8f, 0.8f);
        slot.Name.style.fontSize = 8;
        slot.Name.style.unityTextAlign = TextAnchor.UpperCenter;
        slot.Name.style.whiteSpace = WhiteSpace.Normal;
        slot.Name.style.width = 52;
        
        slot.Container.Add(slot.Icon);
        slot.Container.Add(slot.Name);
        return slot;
    }
    
    InventorySlotUI CreateInventorySlot(int index)
    {
        var slot = new InventorySlotUI
        {
            Container = new VisualElement()
        };
        slot.Container.AddToClassList("inventory-slot"); // For drag-drop detection
        slot.Container.style.width = 70;
        slot.Container.style.height = 85;
        slot.Container.style.marginTop = 3;
        slot.Container.style.marginBottom = 3;
        slot.Container.style.marginLeft = 3;
        slot.Container.style.marginRight = 3;
        slot.Container.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        slot.Container.style.borderTopWidth = 2;
        slot.Container.style.borderBottomWidth = 2;
        slot.Container.style.borderLeftWidth = 2;
        slot.Container.style.borderRightWidth = 2;
        slot.Container.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f);
        slot.Container.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f);
        slot.Container.style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f);
        slot.Container.style.borderRightColor = new Color(0.3f, 0.3f, 0.3f);
        slot.Container.style.paddingTop = 5;
        slot.Container.style.paddingBottom = 5;
        slot.Container.style.paddingLeft = 5;
        slot.Container.style.paddingRight = 5;
        slot.Container.style.alignItems = Align.Center;
        
        slot.Icon = new Image();
        slot.Icon.style.width = 45;
        slot.Icon.style.height = 45;
        slot.Icon.style.backgroundColor = new Color(0, 0, 0, 0.3f);
        slot.Icon.style.marginBottom = 3;
        
        slot.Name = new Label("Empty");
        slot.Name.style.color = new Color(0.5f, 0.5f, 0.5f);
        slot.Name.style.fontSize = 9;
        slot.Name.style.unityTextAlign = TextAnchor.UpperCenter;
        slot.Name.style.whiteSpace = WhiteSpace.Normal;
        slot.Name.style.width = 60;
        
        // Create and register slot data for drag-drop
        slot.SlotData = new ElementSlotData(SlotType.InventorySlot, inventoryIndex: index);
        slot.Container.userData = slot.SlotData;
        
        // Register with drag-drop handler
        if (_dragDropHandler != null)
        {
            _dragDropHandler.RegisterElementSlot(slot.Container, slot.SlotData);
        }
        
        slot.Container.Add(slot.Icon);
        slot.Container.Add(slot.Name);
        return slot;
    }

    public void Toggle()
    {
        if (_inventoryContainer == null)
        {
            Debug.LogError("InventoryUI: _inventoryContainer is null!");
            return;
        }
        
        _isVisible = !_isVisible;
        _inventoryContainer.style.display = _isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        Debug.Log($"InventoryUI: Toggle called, _isVisible = {_isVisible}, display = {_inventoryContainer.style.display.value}");
        
        // Hide/show gun selection panel (HUD)
        if (_gunSelectionPanel != null && _gunSelectionPanel.Panel != null)
        {
            _gunSelectionPanel.Panel.style.display = _isVisible ? DisplayStyle.None : DisplayStyle.Flex;
        }
        
        if (_isVisible)
        {
            Time.timeScale = 0f;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            
            // Refresh all inventory slots when opening inventory
            RefreshAllInventorySlots();
        }
        else
        {
            Time.timeScale = 1f;
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }
    }
    
    void RefreshAllInventorySlots()
    {
        for (int i = 0; i < _inventorySlots.Count; i++)
        {
            UpdateInventorySlot(i);
        }
    }

    private void RefreshAllGunSlots()
    {
        if (_gunSlots.Count == 0)
        {
            return;
        }

        for (int i = 0; i < _gunSlots.Count; i++)
        {
            UpdateGunSlot(i, GetGunAtIndex(i));
        }
    }

    private void SyncGunSelectionFromInventory()
    {
        if (InventoryRef == null || _gunSelectionPanel == null)
        {
            return;
        }

        var currentGun = InventoryRef.CurrentGun;
        int index = currentGun != null ? InventoryRef.guns.IndexOf(currentGun) : -1;
        _gunSelectionPanel.SetSelectedGunIndex(index);
    }

    public void UpdateGunSlot(int index, Gun gun)
    {
        if (index < 0 || index >= _gunSlots.Count)
        {
            Debug.LogWarning($"UpdateGunSlot: Invalid index {index}");
            return;
        }

        var gunSlot = _gunSlots[index];
        
        // Store gun reference for updates
        gunSlot.Gun = gun;

        var magazineContainer = gunSlot.Container.Q<VisualElement>("MagazineContainer");
        if (magazineContainer == null)
        {
            Debug.LogError("MagazineContainer not found!");
            return;
        }

        magazineContainer.Clear();
        gunSlot.MagazineSlots.Clear();

        if (gun != null)
        {
            gunSlot.Icon.sprite = null;
            gunSlot.Name.text = gun.gunDefinition != null ? gun.gunDefinition.gunName : "Gun";

            var blueprint = gun.magazineBlueprint;
            int magazineSize = blueprint != null && blueprint.bullets != null
                ? blueprint.bullets.Length
                : (gun.gunDefinition != null ? gun.gunDefinition.stats.magazineSize : 0);

            for (int magazineIndex = 0; magazineIndex < magazineSize; magazineIndex++)
            {
                var bullet = blueprint != null && blueprint.bullets != null && magazineIndex < blueprint.bullets.Length
                    ? blueprint.bullets[magazineIndex]
                    : null;
                var element = bullet != null ? bullet.element : null;

                var magazineSlot = CreateMagazineSlot();
                UpdateMagazineSlot(magazineSlot, element, true);

                magazineSlot.SlotData = new ElementSlotData(
                    SlotType.MagazineSlot,
                    gunIndex: index,
                    magazineIndex: magazineIndex);
                magazineSlot.SlotData.Element = element;
                magazineSlot.Container.userData = magazineSlot.SlotData;

                if (_dragDropHandler != null)
                {
                    _dragDropHandler.RegisterElementSlot(magazineSlot.Container, magazineSlot.SlotData);
                }

                gunSlot.MagazineSlots.Add(magazineSlot);
                magazineContainer.Add(magazineSlot.Container);
            }
        }
        else
        {
            gunSlot.Icon.sprite = null;
            gunSlot.Name.text = "Empty";
        }
        
        // Refresh gun selection panel when guns change
        _gunSelectionPanel?.RefreshGuns();
    }
    
    void UpdateMagazineSlot(MagazineSlotUI slot, Element element, bool isActive, string emptyLabel = "Empty")
    {
        if (isActive && element != null)
        {
            // Show slot with element
            slot.Container.style.display = DisplayStyle.Flex;
            slot.Icon.sprite = element.elementIcon;
            slot.Name.text = element.elementName;
            slot.Container.style.opacity = 1.0f;
            slot.Container.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            slot.Container.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f);
            slot.Container.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f);
            slot.Container.style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f);
            slot.Container.style.borderRightColor = new Color(0.3f, 0.3f, 0.3f);
        }
        else if (isActive && element == null)
        {
            // Show empty but available slot (can receive elements)
            slot.Container.style.display = DisplayStyle.Flex;
            slot.Icon.sprite = null;
            slot.Name.text = emptyLabel;
            slot.Container.style.opacity = 0.6f;
            slot.Container.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            // Highlight border to show it's droppable
            slot.Container.style.borderTopColor = new Color(0.3f, 0.5f, 0.3f);
            slot.Container.style.borderBottomColor = new Color(0.3f, 0.5f, 0.3f);
            slot.Container.style.borderLeftColor = new Color(0.3f, 0.5f, 0.3f);
            slot.Container.style.borderRightColor = new Color(0.3f, 0.5f, 0.3f);
        }
        else
        {
            // Hide slot completely
            slot.Container.style.display = DisplayStyle.None;
        }
    }

    public void ClearAllSlots()
    {
        for (int i = 0; i < _gunSlots.Count; i++)
        {
            UpdateGunSlot(i, null);
        }
    }
    
    /// <summary>
    /// Gets the currently selected gun index from the gun selection panel
    /// </summary>
    public int GetSelectedGunIndex()
    {
        return _gunSelectionPanel?.GetSelectedGunIndex() ?? -1;
    }
}

/// <summary>
/// Gun selection panel that appears at the top of the screen
/// Shows available guns and highlights the selected one
/// </summary>
public class GunSelectionPanel
{
    private VisualElement _panel;
    private List<GunSlotDisplay> _gunSlots = new();
    private int _selectedGunIndex = -1;
    private Inventory _inventory;

    private const float CollapseDelaySeconds = 3.0f;
    private const float CollapseSpeed = 2.5f; // slower hide
    private const float ExpandSpeed = 10.0f;  // fast pop-up
    private const float CollapsePadding = 12.0f;
    private float _idleTime;
    private bool _targetCollapsed;
    private float _visibility = 1.0f;
    
    public VisualElement Panel => _panel;
    public System.Action<int> OnGunSelected;
    
    private class GunSlotDisplay
    {
        public VisualElement Container;
        public VisualElement IconContainer;
        public Image Icon;
        public Label NameLabel;
        public Label KeyLabel;
        public Label AmmoLabel;
        public VisualElement HighlightBorder;
        public int SlotIndex;
    }
    
    public GunSelectionPanel()
    {
        CreatePanel();
    }
    
    private void CreatePanel()
    {
        // Main panel container at top of screen
        _panel = new VisualElement();
        _panel.name = "GunSelectionPanel";
        _panel.style.position = Position.Absolute;
        _panel.style.top = 10;
        _panel.style.left = new Length(50, LengthUnit.Percent);
        _panel.style.translate = new Translate(new Length(-50, LengthUnit.Percent), 0);
        _panel.style.flexDirection = FlexDirection.Row;
        _panel.style.alignItems = Align.Center;
        _panel.style.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.8f);
        _panel.style.borderTopWidth = 2;
        _panel.style.borderBottomWidth = 2;
        _panel.style.borderLeftWidth = 2;
        _panel.style.borderRightWidth = 2;
        _panel.style.borderTopColor = new Color(0.3f, 0.5f, 0.7f);
        _panel.style.borderBottomColor = new Color(0.3f, 0.5f, 0.7f);
        _panel.style.borderLeftColor = new Color(0.3f, 0.5f, 0.7f);
        _panel.style.borderRightColor = new Color(0.3f, 0.5f, 0.7f);
        _panel.style.paddingTop = 8;
        _panel.style.paddingBottom = 8;
        _panel.style.paddingLeft = 10;
        _panel.style.paddingRight = 10;
        _panel.style.borderTopLeftRadius = 6;
        _panel.style.borderTopRightRadius = 6;
        _panel.style.borderBottomLeftRadius = 6;
        _panel.style.borderBottomRightRadius = 6;
        
        for (int i = 0; i < 9; i++)
        {
            var slot = CreateGunSlot(i);
            _gunSlots.Add(slot);
            _panel.Add(slot.Container);
        }
    }

    public void NotifyGunSwitched()
    {
        _idleTime = 0f;
        _targetCollapsed = false;
    }

    public void Tick(float deltaTime)
    {
        if (_panel == null || _panel.style.display == DisplayStyle.None)
        {
            return;
        }

        _idleTime += deltaTime;
        if (_idleTime >= CollapseDelaySeconds)
        {
            _targetCollapsed = true;
        }

        float target = _targetCollapsed ? 0f : 1f;
        float speed = _targetCollapsed ? CollapseSpeed : ExpandSpeed;
        _visibility = Mathf.MoveTowards(_visibility, target, speed * deltaTime);

        ApplyVisibility();
    }

    private void ApplyVisibility()
    {
        if (_panel == null)
        {
            return;
        }

        float height = Mathf.Max(_panel.resolvedStyle.height, 0f);
        float hiddenOffset = -(height + CollapsePadding);
        float offsetY = Mathf.Lerp(hiddenOffset, 0f, _visibility);

        _panel.style.translate = new Translate(
            new Length(-50, LengthUnit.Percent),
            new Length(offsetY, LengthUnit.Pixel));
        _panel.style.opacity = _visibility;
    }
    
    private GunSlotDisplay CreateGunSlot(int slotIndex)
    {
        var slot = new GunSlotDisplay
        {
            SlotIndex = slotIndex,

            // Main container
            Container = new VisualElement()
        };
        slot.Container.style.marginLeft = 4;
        slot.Container.style.marginRight = 4;
        slot.Container.style.alignItems = Align.Center;
        slot.Container.style.minWidth = 100;
        
        // Highlight border (invisible by default)
        slot.HighlightBorder = new VisualElement();
        slot.HighlightBorder.style.position = Position.Absolute;
        slot.HighlightBorder.style.top = -4;
        slot.HighlightBorder.style.bottom = -4;
        slot.HighlightBorder.style.left = -4;
        slot.HighlightBorder.style.right = -4;
        slot.HighlightBorder.style.borderTopWidth = 3;
        slot.HighlightBorder.style.borderBottomWidth = 3;
        slot.HighlightBorder.style.borderLeftWidth = 3;
        slot.HighlightBorder.style.borderRightWidth = 3;
        slot.HighlightBorder.style.borderTopColor = new Color(1f, 0.8f, 0.2f);
        slot.HighlightBorder.style.borderBottomColor = new Color(1f, 0.8f, 0.2f);
        slot.HighlightBorder.style.borderLeftColor = new Color(1f, 0.8f, 0.2f);
        slot.HighlightBorder.style.borderRightColor = new Color(1f, 0.8f, 0.2f);
        slot.HighlightBorder.style.borderTopLeftRadius = 8;
        slot.HighlightBorder.style.borderTopRightRadius = 8;
        slot.HighlightBorder.style.borderBottomLeftRadius = 8;
        slot.HighlightBorder.style.borderBottomRightRadius = 8;
        slot.HighlightBorder.style.display = DisplayStyle.None;
        slot.Container.Add(slot.HighlightBorder);
        
        // Icon container
        slot.IconContainer = new VisualElement();
        slot.IconContainer.style.width = 60;
        slot.IconContainer.style.height = 60;
        slot.IconContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.25f);
        slot.IconContainer.style.borderTopWidth = 1;
        slot.IconContainer.style.borderBottomWidth = 1;
        slot.IconContainer.style.borderLeftWidth = 1;
        slot.IconContainer.style.borderRightWidth = 1;
        slot.IconContainer.style.borderTopColor = new Color(0.4f, 0.4f, 0.4f);
        slot.IconContainer.style.borderBottomColor = new Color(0.4f, 0.4f, 0.4f);
        slot.IconContainer.style.borderLeftColor = new Color(0.4f, 0.4f, 0.4f);
        slot.IconContainer.style.borderRightColor = new Color(0.4f, 0.4f, 0.4f);
        slot.IconContainer.style.alignItems = Align.Center;
        slot.IconContainer.style.justifyContent = Justify.Center;
        slot.IconContainer.style.borderTopLeftRadius = 4;
        slot.IconContainer.style.borderTopRightRadius = 4;
        slot.IconContainer.style.borderBottomLeftRadius = 4;
        slot.IconContainer.style.borderBottomRightRadius = 4;
        
        // Gun icon
        slot.Icon = new Image();
        slot.Icon.style.width = 50;
        slot.Icon.style.height = 50;
        slot.IconContainer.Add(slot.Icon);
        slot.Container.Add(slot.IconContainer);
        
        // Gun name
        slot.NameLabel = new Label("Empty");
        slot.NameLabel.style.fontSize = 11;
        slot.NameLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
        slot.NameLabel.style.marginTop = 4;
        slot.NameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        slot.NameLabel.style.maxWidth = 90;
        slot.NameLabel.style.overflow = Overflow.Hidden;
        slot.NameLabel.style.textOverflow = TextOverflow.Ellipsis;
        slot.NameLabel.style.whiteSpace = WhiteSpace.NoWrap;
        slot.Container.Add(slot.NameLabel);
        
        // Key label
        slot.KeyLabel = new Label((slotIndex + 1).ToString());
        slot.KeyLabel.style.fontSize = 10;
        slot.KeyLabel.style.color = new Color(0.5f, 0.6f, 0.7f);
        slot.KeyLabel.style.marginTop = 2;
        slot.KeyLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        slot.KeyLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        slot.Container.Add(slot.KeyLabel);
        
        // Ammo label
        slot.AmmoLabel = new Label("--/--");
        slot.AmmoLabel.style.fontSize = 11;
        slot.AmmoLabel.style.color = new Color(0.7f, 0.9f, 1f);
        slot.AmmoLabel.style.marginTop = 2;
        slot.AmmoLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        slot.AmmoLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        slot.Container.Add(slot.AmmoLabel);
        
        return slot;
    }
    
    public void SetInventory(Inventory inventory)
    {
        _inventory = inventory;
        RefreshGuns();
    }
    
    public void RefreshGuns()
    {
        if (_inventory == null) return;
        
        for (int i = 0; i < _gunSlots.Count; i++)
        {
            var slot = _gunSlots[i];
            var gun = GetGunAtIndex(i);
            
            if (gun != null)
            {
                slot.Icon.sprite = null;
                slot.Icon.style.display = DisplayStyle.None;
                slot.NameLabel.text = gun.gunDefinition != null ? gun.gunDefinition.gunName : "Gun";
                slot.NameLabel.style.color = new Color(0.9f, 0.9f, 0.9f);
                slot.IconContainer.style.backgroundColor = new Color(0.25f, 0.25f, 0.3f);
            }
            else
            {
                slot.Icon.sprite = null;
                slot.Icon.style.display = DisplayStyle.None;
                slot.NameLabel.text = "Empty";
                slot.NameLabel.style.color = new Color(0.5f, 0.5f, 0.5f);
                slot.IconContainer.style.backgroundColor = new Color(0.15f, 0.15f, 0.2f);
            }
        }
        
        UpdateSelectionHighlight();
        UpdateGunSlots();
    }
    
    public void UpdateGunSlots()
    {
        if (_inventory == null) return;
        
        for (int i = 0; i < _gunSlots.Count; i++)
        {
            var slot = _gunSlots[i];
            var gun = GetGunAtIndex(i);
            
            if (gun != null)
            {
                int magazineCount = gun.GetMagazine() != null ? gun.GetMagazine().Count : 0;
                int magazineSize = gun.gunDefinition != null ? gun.gunDefinition.stats.magazineSize : 0;
                slot.AmmoLabel.text = $"{magazineCount}/{magazineSize}";
                slot.AmmoLabel.style.display = DisplayStyle.Flex;
                slot.AmmoLabel.style.color = new Color(0.7f, 0.9f, 1f);
            }
            else
            {
                // Empty slot - hide ammo display
                slot.AmmoLabel.style.display = DisplayStyle.None;
            }
        }
    }
    
    public void SelectGun(int index)
    {
        if (index < 0 || index >= _gunSlots.Count) return;
        
        if (_inventory != null)
        {
            var gun = GetGunAtIndex(index);
            if (gun == null) return; // Can't select empty slot
            _inventory.SwitchGun(index);
        }
        
        _selectedGunIndex = index;
        UpdateSelectionHighlight();
        OnGunSelected?.Invoke(index);
    }

    public void SetSelectedGunIndex(int index)
    {
        if (index < -1 || index >= _gunSlots.Count)
        {
            return;
        }

        _selectedGunIndex = index;
        UpdateSelectionHighlight();
    }
    
    public int GetSelectedGunIndex()
    {
        return _selectedGunIndex;
    }
    
    private void UpdateSelectionHighlight()
    {
        for (int i = 0; i < _gunSlots.Count; i++)
        {
            var slot = _gunSlots[i];
            bool isSelected = (i == _selectedGunIndex);
            
            slot.HighlightBorder.style.display = isSelected ? DisplayStyle.Flex : DisplayStyle.None;
            
            if (isSelected)
            {
                slot.IconContainer.style.borderTopColor = new Color(1f, 0.8f, 0.2f);
                slot.IconContainer.style.borderBottomColor = new Color(1f, 0.8f, 0.2f);
                slot.IconContainer.style.borderLeftColor = new Color(1f, 0.8f, 0.2f);
                slot.IconContainer.style.borderRightColor = new Color(1f, 0.8f, 0.2f);
                slot.IconContainer.style.borderTopWidth = 2;
                slot.IconContainer.style.borderBottomWidth = 2;
                slot.IconContainer.style.borderLeftWidth = 2;
                slot.IconContainer.style.borderRightWidth = 2;
                slot.NameLabel.style.color = new Color(1f, 0.9f, 0.6f);
                slot.KeyLabel.style.color = new Color(1f, 0.8f, 0.2f);
            }
            else
            {
                slot.IconContainer.style.borderTopColor = new Color(0.4f, 0.4f, 0.4f);
                slot.IconContainer.style.borderBottomColor = new Color(0.4f, 0.4f, 0.4f);
                slot.IconContainer.style.borderLeftColor = new Color(0.4f, 0.4f, 0.4f);
                slot.IconContainer.style.borderRightColor = new Color(0.4f, 0.4f, 0.4f);
                slot.IconContainer.style.borderTopWidth = 1;
                slot.IconContainer.style.borderBottomWidth = 1;
                slot.IconContainer.style.borderLeftWidth = 1;
                slot.IconContainer.style.borderRightWidth = 1;
                
                var gun = GetGunAtIndex(i);
                if (gun != null)
                {
                    slot.NameLabel.style.color = new Color(0.9f, 0.9f, 0.9f);
                }
                else
                {
                    slot.NameLabel.style.color = new Color(0.5f, 0.5f, 0.5f);
                }
                slot.KeyLabel.style.color = new Color(0.5f, 0.6f, 0.7f);
            }
        }
    }

    private Gun GetGunAtIndex(int index)
    {
        if (_inventory == null || index < 0 || index >= _inventory.guns.Count)
        {
            return null;
        }

        return _inventory.guns[index];
    }
}