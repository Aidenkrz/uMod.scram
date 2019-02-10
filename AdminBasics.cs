// Reference: Assembly-CSharp
// Reference: Assembly-CSharp-firstpass
// Reference: UnityEngine
// Reference: bolt.user

using System;
using System.Linq;
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
		
		public static List<string> admins = new List<string>();
		public List<string> bans = new List<string>();
		public List<string> vip = new List<string>();
		public int vipslots = 0;
		
		void Init()
		{
			Puts("LOADED.");
			try
			{
				vipslots = (int)Config["vipslots"];
			}
			catch (Exception e)
			{
				Puts("Unable to load vipslots");
			}
			try
			{
				vip = ((IEnumerable<object>)Config["vip"]).OfType<string>().ToList();
			}
			catch (Exception e)
			{
				Puts("Unable to load vip");
			}
			try
			{
				admins = ((IEnumerable<object>)Config["admins"]).OfType<string>().ToList();
			}
			catch (Exception e)
			{
				Puts("Unable to load admins");
			}
			try
			{
				bans = ((IEnumerable<object>)Config["bans"]).OfType<string>().ToList();
			}
			catch (Exception e)
			{
				Puts("Unable to load bans!");
				Debug.Log("Unable to load bans!");
			}
		}
		
		protected override void LoadDefaultConfig()
		{
			Puts("Creating a new configuration file");
			Config["SpawnMessage"] = "Disclaimer: This is a community server.";
			Config["admins"] = new List<string>() { "steam64id1", "steam64id2" };
			Config["vip"] = new List<string>() { "steam64id1", "steam64id2" };
			Config["vipslots"] = 0;
			Config["bans"] = new List<string>() {};
		}
		
		object OnPlayerJoinMatch(PlayerConnection connection)
		{
			if (bans.Contains(connection.SteamID.m_SteamID.ToString()))
			{
				BoltConnection boltConnection = connection.BoltConnection;
				RefuseToken refuseToken1 = new RefuseToken();
				refuseToken1.RefuseReason = "Banned by admin.";
				RefuseToken refuseToken2 = refuseToken1;
				boltConnection.Disconnect((IProtocolToken) refuseToken2);
				return true;
			}
			
			if (BoltNetwork.connections.Count() >= SteamHeadless.RoomPlayerLimit - vipslots && !vip.Contains(connection.SteamID.m_SteamID.ToString()))
			{
				BoltConnection boltConnection = connection.BoltConnection;
				RefuseToken refuseToken1 = new RefuseToken();
				refuseToken1.RefuseReason = "the server has reserved slots.";
				RefuseToken refuseToken2 = refuseToken1;
				boltConnection.Disconnect((IProtocolToken) refuseToken2);
				return true;
			}
			return null;
		}
		
		void OnSpawn(PlayerConnection connection, string creature, string gadget, string armor, Vector3 position, float yaw)
		{
			if (bans.Contains(connection.SteamID.m_SteamID.ToString()))
			{
				BoltConnection boltConnection = connection.BoltConnection;
				RefuseToken refuseToken1 = new RefuseToken();
				refuseToken1.RefuseReason = "Banned by admin.";
				RefuseToken refuseToken2 = refuseToken1;
				boltConnection.Disconnect((IProtocolToken) refuseToken2);
			}
			BoltGlobalEvent.SendObjectiveEvent((string)Config["SpawnMessage"], "Alert", new Color32(255, 133, 0, 255), connection.BoltConnection);
		}
		
		void OnMoveNext(object that)
		{
			Scram.HostMessage hostmsg = ((Scram.HostMessage)that.GetType().GetField("$this", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).GetValue(that));
			Debug.Log(hostmsg.moderators);
			foreach (string i in admins)
				hostmsg.moderators += "|" + i;
		}
		
		// chat commands:
		// /kick <username>
		// /kickid <steam64id>
		// /ssay <message to say as server>
		// /ban <>
		// /banid <>
		// /rl
		// /reload
		// unban <>
		object OnEvent(ChatEvent ev)
		{
			string user = ev.RaisedBy.GetPlayerConnection().SteamID.m_SteamID.ToString();
			
			if (bans.Contains(user))
			{
				BoltConnection boltConnection = ev.RaisedBy;
				RefuseToken refuseToken1 = new RefuseToken();
				refuseToken1.RefuseReason = "Banned by admin.";
				RefuseToken refuseToken2 = refuseToken1;
				boltConnection.Disconnect((IProtocolToken) refuseToken2);
				return true;
			}
			
			if (admins.IndexOf(user) == -1) return null;
			string cmd = ev.Text;
			if (cmd.StartsWith("/")) ev.Text = "";
			else return null;
			//ssay
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
			//rl or reload
			if (cmd.StartsWith("/rl ") || cmd.StartsWith("/reload"))
			{
				try
				{
					admins = ((IEnumerable<object>)Config["admins"]).OfType<string>().ToList();
				}
				catch (Exception e)
				{
					Puts("Unable to load admins");
					BoltGlobalEvent.SendPrivateMessage("Unable to load admins", new Color32(255, 0, 0, 255), ev.RaisedBy);
				}
				try
				{
					bans = ((IEnumerable<object>)Config["bans"]).OfType<string>().ToList();
				}
				catch (Exception e)
				{
					Puts("Unable to load bans!");
					BoltGlobalEvent.SendPrivateMessage("Unable to load bans!", new Color32(255, 0, 0, 255), ev.RaisedBy);
					Debug.Log("Unable to load bans!");
				}
				try
				{
					vipslots = (int)Config["vipslots"];
				}
				catch (Exception e)
				{
					Puts("Unable to load vipslots");
					BoltGlobalEvent.SendPrivateMessage("Unable to load vipslots", new Color32(255, 0, 0, 255), ev.RaisedBy);
				}
				try
				{
					vip = ((IEnumerable<object>)Config["vip"]).OfType<string>().ToList();
				}
				catch (Exception e)
				{
					Puts("Unable to load vip");
					BoltGlobalEvent.SendPrivateMessage("Unable to load vip", new Color32(255, 0, 0, 255), ev.RaisedBy);
				}
				ev.Text = "";
			}
			//unban
			if (cmd.StartsWith("/unban "))
			{
				string[] args = cmd.Split(' ');
				string kickuser = "";
				for (int i = 1; i < args.Length; i++)
				{
					kickuser += args[i];
					if (i < args.Length - 1)
					{
						kickuser += " ";
					}
				}
				if (kickuser != "" && bans.Contains(kickuser))
				{
					bans.Remove(kickuser);
					Config["bans"] = bans;
					SaveConfig();
					BoltGlobalEvent.SendMessage("<color=#CCCCCC>[<color=red>Server<color=#CCCCCC>] Unbanned: " + kickuser, (Color32) Color.white);
				}
				else
				{
					BoltGlobalEvent.SendPrivateMessage("/unban <steam64id>", new Color32(255, 0, 0, 255), ev.RaisedBy);
				}
				ev.Text = "";
			}
			//banid
			if (cmd.StartsWith("/banid "))
			{
				string[] args = cmd.Split(' ');
				string kickuser = "";
				for (int i = 1; i < args.Length; i++)
				{
					kickuser += args[i];
					if (i < args.Length - 1)
					{
						kickuser += " ";
					}
				}
				if (kickuser != "")
				{
					using (IEnumerator<BoltConnection> enumerator = BoltNetwork.connections.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							BoltConnection current = enumerator.Current;
							if (current.GetPlayerConnection().PlayerInfo.state.SteamID.ToString() == kickuser)
							{
								bans.Add(kickuser);
								Config["bans"] = bans;
								SaveConfig();
								BoltGlobalEvent.SendMessage("<color=#CCCCCC>[<color=red>Server<color=#CCCCCC>] Banned: " + current.GetPlayerConnection().PlayerInfo.state.PenName, (Color32) Color.white);
								BoltConnection boltConnection = current;
								RefuseToken refuseToken1 = new RefuseToken();
								refuseToken1.RefuseReason = "Banned by admin.";
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
			//ban
			if (cmd.StartsWith("/ban "))
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
								bans.Add(current.GetPlayerConnection().PlayerInfo.state.SteamID);
								Config["bans"] = bans;
								SaveConfig();
								BoltGlobalEvent.SendMessage("<color=#CCCCCC>[<color=red>Server<color=#CCCCCC>] Banned: " + current.GetPlayerConnection().PlayerInfo.state.PenName, (Color32) Color.white);
								BoltConnection boltConnection = current;
								RefuseToken refuseToken1 = new RefuseToken();
								refuseToken1.RefuseReason = "Banned by admin.";
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
			//kickid
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
			//kick
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
		}
        
    }
}
