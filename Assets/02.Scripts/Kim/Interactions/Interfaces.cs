using UnityEngine;

public interface IHoverable
{
     void OnHover();
     void OnHoverExit();
}

public interface IPlayerReceiver
{
    void SetPlayer(Transform player);
}

public interface IClickable
{
    void OnClick();
}