using System.ComponentModel;

namespace PoE_Trade_Bot.Enums
{
    public enum ChatCommand
    {
        [Description("/kick ")]
        KICK,
        [Description("/invite ")]
        INVITE,
        [Description("/tradewith ")]
        TRADE,
        [Description("/afk ")]
        AFK,
        [Description("/afkoff")]
        AFK_OFF,
        [Description("/hideout")]
        GOTO_MY_HIDEOUT,
        [Description("/hideout ")]
        GOTO_HIDEOUT
    }
}
