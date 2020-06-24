using CitizenFX.Core;
using CitizenFX.Core.Native;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client
{
    public static class MenuManager
    {
        //These lists contain the strings to be drawn in the lists when creating or editing text
        private static List<object> TextSizes = new List<object> { "1", "2", "3", "4", "6", "8", "10" };
        private static List<object> TextFonts = new List<object> { "Chalet london", "House script", "Monospace", "Charlet comprime colonge", "Pricedown" };
        private static List<object> TextRanges = new List<object> { "5m", "10m", "20m", "50m", "100m" };

        private static MenuPool TextMenuPool = new MenuPool();
        private static UIMenu TextMenu;
        private static UIMenu CreateMenu;
        private static UIMenu ListMenu;
        private static UIMenu EditMenu;

        private static int EditID;

        public static void SetupMenu()
        {
            TextMenu = new UIMenu("SimpleText", "~b~The configuration menu for SimpleText");
            TextMenuPool.Add(TextMenu);

            //Set up the menu for creating new text
            CreateMenu = TextMenuPool.AddSubMenu(TextMenu, "Create text", "Opens a menu where you can create new text");
            CreateMenu.AddItem(new UIMenuListItem("Size", TextSizes, 1, "Changes the size of the text"));
            CreateMenu.AddItem(new UIMenuListItem("Font", TextFonts, 0, "Changes the texts font"));
            CreateMenu.AddItem(new UIMenuListItem("Range", TextRanges, 2, "Changes at what distance the text becomes vissible (measured from the camera)"));
            CreateMenu.AddItem(new UIMenuItem("~g~Create text", "Creates the text"));

            //Create the menu for listing the text but don't add any items as this is done dynamically
            ListMenu = TextMenuPool.AddSubMenu(TextMenu, "Edit text", "Opens a menu where you can choose some text to edit");

            //Set up the menu for editing existing text
            EditMenu = TextMenuPool.AddSubMenu(ListMenu, "~b~Edit text");
            EditMenu.AddItem(new UIMenuListItem("Size", TextSizes, 0, "Creates the text"));
            EditMenu.AddItem(new UIMenuListItem("Font", TextFonts, 0, "Creates the text"));
            EditMenu.AddItem(new UIMenuListItem("Range", TextRanges, 0, "Creates the text"));
            EditMenu.AddItem(new UIMenuItem("~g~Move text", "Move the text to your position"));
            EditMenu.AddItem(new UIMenuItem("~b~Edit text"));
            EditMenu.AddItem(new UIMenuItem("~r~Delete text"));
            EditMenu.RefreshIndex();

            //Add functions to call when an item is selected
            CreateMenu.OnItemSelect += CreateMenuItemSelected;
            ListMenu.OnItemSelect += ListMenuItemSelected;
            EditMenu.OnItemSelect += EditMenuItemSelected;

            //Add functions to call when a list changes
            EditMenu.OnListChange += EditMenuListChange;

            //A NativeUI function that has to be called after adding or removing items
            TextMenu.RefreshIndex();
            CreateMenu.RefreshIndex();
            EditMenu.RefreshIndex();
        }

        public static void CreateMenuItemSelected(UIMenu Sender, UIMenuItem SelectedItem, int Index)
        {
            if (SelectedItem.GetType() == typeof(UIMenuItem))
            {
                //Open the keyboard and provide a callback (a action in this function) that will be called when the user has finished typing
                Utils.OpenKeyboard((bool Success, string InputText) =>
                {
                    if (Success)
                    {
                        BaseScript.TriggerServerEvent("SimpleText:Server:CreateText", InputText, Game.Player.Character.Position, ((UIMenuListItem) Sender.MenuItems[0]).Index, ((UIMenuListItem)Sender.MenuItems[1]).Index, ((UIMenuListItem)Sender.MenuItems[2]).Index);
                    }
                    else
                    {
                        Main.ShowNotification("~r~Something went wrong... ~s~Remember to press ~b~ENTER ~s~when you are done writing.");
                    }
                });
            }
        }

        //This function is called every time a item is selected in the list of text
        public static void ListMenuItemSelected(UIMenu Sender, UIMenuItem SelectedItem, int Index)
        {
            //Figure out the ID of the text the user selected
            EditID = Main.ActiveText[Sender.MenuItems.FindIndex(item => item == SelectedItem)].ID;

            SetupEditMenu();

            ListMenu.Visible = false;
            EditMenu.Visible = true;
        }

        //This function is called every time a item is selected on the edit menu
        public static void EditMenuItemSelected(UIMenu Sender, UIMenuItem SelectedItem, int Index)
        {
            //Make sure it's not one of the list items being selected
            if (SelectedItem.GetType() == typeof(UIMenuItem))
            {
                //Check the position (the same as it's index) of the item to see which one it is and trigger the appropriate event
                if (Sender.MenuItems.FindIndex(item => item == SelectedItem) == 3)
                {
                    BaseScript.TriggerServerEvent("SimpleText:Server:MoveText", EditID, Game.Player.Character.Position);
                }
                if (Sender.MenuItems.FindIndex(item => item == SelectedItem) == 4)
                {
                    Utils.OpenKeyboard((bool Success, string InputText) =>
                    {
                        if (Success)
                        {
                            BaseScript.TriggerServerEvent("SimpleText:Server:EditText", EditID, InputText, ((UIMenuListItem)Sender.MenuItems[0]).Index, ((UIMenuListItem)Sender.MenuItems[1]).Index, ((UIMenuListItem)Sender.MenuItems[2]).Index);
                        }
                        else
                        {
                            Main.ShowNotification("~r~Something went wrong... ~s~Remember to press ~b~ENTER ~s~when you are done writing.");
                        }
                    }, Main.ActiveText[EditID].Text);
                }
                else if(Sender.MenuItems.FindIndex(item => item == SelectedItem) == 5)
                {
                    BaseScript.TriggerServerEvent("SimpleText:Server:DeleteText", EditID);
                    EditMenu.Visible = false;
                    ListMenu.Visible = true;
                }
            }
        }

        //Trigger the "EditText" event on the server
        public static void EditMenuListChange(UIMenu Sender, UIMenuItem SelectedItem, int Index)
        {
            BaseScript.TriggerServerEvent("SimpleText:Server:EditText", EditID, Main.ActiveText.Find(text => text.ID == EditID).Text, ((UIMenuListItem)Sender.MenuItems[0]).Index, ((UIMenuListItem)Sender.MenuItems[1]).Index, ((UIMenuListItem)Sender.MenuItems[2]).Index);
        }

        //Add one item for every text currently in the world
        //This funcation is ran every time a new list is recieved so it should be up to date at all times
        public static void SetupListMenu()
        {
            ListMenu.Clear();
            for (int n = 0; n < Main.ActiveText.Count; n++)
            {
                string PreviewText = Main.ActiveText[n].Text.Substring(0, Math.Min(Main.ActiveText[n].Text.Length, 45));
                ListMenu.AddItem(new UIMenuItem(PreviewText + (Main.ActiveText[n].Text.Length > 45 ? "..." : ""), Main.ActiveText[n].Text));
            }
            ListMenu.RefreshIndex();
        }

        //Set the currently selected items to the same items that the text currently has
        public static void SetupEditMenu()
        {
            Utils.InGameText CurrentText = Main.ActiveText.Find(text => text.ID == EditID);
            ((UIMenuListItem)EditMenu.MenuItems[0]).Index = CurrentText.Size;
            ((UIMenuListItem)EditMenu.MenuItems[1]).Index = CurrentText.Font;
            ((UIMenuListItem)EditMenu.MenuItems[2]).Index = CurrentText.Range;
        }

        //Just something that NativeUI needs to do every tick
        public static async Task UpdateMenus()
        {
            TextMenuPool.ProcessMenus();
        }

        //Opens (or closes if false is given as argument) the default menu
        public static void OpenMenu(bool open = true)
        {
            TextMenu.Visible = open;
        }
    }
}
