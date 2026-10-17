using UnityEngine.UIElements;

namespace Game.Presentation.UI.Menus;

public class SimpleDisplayMenu(VisualElement root) : IMenu
{
    public void Show()
    {
        root.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        root.style.display = DisplayStyle.None;
    }
}