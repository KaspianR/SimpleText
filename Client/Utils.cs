using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Dynamic;

namespace Client
{
    public static class Utils
    {
        public static void Draw3DText(Vector3 Position, string text)
        {
            API.SetDrawOrigin(Position.X, Position.Y, Position.Z, 0);

            Vector3 CameraPosition = API.GetGameplayCamCoord();
            float Distance = API.GetDistanceBetweenCoords(CameraPosition.X, CameraPosition.Y, CameraPosition.Z, Position.X, Position.Y, Position.Z, true);
            float SimpleScale = (1 / Distance) * 2;
            float FOV = (1 / API.GetGameplayCamFov()) * 100;
            float AdjustedScale = SimpleScale * FOV;

            API.SetTextScale(0 * AdjustedScale, .35f * AdjustedScale);
            API.SetTextFont(0);
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

        public class InGameText
        {
            public Vector3 Position;
            public string Text;
            public readonly int ID;

            public InGameText(Vector3 pos, string text, int id)
            {
                Position = pos;
                Text = text;
                ID = id;
            }

            public void Draw() { Draw3DText(Position, Text); }

            public dynamic ToExpandoObject()
            {
                dynamic obj = new ExpandoObject();
                obj.Position = Position;
                obj.Text = Text;
                obj.ID = ID;
                return obj;
            }
        }
    }
}
