using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Dynamic;
using System.Threading.Tasks;

namespace Client
{
    public static class Utils
    {
        //These arrays are simply translation tables between the index in the list and the actual values I want to use when drawing
        public static float[] TextSizes = new float[] { 1, 2, 3, 4, 6, 8, 10 };
        public static int[] TextFonts = new int[] { 0, 1, 2, 4, 7 };
        public static float[] TextRanges = new float[] { 5, 10, 20, 50, 100 };

        public static void Draw3DText(Vector3 Position, string text, int size, int font, int range)
        {
            Vector3 CameraPosition = API.GetGameplayCamCoord();
            float Distance = API.GetDistanceBetweenCoords(CameraPosition.X, CameraPosition.Y, CameraPosition.Z, Position.X, Position.Y, Position.Z, true);

            if (Distance <= TextRanges[range])
            {
                float SimpleScale = (1 / Distance) * TextSizes[size];
                float FOV = (1 / API.GetGameplayCamFov()) * 100;
                float AdjustedScale = SimpleScale * FOV;

                API.SetDrawOrigin(Position.X, Position.Y, Position.Z, 0);

                API.SetTextScale(0 * AdjustedScale, .35f * AdjustedScale);
                API.SetTextFont(TextFonts[font]);
                API.SetTextProportional(true);
                API.SetTextColour(255, 255, 255, 255);
                API.SetTextDropshadow(0, 0, 0, 0, 255);
                API.SetTextEdge(2, 0, 0, 0, 150);
                API.SetTextDropShadow();
                API.SetTextOutline();
                API.SetTextEntry("STRING");
                API.SetTextCentre(true);
                API.AddTextComponentString(text);
                API.DrawText(0, 0);

                API.ClearDrawOrigin();
            }
        }

        private static Action<bool, string> CallbackAction;
        private static bool TypingOnKeyboard = false;
        
        public static void OpenKeyboard(Action<bool, string> Callback, string Text = "")
        {
            if (!TypingOnKeyboard) {
                //Open the keyboard and save variables needed in "GetKeyboardInput()"
                CallbackAction = Callback;
                API.DisplayOnscreenKeyboard(0, "FMMC_KEY_TIP8S", "", Text, "", "", "", 200);
                TypingOnKeyboard = true;
            }
            else
            {
                //This should probably never happen since it's imposible to press a button in a menu while typing but if it were to happen it's good to know
                Debug.WriteLine("Was asked to open keyboard but keyboard was already open!");
            }
        }

        public static async Task GetKeyboardInput()
        {
            //Check if the user is currently typing in a keyboard we have created
            if (TypingOnKeyboard)
            {
                API.HideHudAndRadarThisFrame();
                //Get what the user did this frame
                int Update = API.UpdateOnscreenKeyboard();
                if (Update == 1)
                {
                    //If the user stopped editing by pressing enter we get the result and call the callback function with the first argument as true and the second as the text they entered
                    string InputText = API.GetOnscreenKeyboardResult();
                    if (InputText.Length > 0)
                    {
                        TypingOnKeyboard = false;
                        CallbackAction.Invoke(true, InputText);
                    }
                    else
                    {
                        //They didn't write anything - Open the keyboard again to allow them to enter text in order to prevent empty text
                        API.DisplayOnscreenKeyboard(0, "FMMC_KEY_TIP8S", "", "", "", "", "", 200);
                    }
                }
                else if (Update == 2 || Update == 3)
                {
                    //The user must have exited the keyboard - call the callback with false as first argument indicating that it didn't succeed
                    TypingOnKeyboard = false;
                    CallbackAction.Invoke(false, "");
                }
            }
        }

        public class InGameText
        {
            public Vector3 Position;
            public string Text;
            public int Size;
            public int Font;
            public int Range;

            public readonly int ID;

            public InGameText(Vector3 pos, string text, int size, int font, int range, int id)
            {
                Position = pos;
                Text = text;
                Size = size;
                Font = font;
                Range = range;
                ID = id;
            }

            public void Draw() { Draw3DText(Position, Text, Size, Font, Range); }

            public dynamic ToExpandoObject()
            {
                dynamic obj = new ExpandoObject();
                obj.Position = Position;
                obj.Text = Text;
                obj.Size = Size;
                obj.Font = Font;
                obj.Range = Range;
                obj.ID = ID;
                return obj;
            }
        }
    }
}
