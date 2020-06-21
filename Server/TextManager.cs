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
        public static void CreateText([FromSource] Player PlayerExecutingCommand, string Text, Vector3 Position)
        {
            ActiveText.Add(new Utils.InGameText(Position, Text, NextID));
            PlayerExecutingCommand.TriggerEvent("SimpleText:Client:ShowNotification", $"~g~The text has been spawned! ~s~It got ID: ~b~{NextID++}");
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
                PlayerExecutingCommand.TriggerEvent("SimpleText:Client:ShowNotification", "~r~The text could not be moved! ~s~You have either entered an invalid ~b~ID~s~ or this text have been ~r~deleted.");
            }
        }

        //Find the text with the same ID and change it's text
        public static void EditText([FromSource] Player PlayerExecutingCommand, int ID, string Text)
        {
            int index = ActiveText.FindIndex(t => t.ID == ID);
            if (index > -1)
            {
                ActiveText[index].Text = Text;
                PlayerExecutingCommand.TriggerEvent("SimpleText:Client:ShowNotification", "~g~The text has been edited!");
                UpdateAllTextLists();
            }
            else
            {
                PlayerExecutingCommand.TriggerEvent("SimpleText:Client:ShowNotification", "~r~The text could not be edited! ~s~You have either entered an invalid ~b~ID~s~ or this text have been ~r~deleted.");
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
                PlayerExecutingCommand.TriggerEvent("SimpleText:Client:ShowNotification", "~r~The text could not be deleted! ~s~You have either entered an invalid ~b~ID~s~ or this text have already been ~r~deleted.");
            }
        }
    }
}
