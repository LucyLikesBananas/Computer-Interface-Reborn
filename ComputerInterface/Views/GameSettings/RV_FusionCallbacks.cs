using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings
{
    internal class RV_FusionCallbacks : MonoBehaviour, INetworkRunnerCallbacks
    {
        public RoomView roomView;

        public void Start()
        {
            if (NetworkSystem.Instance.TryGetComponent(out NetworkSystemFusion fusion))
            {
                fusion.runner.AddCallbacks(this);
            }

            NetworkSystem.Instance.OnMultiplayerStarted += (Action) OnJoinedRoom;
            NetworkSystem.Instance.OnReturnedToSinglePlayer += (Action) OnLeftRoom;
        }

        public void OnDestroy()
        {
            if (NetworkSystem.Instance.TryGetComponent(out NetworkSystemFusion fusion))
            {
                fusion.runner.RemoveCallbacks(this);
            }

            NetworkSystem.Instance.OnMultiplayerStarted -= (Action) OnJoinedRoom;
            NetworkSystem.Instance.OnReturnedToSinglePlayer -= (Action) OnLeftRoom;
        }

        public void OnJoinedRoom() => roomView.Redraw(useTemporaryState: true, temporaryState: NetSystemState.InGame);
        public void OnLeftRoom() => roomView.Redraw(useTemporaryState: true, temporaryState: NetSystemState.Idle);

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {

        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {

        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {

        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {

        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            roomView.Redraw(useTemporaryState: true, temporaryState: NetSystemState.Idle);
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            roomView.Redraw(useTemporaryState: true, temporaryState: NetSystemState.Idle);
        }

        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
            roomView.Redraw(useTemporaryState: true, temporaryState: NetSystemState.Idle);
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {

        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {

        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {

        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {

        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {

        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            roomView.Redraw(useTemporaryState: true, temporaryState: NetSystemState.Idle);
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
        {

        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {

        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {

        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            // CI has no need to react to AOI changes; intentionally left empty.
            // Previously threw NotImplementedException, which crashed Fusion when invoked.
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            // CI has no need to react to AOI changes; intentionally left empty.
            // Previously threw NotImplementedException, which crashed Fusion when invoked.
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            // Newer Fusion overload of OnDisconnectedFromServer. Mirror the single-arg
            // behaviour so the room view still updates instead of throwing.
            roomView.Redraw(useTemporaryState: true, temporaryState: NetSystemState.Idle);
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
            // CI does not send/receive custom reliable data; intentionally left empty.
            // Previously threw NotImplementedException, which crashed Fusion when invoked.
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
            // CI does not track reliable data progress; intentionally left empty.
            // Previously threw NotImplementedException, which crashed Fusion when invoked.
        }
    }
}
