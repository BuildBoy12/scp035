﻿using System;
using System.Collections.Generic;
using System.Linq;
using EXILED;
using MEC;
using UnityEngine;

namespace scp035
{
	partial class EventHandlers
	{
		public Plugin plugin;
		public EventHandlers(Plugin plugin) => this.plugin = plugin;

		private Dictionary<Pickup, float> scpPickups = new Dictionary<Pickup, float>();
		private List<int> ffPlayers = new List<int>();
		internal static ReferenceHub scpPlayer;
		private bool isHidden;
		private bool isRoundStarted;
		private bool isRotating;
		private const float dur = 327;
		private System.Random rand = new System.Random();

		private List<CoroutineHandle> coroutines = new List<CoroutineHandle>();

		public void OnWaitingForPlayers()
		{
			Configs.ReloadConfig();
		}

		public void OnRoundStart()
		{
			isRoundStarted = true;
			isRotating = true;
			scpPickups.Clear();
			scpPlayer = null;

			coroutines.Add(Timing.RunCoroutine(DelayAction(1f, () => Timing.RunCoroutine(RotatePickup()))));
			coroutines.Add(Timing.RunCoroutine(CorrodeUpdate()));
		}

		public void OnRoundEnd()
		{
			isRoundStarted = false;
		}

		public void OnRoundRestart()
		{
			// In case the round is force restarted
			isRoundStarted = false;

			foreach (CoroutineHandle handle in coroutines)
			{
				Timing.KillCoroutines(handle);
			}
			coroutines.Clear();
		}

		public void OnPickupItem(ref PickupItemEvent ev)
		{
			if (ev.Item.info.durability == dur)
			{
				ev.Allow = false;
				InfectPlayer(ev.Player, ev.Item);
			}
		}

		public void OnPlayerHurt(ref PlayerHurtEvent ev)
		{
			if (ffPlayers.Contains(ev.Attacker.queryProcessor.PlayerId))
			{
				GrantFF(ev.Attacker);
			}

			if (scpPlayer != null)
			{
				if (!Configs.scpFriendlyFire &&
					((ev.Attacker.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId &&
					Plugin.GetTeam(ev.Player.characterClassManager.CurClass) == Team.SCP) ||
					(ev.Player.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId &&
					Plugin.GetTeam(ev.Attacker.characterClassManager.CurClass) == Team.SCP)))
				{
					ev.Info = new PlayerStats.HitInfo(0f, ev.Attacker.nicknameSync.name, ev.Info.GetDamageType(), ev.Attacker.queryProcessor.PlayerId);
				}

				if (!Configs.tutorialFriendlyFire &&
					ev.Attacker.queryProcessor.PlayerId != ev.Player.queryProcessor.PlayerId &&
					((ev.Attacker.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId &&
					Plugin.GetTeam(ev.Player.characterClassManager.CurClass) == Team.TUT) ||
					(ev.Player.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId &&
					Plugin.GetTeam(ev.Attacker.characterClassManager.CurClass) == Team.TUT)))
				{
					ev.Info = new PlayerStats.HitInfo(0f, ev.Attacker.nicknameSync.name, ev.Info.GetDamageType(), ev.Attacker.queryProcessor.PlayerId);
				}
			}
		}

		public void OnShoot(ref ShootEvent ev)
		{
			if (ev.Target == null || scpPlayer == null) return;
			ReferenceHub target = Plugin.GetPlayer(ev.Target);

			if ((ev.Shooter.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId && Plugin.GetTeam(target.characterClassManager.CurClass) == Plugin.GetTeam(scpPlayer.characterClassManager.CurClass)) || (target.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId && Plugin.GetTeam(ev.Shooter.characterClassManager.CurClass) == Plugin.GetTeam(scpPlayer.characterClassManager.CurClass)))
			{
				ev.Shooter.weaponManager.NetworkfriendlyFire = true;
				ffPlayers.Add(ev.Shooter.queryProcessor.PlayerId);
			}
		}

		public void OnPlayerDie(ref PlayerDeathEvent ev)
		{
			if (ev.Player.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId)
			{
				KillScp035();
			}
		}

		public void OnPocketDimensionEnter(PocketDimEnterEvent ev)
		{
			if (ev.Player.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId && !Configs.scpFriendlyFire)
			{
				ev.Allow = false;
			}
		}

		public void OnCheckRoundEnd(ref CheckRoundEndEvent ev)
		{
			List<Team> pList = Plugin.GetHubs().Where(x => x.queryProcessor.PlayerId != scpPlayer?.queryProcessor.PlayerId).Select(x => Plugin.GetTeam(x.characterClassManager.CurClass)).ToList();

			// If everyone but SCPs and 035 or just 035 is alive, end the round
			if ((!pList.Contains(Team.CHI) && !pList.Contains(Team.CDP) && !pList.Contains(Team.MTF) && !pList.Contains(Team.RSC) && ((pList.Contains(Team.SCP) && scpPlayer != null) || !pList.Contains(Team.SCP) && scpPlayer != null)) ||
				(Configs.winWithTutorial && !pList.Contains(Team.CHI) && !pList.Contains(Team.CDP) && !pList.Contains(Team.MTF) && !pList.Contains(Team.RSC) && pList.Contains(Team.TUT) && scpPlayer != null))
			{
				ev.LeadingTeam = RoundSummary.LeadingTeam.Anomalies;
				ev.ForceEnd = true;
			}

			// If 035 is the only scp alive keep the round going
			else if (scpPlayer != null && !pList.Contains(Team.SCP))
			{
				ev.Allow = false;
			}
		}

		public void OnCheckEscape(ref CheckEscapeEvent ev)
		{
			if (ev.Player.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId) ev.Allow = false;
		}

		public void OnSetClass(SetClassEvent ev)
		{
			if (ev.Player.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId)
			{
				KillScp035();
			}
		}

		public void OnPlayerLeave(PlayerLeaveEvent ev)
		{
			if (ev.Player.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId)
			{
				KillScp035(false);
			}
		}

		public void OnContain106(Scp106ContainEvent ev)
		{
			if (ev.Player.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId && !Configs.scpFriendlyFire)
			{
				ev.Allow = false;
			}
		}

		public void OnInsertTablet(ref GeneratorInsertTabletEvent ev)
		{
			if (ev.Player.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId && !Configs.scpFriendlyFire)
			{
				ev.Allow = false;
			}
		}

		public void OnPocketDimensionDie(PocketDimDeathEvent ev)
		{
			if (ev.Player.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId)
			{
				ev.Allow = false;
				ev.Player.plyMovementSync.OverridePosition(GameObject.FindObjectOfType<SpawnpointManager>().GetRandomPosition(RoleType.Scp096).transform.position, 0);
			}
		}

		public void OnUseMedicalItem(MedicalItemEvent ev)
		{
			if (ev.Player.queryProcessor.PlayerId == scpPlayer?.queryProcessor.PlayerId && (ev.Item == ItemType.Adrenaline || ev.Item == ItemType.Painkillers || ev.Item == ItemType.Medkit))
			{
				ev.Allow = false;
			}
		}
	}
}