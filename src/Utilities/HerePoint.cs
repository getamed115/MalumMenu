using UnityEngine;

namespace MalumMenu;

public sealed class HerePoint
{
    public PlayerControl Player;
    public SpriteRenderer Sprite;

    public HerePoint(PlayerControl player, SpriteRenderer sprite) => (Player, Sprite) = (player, sprite);
}
