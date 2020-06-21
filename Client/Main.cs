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
        private static List<Utils.InGameText> ActiveText = new List<Utils.InGameText>();
        public static string Command = "text";

        public Main()
        {
            API.RegisterCommand(Command, new Action<int, List<object>, string>(TextCommand), false);

            Tick += DrawAllText;

            EventHandlers["SimpleText:Client:Update"] += new Action<dynamic>(UpdateText);
            EventHandlers["SimpleText:Client:ShowNotification"] += new Action<string>(ShowNotification);

            TriggerServerEvent("SimpleText:Server:UpdateTextList");
        }

        private static void TextCommand(int Source, List<object> Arguments, string RawCommand)
        {
            if (Arguments.Count < 2)
            {
                WriteError($"Error! The avaiable commands are:\n^2Create: ^0/text c ^3[Text]\n^6Move here: ^0/text m ^4[ID]\n^5Edit: ^0/text e ^4[ID] ^3[New text]\n^1Delete: ^0/text d ^4[ID]");
            }

            string action = Arguments[0].ToString();
            if (action == "c")
            {
                Debug.WriteLine(Source.ToString());
                TriggerServerEvent("SimpleText:Server:CreateText", string.Join(" ", Arguments.GetRange(1, Arguments.Count - 1)), Game.Player.Character.Position);
            }
            else if (action == "m")
            {
                int ID;
                if(Arguments.Count == 2 && int.TryParse(Arguments[1].ToString(), out ID))
                {
                    TriggerServerEvent("SimpleText:Server:MoveText", ID, Game.Player.Character.Position);
                }
                else
                {
                    WriteError($"Error: Improper command usage. Correct usage: /text ^6m ^4[ID]");
                }
            }
            else if (action == "e")
            {
                int ID;
                if (int.TryParse(Arguments[1].ToString(), out ID))
                {
                    TriggerServerEvent("SimpleText:Server:EditText", ID, string.Join(" ", Arguments.GetRange(2, Arguments.Count - 2)));
                }
                else
                {
                    WriteError($"Error: Improper command usage. Correct usage: /text ^5e ^4[ID] ^3[New text]");
                }
            }
            else if (action == "d")
            {
                int ID;
                if (Arguments.Count == 2 && int.TryParse(Arguments[1].ToString(), out ID))
                {
                    TriggerServerEvent("SimpleText:Server:DeleteText", ID);
                }
                else
                {
                    WriteError($"Error: Improper command usage. Correct usage: /text ^1d ^4[ID]");
                }
            }
            else
            {
                WriteError($"Error! {'"' + action + '"'} is not a valid action. The avaiable actions are:\n^2Create: ^0/text c ^3[Text]\n^6Move here: ^0/text m ^4[ID]\n^5Edit: ^0/text e ^4[ID] ^3[New text]\n^1Delete: ^0/text d ^4[ID]");
            }
        }

        //Loop through all the text and draw each one
        private static async Task DrawAllText()
        {
            foreach(Utils.InGameText text in ActiveText)
            {
                text.Draw();
            }
        }

        private static void UpdateText(dynamic RawActiveText)
        {
            if (RawActiveText.Count > 0) {
                Debug.WriteLine("==============");
                IDictionary<string, object> propertyValues = RawActiveText[0];

                foreach (var property in propertyValues.Keys)
                {
                    Debug.WriteLine(string.Format("{0} : {1}", property, propertyValues[property]));
                }
                Debug.WriteLine("==============");
            }

            Debug.WriteLine(RawActiveText.Count > 0 ? "> " + RawActiveText[0].ToString() + " <" : "-");
            ActiveText = new List<Utils.InGameText>();
            for (int n = 0; n < RawActiveText.Count; n++)
            {
                Debug.WriteLine(RawActiveText[n].Text + ": " + RawActiveText[n].Position.ToString() + " (" + RawActiveText[n].ID.ToString() + ")");
                ActiveText.Add(new Utils.InGameText(RawActiveText[n].Position, RawActiveText[n].Text, RawActiveText[n].ID));
            }
        }

        private static void ShowNotification(string Notification)
        {
            Screen.ShowNotification(Notification);
        }

        private static void WriteError(string Error)
        {
            TriggerEvent("chatMessage", "SimpleText", new object[] { 255, 50, 50 }, Error);
        }
    }
}
