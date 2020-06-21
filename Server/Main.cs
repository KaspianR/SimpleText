using CitizenFX.Core;
using System;

namespace Server
{
    public class Main : BaseScript
    {
        public Main()
        {
            EventHandlers["SimpleText:Server:UpdateTextList"] += new Action<Player>(TextManager.UpdateTextList);

            EventHandlers["SimpleText:Server:CreateText"] += new Action<Player, string, Vector3>(TextManager.CreateText);
            EventHandlers["SimpleText:Server:MoveText"] += new Action<Player, int, Vector3>(TextManager.MoveText);
            EventHandlers["SimpleText:Server:EditText"] += new Action<Player, int, string>(TextManager.EditText);
            EventHandlers["SimpleText:Server:DeleteText"] += new Action<Player, int>(TextManager.DeleteText);
        }
    }
}
