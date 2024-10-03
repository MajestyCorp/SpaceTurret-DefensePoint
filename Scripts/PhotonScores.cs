using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Crion
{
    public class PhotonScores : MonoBehaviour, IPunObservable
    {
        private List<PlayerStats> listStats = null;
        private List<PlayerStats> botStats = null;
        private int scores;
        private PhotonView view;

        public void FinishGame()
        {
            byte[] array = Serialize();

            view.RPC("RPC_End", RpcTarget.AllBuffered, array);
        }

        public List<PlayerStats> GetList()
        {
            return listStats;
        }

        [PunRPC]
        public void RPC_End(byte[] array, PhotonMessageInfo info)
        {
            //read incoming data
            Deserialize(array);

            //depending on victory or defeat, show message
            PlayerStats stats = GetStatsByActorId(PhotonNetwork.LocalPlayer.ActorNumber);
            LobbyManager.Instance.ShowGameOver(stats.victory);
        }

        public byte[] Serialize()
        {
            byte[] array = null;

            using (var ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    writer.Write(listStats.Count);

                    for (int i = 0; i < listStats.Count; i++)
                    {
                        writer.Write(listStats[i].ActorId);
                        listStats[i].Serialize(writer);
                    }

                    writer.Write(botStats.Count);
                    for(int i=0;i<botStats.Count;i++)
                    {
                        botStats[i].SerializeBot(writer);
                    }
                }
                array = ms.ToArray();
            }

            return array;
        }

        public void Deserialize(byte[] data)
        {
            int count;
            int actorId;
            PlayerStats stats;

            using (MemoryStream m = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    count = reader.ReadInt32();

                    for (int i = 0; i < count; i++)
                    {
                        actorId = reader.ReadInt32();
                        stats = GetStatsByActorId(actorId);
                        stats.Deserialize(reader);
                    }

                    count = reader.ReadInt32();
                    botStats.Clear();
                    for(int i=0;i<count;i++)
                    {
                        stats = new PlayerStats();
                        stats.DeserializeBot(reader);
                        botStats.Add(stats);
                    }

                }
            }
        }

        private PlayerStats GetStatsByActorId(int actorId)
        {
            for (int i = 0; i < listStats.Count; i++)
                if (listStats[i].ActorId == actorId)
                    return listStats[i];
            return null;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if(stream.IsWriting)
            {
                for (int i = 0; i < listStats.Count; i++)
                    stream.SendNext(listStats[i].Scores);
            } else
            {
                for(int i=0;i<listStats.Count;i++)
                {
                    scores = (int)stream.ReceiveNext();
                    listStats[i].SetScores(scores);
                }
            }
        }

        private void Awake()
        {
            listStats = LobbyManager.Instance.GetPlayerStatsList();
            view = GetComponent<PhotonView>();
            botStats = LobbyManager.Instance.GetBotStats();
        }
    }
}
