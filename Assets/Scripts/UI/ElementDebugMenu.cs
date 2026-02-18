using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ElementDebugMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private ElementDatabase elementDatabase;

    [SerializeField]
    private ElementInventory elementInventory;

    [Header("Toggle")]
    [SerializeField]
    private KeyCode toggleKey = KeyCode.F1;

    [SerializeField]
    private bool pauseOnOpen;

    private UIDocument document;
    private VisualElement root;
    private VisualElement menuContainer;
    private ScrollView buttonScroll;
    private bool isVisible;

    private void Awake()
    {
        document = GetComponent<UIDocument>();
        root = document.rootVisualElement;

        if (elementInventory == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                elementInventory = player.GetComponent<ElementInventory>();
            }
        }

        BuildUI();
        SetVisible(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        SetVisible(!isVisible);
    }

    private void SetVisible(bool visible)
    {
        isVisible = visible;
        if (menuContainer != null)
        {
            menuContainer.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        if (pauseOnOpen)
        {
            Time.timeScale = visible ? 0f : 1f;
        }

        UnityEngine.Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
        UnityEngine.Cursor.visible = visible;
    }

    private void BuildUI()
    {
        if (root == null)
        {
            return;
        }

        root.Clear();

        menuContainer = new VisualElement();
        menuContainer.style.position = Position.Absolute;
        menuContainer.style.top = 0;
        menuContainer.style.left = 0;
        menuContainer.style.width = new Length(100, LengthUnit.Percent);
        menuContainer.style.height = new Length(100, LengthUnit.Percent);
        menuContainer.style.backgroundColor = new Color(0f, 0f, 0f, 0.75f);
        menuContainer.style.paddingTop = 20;
        menuContainer.style.paddingLeft = 20;
        menuContainer.style.paddingRight = 20;
        menuContainer.style.paddingBottom = 20;

        var title = new Label("ELEMENT DEBUG MENU");
        title.style.fontSize = 24;
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        title.style.color = Color.white;
        title.style.marginBottom = 12;
        menuContainer.Add(title);

        buttonScroll = new ScrollView(ScrollViewMode.Vertical);
        buttonScroll.style.flexGrow = 1;
        buttonScroll.style.backgroundColor = new Color(0f, 0f, 0f, 0.35f);
        buttonScroll.style.paddingTop = 8;
        buttonScroll.style.paddingBottom = 8;
        buttonScroll.style.paddingLeft = 8;
        buttonScroll.style.paddingRight = 8;
        buttonScroll.style.borderTopLeftRadius = 6;
        buttonScroll.style.borderTopRightRadius = 6;
        buttonScroll.style.borderBottomLeftRadius = 6;
        buttonScroll.style.borderBottomRightRadius = 6;
        menuContainer.Add(buttonScroll);

        var closeHint = new Label($"Toggle: {toggleKey}");
        closeHint.style.fontSize = 12;
        closeHint.style.color = new Color(0.8f, 0.8f, 0.8f);
        closeHint.style.marginTop = 8;
        menuContainer.Add(closeHint);

        root.Add(menuContainer);

        RefreshButtons();
    }

    private void RefreshButtons()
    {
        if (buttonScroll == null)
        {
            return;
        }

        buttonScroll.Clear();

        if (elementDatabase == null)
        {
            var label = new Label("ElementDatabase not assigned.");
            label.style.color = new Color(1f, 0.5f, 0.5f);
            buttonScroll.Add(label);
            return;
        }

        if (elementInventory == null)
        {
            var label = new Label("ElementInventory not found.");
            label.style.color = new Color(1f, 0.5f, 0.5f);
            buttonScroll.Add(label);
            return;
        }

        var elements = elementDatabase.elements;
        if (elements == null || elements.Length == 0)
        {
            var label = new Label("No elements registered in database.");
            label.style.color = new Color(1f, 0.8f, 0.5f);
            buttonScroll.Add(label);
            return;
        }

        foreach (var element in elements)
        {
            if (element == null)
            {
                continue;
            }

            var button = new Button(() => elementInventory.AddElement(element, 1))
            {
                text = $"Add {element.elementName}"
            };
            button.style.marginBottom = 6;
            button.style.height = 32;
            button.style.unityTextAlign = TextAnchor.MiddleCenter;
            buttonScroll.Add(button);
        }
    }
}
