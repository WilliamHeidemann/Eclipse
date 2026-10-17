using LitMotion;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Presentation.UI.Menus;

public class VerticalShiftMenu : IMenu
{
    private readonly VisualElement _root;
    private readonly Vector2 _inScreen;
    private readonly Vector2 _outScreen;
    
    private MotionHandle _motionHandle;
    
    public VerticalShiftMenu(VisualElement root, bool isSubMenu)
    {
        _root = root;
        root.usageHints = UsageHints.DynamicTransform;
        Translate start = root.style.translate.value;
        _inScreen = new Vector2(start.x.value, start.y.value);
        Vector2 slideDirection = isSubMenu ? Vector2.left : Vector2.right;
        _outScreen = _inScreen - slideDirection * 1000;
    }

    public void Show()
    {
        Slide(_outScreen, _inScreen, false);
    }

    public void Hide()
    {
        Slide(_inScreen, _outScreen, true);
    }

    private void Slide(Vector2 start, Vector2 end, bool hideAfter)
    {
        if (_motionHandle.IsActive())
        {
            _motionHandle.Cancel();
        }
        
        _root.style.display = DisplayStyle.Flex;
        _motionHandle = LMotion
            .Create(start, end, 1f)
            .WithEase(Ease.OutExpo)
            .WithOnComplete(() =>
            {
                if (hideAfter)
                {
                    _root.style.display = DisplayStyle.None;
                }
            })
            .Bind(value => _root.style.translate = new Translate(value.x, value.y));
    }
}