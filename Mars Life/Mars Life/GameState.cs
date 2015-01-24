using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mars_Life
{
    public enum GameState
    {
        None,
        SplashScreen,
        LogIn,
        MainMenu,
        Credits, CreditsTransition,
        PlayGame, NewGame, LoadGame,
        Paused,
        ExitGame
    }
}
