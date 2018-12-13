using System;
using System.Reflection;
using SwinGameSDK;
using Color = System.Drawing.Color;

namespace MyGame
{
    public class GameMain
    {
        public static void Main()
        {
            //Start the audio system so sound can be played
            SwinGame.OpenAudio();
			SwinGame.LoadFontNamed("GUI_text", "arial", 20);
            
            //Open the game window
			SwinGame.OpenGraphicsWindow("AI Test", 1680, 840);
			//SwinGame.ToggleFullScreen();
            
			Game theGame = new Game ();
			theGame.main();
            
            //End the audio
            SwinGame.CloseAudio();
            
            //Close any resources we were using
            SwinGame.ReleaseAllResources();
        }
    }
}