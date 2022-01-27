
// This file is provided under The MIT License as part of RiptideSteamTransport.
// Copyright (c) 2021 Tom Weiland
// For additional information please see the included LICENSE.md file or view it on GitHub: https://github.com/tom-weiland/RiptideSteamTransport/blob/main/LICENSE.md

using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

namespace RiptideDemos.SteamTransport.PlayerHosted
{
    public class ClientPlayer : MonoBehaviour
    {
        public static Dictionary<ushort, ClientPlayer> list = new Dictionary<ushort, ClientPlayer>();

        [SerializeField] private ushort id;
        [SerializeField] private string username;

        public void Move(Vector3 newPosition, Vector3 forward)
        {
            transform.position = newPosition;

            if (id != NetworkManager.Singleton.Client.Id) // Don't overwrite local player's forward direction to avoid noticeable rotational snapping
                transform.forward = forward;
        }

        private void OnDestroy()
        {
            list.Remove(id);
        }

        public static void Spawn(ushort id, string username, Vector3 position)
        {
            ClientPlayer player;
            if (id == NetworkManager.Singleton.Client.Id)
                player = Instantiate(NetworkManager.Singleton.LocalPlayerPrefab, position, Quaternion.identity).GetComponent<ClientPlayer>();
            else
            {
                player = Instantiate(NetworkManager.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<ClientPlayer>();
                player.GetComponentInChildren<PlayerUIManager>().SetName(username);
            }

            player.name = $"Client Player {id} ({username})";
            player.id = id;
            player.username = username;
            list.Add(player.id, player);
        }

        #region Messages
        [MessageHandler((ushort)ServerToClientId.spawnPlayer, NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void SpawnPlayer(Message message)
        {
            Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
        }

        [MessageHandler((ushort)ServerToClientId.playerMovement, NetworkManager.PlayerHostedDemoMessageHandlerGroupId)]
        private static void PlayerMovement(Message message)
        {
            ushort playerId = message.GetUShort();
            if (list.TryGetValue(playerId, out ClientPlayer player))
                player.Move(message.GetVector3(), message.GetVector3());
        }
        #endregion
    }
}