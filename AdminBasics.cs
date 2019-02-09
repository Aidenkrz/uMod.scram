// Reference: Assembly-CSharp
// Reference: Assembly-CSharp-firstpass
// Reference: UnityEngine
// Reference: bolt.user

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using uMod.Plugins;
using Scram;
using UnityEngine;
using UnityEngine.Networking;
using Bolt;
using Steamworks;

namespace uMod.Plugins
{
    [Info("AdminBasics", "Virtual", "1.0")]
    [Description("AdminBasics")]

    public class AdminBasics : CSharpPlugin
    {
		
		public static List<string> admins = new List<string>() { "76561198164512098", "76561198849487373", "76561198414685366" };
		
		void Init()
		{
		}
		
		void OnSpawn(PlayerConnection connection, string creature, string gadget, string armor, Vector3 position, float yaw)
		{
			//BoltGlobalEvent.SendObjectiveEvent("Disclaimer: This is a community server.", "Alert", new Color32(255, 133, 0, 255), connection.BoltConnection);
		}
		
		void OnMoveNext(object that)
		{
			Scram.HostMessage hostmsg = ((Scram.HostMessage)that.GetType().GetField("$this", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).GetValue(that));
			Debug.Log(hostmsg.moderators);
			foreach (string i in admins)
				hostmsg.moderators += "|" + i;
		}
		
		object OnEvent(ChatEvent ev)
		{
			string user = ev.RaisedBy.GetPlayerConnection().SteamID.m_SteamID.ToString();
			
			if (admins.IndexOf(user) == -1) return null;
			string cmd = ev.Text;
			if (cmd.StartsWith("/")) ev.Text = "";
			else return null;
			if (cmd.StartsWith("/ssay "))
			{
				string[] args = cmd.Split(' ');
				string kickuser = "";
				for (int i = 1; i < args.Length; i++)
				{
					kickuser += args[i];
					if (i < args.Length)
					{
						kickuser += " ";
					}
				}
				// string[] args1 = args.split('"');
				if (kickuser != "" && args.Length > 2)
				{
					BoltGlobalEvent.SendMessage("<color=#CCCCCC>[<color=red>Server<color=#CCCCCC>] Says: " + kickuser, (Color32) Color.white);
				}
				else
				{
					BoltGlobalEvent.SendPrivateMessage("/ssay <server message 2+ words long>", new Color32(255, 0, 0, 255), ev.RaisedBy);
				}
				ev.Text = "";
			}
			if (cmd.StartsWith("/kickid "))
			{
				string[] args = cmd.Split(' ');
				string kickuser = "";
				for (int i = 1; i < args.Length; i++)
				{
					kickuser += args[i];
					if (i < args.Length)
					{
						kickuser += " ";
					}
				}
				// string[] args1 = args.split('"');
				if (kickuser != "")
				{
					using (IEnumerator<BoltConnection> enumerator = BoltNetwork.connections.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							BoltConnection current = enumerator.Current;
							if (current.GetPlayerConnection().PlayerInfo.state.SteamID.ToString() == kickuser)
							{
								BoltGlobalEvent.SendMessage("<color=#CCCCCC>[<color=red>Server<color=#CCCCCC>] Kicked: " + current.GetPlayerConnection().PlayerInfo.state.PenName, (Color32) Color.white);
								BoltConnection boltConnection = current;
								RefuseToken refuseToken1 = new RefuseToken();
								refuseToken1.RefuseReason = "Kicked by admin.";
								RefuseToken refuseToken2 = refuseToken1;
								boltConnection.Disconnect((IProtocolToken) refuseToken2);
								break;
							}
						}
					}
				}
				else
				{
					BoltGlobalEvent.SendPrivateMessage("/kickid <steamid>", new Color32(255, 0, 0, 255), ev.RaisedBy);
				}
				ev.Text = "";
			}
			if (cmd.StartsWith("/kick "))
			{
				string[] args = cmd.Split(' ');
				string kickuser = "";
				for (int i = 1; i < args.Length; i++)
				{
					kickuser += args[i];
					if (i + 1 < args.Length)
					{
						kickuser += " ";
					}
				}
				// string[] args1 = args.split('"');
				if (kickuser != "")
				{
					using (IEnumerator<BoltConnection> enumerator = BoltNetwork.connections.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							BoltConnection current = enumerator.Current;
							string newname = current.GetPlayerConnection().PlayerInfo.state.PenName.Substring(9, current.GetPlayerConnection().PlayerInfo.state.PenName.Length - 19);
							if (newname.ToLower().IndexOf(kickuser.ToLower()) != -1 && current.GetPlayerConnection().PlayerInfo.state.SteamID != user)
							{
								
								BoltGlobalEvent.SendMessage("<color=#CCCCCC>[<color=red>Server<color=#CCCCCC>] Kicked: " + current.GetPlayerConnection().PlayerInfo.state.PenName, (Color32) Color.white);
								BoltConnection boltConnection = current;
								RefuseToken refuseToken1 = new RefuseToken();
								refuseToken1.RefuseReason = "Kicked by admin.";
								RefuseToken refuseToken2 = refuseToken1;
								boltConnection.Disconnect((IProtocolToken) refuseToken2);
								break;
							}
						}
					}
				}
				else
				{
					BoltGlobalEvent.SendPrivateMessage("/kick <username>", new Color32(255, 0, 0, 255), ev.RaisedBy);
				}
				ev.Text = "";
			}
			if (cmd.StartsWith("/")) ev.Text = "";
			return true;
			//return ev;
		}
        
    }
}
