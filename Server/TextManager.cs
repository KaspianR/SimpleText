using Client;
using CitizenFX.Core;
using System.Collections.Generic;
using System.Linq;

namespace Server
{
    public static class TextManager
    {
        private static List<Utils.InGameText> ActiveText = new List<Utils.InGameText>();
        private static int NextID = 0;

        //Send the new list of active texts to the player for them to store locally
        public static void UpdateTextList([FromSource] Player PlayerToUpdate)
        {
            PlayerToUpdate.TriggerEvent("SimpleText:Client:Update", ActiveText.Select(text => text.ToExpandoObject()));
        }

        //Send the new list of active texts to all players for them to store locally
        public static void UpdateAllTextLists()
        {
            BaseScript.TriggerClientEvent("SimpleText:Client:Update", ActiveText.Select(text => text.ToExpandoObject()));
        }

        //Append a new "InGameText" object to the list of text and return it's ID
        public static void CreateText([FromSource] Player PlayerExecutingCommand, string Text, Vector3 Position, int Size, int Font, int Range)
        {
            ActiveText.Add(new Utils.InGameText(Position, Text, Size, Font, Range, NextID));
            PlayerExecutingCommand.TriggerEvent("SimpleText:Client:ShowNotification", $"~g~The text has been spawned!");
            UpdateAllTextLists();
        }

        //Find the text with the same ID and change it's position
        public static void MoveText([FromSource] Player PlayerExecutingCommand, int ID, Vector3 Position)
        {
            int index = ActiveText.FindIndex(t => t.ID == ID);
            if (index > -1)
            {
                ActiveText[index].Position = Position;
                PlayerExecutingCommand.TriggerEvent("SimpleText:Client:ShowNotification", "~g~The text has been moved!");
                UpdateAllTextLists();
            }
            else
            {
                PlayerExecutingCommand.TriggerEvent("SimpleText:Client:ShowNotification", "~r~The text could not be moved! ~s~The text might have been deleted so please go back to the list and try again. If this issue persists: please report this to the developer!");
            }
        }

        //Find the text with the same ID and change it's text
        public static void EditText([FromSource] Player PlayerExecutingCommand, int ID, string Text, int Size, int Font, int Range)
        {
            int index = ActiveText.FindIndex(t => t.ID == ID);
            if (index > -1)
            {
                ActiveText[index].Text = Text;
                ActiveText[index].Size = Size;
                ActiveText[index].Font = Font;
                ActiveText[index].Range = Range;
                PlayerExecutingCommand.TriggerEvent("SimpleText:Client:ShowNotification", "~g~The text has been edited!");
                UpdateAllTextLists();
            }
            else
            {
                PlayerExecutingCommand.TriggerEvent("SimpleText:Client:ShowNotification", "~r~The text could not be edited! ~s~The text might have been deleted so please go back to the list and try again. If this issue persists: please report this to the developer!");
            }
        }

        //Find the text with the same ID and remove it
        public static void DeleteText([FromSource] Player PlayerExecutingCommand, int ID)
        {
            int index = ActiveText.FindIndex(t => t.ID == ID);
            if(index > -1)
            {
                ActiveText.RemoveAt(index);
                PlayerExecutingCommand.TriggerEvent("SimpleText:Client:ShowNotification", "~g~The text has been deleted!");
                UpdateAllTextLists();
            }
            else
            {
                PlayerExecutingCommand.TriggerEvent("SimpleText:Client:ShowNotification", "~r~The text could not be deleted! ~s~The text might already have been deleted so please go back to the list and try again. If this issue persists: please report this to the developer!");
            }
        }
    }
}
