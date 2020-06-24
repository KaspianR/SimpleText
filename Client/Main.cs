using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;

namespace Client
{
    public class Main : BaseScript
    {
        public static List<Utils.InGameText> ActiveText = new List<Utils.InGameText>();
        public static string Command = "text";

        public Main()
        {
            //Register the command for opening the menu (I didn't want to use a keybind since I don't think this will be used as often as other things that might need that keybind)
            API.RegisterCommand(Command, new Action(() => MenuManager.OpenMenu()), false);

            //Set up the menus
            MenuManager.SetupMenu();

            //Add all functions that have to be called every tick
            Tick += DrawAllText;
            Tick += MenuManager.UpdateMenus;
            Tick += Utils.GetKeyboardInput;

            //Register events
            EventHandlers["SimpleText:Client:Update"] += new Action<dynamic>(UpdateText);
            EventHandlers["SimpleText:Client:ShowNotification"] += new Action<string>(ShowNotification);

            //If there are already text active in the server we ask the server to send the active text to just this user
            TriggerServerEvent("SimpleText:Server:UpdateTextList");
        }

        //Loop through all the text and draw each one
        private static async Task DrawAllText()
        {
            foreach(Utils.InGameText text in ActiveText)
            {
                text.Draw();
            }
        }

        //Since the event didn't like me sending my custom "InGameText" class they are sent as ExpandoObjects and made in to "InGameText" when received
        private static void UpdateText(dynamic RawActiveText)
        {
            ActiveText = new List<Utils.InGameText>();
            for (int n = 0; n < RawActiveText.Count; n++)
            {
                ActiveText.Add(new Utils.InGameText(RawActiveText[n].Position, RawActiveText[n].Text, RawActiveText[n].Size, RawActiveText[n].Font, RawActiveText[n].Range, RawActiveText[n].ID));
            }
            MenuManager.SetupListMenu();
        }

        //Show a notification on the users screen
        //Notifications are used instead of the chat to make sure that less important information doesn't get in the way of more important information from other players
        public static void ShowNotification(string Notification)
        {
            Screen.ShowNotification(Notification);
        }
    }
}
