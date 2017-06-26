// Copyright 2001-2016 Crytek GmbH / Crytek Group. All rights reserved.

using System;
using System.Collections.Generic;
using CryEngine.Common;

namespace CryEngine
{
	public interface IGameUpdateReceiver
	{
		void OnUpdate();
	}

	public interface IGameRenderReceiver
	{
		void OnRender();
	}

	/// <summary>
	/// Listens for engine sided updates and informs registered objects by calling their OnUpdate function. Objects may register here, in order to be invoked on global update calls. Controls target FPS.
	/// </summary>
	public class GameFramework : IGameFrameworkListener
	{
		internal static GameFramework Instance { get; set; }
		private static List<Action> _destroyActions = new List<Action>();
		private static List<IGameUpdateReceiver> _updateReceivers = new List<IGameUpdateReceiver>();
		private static List<IGameRenderReceiver> _renderReceivers = new List<IGameRenderReceiver>();

		public override void OnSaveGame(ISaveGame pSaveGame)
		{
			Log.Info<GameFramework>("OnSaveGame");
		}

		public override void OnLoadGame(ILoadGame pLoadGame)
		{
			// nada
		}

		public override void OnForceLoadingWithFlash()
		{
			// nada
		}

		public override void OnLevelEnd(string nextLevel)
		{
			// nada
		}

		public override void OnSavegameFileLoadedInMemory(string pLevelName)
		{
			// nada
		}

		/// <summary>
		/// Registered object will be invoked on function OnUpdate if CryEngine's GameFramework raises OnPostUpdate.
		/// </summary>
		public static void RegisterForUpdate(IGameUpdateReceiver obj)
		{
			_updateReceivers.Add(obj);
		}

		/// <summary>
		/// Registered object will be invoked on function OnRender if CryEngine's GameFramework raises OnPreRender.
		/// </summary>
		public static void RegisterForRender(IGameRenderReceiver obj)
		{
			_renderReceivers.Add(obj);
		}

		/// <summary>
		/// Unregisters object from OnPostUpdate.
		/// </summary>
		public static void UnregisterFromUpdate(IGameUpdateReceiver obj)
		{
			_updateReceivers.Remove(obj);
		}

		/// <summary>
		/// Unregisters object from OnPreRender.
		/// </summary>
		public static void UnregisterFromRender(IGameRenderReceiver obj)
		{
			_renderReceivers.Remove(obj);
		}

		internal static void AddDestroyAction(Action destroyAction)
		{
			_destroyActions.Add(destroyAction);
		}

		/// <summary>
		/// Called by CryEngine. Do not call directly.
		/// </summary>
		public override void OnActionEvent(SActionEvent arg0)
		{
		}

		/// <summary>
		/// Called by CryEngine. Do not call directly.
		/// </summary>
		public override void OnPostUpdate(float fDeltaTime)
		{
			// Calculate time used to render last frame.
			FrameTime.Delta = fDeltaTime;

			if(_destroyActions.Count > 0)
			{
				var destroyActions = new List<Action>(_destroyActions);
				_destroyActions.Clear();
				foreach(var action in destroyActions)
				{
					action?.Invoke();
				}
			}

			var updateReceivers = new List<IGameUpdateReceiver>(_updateReceivers);
			foreach (IGameUpdateReceiver obj in updateReceivers)
			{
				obj.OnUpdate();
			}
		}

		/// <summary>
		/// Called by CryEngine. Do not call directly.
		/// </summary>
		public override void OnPreRender()
		{
			var renderReceivers = new List<IGameRenderReceiver>(_renderReceivers);
			foreach (IGameRenderReceiver obj in renderReceivers)
			{
				obj.OnRender();
			}
		}

		internal GameFramework()
		{
			AddListener();

			Engine.EndReload += AddListener;
		}

		void AddListener()
		{
			Engine.GameFramework.RegisterListener(this, "MonoGameFramework", EFRAMEWORKLISTENERPRIORITY.FRAMEWORKLISTENERPRIORITY_DEFAULT);
		}

		public override void Dispose()
		{
			Engine.GameFramework.UnregisterListener(this);

			base.Dispose();
		}
	}
}
